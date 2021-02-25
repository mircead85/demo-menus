/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

using MenusWebsite.Controllers;

using System.Web.Http;
using System.Web.Http.Results;

using MenusAPI;
using System.Net.Http;
using System.Net;

namespace MenusWebsite.Tests
{
    [TestClass]
    public class UnitTests
    {
        MenusServiceBidingController controllerUnderTest;

        [TestInitialize]
        public void SetUp()
        {
            controllerUnderTest = new MenusServiceBidingController();
        }

        [TestCleanup]
        public void Cleanup()
        {
            
        }
        
        internal static WebRequestReply getReply(HttpResponseMessage fromResponse)
        {
            return ((WebRequestReply)((ObjectContent<WebRequestReply>)fromResponse.Content).Value);
        }

        internal static HttpResponseMessage getResponseMsg(IHttpActionResult fromActionResult)
        {
            return fromActionResult.ExecuteAsync(new System.Threading.CancellationToken()).Result;
        }

        HttpRequestMessage ApplyAuthDataToRequest(string userName, string userPassword, HttpRequestMessage originalRequest = null)
        {
            if (originalRequest == null)
                originalRequest = new HttpRequestMessage();

            originalRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.GetEncoding("iso-8859-1").GetBytes(userName + ":" + userPassword)));

            return originalRequest;
        }

        [TestMethod]
        public void LoginDefaultAdmin()
        {
            controllerUnderTest.Request = ApplyAuthDataToRequest("admin", "adminpass");
            var response = getResponseMsg(controllerUnderTest.AuthenticateUser());
            Assert.IsFalse(getReply(response).HasError);
            var userBO = (UserBO)((ReadObjectsReply)getReply(response).Result).ReadObjects[0];
            Assert.AreEqual("admin",userBO.UserCredentials.UserName);
            var bIsAdmin = userBO.UserRoles.Any(role => role.IsAdmin);
            Assert.IsTrue(bIsAdmin);
        }


        [TestMethod]
        public void GetUsersTest()
        {
            controllerUnderTest.Request = ApplyAuthDataToRequest("admin", "adminpass");
            var response = getResponseMsg(controllerUnderTest.GetUsers(-1));
            Assert.IsFalse(getReply(response).HasError);
        }


        [TestMethod]
        public void GetRolesTest()
        {
            controllerUnderTest.Request = ApplyAuthDataToRequest("admin", "adminpass");
            var response = getResponseMsg(controllerUnderTest.GetRoles(-1));
            Assert.IsFalse(getReply(response).HasError);
        }


        [TestMethod]
        public void GetMenusTest()
        {
            controllerUnderTest.Request = ApplyAuthDataToRequest("admin", "adminpass");
            var response = getResponseMsg(controllerUnderTest.GetMenus(pageNum: -1, ownerID: -1, bFilterMenus: false));
            Assert.IsFalse(getReply(response).HasError);
        }

        [TestMethod]
        public void CreateUserLogInEditAndThenDeleteTest()
        {
            var usrName = "__test";
            var usrPass = "__testPass";

            var response = getResponseMsg(controllerUnderTest.CreateUser(usrName, usrPass, usrName+" Display", 250, 11431));
            Assert.IsFalse(getReply(response).HasError);

            controllerUnderTest.Request = ApplyAuthDataToRequest(usrName, usrPass);
            response = getResponseMsg(controllerUnderTest.AuthenticateUser());
            Assert.IsFalse(getReply(response).HasError);

            var userBO = (UserBO)(getReply(response).Result as ReadObjectsReply).ReadObjects[0];
            
            response = getResponseMsg(controllerUnderTest.EditUser(userBO.UserID.Value, usrName, null, usrName+"Display2", 250, null));
            Assert.IsFalse(getReply(response).HasError);
            Assert.AreEqual(1,((CUDOperationsReply)getReply(response).Result).NumberOfEntriesWhichGeneratedChanges);

            response = getResponseMsg(controllerUnderTest.DeleteUser(userBO.UserID.Value));
            Assert.IsFalse(getReply(response).HasError);
        }

        [TestMethod]
        public void FailLoginReasonInvalidPassword()
        {
            controllerUnderTest.Request = ApplyAuthDataToRequest("admin", "thisisnotthevalidpassowrd!");
            var response = getResponseMsg(controllerUnderTest.AuthenticateUser());
            
            Assert.IsTrue(getReply(response).HasError);
            Assert.IsTrue((HttpStatusCode)getReply(response).HttpStatusCode == HttpStatusCode.Forbidden);
       }

        [TestMethod]
        public void CreateRetreiveAndThenDeleteMenuAsDefaultAdminThenLogout()
        {

            controllerUnderTest.Request = ApplyAuthDataToRequest("admin", "adminpass");
            var response = getResponseMsg(controllerUnderTest.AuthenticateUser());
            Assert.IsFalse(getReply(response).HasError);
            var authReply = (AuthenticateUserReply)getReply(response).Result;
            var userBO = (UserBO)authReply.ReadObjects[0];

            Assert.IsNotNull(authReply.SecurityToken);
            
            controllerUnderTest.Request = ApplyAuthDataToRequest(":"+authReply.SecurityToken.TokenId.ToString(), ":ignored");
            response = getResponseMsg(controllerUnderTest.EditMenu(-1,userBO.UserID.Value, "NewAdminTestMenu", 2017, 12, 1, 15, 33, 400));
            Assert.IsFalse(getReply(response).HasError);
            var muEntryBO = (MenuEntryBO)((CUDOperationsReply)getReply(response).Result).NewlyCreatedObjectsWithIds[0];
            Assert.AreEqual("NewAdminTestMenu", muEntryBO.Text);
            Assert.AreEqual(userBO.UserID, muEntryBO.Owner.UserID);
            
            response = getResponseMsg(controllerUnderTest.GetMenu(muEntryBO.EntryID.Value));
            Assert.IsFalse(getReply(response).HasError);
            var muEntryBO2 = (MenuEntryBO)((ReadObjectsReply)getReply(response).Result).ReadObjects[0];
            Assert.AreEqual(400, muEntryBO2.NumCalories);
            Assert.AreEqual(userBO.DisplayName, muEntryBO2.Owner.DisplayName);

            response = getResponseMsg(controllerUnderTest.GetMenus(-1, userBO.UserID.Value, true, 2017,1,1,2018,1,1,15,0,23,59));
            Assert.IsFalse(getReply(response).HasError);
            var muEntryBO3 = ((ReadObjectsReply)getReply(response).Result).ReadObjects.Where(entry => ((MenuEntryBO)entry).EntryID.Value == muEntryBO.EntryID.Value).FirstOrDefault() as MenuEntryBO;
            Assert.IsFalse(muEntryBO3 == null);
            Assert.AreEqual(400, muEntryBO3.NumCalories);
            Assert.AreEqual(userBO.DisplayName, muEntryBO3.Owner.DisplayName);
            
            response = getResponseMsg(controllerUnderTest.DeleteMenu(muEntryBO.EntryID.Value));
            Assert.IsFalse(getReply(response).HasError);
            
            response = getResponseMsg(controllerUnderTest.GetMenus(-1, userBO.UserID.Value, true, 2017, 1, 1, 2018, 1, 1, 15, 0, 23, 59));
            Assert.IsFalse(getReply(response).HasError);
            var muEntryBO4 = ((ReadObjectsReply)getReply(response).Result).ReadObjects.Where(entry => ((MenuEntryBO)entry).EntryID.Value == muEntryBO.EntryID.Value).FirstOrDefault() as MenuEntryBO;
            Assert.IsTrue(muEntryBO4 == null);

            response = getResponseMsg(controllerUnderTest.LogoutUser());
            Assert.IsFalse(getReply(response).HasError);
        }
    }
}
