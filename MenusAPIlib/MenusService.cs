/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Data.Entity;

using MenusAPI;
using MenusAPIlib.Logging;
using MenusAPIlib.Model;
using MenusAPIlib.Tokenizer;

using System.Data.Entity.Core.Objects;
using System.Collections.Concurrent;

namespace MenusAPIlib
{
    [ServiceBehavior(AutomaticSessionShutdown = true, ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class MenusService : Service, IMenusService
    {
        public MenusService()
        {
            Start();
        }

        ILoggingService _Logger = null;

        protected ILoggingService Logger
        {
            get
            {
                if (_Logger != null)
                {
                    if (!_Logger.IsStarted)
                        _Logger.Start();

                    return _Logger;
                }

                return _Logger;
            }
        }


        protected TokenizedCollection<UserCredentials> _LoginTokensStore = null;
        protected ConcurrentDictionary<int, HashSet<int>> _UserID2Tokens = null;

        protected ReadObjectsReply AuthenticateUserAction(MenusEntityModelContainer context, User authenticatedUser, AuthenticateUserRequest request)
        {
            var result = new AuthenticateUserReply();
            result.SecurityToken = null;
            if(request.RequestorCredentials.TokenID.HasValue && APILibUtils.EnableTokenBasedAuthentication) //This will execute AFTER LoginUser(), so the token is properly filled out if applicable.
                result.SecurityToken = _LoginTokensStore.GetToken(request.RequestorCredentials.TokenID.Value);
            result.ReadObjects.Add(authenticatedUser.CreateBusinessObject(translatePassword: false, getSatteliteData: request.GetSatteliteData));
            return result;
        }

        public AuthenticateUserReply AuthenticateUser(AuthenticateUserRequest request)
        {
            return ProcessRequest<AuthenticateUserReply>(request,
                action: (context, bcontext, user) => AuthenticateUserAction(context, user, (AuthenticateUserRequest)bcontext.OriginalRequest),
                transactional: true
                );
        }

        protected APIReply LogoutUserAction(MenusEntityModelContainer context, User authenticatedUser, LogoutUserRequest request)
        {
            var result = new APIReply();

            if (!APILibUtils.EnableTokenBasedAuthentication)
                return result;

            if(request.LogOutAllTokens || !request.RequestorCredentials.TokenID.HasValue)
            { //Logout all tokens
                var userTokens = _UserID2Tokens[authenticatedUser.Id];

                lock(userTokens)
                {
                    foreach (var oldTokenId in userTokens)
                        _LoginTokensStore.InvalidateToken(oldTokenId);

                    userTokens.Clear();
                }
            }
            else
            {
                var userTokens = _UserID2Tokens[authenticatedUser.Id];
                lock(userTokens)
                {
                    if (!userTokens.Contains(request.RequestorCredentials.TokenID.Value))
                        throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "Nice try! You cannot logout another user except your self. Either you tried this, or you tried to log out from a session which already expired!");

                    _LoginTokensStore.InvalidateToken(request.RequestorCredentials.TokenID.Value);
                    userTokens.Remove(request.RequestorCredentials.TokenID.Value);
                }
            }

            return result;
        }

        public APIReply LogoutUser(LogoutUserRequest request)
        {
            return ProcessRequest<APIReply>(request,
                action: (context, bcontext, user) => LogoutUserAction(context, user, (LogoutUserRequest)bcontext.OriginalRequest),
                transactional: true
                );
        }


        public APIReply CreateNewAccountAction(MenusEntityModelContainer context, BusinessContext businessContext, User authenticatedUser, CreateNewAccountRequest request)
        {
            var UserBO = new UserBO(new UserCredentials() { UserName = request.RequestorCredentials.UserName, Password = request.RequestorCredentials.Password });
            if (request.DisplayName == null)
                throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Cannot create a user with no display name.");
            if (request.DisplayName.Length < 1 || request.DisplayName.Length > APIUtils.MaxNamesLength)
                throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Invalid length for user display name");
            UserBO.DisplayName = request.DisplayName;
            UserBO.ExpectedNumCalories = request.ExpectedNumCalories;
            UserBO.IsNew = true;

            var userRoleDAL = context.UserRoles.Where(role => !role.IsAdmin && !role.CanCRUDOthersEntries && !role.CanCRUDUsers && role.RoleName == APIUtils.RegularRoleName).FirstOrDefault();
            UserRoleBO userRoleBO = null;
            if (userRoleDAL == null)
            {
                userRoleBO = new UserRoleBO(null, APIUtils.RegularRoleName, false, false, false);
                userRoleBO.IsNew = true;
            }
            else
                userRoleBO = userRoleDAL.CreateBusinessObject();
            UserBO.UserRoles.Add(userRoleBO);

            UserBO.UpdateDALWithChanges(context, businessContext);

            return new APIReply();
        }

        public APIReply CreateNewAccount(CreateNewAccountRequest request)
        {
            return ProcessRequest<APIReply>(request,
                action: (context,bcontext, user) => CreateNewAccountAction(context, bcontext, user, (CreateNewAccountRequest)bcontext.OriginalRequest),
                transactional: true);
        }
        
        public CUDOperationsReply CUDRoles(CUDRolesRequest request)
        {
            return ProcessRequest<CUDOperationsReply>(request,
                action: (context, bcontext, user) => PerformCUDOperations(request.RolesToCUD, context, bcontext, (CUDRequest)bcontext.OriginalRequest),
                transactional: true);
        }

        public CUDOperationsReply CUDMenus(CUDMenusRequest request)
        {
            return ProcessRequest<CUDOperationsReply>(request,
                action: (context, bcontext, user) => PerformCUDOperations(request.MenusToCUD, context, bcontext, (CUDRequest)bcontext.OriginalRequest),
                transactional: true);
        }

        public CUDOperationsReply CUDUsers(CUDUsersRequest request)
        {
            return ProcessRequest<CUDOperationsReply>(request,
                action: (context, bcontext, user) => PerformCUDOperations(request.UsersAndDataToCUD, context, bcontext, (CUDRequest)bcontext.OriginalRequest),
                transactional: true);
        }

        public APIReply IsDatabaseUp(IsDatabaseUpRequest request)
        {
            return ProcessRequest<APIReply>(request,
                action: (context, bcontexct, user) => new APIReply(),
                transactional: false);
        }

        protected ReadObjectsReply ReadLogAction(MenusEntityModelContainer context, User authenticatedUser, ReadLogRequest request)
        {
            if (request.NoLastDays < -1 || request.NoLastDays > 1000)
                throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Invalid time period specified for getting log.");

            var earliestMoment = DateTime.Today.AddDays(request.NoLastDays);
            if (request.NoLastDays == -1)
                earliestMoment = DateTime.MinValue;
            
            var result = ReadObjects(request,
                                       context.LogEntries,
                                       logEntry => logEntry.CreateBusinessObject(),
                                       (logEntry, idx) => logEntry.Moment >= earliestMoment,
                                       sortOnDefaultComparer: true);
            return result;
        }

        public ReadObjectsReply ReadLog(ReadLogRequest request)
        {
            return ProcessRequest<ReadObjectsReply>(request,
                action: (context, bcontext, user) => ReadLogAction(context, user, (ReadLogRequest)bcontext.OriginalRequest),
                transactional: false);
        }

        public ReadObjectsReply ReadRoles(ReadRolesRequest request)
        {
            return ProcessRequest<ReadObjectsReply>(request,
                action: (context, bcontext, user) => ReadObjects(bcontext.OriginalRequest, context.UserRoles, userRole => userRole.CreateBusinessObject()),
                transactional: true);
        }

        protected void FillOutCaloricColorCodingOnMenuEntries(MenusEntityModelContainer context, ReadObjectsReply result)
        {

            Dictionary<int, Dictionary<DateTime, int?>> colorCodingDict = new Dictionary<int, Dictionary<DateTime, int?>>();
            List<DateTime> allDates = new List<DateTime>();
            foreach (var menuEntryBO in result.ReadObjects.OfType<MenuEntryBO>())
            {
                if (!colorCodingDict.ContainsKey(menuEntryBO.Owner.UserID.Value))
                    colorCodingDict[menuEntryBO.Owner.UserID.Value] = new Dictionary<DateTime, int?>();

                var moment2ColorCode = colorCodingDict[menuEntryBO.Owner.UserID.Value];
                if (!moment2ColorCode.ContainsKey(menuEntryBO.Moment.Date))
                {
                    moment2ColorCode[menuEntryBO.Moment.Date] = null;
                    allDates.Add(menuEntryBO.Moment.Date);
                }
            }

            allDates = allDates.Distinct().ToList();

            if (colorCodingDict.Count > 0)
            {
                var queryCaloricStatusResults = context.Users
                    .Where(user => colorCodingDict.Keys.Contains(user.Id))
                    .Select(user => new {
                        userID = user.Id,
                        userExpectedNumCalories = user.ExpectedNumCalories,
                        userCaloriesByDay = user.MenuEntries.Where(menuEntry => allDates.Contains(DbFunctions.TruncateTime(menuEntry.Moment).Value)) //Because colorCodingDict[user.Id].Keys.Contains(menuEntry.Moment.Date) doesn't yet work in LINQ.
                                        .GroupBy(menuEntry => DbFunctions.TruncateTime(menuEntry.Moment).Value)
                                        .Select(grp => new {
                                            momentDate = grp.Key,
                                            sumCalories = grp.Sum(muEntry => muEntry.NumCalories)
                                        })
                                        .ToList()
                        })
                        .ToList();

                foreach (var entry in queryCaloricStatusResults)
                    foreach (var grpEntry in entry.userCaloriesByDay)
                    {
                        //colorCodingDict[entry.Item1][grpEntry.Item1] = null;
                        if (entry.userExpectedNumCalories.HasValue)
                            colorCodingDict[entry.userID][grpEntry.momentDate] = entry.userExpectedNumCalories.Value - grpEntry.sumCalories;
                    }

                foreach (var menuEntryBO in result.ReadObjects.OfType<MenuEntryBO>())
                {
                    menuEntryBO.ColorCodingWithRegardToOwnerSettings = colorCodingDict[menuEntryBO.Owner.UserID.Value][menuEntryBO.Moment.Date];
                }
            }
        }

        protected ReadObjectsReply ReadMenusAction(MenusEntityModelContainer context, User authenticatedUser, ReadMenusRequest request)
        {
            Func<MenuEntry, int, bool> timeFilter = null;
            if (request.TimeFilterSpecs != null)
                timeFilter = (MenuEntry entry, int idx) => request.TimeFilterSpecs.BuildFilter()(entry.Moment);

            var result = new ReadObjectsReply();

            if (request.OwnerOfMenuEntries != null)
            {
                if (!request.OwnerOfMenuEntries.UserID.HasValue)
                    throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Cannot get Menus for a user which does not have an ID specified.");

                if (request.OwnerOfMenuEntries.UserID == authenticatedUser.Id)
                    result = ReadObjects(request, authenticatedUser.MenuEntries,
                     translator: Menu => Menu.CreateBusinessObject(),
                     filter: timeFilter,
                     sortOnDefaultComparer: true);
                else
                    result = ReadObjects(request, context.Users.Find(request.OwnerOfMenuEntries.UserID).MenuEntries,
                     translator: Menu => Menu.CreateBusinessObject(),
                     filter: timeFilter,
                     sortOnDefaultComparer: true);
            }
            else
            {
                result = ReadObjects(request, context.MenuEntries,
                        translator: Menu => Menu.CreateBusinessObject(),
                        filter: timeFilter,
                        sortOnDefaultComparer: true);
            }

            if(request.bFillOutCaloricColorCoding)
                FillOutCaloricColorCodingOnMenuEntries(context, result);

            foreach (var menuEntry in result.ReadObjects.OfType<MenuEntryBO>())
                if (menuEntry.Owner != null)
                    menuEntry.Owner.ClearSensitiveDateFromUserBOObject(); //Don't send user roles from owners of menu entries.

            return result;
        }

        public ReadObjectsReply ReadMenus(ReadMenusRequest request)
        {
            return ProcessRequest<ReadObjectsReply>(request,
                action: (context, bcontext, user) => ReadMenusAction(context, user, (ReadMenusRequest)bcontext.OriginalRequest),
                transactional: false);
        }

        public ReadObjectsReply ReadUsers(ReadUsersRequest request)
        {
            return ProcessRequest<ReadObjectsReply>(request,
                action: (context, bcontext, user) => ReadObjects(bcontext.OriginalRequest, context.Users, usr => usr.CreateBusinessObject(translatePassword: false, getSatteliteData: false)),
                transactional: true);
        }

        protected void CreateDefaultObjects(MenusEntityModelContainer context, BusinessContext businessContext)
        {
            UserBO adminUser = new UserBO(null, APILibUtils.DefaultAdminUsername, APILibUtils.DefaultAdminPassword, "Administrator");
            adminUser.IsNew = true;
            UserRoleBO adminRole = null;
            var userRoleDAL = context.UserRoles.Where(role => role.IsAdmin && role.RoleName == APIUtils.DefaultAdminRoleName).FirstOrDefault();
            if (userRoleDAL == null)
            {
                adminRole = new UserRoleBO(null, "admin", true, true, true);
                adminRole.IsNew = true;
            }
            else
                adminRole = userRoleDAL.CreateBusinessObject();

            adminUser.UserRoles.Add(adminRole);
            adminUser.UpdateDALWithChanges(context, businessContext);
        }

        protected APIReply ResetDatabaseAction(MenusEntityModelContainer context, BusinessContext businessContext, User authenticatedUser, ResetDatabaseRequest req)
        {
            context.Database.Delete();
            context.Database.Create();
            CreateDefaultObjects(context, businessContext);

            return new APIReply();
        }

        public APIReply ResetDatabase(ResetDatabaseRequest request)
        {
            return ProcessRequest<APIReply>(request,
                action: (context, bcontext, user) => ResetDatabaseAction(context, bcontext, user, (ResetDatabaseRequest)bcontext.OriginalRequest),
                transactional: true);
        }

        public override bool Start()
        {
            if (APILibUtils.LogAllErrors || APILibUtils.LogAllRequests)
                _Logger = new DbLogger();
            else
                _Logger = new EmptyLogger();

            _LoginTokensStore = new TokenizedCollection<UserCredentials>(TimeSpan.FromMinutes(APILibUtils.LoginTokenExpiryInMinutes));
            _UserID2Tokens = new ConcurrentDictionary<int, HashSet<int>>();

            return base.Start();
        }
        
        protected User LoginUser(UserCredentials requestorCredentials, BusinessContext businessContext, MenusEntityModelContainer context)
        {
            if (!context.Users.Any(user => true)) //If we have no users
                CreateDefaultObjects(context, businessContext);

            if (requestorCredentials == null)
                return null;

            var testCredentialsAreOkish = new UserCredentials(requestorCredentials); //Security reasons
            
            if (requestorCredentials.TokenID.HasValue && APILibUtils.EnableTokenBasedAuthentication)
            {
                var newUserCredentials = _LoginTokensStore[requestorCredentials.TokenID.Value];
                if (newUserCredentials == null)
                {
                    requestorCredentials.TokenID = null;
                }
                else
                    requestorCredentials = newUserCredentials;
            }

            var authenticatedUsers = context.Users.Where(user =>
                    user.Username == requestorCredentials.UserName).Take(2).ToList();

            authenticatedUsers.AddRange(context.Users.Local.Where(user =>
                    user.Username == requestorCredentials.UserName).Take(2).ToList());

            authenticatedUsers = authenticatedUsers.Distinct().ToList();

            if (authenticatedUsers.Count > 1)
                throw new InternalErrorException(System.Net.HttpStatusCode.InternalServerError, "Database Corrupted: Multiple users with same username.");

            if (authenticatedUsers.Count < 1)
                return null;

            if (!requestorCredentials.Password.HasPasswordHash(authenticatedUsers[0].Password))
                throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "Specified password is invalid.");

            var authenticatedUser = authenticatedUsers[0];

            if (requestorCredentials.TokenID == null && APILibUtils.EnableTokenBasedAuthentication)
            {
                var generatedToken = _LoginTokensStore.Add(requestorCredentials);
                requestorCredentials.TokenID = generatedToken.TokenId;

                lock (_UserID2Tokens)
                {
                    if (!_UserID2Tokens.ContainsKey(authenticatedUser.Id))
                        _UserID2Tokens.TryAdd(authenticatedUser.Id, new HashSet<int>());
                }

                var userTokens = _UserID2Tokens[authenticatedUser.Id];

                if(userTokens == null)
                    throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "User has been logged out from all tokens.");

                lock (userTokens)
                {
                    userTokens.Add(generatedToken.TokenId);
                }
            }

            return authenticatedUser;
        }

        protected void AuthorizeUser(User userToAuthorize, APIInboundRequest request, MenusEntityModelContainer context)
        {
            if (userToAuthorize == null)
            {
                if (request.TypeOfThisRequest != UserRequestTypes.CreateNewAccount)
                    throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "No user to authorize: The login credentials provided where either incorrect, or the user may have been deleted.");

                var challengeResponse = ((CreateNewAccountRequest)request).NumericalProof;
                if (!APILibUtils.ConfirmChallenge(challengeResponse))
                    throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "Unauthorized attempt to create a new account. New accounts can only be created using authorized systems.");

                return;
            }

            if (userToAuthorize.UserRoles.Any(role => role.IsAdmin))
                return;

            switch (request.TypeOfThisRequest)
            {
                case UserRequestTypes.AuthenticateUser:
                    return;
                case UserRequestTypes.LogoutUser:
                    return;
                case UserRequestTypes.Base:
                    break;
                case UserRequestTypes.ClearLog:
                    break;
                case UserRequestTypes.CUDRoles:
                    break;
                case UserRequestTypes.CUDMenus:
                    if (userToAuthorize.UserRoles.Any(role => role.CanCRUDOthersEntries))
                        return;

                    var CUDMenusReq = (CUDMenusRequest)request;
                    //Cheating by entering the ID of an entry for whom the user is not the owner will be caught when processing the Menu entry, but still we can do it here too.
                    if (APILibUtils.IsAnyMenuNotOwnedBy(elements: CUDMenusReq.MenusToCUD, ownerId: userToAuthorize.Id, context: context))
                        break;

                    return;
                case UserRequestTypes.CUDUsers:
                    var CUDUsersReq = (CUDUsersRequest)request;

                    if (CUDUsersReq.UsersAndDataToCUD.Any(userBO => userBO.UserRoles.Count > 0))
                        throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "User is not authorized to CUD Roles. Access denided. Do not include Roles within the request.");

                    if (!userToAuthorize.UserRoles.Any(role => role.CanCRUDUsers))
                    {
                        if (CUDUsersReq.UsersAndDataToCUD.Any(userBO => userBO.UserID != userToAuthorize.Id))
                            throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "User is not authorized to CUD other Users except himself. Access denied.");
                    }
                    else
                        if (APILibUtils.IsAnyAdmin(CUDUsersReq.UsersAndDataToCUD, context))
                        throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "A non-Admin user is not authorized to CUD an Admin user; potentially just his Menus. Access denied.");

                    if (!userToAuthorize.UserRoles.Any(role => role.CanCRUDOthersEntries))
                    {
                        if (CUDUsersReq.UsersAndDataToCUD.Where(userBO => userBO.Menus.Count > 0).Any(userBO => userBO.UserID != userToAuthorize.Id))
                            throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "User is not authorized to CUD Menu entries of other users except himself. Access denied.");
                        
                        //Cheating by entering the ID of an entry for whom the user is not the owner will be caught when processing the Menu entry, but still we can do it here too.
                        if (APILibUtils.IsAnyMenuNotOwnedBy(CUDUsersReq.UsersAndDataToCUD.SelectMany(userBO => userBO.Menus).Where(MenuBO => MenuBO.EntryID.HasValue), userToAuthorize.Id, context))
                            throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "User is not authorized to CUD Menu entries of other users except himself. Access denied.");
                    }

                    return;
                case UserRequestTypes.IsDatabaseUp:
                    return;
                case UserRequestTypes.Other:
                    break;
                case UserRequestTypes.ReadLog:
                    break;
                case UserRequestTypes.ReadMenus:
                    var ReadMenusReq = (ReadMenusRequest)request;

                    if (!userToAuthorize.UserRoles.Any(role => role.CanCRUDOthersEntries))
                    {
                        if (ReadMenusReq.OwnerOfMenuEntries == null)
                            throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "User is not authorized to CUD Menu entries of other users except himself. Access denied.");
                        if (ReadMenusReq.OwnerOfMenuEntries.UserID != userToAuthorize.Id)
                            throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "User is not authorized to CUD Menu entries of other users except himself. Access denied.");
                    }
                    return;
                case UserRequestTypes.ReadUsers:
                    if (!userToAuthorize.UserRoles.Any(role => role.CanCRUDUsers))
                        break;
                    return;

                case UserRequestTypes.ResetDatabase:
                    break;
            }

            throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "User is not authorized to perform requested operation. Access denined.");
        }


        #region Generic Operations

        protected virtual TReply ProcessRequest<TReply>(APIInboundRequest request,
            Func<MenusEntityModelContainer, BusinessContext, User, APIReply> action, bool transactional = false)
            where TReply :APIReply, new()
        {

            if (APILibUtils.LogAllRequests)
                Logger.LogMessage(request.ToString(), request.RequestorCredentials);

            var result = new TReply();
            try
            {
                using (var context = new MenusEntityModelContainer())
                {
                    if (transactional)
                        context.Database.BeginTransaction(System.Data.IsolationLevel.Snapshot);

                    var businessContext = new BusinessContext(request);

                    User authenticatedUser = null;
                    if(request.TypeOfThisRequest != UserRequestTypes.CreateNewAccount)
                        authenticatedUser = LoginUser(request.RequestorCredentials, businessContext, context);

                    AuthorizeUser(authenticatedUser, request, context);

                    var queryResult = action(context, businessContext, authenticatedUser);

                    if (queryResult == null)
                        throw new OperationException(System.Net.HttpStatusCode.InternalServerError, "No reply generated for specified request.");

                    if (queryResult.Error != null)
                        throw queryResult.Error;

                    context.SaveChanges();

                    if (transactional)
                        context.Database.CurrentTransaction.Commit();

                    result = (TReply)queryResult;
                }
            }
            catch (Exception Ex)
            {
                if (APILibUtils.LogAllErrors)
                    Logger.LogException(Ex, request.RequestorCredentials);
                
                FaultException<FaultDetails> faultExceptionToReturn = new FaultException<FaultDetails>(new FaultDetails());
                faultExceptionToReturn.Detail.ExceptionType = Ex.GetType().ToString();
                faultExceptionToReturn.Detail.ExceptionMessage = Ex.Message;
                faultExceptionToReturn.Detail.ExceptionToString = Ex.ToString();
                faultExceptionToReturn.Detail.HttpStatusCode = (int)System.Net.HttpStatusCode.InternalServerError;

                if (Ex is WebReadyException)
                    faultExceptionToReturn.Detail.HttpStatusCode = (int)((WebReadyException)Ex).HttpStatusCode;

                result.Error = faultExceptionToReturn;
            }

            return result;
        }

        protected void ValidateGlobalBusinessRules(MenusEntityModelContainer context, BusinessContext businessContext, CUDRequest originalRequest)
        {
            foreach (var businessRule in APILibUtils.GlobalBusinessRules)
                if (!businessRule.IsRespectedBy(context, businessContext))
                    throw new OperationException(System.Net.HttpStatusCode.Conflict, string.Format("Cannot perform operation because it would result in violation of business rule: {0}", businessRule.ToString()));
        }

        protected CUDOperationsReply PerformCUDOperations(IEnumerable<BusinessObject> objectsToCUD, 
            MenusEntityModelContainer context, 
            BusinessContext businessContext,
            CUDRequest originalRequest)
        {
            var result = new CUDOperationsReply();
            try
            {
                result.NumberOfEntriesWhichGeneratedChanges = 0;

                foreach (var objectBO in objectsToCUD)
                {
                    bool bChanges = objectBO.UpdateDALWithChanges(context, businessContext);
                    result.NumberOfEntriesWhichGeneratedChanges += bChanges ? 1 : 0;
                }

                context.SaveChanges();

                ValidateGlobalBusinessRules(context, businessContext, originalRequest);

                result.NewlyCreatedObjectsWithIds = businessContext.GenerateNewlyCreatedObjectsList();
            }
            catch (Exception Ex)
            {
                if (APILibUtils.LogAllErrors)
                    Logger.LogException(Ex, originalRequest.RequestorCredentials);

                throw;
            }

            return result;
        }

        protected ReadObjectsReply ReadObjects<TEntity, TBusinessObject>(APIInboundRequest originalRequest,
            IEnumerable<TEntity> fromSet, 
            Func<TEntity, TBusinessObject> translator,
            Func<TEntity, int, bool> filter = null,
            bool sortOnDefaultComparer = false)
            where TEntity : class
            where TBusinessObject : BusinessObject
        {
            var result = new ReadObjectsReply();
            try
            {
                var readObjectsRequest = originalRequest as ReadObjectsRequest;


                IEnumerable<TEntity> entities = null;
                if (filter != null)
                    entities = fromSet.OfType<TEntity>().Where(filter);
                else
                    entities = fromSet.OfType<TEntity>();


                if (readObjectsRequest?.SpecificObjectId.HasValue == true)
                {
                    entities = entities.Where(entity => ((dynamic)entity).Id == readObjectsRequest.SpecificObjectId.Value);
                }
                
                result.PagingInfo = null;

                if (readObjectsRequest.PageingInfo != null)
                {
                    if (readObjectsRequest.PageingInfo.PageNumber >= 0)
                    {
                        int itemsPerPage = APILibUtils.DefaultItemsPerPage;
                        if (readObjectsRequest.PageingInfo.ItemsPerPage > 0)
                            itemsPerPage = readObjectsRequest.PageingInfo.ItemsPerPage.Value;

                        int countObjs = entities.Count();
                        result.PagingInfo = new PagingInfo() { ItemsPerPage = itemsPerPage, PageNumber = readObjectsRequest.PageingInfo.PageNumber, TotalItems = countObjs };
                        entities = entities.Skip(itemsPerPage * readObjectsRequest.PageingInfo.PageNumber).Take(itemsPerPage);
                    }
                }

                foreach (var entity in entities)
                    result.ReadObjects.Add(translator(entity));
                
                if (result.ReadObjects.Count > APILibUtils.MaxReturnedObjectsCount)
                    result.ReadObjects.RemoveRange(APILibUtils.MaxReturnedObjectsCount, result.ReadObjects.Count - APILibUtils.MaxReturnedObjectsCount);

                if (sortOnDefaultComparer)
                    result.ReadObjects.OrderBy(obj => (IComparable)obj);
            }
            catch (Exception Ex)
            {
                if (APILibUtils.LogAllErrors)
                    Logger.LogException(Ex, originalRequest.RequestorCredentials);

                FaultException<FaultDetails> faultExceptionToReturn = new FaultException<FaultDetails>(new FaultDetails());
                faultExceptionToReturn.Detail.ExceptionType = Ex.GetType().ToString();
                faultExceptionToReturn.Detail.ExceptionMessage = Ex.Message;
                faultExceptionToReturn.Detail.ExceptionToString = Ex.ToString();


                result.Error = faultExceptionToReturn;
            }

            return result;
        }

        #endregion
    }
}
