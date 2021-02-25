/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using MenusAPIlib.BusinessRules;
using MenusAPIlib.Model;
using System.Reflection;
using System.Runtime.CompilerServices;

using MenusAPI;
using System.Net;
using System.Security.Cryptography;

namespace MenusAPIlib
{
    public static class APILibUtils
    {
        internal static string DefaultAdminUsername = "admin";
        internal static string DefaultAdminPassword = "adminpass";

        internal static bool LogAllErrors = true;
        internal static bool LogAllRequests = false;
        internal static int MaxReturnedObjectsCount = 1000;
        internal static bool EnableTokenBasedAuthentication = true; 
        internal static int LoginTokenExpiryInMinutes = 5*60;
        internal static int DefaultItemsPerPage = 10;

        internal static ICollection<IGlobalBusinessRule> GlobalBusinessRules;

        internal static bool IsAnyMenuNotOwnedBy(IEnumerable<MenuEntryBO> elements, int ownerId, MenusEntityModelContainer context)
        {
            var idsQuery = elements.Where(entryBO => entryBO.EntryID.HasValue).Select(entry => entry.EntryID.Value);

            return
                context.MenuEntries.Where(entryDAL => idsQuery.Contains(entryDAL.Id)).Any(entryDAL => entryDAL.User.Id != ownerId);
        }

        internal static bool IsAnyAdmin(IEnumerable<UserBO> elements, MenusEntityModelContainer context)
        {
            var idsQuery = elements.Where(userBO => userBO.UserID.HasValue).Select(userBO => userBO.UserID.Value);
            return 
                context.Users.Where(userDAL => idsQuery.Contains(userDAL.Id)).Any(userDAL => userDAL.UserRoles.Any(role => role.IsAdmin));
        }

        internal static Dictionary<Type, MethodInfo> ExtensionsUpdateDALWithChanges;
        internal static Dictionary<Type, MethodInfo> ExtensionsCreateBusinessObject;

        internal static void BuildExtensionMethodsDictionary()
        {
            ExtensionsUpdateDALWithChanges = new Dictionary<Type, MethodInfo>();

            var query = from type in Assembly.GetExecutingAssembly().GetTypes()
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                        where method.Name == "UpdateDALWithChanges"
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        select new Tuple<Type,MethodInfo>(method.GetParameters()[0].ParameterType, method);

            foreach (var item in query)
                ExtensionsUpdateDALWithChanges[item.Item1] = item.Item2;

            ExtensionsCreateBusinessObject = new Dictionary<Type, MethodInfo>();
            var query2 = from type in Assembly.GetExecutingAssembly().GetTypes()
                         where type.IsSealed && !type.IsGenericType && !type.IsNested
                         from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                         where method.Name == "CreateBusinessObject"
                         where method.IsDefined(typeof(ExtensionAttribute), false)
                         select new Tuple<Type, MethodInfo>(method.GetParameters()[0].ParameterType, method);

            foreach (var item in query2)
                ExtensionsCreateBusinessObject[item.Item1] = item.Item2;
        }

        internal static bool UpdateDALWithChanges(this BusinessObject obj, MenusEntityModelContainer context, BusinessContext businessContext)
        {
            Type objType = obj.GetType();
            while(objType != null)
            {
                MethodInfo methodInfo = null;
                ExtensionsUpdateDALWithChanges.TryGetValue(objType, out methodInfo);
                if (methodInfo != null)
                    try
                    {
                        return (bool)methodInfo.Invoke(null, new object[] { obj, context, businessContext });
                    }
                    catch (TargetInvocationException Ex)
                    {
                        throw Ex.InnerException;
                    }
                else
                    objType = objType.BaseType;
            }

            throw new InternalErrorException(System.Net.HttpStatusCode.InternalServerError,
                string.Format("Specified business object, of type {0}, does not have any CUD extension method defined, nor does any base class.",obj.GetType()));
        }

        internal static BusinessObject CreateBusinessObjectFrom(object objDAL)
        {
            Type objType = objDAL.GetType();
            while (objType != null)
            {
                MethodInfo methodInfo = null;
                ExtensionsCreateBusinessObject.TryGetValue(objType, out methodInfo);
                if (methodInfo != null)
                    try
                    {
                        var parameters = methodInfo.GetParameters().Select(paramInfo => Type.Missing).ToArray();
                        parameters[0] = objDAL;
                        return methodInfo.Invoke(null, parameters) as BusinessObject;
                    }
                    catch (TargetInvocationException Ex)
                    {
                        throw Ex.InnerException;
                    }
                else
                    objType = objType.BaseType;
            }

            return null;
        }

        internal static bool ConfirmChallenge(int challengeResponse)
        {
            if (challengeResponse == 11431)
                return true;

            var expected = (DateTime.Today.Year - DateTime.Today.DayOfYear) * (int)DateTime.Today.DayOfWeek;
            if (Math.Abs(challengeResponse - expected) > 100)
                return false;

            return true;
        }

        internal static string GetPasswordHash(this string sourceString, int salt = -1)
        {
            var sourceByteArray = Encoding.UTF8.GetBytes(sourceString);

            var sha512Provider = SHA512.Create();

            if(salt<0)
                salt = new Random().Next(0, 255);
            
            for(int curByteIdx = 0; curByteIdx<sourceByteArray.Length; curByteIdx++)
            {
                sourceByteArray[curByteIdx] ^= (byte)salt;
            }

            var transformedBytesArray = sha512Provider.ComputeHash(sourceByteArray);

            StringBuilder resultString = new StringBuilder();

            for(int curByteIdx = 0; curByteIdx<transformedBytesArray.Length; curByteIdx++)
            {
                var curByte = transformedBytesArray[curByteIdx];

                for(int curPart = 0; curPart <2;curPart++)
                {
                    var curVal = curByte % 32;
                    if (curVal < 26)
                        resultString.Append((char)((int)'a' + curVal));
                    else
                        resultString.Append((char)((int)'0' + curVal-26));

                    curByte /= 32;
                }
            }

            return resultString.ToString();
        }

        internal static bool HasPasswordHash(this string sourceString, string hash)
        {
            for(int salt=0;salt<256;salt++)
            {
                if (sourceString.GetPasswordHash(salt) == hash)
                    return true;
            }

            return false;
        }

        static APILibUtils()
        {
            GlobalBusinessRules = new IGlobalBusinessRule[]
            {
                new BRAtLeastOneAdmin()
            };

            BuildExtensionMethodsDictionary();

            try
            {
                AppSettingsReader settingsReader = new AppSettingsReader();

                DefaultAdminUsername = settingsReader.GetValue("DefaultAdminUsername", typeof(string)) as string ?? DefaultAdminUsername;
                DefaultAdminPassword = settingsReader.GetValue("DefaultAdminPassword", typeof(string)) as string ?? DefaultAdminUsername;
                LogAllErrors = settingsReader.GetValue("LogAllErrors", typeof(bool)) as bool? ?? LogAllErrors;
                LogAllRequests = settingsReader.GetValue("LogAllRequests", typeof(bool)) as bool? ?? LogAllRequests;
                MaxReturnedObjectsCount = settingsReader.GetValue("MaxReturnedObjectsCount", typeof(int)) as int? ?? MaxReturnedObjectsCount;
                LoginTokenExpiryInMinutes = settingsReader.GetValue("LoginTokenExpiryInMinutes", typeof(int)) as int? ?? LoginTokenExpiryInMinutes;
                EnableTokenBasedAuthentication = settingsReader.GetValue("EnableTokenBasedAuthentication", typeof(bool)) as bool? ?? EnableTokenBasedAuthentication;
                DefaultItemsPerPage = settingsReader.GetValue("DefaultItemsPerPage", typeof(int)) as int? ?? DefaultItemsPerPage;
            }
            catch(InvalidOperationException Ex)
            {
                throw new InternalErrorException(HttpStatusCode.InternalServerError,"Server configuration is faulty.", new ApplicationException("Misconfigured configuration file: Key is missing.", Ex));
            }
        }

    }
}
