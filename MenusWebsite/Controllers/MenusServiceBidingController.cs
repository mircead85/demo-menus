/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using MenusAPI;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace MenusWebsite.Controllers
{
    [RoutePrefix("api/MenusServiceBiding")]
    public class MenusServiceBidingController : ApiController
    {
        ServiceReference.IMenusService backendServiceClient;

        public MenusServiceBidingController()
        {
            backendServiceClient = new ServiceReference.MenusServiceClient();
        }

        internal string GetUrlOfNewlyCreatedObject(BusinessObject newObjectBO)
        {
            var newUrl = "api/MenusServiceBiding/";
            if (newObjectBO is UserBO)
                newUrl += "Users/" + ((UserBO)newObjectBO).UserID;
            else if (newObjectBO is UserRoleBO)
                newUrl += "Roles/" + ((UserRoleBO)newObjectBO).RoleID;
            else if (newObjectBO is MenuEntryBO)
                newUrl += "Menus/" + ((MenuEntryBO)newObjectBO).EntryID;
            else
                newUrl += "UnaccesibleObjects/noid/";
           
            return newUrl;
        }

        internal IHttpActionResult BuildResult(WebRequestReply resultContent)
        {
            IContentNegotiator negotiator = this.Configuration?.Services?.GetContentNegotiator();

            ContentNegotiationResult contentNegociationResult = null;
            if (negotiator != null)
                contentNegociationResult = negotiator.Negotiate(typeof(WebRequestReply), this.Request, this.Configuration.Formatters);
            else
                contentNegociationResult = new ContentNegotiationResult(new JsonMediaTypeFormatter(), JsonMediaTypeFormatter.DefaultMediaType);

            if (contentNegociationResult == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable));
            }

            var response = this.Configuration != null? Request.CreateResponse((HttpStatusCode)resultContent.HttpStatusCode) : new HttpResponseMessage((HttpStatusCode)resultContent.HttpStatusCode);
            response.StatusCode = (HttpStatusCode)resultContent.HttpStatusCode;
            response.Content = new ObjectContent<WebRequestReply>(resultContent, contentNegociationResult.Formatter, contentNegociationResult.MediaType);

            var cudReply = resultContent.Result as CUDOperationsReply;

            if (cudReply?.NewlyCreatedObjectsWithIds != null)
            {
                foreach (var newObjectBO in cudReply.NewlyCreatedObjectsWithIds)
                {
                    response.Headers.Add("Location", GetUrlOfNewlyCreatedObject(newObjectBO));
                }
            }

            return new CustomTrivialIHttpActionResult(response);
        }

        internal UserCredentials BuildUserCredentials()
        {
            var authHeader = this.Request.Headers.Authorization;

            if (authHeader == null)
                return null;

            if (!authHeader.Scheme.StartsWith("Basic"))
                return null;

            var authString = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(Convert.FromBase64String(authHeader.Parameter));

            int seperatorIndex = authString.IndexOf(':');

            string userName = null;
            string userPassword = null;

            if (seperatorIndex == 0)
            {
                seperatorIndex = authString.Substring(1).IndexOf(':');
                if (seperatorIndex < 0)
                {
                    userName = ":"+authString.Substring(1);
                    userPassword = ":ignored";
                }
                else
                    seperatorIndex++;
            }

            if (seperatorIndex >= 0)
            {
                userName = authString.Substring(0, seperatorIndex);
                userPassword = authString.Substring(seperatorIndex + 1);
            }

            if (userName == null || userName == "null")
                return null;

            if (userName.Length > APIUtils.MaxUsernameLength)
                return null;

            if (userPassword != null)
                if (userPassword.Length > APIUtils.MaxPasswordLength)
                    return null;
            
            var result = new UserCredentials(userName, userPassword);
            return result;
        }

        internal IHttpActionResult BuildResult(APIReply fromReply)
        {
            var statusCode = fromReply.Error == null ? HttpStatusCode.OK : (HttpStatusCode)fromReply.Error.Detail.HttpStatusCode;
            var replyObj = new WebRequestReply(statusCode, fromReply.Error != null, null, fromReply);
            if
                (fromReply.Error == null)
                replyObj.Message = "OK: Operation Succeded";
            else
                replyObj.Message = "NOK: " + fromReply.Error.Detail.ExceptionToString.Substring(0, 250) + "(...)";

            return BuildResult(replyObj);
        }

        [AcceptVerbs("GET")]
        [Route("Authenticate/")]
        public IHttpActionResult AuthenticateUser()
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));
            }
            var req = new AuthenticateUserRequest(userCredentials, false);
            var response = backendServiceClient.AuthenticateUser(req);

            var reply = BuildResult(response);
            return reply;
        }

        [AcceptVerbs("GET")]
        [Route("Logout/")]
        public IHttpActionResult LogoutUser()
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));
            }
            var req = new LogoutUserRequest(userCredentials, false);
            var response = backendServiceClient.LogoutUser(req);

            var reply = BuildResult(response);
            return reply;
        }

        [AcceptVerbs("POST")]
        [Route("Users/new/{userName}/{userPassword}/{displayName}/{expectedNumCalories}/{challengeResponse}")]
        public IHttpActionResult CreateUser(string userName, string userPassword, string displayName, int? expectedNumCalories = null, int challengeResponse = -1)
        {
            if(userName.Contains(':'))
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Username must not contain some special characters.", null));

            var userCredentials = new UserCredentials(userName, userPassword);
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request.", null));
            }

            var req = new CreateNewAccountRequest() { RequestorCredentials = userCredentials, DisplayName = displayName, NumericalProof = challengeResponse };
            if (expectedNumCalories.HasValue)
                req.ExpectedNumCalories = expectedNumCalories.Value;

            var response = backendServiceClient.CreateNewAccount(req);

            var reply = BuildResult(response);
            return reply;
        }

        [AcceptVerbs("GET")]
        [Route("Users/all/{pageNum}")]
        public IHttpActionResult GetUsers(int pageNum)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));

            }
            var req = new ReadUsersRequest() { RequestorCredentials = userCredentials };
            if (pageNum >= 0)
                req.PageingInfo = new PagingInfo() { PageNumber = pageNum, ItemsPerPage = null };
            var response = backendServiceClient.ReadUsers(req);

            var reply = BuildResult(response);
            return reply;
        }


        [AcceptVerbs("GET")]
        [Route("Users/{userID}/")]
        public IHttpActionResult GetUser(int userID = -1)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));

            }

            if (userID < 0)
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed ID of object.", null));

            var req = new ReadUsersRequest() { RequestorCredentials = userCredentials };
            req.SpecificObjectId = userID;

            var response = backendServiceClient.ReadUsers(req);

            var reply = BuildResult(response);
            return reply;
        }

        [AcceptVerbs("GET")]
        [Route("Roles/all/{pageNum}")]
        public IHttpActionResult GetRoles(int pageNum)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));

            }

            var req = new ReadRolesRequest() { RequestorCredentials = userCredentials };
            if (pageNum >= 0)
                req.PageingInfo = new PagingInfo() { PageNumber = pageNum, ItemsPerPage = null };
            var response = backendServiceClient.ReadRoles(req);

            var reply = BuildResult(response);
            return reply;
        }


        [AcceptVerbs("GET")]
        [Route("Roles/{roleID}/")]
        public IHttpActionResult GetRole(int roleID = -1)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));

            }

            if (roleID < 0)
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed ID of object.", null));

            var req = new ReadRolesRequest() { RequestorCredentials = userCredentials };
            req.SpecificObjectId = roleID;

            var response = backendServiceClient.ReadRoles(req);

            var reply = BuildResult(response);
            return reply;
        }

        [AcceptVerbs("GET")]
        [Route("Menus/all/{pageNum}/{ownerID}/{bFilterMenus}/{dateFromYear}/{dateFromMonth}/{dateFromDay}/{dateToYear}/{dateToMonth}/{dateToDay}/{hourFrom}/{minuteFrom}/{hourTo}/{minuteTo}/")]
        public IHttpActionResult GetMenus(int pageNum, int ownerID = -1, bool bFilterMenus = false, int? dateFromYear = null, int? dateFromMonth = null, int? dateFromDay = null, int? dateToYear = null, int? dateToMonth = null, int? dateToDay = null, int? hourFrom = null, int? minuteFrom = null, int? hourTo = null, int? minuteTo = null)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));

            }

            var req = new ReadMenusRequest() { RequestorCredentials = userCredentials };
            req.bFillOutCaloricColorCoding = true;
            req.OwnerOfMenuEntries = null;
            if (ownerID != -1)
                req.OwnerOfMenuEntries = new UserBO(ownerID);
            if (pageNum >= 0)
                req.PageingInfo = new PagingInfo() { PageNumber = pageNum, ItemsPerPage = null };
            if (bFilterMenus)
            {
                if (!dateFromYear.HasValue || !dateFromMonth.HasValue || !dateFromDay.HasValue
                || !dateToYear.HasValue || !dateToMonth.HasValue || !dateToDay.HasValue
                || !hourFrom.HasValue || !minuteFrom.HasValue
                || !hourTo.HasValue || !minuteTo.HasValue)
                    return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed filter specifications in request.", null));

                if (dateFromYear < 0 || dateFromMonth < 0 || dateFromMonth > 12 || dateFromDay < 0 || dateFromDay > 31
                    || dateToYear < 0 || dateToMonth < 0 || dateToMonth > 12 || dateToDay < 0 || dateToDay > 31
                    || hourFrom < 0 || hourFrom > 23 || minuteFrom < 0 || minuteFrom > 59
                    || hourTo < 0 || hourTo > 23 || minuteTo < 0 || minuteTo > 59)
                    return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed filter specifications in request.", null));
                
                req.TimeFilterSpecs = new TimeFilterSpecs();
                try
                {
                    req.TimeFilterSpecs.DateFrom = new DateTime(dateFromYear.Value, dateFromMonth.Value, dateFromDay.Value);
                    req.TimeFilterSpecs.DateTo = new DateTime(dateToYear.Value, dateToMonth.Value, dateToDay.Value);
                    req.TimeFilterSpecs.MinuteInDayFrom = hourFrom.Value * 60 + minuteFrom.Value;
                    req.TimeFilterSpecs.MinuteInDayTo = hourTo.Value * 60 + minuteTo.Value;
                }
                catch (ArgumentOutOfRangeException Ex)
                {
                    return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed filter specifications in request.", null));
                }
            }

            var response = backendServiceClient.ReadMenus(req);

            var reply = BuildResult(response);
            return reply;
        }

        [AcceptVerbs("GET")]
        [Route("Menus/{menuID}/")]
        public IHttpActionResult GetMenu(int menuID = -1)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));

            }
            
            if (menuID < 0)
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed ID of object.", null));

            var req = new ReadMenusRequest() { RequestorCredentials = userCredentials };
            req.SpecificObjectId = menuID;
            req.bFillOutCaloricColorCoding = true;

            var response = backendServiceClient.ReadMenus(req);

            var reply = BuildResult(response);
            return reply;
        }

        [AcceptVerbs("PUT")]
        [Route("Users/{userToEditID}/{reqUserName}/{reqPassword}/{reqDisplayName}/{reqCalories}/{reqEditRoles}")]
        public IHttpActionResult EditUser(int userToEditID, string reqUsername = null, string reqPassword = null, string reqDisplayName = null, int reqCalories = -1, string reqEditRoles = null)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));

            }

            if (reqUsername == "null")
                reqUsername = null;
            if (reqPassword == "null")
                reqPassword = null;
            if (reqDisplayName == "null")
                reqDisplayName = null;

            if (reqEditRoles == "null")
                reqEditRoles = null;

            if (reqUsername == null || reqDisplayName == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed request data.", null));
            }

            var req = new CUDUsersRequest() { RequestorCredentials = userCredentials };
            var userToCUD = new UserBO(userToEditID, reqUsername, reqPassword, reqDisplayName);
            userToCUD.ExpectedNumCalories = reqCalories;
            if (reqCalories < 0)
                userToCUD.ExpectedNumCalories = null;

            userToCUD.IsNew = userToEditID == -1;

            if (reqEditRoles != null)
            {
                try
                {
                    foreach (var reqRole in reqEditRoles.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        int roleID = int.Parse(reqRole.Split('_')[0]);
                        bool isDeleted = reqRole.EndsWith("false");

                        UserRoleBO roleBO = new UserRoleBO(roleID, null);
                        if (isDeleted)
                            roleBO.IsDeleted = true;

                        userToCUD.UserRoles.Add(roleBO);
                    }
                }
                catch (Exception Ex)
                {
                    return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed request: " + Ex.Message + " .", null));
                }
            }

            req.ObjectsToCUD.Add(userToCUD);
            req.JustUpdateAssociationForReferencedRoles = true;

            var response = backendServiceClient.CUDUsers(req);

            var reply = BuildResult(response);
            return reply;
        }


        [AcceptVerbs("DELETE")]
        [Route("Users/{userToDeleteID}")]
        public IHttpActionResult DeleteUser(int userToDeleteID)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));
            }

            if (userToDeleteID < 0)
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed ID of object.", null));

            var req = new CUDUsersRequest() { RequestorCredentials = userCredentials };
            var userToCUD = new UserBO(userToDeleteID, null, null, null);
            userToCUD.IsNew = false;
            userToCUD.IsDeleted = true;


            req.ObjectsToCUD.Add(userToCUD);

            var response = backendServiceClient.CUDUsers(req);

            var reply = BuildResult(response);
            return reply;
        }


        [AcceptVerbs("PUT")]
        [Route("Roles/{roleToEditID}/{reqRoleName}/{reqRoleIsAdmin}/{reqRoleCanCUDUsers}/{reqRoleCanCUDEntries}")]
        public IHttpActionResult EditRole(int roleToEditID, string reqRoleName = null, bool reqRoleIsAdmin = false, bool reqRoleCanCUDUsers = false, bool reqRoleCanCUDEntries = false)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));
            }

            if (reqRoleName == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed request data.", null));
            }
            var req = new CUDRolesRequest() { RequestorCredentials = userCredentials };
            var roleToCUD = new UserRoleBO(roleToEditID, reqRoleName, reqRoleCanCUDUsers, reqRoleCanCUDEntries, reqRoleIsAdmin);
            roleToCUD.IsNew = roleToEditID == -1;
            req.ObjectsToCUD.Add(roleToCUD);

            var response = backendServiceClient.CUDRoles(req);

            var reply = BuildResult(response);
            return reply;
        }

        [AcceptVerbs("DELETE")]
        [Route("Roles/{roleToDeleteID}")]
        public IHttpActionResult DeleteRole(int roleToDeleteID)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));
            }

            if (roleToDeleteID < 0)
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed ID of object.", null));

            var req = new CUDRolesRequest() { RequestorCredentials = userCredentials };
            var userRoleToCUD = new UserRoleBO(roleToDeleteID, null);
            userRoleToCUD.IsNew = false;
            userRoleToCUD.IsDeleted = true;

            req.ObjectsToCUD.Add(userRoleToCUD);

            var response = backendServiceClient.CUDRoles(req);

            var reply = BuildResult(response);
            return reply;
        }

        [AcceptVerbs("PUT")]
        [Route("Menus/{menuToEditID}/{ownerID}/{reqText}/{reqMomentYear}/{reqMomentMonth}/{reqMomentDay}/{reqMomentHour}/{reqMomentMinute}/{reqNumCalories}")]
        public IHttpActionResult EditMenu(int menuToEditID, int ownerID, string reqText, int reqMomentYear, int reqMomentMonth, int reqMomentDay, int reqMomentHour, int reqMomentMinute, int reqNumCalories)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));
            }

            if (ownerID < 0)
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed ID of object.", null));


            if (reqText == null || reqText == "null")
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed request data.", null));
            }

            var req = new CUDMenusRequest() { RequestorCredentials = userCredentials };
            var menuToCUD = new MenuEntryBO(menuToEditID);
            menuToCUD.Text = reqText;

            try
            {
                menuToCUD.Moment = new DateTime(reqMomentYear, reqMomentMonth, reqMomentDay, reqMomentHour, reqMomentMinute, 0);
                if (reqNumCalories < 0)
                    throw new ArgumentOutOfRangeException();
                menuToCUD.NumCalories = reqNumCalories;
            }
            catch (ArgumentOutOfRangeException Ex)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Invalid data in request.", null));
            }
            menuToCUD.IsNew = menuToEditID == -1;
            if (ownerID != -1)
                menuToCUD.Owner = new UserBO(ownerID);

            req.ObjectsToCUD.Add(menuToCUD);

            var response = backendServiceClient.CUDMenus(req);

            var reply = BuildResult(response);
            return reply;
        }

        [AcceptVerbs("DELETE")]
        [Route("Menus/{menuToDeleteID}")]
        public IHttpActionResult DeleteMenu(int menuToDeleteID)
        {
            var userCredentials = BuildUserCredentials();
            if (userCredentials == null)
            {
                return BuildResult(new WebRequestReply(HttpStatusCode.Unauthorized, true, "NOK: Malformed or missing authentication data in request, or invalid authentication scheme.", null));
            }

            if (menuToDeleteID < 0)
                return BuildResult(new WebRequestReply(HttpStatusCode.BadRequest, true, "NOK: Malformed ID of object.", null));

            var req = new CUDMenusRequest() { RequestorCredentials = userCredentials };
            var menuToCUD = new MenuEntryBO(menuToDeleteID);
            menuToCUD.IsNew = false;
            menuToCUD.IsDeleted = true;

            req.ObjectsToCUD.Add(menuToCUD);

            var response = backendServiceClient.CUDMenus(req);

            var reply = BuildResult(response);
            return reply;
        }

    }
}
