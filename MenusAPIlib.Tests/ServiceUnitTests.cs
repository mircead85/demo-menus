/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MenusAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MenusAPIlib.Tests.DesignTimeServiceReference;
using System.ServiceModel;
using System.Net;

namespace MenusAPIlib.Tests
{
    [TestClass]
    public class ServiceUnitTests
    {
        MenusAPIlib.Tests.DesignTimeServiceReference.IMenusService serviceUnderTest;

        [TestInitialize]
        public void SetUp()
        {
            serviceUnderTest = new MenusServiceClient();
        }

        bool bLeaveDemoData = false;

        [TestCleanup]
        public void Cleanup()
        {
            if (!bLeaveDemoData)
            {
                var adminCredentials = new UserCredentials("admin", "adminpass");
                DeleteBasicUsersDataIfExisting(adminCredentials);
                DeleteSomeRolesIfTheyExist(adminCredentials);
            }

            serviceUnderTest = null;
        }

        [TestMethod]
        public void LoginDefaultAdmin()
        {
            var userCredentials = new UserCredentials("admin", "adminpass");

            var response = serviceUnderTest.AuthenticateUser(new AuthenticateUserRequest(userCredentials, false));
            if (response.Error != null)
                throw response.Error;
            var userBO = (UserBO)response.ReadObjects[0];

            Assert.AreEqual("admin", userBO.UserCredentials.UserName);
            Assert.AreEqual(null, userBO.UserCredentials.Password);
            Assert.AreEqual(true, userBO.UserRoles.Any(role => role.IsAdmin));
        }


        [TestMethod]
        public void LoginLogoutDefaultAdminWithSecurityToken()
        {
            var userCredentials = new UserCredentials("admin", "adminpass");

            var response = serviceUnderTest.AuthenticateUser(new AuthenticateUserRequest(userCredentials, false));
            if (response.Error != null)
                throw response.Error;
            var userBO = (UserBO)response.ReadObjects[0];

            Assert.AreEqual("admin", userBO.UserCredentials.UserName);
            Assert.AreEqual(null, userBO.UserCredentials.Password);
            Assert.AreEqual(true, userBO.UserRoles.Any(role => role.IsAdmin));

            userCredentials.TokenID = response.SecurityToken.TokenId;
            userCredentials.UserName = ":ignored";
            userCredentials.Password = ":ignored";

            response = serviceUnderTest.AuthenticateUser(new AuthenticateUserRequest(userCredentials, false));
            if (response.Error != null)
                throw response.Error;
            userBO = (UserBO)response.ReadObjects[0];

            Assert.AreEqual("admin", userBO.UserCredentials.UserName);
            Assert.AreEqual(null, userBO.UserCredentials.Password);
            Assert.AreEqual(true, userBO.UserRoles.Any(role => role.IsAdmin));
            Assert.AreEqual(userCredentials.TokenID, response.SecurityToken.TokenId);

            var response2 = serviceUnderTest.LogoutUser(new LogoutUserRequest(userCredentials, true));
            Assert.IsNull(response2.Error);

            var response3 = serviceUnderTest.AuthenticateUser(new AuthenticateUserRequest(userCredentials, false));
            Assert.IsNotNull(response3.Error);

            Assert.IsTrue(response3.Error.Detail.HttpStatusCode == (int)HttpStatusCode.Forbidden);
        }

        [TestMethod]
        public void FailLoginAsDefaultAdminWithWrongPassword()
        {
            var userCredentials = new UserCredentials("admin", "thisisnottherightpassword");

            var response = serviceUnderTest.AuthenticateUser(new AuthenticateUserRequest(userCredentials, false));
            Assert.IsTrue(response.Error != null);

            Assert.IsTrue(response.Error.Detail.ExceptionToString.StartsWith("MenusAPI.SecurityException: Specified password is invalid."));
        }

        public readonly string AliceUN = "_Alice";
        public readonly string AliceUN2 = "_NewAlice";
        public readonly string BobUN = "_Bob";
        public readonly string JoeUN = "_Joe";

        public int ManagerRoleId = -1;
        public int PseudoAdminRoleId = -1;
        public int RegularUserRoleId = -1;

        public readonly string ManagerRoleName = "Manager";
        public readonly string PseudoAdminRoleName = "PseudoAdmin";
        public readonly string RegularUserRoleName = "Regular";


        public List<UserBO> GetUsers(UserCredentials userCredentials)
        {
            ReadUsersRequest req = new ReadUsersRequest();
            req.RequestorCredentials = userCredentials;

            var reply = serviceUnderTest.ReadUsers(req);
            if (reply.Error != null)
                throw reply.Error;

            return reply.ReadObjects.OfType<UserBO>().ToList();
        }

        public int DeleteBasicUsersDataIfExisting(UserCredentials userCredentials)
        {
            var users = GetUsers(userCredentials);

            var alice = users.Where(user => user.UserCredentials.UserName == AliceUN).FirstOrDefault();
            var alice2 = users.Where(user => user.UserCredentials.UserName == AliceUN2).FirstOrDefault();
            var bob = users.Where(user => user.UserCredentials.UserName == BobUN).FirstOrDefault();
            var joe = users.Where(user => user.UserCredentials.UserName == JoeUN).FirstOrDefault();

            CUDUsersRequest req = new CUDUsersRequest();
            req.RequestorCredentials = userCredentials;

            if (alice != null)
            {
                alice.IsDeleted = true;
                req.ObjectsToCUD.Add(alice);
            }

            if (alice2 != null)
            {
                alice2.IsDeleted = true;
                req.ObjectsToCUD.Add(alice2);
            }

            if (bob != null)
            {
                bob.IsDeleted = true;
                req.ObjectsToCUD.Add(bob);
            }

            if (joe != null)
            {
                joe = new UserBO(joe.UserID);
                joe.IsDeleted = true;
                joe.IsNew = false;
                req.ObjectsToCUD.Add(joe);
            }

            var response = serviceUnderTest.CUDUsers(req);
            Assert.IsTrue(response.Error == null);

            return response.NumberOfEntriesWhichGeneratedChanges;
        }

        public void AssignUsersSomeRoles(UserCredentials userCredentials)
        {
            var users = GetUsers(userCredentials);

            var alice = users.Where(user => user.UserCredentials.UserName == AliceUN).FirstOrDefault();
            var bob = users.Where(user => user.UserCredentials.UserName == BobUN).FirstOrDefault();
            var joe = users.Where(user => user.UserCredentials.UserName == JoeUN).FirstOrDefault();

            Assert.IsNotNull(alice);
            Assert.IsNotNull(bob);
            Assert.IsNotNull(joe);

            var roles = GetRoles(userCredentials);

            var managerRoleBO = roles.Where(role => role.RoleID == ManagerRoleId).FirstOrDefault();
            var pseudoAdminRoleBO = roles.Where(role => role.RoleID == PseudoAdminRoleId).FirstOrDefault();
            var regularRoleBO = roles.Where(role => role.RoleID == RegularUserRoleId).FirstOrDefault();

            alice.UserRoles.Add(pseudoAdminRoleBO);
            alice.UserRoles.Add(regularRoleBO);

            bob.UserRoles.Add(managerRoleBO);
            bob.UserRoles.Add(pseudoAdminRoleBO);
            bob.UserRoles.Add(regularRoleBO);

            joe.UserRoles.Add(regularRoleBO);

            var req = new CUDUsersRequest();
            req.RequestorCredentials = userCredentials;

            req.ObjectsToCUD.Add(alice);
            req.ObjectsToCUD.Add(bob);
            req.ObjectsToCUD.Add(joe);

            req.JustUpdateAssociationForReferencedRoles = false;

            var response = serviceUnderTest.CUDUsers(req);
            Assert.IsTrue(response.Error == null);
            Assert.IsTrue(response.NumberOfEntriesWhichGeneratedChanges == 3);

            pseudoAdminRoleBO.IsDeleted = true;
            req.ObjectsToCUD.Clear();
            req.ObjectsToCUD.Add(bob);
            req.JustUpdateAssociationForReferencedRoles = true;

            var response2 = serviceUnderTest.CUDUsers(req);
            Assert.IsTrue(response2.Error == null);
            Assert.IsTrue(response2.NumberOfEntriesWhichGeneratedChanges == 1);

            var response3 = serviceUnderTest.CUDUsers(req);
            Assert.IsTrue(response3.Error == null);
            Assert.IsTrue(response3.NumberOfEntriesWhichGeneratedChanges == 0);

            req.JustUpdateAssociationForReferencedRoles = false;
            var response4 = serviceUnderTest.CUDUsers(req);
            Assert.IsTrue(response4.Error == null);
            Assert.IsTrue(response4.NumberOfEntriesWhichGeneratedChanges == 1);
        }

        public void CreateBasicUserData(UserCredentials userCredentialsAdmin)
        {
            var alice = new UserBO(null, AliceUN, AliceUN + "pass", "Aliceuta") { ExpectedNumCalories = null };
            alice.IsNew = true;

            var bob = new UserBO(null, BobUN, BobUN + "pass", "THeBob") { ExpectedNumCalories = 200 };
            bob.IsNew = true;
            var joe = new UserBO(null, JoeUN, JoeUN + "pass", "IamJoe") { ExpectedNumCalories = 400 };

            CUDUsersRequest req1 = new CUDUsersRequest();
            req1.RequestorCredentials = userCredentialsAdmin;
            req1.ObjectsToCUD.Add(alice);
            var response1 = serviceUnderTest.CUDUsers(req1);

            CreateNewAccountRequest req2 = new CreateNewAccountRequest();
            req2.RequestorCredentials = bob.UserCredentials;
            req2.DisplayName = bob.DisplayName;
            req2.ExpectedNumCalories = bob.ExpectedNumCalories;
            req2.NumericalProof = 11431;
            var response2 = serviceUnderTest.CreateNewAccount(req2);

            CUDUsersRequest req3f = new CUDUsersRequest();
            req3f.RequestorCredentials = userCredentialsAdmin;
            req3f.ObjectsToCUD.Add(bob);
            var response3 = serviceUnderTest.CUDUsers(req3f);

            CreateNewAccountRequest req4 = new CreateNewAccountRequest();
            req4.RequestorCredentials = joe.UserCredentials;
            req4.DisplayName = joe.DisplayName;
            req4.ExpectedNumCalories = joe.ExpectedNumCalories;
            req4.NumericalProof = 12945385;
            var response4 = serviceUnderTest.CreateNewAccount(req4);

            req4.NumericalProof = 11431;
            var response5 = serviceUnderTest.CreateNewAccount(req4);

            Assert.IsTrue(response1.Error == null);
            Assert.IsTrue(response1.NumberOfEntriesWhichGeneratedChanges == 1);
            Assert.IsTrue(response2.Error == null);
            Assert.IsTrue(response3.Error.Detail.ExceptionToString.StartsWith("MenusAPI.OperationException: Specified username already exists"));
            Assert.IsTrue(response4.Error.Detail.ExceptionToString.StartsWith("MenusAPI.SecurityException: Unauthorized attempt to create a new account"));
            Assert.IsTrue(response5.Error == null);
        }

        public void CreateSomeRoles(UserCredentials userCredentialsAdmin)
        {
            var req = new CUDRolesRequest();
            req.RequestorCredentials = userCredentialsAdmin;

            var mngrRoleBO = new UserRoleBO(null, ManagerRoleName, false, true, false);
            var pseudoAdminRoleBO = new UserRoleBO(null, PseudoAdminRoleName, true, true, false);
            var regularRoleBO = new UserRoleBO(null, RegularUserRoleName, false, false, true);

            mngrRoleBO.IsNew = true;
            mngrRoleBO.CorrelationId = 0;
            pseudoAdminRoleBO.IsNew = true;
            pseudoAdminRoleBO.CorrelationId = 1;
            regularRoleBO.IsNew = true;
            regularRoleBO.CorrelationId = 2;

            req.ObjectsToCUD.Add(mngrRoleBO);
            req.ObjectsToCUD.Add(pseudoAdminRoleBO);
            req.ObjectsToCUD.Add(regularRoleBO);

            var response = serviceUnderTest.CUDRoles(req);
            Assert.IsTrue(response.Error == null);
            Assert.IsTrue(response.NumberOfEntriesWhichGeneratedChanges == 3);

            mngrRoleBO = response.NewlyCreatedObjectsWithIds.OfType<UserRoleBO>().Where(obj => obj.CorrelationId == mngrRoleBO.CorrelationId).FirstOrDefault();
            pseudoAdminRoleBO = response.NewlyCreatedObjectsWithIds.OfType<UserRoleBO>().Where(obj => obj.CorrelationId == pseudoAdminRoleBO.CorrelationId).FirstOrDefault();
            regularRoleBO = response.NewlyCreatedObjectsWithIds.OfType<UserRoleBO>().Where(obj => obj.CorrelationId == regularRoleBO.CorrelationId).FirstOrDefault();

            ManagerRoleId = mngrRoleBO.RoleID.Value;
            PseudoAdminRoleId = pseudoAdminRoleBO.RoleID.Value;
            RegularUserRoleId = regularRoleBO.RoleID.Value;

            req.ObjectsToCUD.Clear();
            regularRoleBO.IsAdmin = false;

            regularRoleBO.IsNew = false;
            req.ObjectsToCUD.Add(regularRoleBO);

            var response2 = serviceUnderTest.CUDRoles(req);
            Assert.IsTrue(response2.Error == null);
            Assert.IsTrue(response2.NumberOfEntriesWhichGeneratedChanges == 1);
        }

        public List<UserRoleBO> GetRoles(UserCredentials userCredentialsAdmin)
        {
            var req = new ReadRolesRequest();
            req.RequestorCredentials = userCredentialsAdmin;

            var response = serviceUnderTest.ReadRoles(req);
            Assert.IsTrue(response.Error == null);

            return response.ReadObjects.OfType<UserRoleBO>().ToList();
        }

        public void DeleteSomeRolesIfTheyExist(UserCredentials userCredentialsAdmin)
        {
            var roles = GetRoles(userCredentialsAdmin);

            var listToDelete = roles.Where(role => new[] { ManagerRoleName, PseudoAdminRoleName, RegularUserRoleName }.Contains(role.RoleName)).ToList();

            var req2 = new CUDRolesRequest();
            req2.RequestorCredentials = userCredentialsAdmin;
            foreach (var roleToDelete in listToDelete)
            {
                roleToDelete.IsDeleted = true;
                req2.ObjectsToCUD.Add(roleToDelete);
            }

            var response2 = serviceUnderTest.CUDRoles(req2);
            Assert.IsTrue(response2.Error == null);

            var list2ToDelete = roles.OfType<UserRoleBO>().Where(role => role.IsAdmin).ToList();
            req2.ObjectsToCUD.Clear();
            foreach (var roleBO in list2ToDelete)
            {
                roleBO.IsDeleted = true;
                req2.ObjectsToCUD.Add(roleBO);
            }

            var response3 = serviceUnderTest.CUDRoles(req2);
            Assert.IsTrue(response3.Error.Detail.ExceptionToString.StartsWith("MenusAPI.OperationException: Cannot perform operation because it would result in violation of business rule"));
        }

        [TestMethod]
        public void GetUsersTest()
        {
            var userCredentials = new UserCredentials("admin", "adminpass");

            var users = GetUsers(userCredentials);

            Assert.IsTrue(users.Count > 0);
            Assert.IsTrue(users.Any(user => user.UserCredentials.UserName == "admin"));
        }

        [TestMethod]
        public void GetUsersTestException1()
        {
            var userCredentials1 = new UserCredentials("__nosuchuser", "pass");
            try
            {
                var users1 = GetUsers(userCredentials1);
                Assert.Fail();
            }
            catch (FaultException<FaultDetails> Ex)
            {
                Assert.IsTrue(Ex.Detail.ExceptionToString.StartsWith("MenusAPI.SecurityException: No user to authorize"));
            }
        }

        [TestMethod]
        public void CreateUsersTest1()
        {
            var adminCredentials = new UserCredentials("admin", "adminpass");
            DeleteBasicUsersDataIfExisting(adminCredentials);
            CreateBasicUserData(adminCredentials);
        }

        [TestMethod]
        public void GetUsersTestException2()
        {
            CreateUsersTest1();

            var userCredentials1 = new UserCredentials(JoeUN, JoeUN + "pass");
            try
            {
                var users1 = GetUsers(userCredentials1);
                Assert.Fail();
            }
            catch (FaultException<FaultDetails> Ex)
            {
                Assert.IsTrue(Ex.Detail.ExceptionToString.StartsWith("MenusAPI.SecurityException: User is not authorized to perform"));
            }
        }

        [TestMethod]
        public void RolesGettingAndCUDTest()
        {
            var adminCredentials = new UserCredentials("admin", "adminpass");

            DeleteSomeRolesIfTheyExist(adminCredentials);
            CreateSomeRoles(adminCredentials);
            DeleteSomeRolesIfTheyExist(adminCredentials);
        }


        public void TestAliceAuthRoles()
        {
            var userCredentials = new UserCredentials(AliceUN, AliceUN + "pass");
            var req = new AuthenticateUserRequest(userCredentials, false);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var aliceBO = response.ReadObjects[0] as UserBO;

            Assert.IsTrue(aliceBO.UserCredentials.UserName == AliceUN);
            Assert.IsTrue(aliceBO.UserCredentials.Password == null);
            Assert.IsTrue(aliceBO.UserRoles.Count(userRole => userRole.RoleID == RegularUserRoleId) == 1);
            Assert.IsTrue(aliceBO.UserRoles.Count == 1);
        }

        public void TestBobAuthRoles()
        {
            var userCredentials = new UserCredentials(BobUN, BobUN + "pass");
            var req = new AuthenticateUserRequest(userCredentials, false);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var bobBO = response.ReadObjects[0] as UserBO;

            Assert.IsTrue(bobBO.UserCredentials.UserName == BobUN);
            Assert.IsTrue(bobBO.UserCredentials.Password == null);
            Assert.IsTrue(bobBO.UserRoles.Count(userRole => userRole.RoleID == RegularUserRoleId) == 1);
            Assert.IsTrue(bobBO.UserRoles.Count(userRole => userRole.RoleID == ManagerRoleId) == 1);
            Assert.IsTrue(bobBO.UserRoles.Count == 2);
        }

        public void TestJoeAuthRoles()
        {
            var userCredentials = new UserCredentials(JoeUN, JoeUN + "pass");
            var req = new AuthenticateUserRequest(userCredentials, false);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var joeBO = response.ReadObjects[0] as UserBO;

            Assert.IsTrue(joeBO.UserCredentials.UserName == JoeUN);
            Assert.IsTrue(joeBO.UserCredentials.Password == null);
            Assert.IsTrue(joeBO.UserRoles.Count(userRole => userRole.RoleID == RegularUserRoleId) == 1);
            Assert.IsTrue(joeBO.UserRoles.Count == 1);
        }


        public void TestAliceChangeCredentials()
        {
            UserCredentials credentials = new UserCredentials(AliceUN, AliceUN + "pass");

            AuthenticateUserRequest req = new AuthenticateUserRequest(credentials, false);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var aliceBO = response.ReadObjects[0] as UserBO;

            aliceBO.UserCredentials.UserName = AliceUN2;
            aliceBO.UserCredentials.Password = AliceUN2 + "pass";

            CUDUsersRequest req2 = new CUDUsersRequest();
            req2.RequestorCredentials = credentials;
            req2.ObjectsToCUD.Add(aliceBO);

            var response2 = serviceUnderTest.CUDUsers(req2);
            Assert.IsTrue(response2.Error.Detail.ExceptionToString.StartsWith("MenusAPI.SecurityException: User is not authorized to CUD Roles"));

            aliceBO.UserRoles.Clear();

            var response3 = serviceUnderTest.CUDUsers(req2);
            Assert.IsTrue(response3.Error == null);

            req2.RequestorCredentials = new UserCredentials(AliceUN2, AliceUN2 + "pass");
            aliceBO.UserCredentials.UserName = JoeUN;
            aliceBO.UserCredentials.Password = null;

            var response4 = serviceUnderTest.CUDUsers(req2);
            Assert.IsTrue(response4.Error.Detail.ExceptionToString.StartsWith("MenusAPI.OperationException: Specified username already exists"));

            aliceBO.UserCredentials.UserName = AliceUN;
            aliceBO.UserCredentials.Password = AliceUN + "pass";

            var response5 = serviceUnderTest.CUDUsers(req2);
            Assert.IsTrue(response5.Error == null);

            var response6 = serviceUnderTest.AuthenticateUser(req);
            Assert.IsTrue(response6.Error == null);
        }

        void ClearMenuEntriesByAlice()
        {
            UserCredentials credentials = new UserCredentials(AliceUN, AliceUN + "pass");

            AuthenticateUserRequest req = new AuthenticateUserRequest(credentials, true);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var aliceBO = response.ReadObjects[0] as UserBO;

            foreach (var MenuEntry in aliceBO.Menus)
                MenuEntry.IsDeleted = true;

            aliceBO.UserRoles.Clear();

            CUDUsersRequest req2 = new CUDUsersRequest();
            req2.RequestorCredentials = credentials;

            req2.ObjectsToCUD.Add(aliceBO);
            var response2 = serviceUnderTest.CUDUsers(req2);
            Assert.IsTrue(response2.Error == null);
        }

        void ClearMenuEntriesByBob()
        {
            UserCredentials credentials = new UserCredentials(BobUN, BobUN + "pass");

            AuthenticateUserRequest req = new AuthenticateUserRequest(credentials, true);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var bobBO = response.ReadObjects[0] as UserBO;

            foreach (var MenuEntry in bobBO.Menus)
                MenuEntry.IsDeleted = true;

            bobBO.UserRoles.Clear();

            CUDUsersRequest req2 = new CUDUsersRequest();
            req2.RequestorCredentials = credentials;

            req2.ObjectsToCUD.Add(bobBO);
            var response2 = serviceUnderTest.CUDUsers(req2);
            Assert.IsTrue(response2.Error == null);
        }

        void ClearMenuEntriesByJoe()
        {
            UserCredentials credentials = new UserCredentials(JoeUN, JoeUN + "pass");

            AuthenticateUserRequest req = new AuthenticateUserRequest(credentials, true);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var joeBO = response.ReadObjects[0] as UserBO;

            foreach (var MenuEntry in joeBO.Menus)
                MenuEntry.IsDeleted = true;

            joeBO.UserRoles.Clear();

            CUDUsersRequest req2 = new CUDUsersRequest();
            req2.RequestorCredentials = credentials;

            req2.ObjectsToCUD.Add(joeBO);
            var response2 = serviceUnderTest.CUDUsers(req2);
            Assert.IsTrue(response2.Error == null);
        }

        void CreateSomeMenuEntriesAsAliceAndVerifyCorrect()
        {
            UserCredentials credentials = new UserCredentials(AliceUN, AliceUN + "pass");

            AuthenticateUserRequest req = new AuthenticateUserRequest(credentials, false);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var aliceBO = response.ReadObjects[0] as UserBO;

            aliceBO.UserRoles.Clear();
            aliceBO.Menus.Clear();

            MenuEntryBO entry1 = new MenuEntryBO() { Text = "AliceFirst", Moment = new DateTime(2016, 8, 1, 9, 30, 0), NumCalories = 180, CorrelationId = 1 };
            MenuEntryBO entry2 = new MenuEntryBO() { Text = "AliceSecond", Moment = new DateTime(2016, 8, 1, 13, 30, 0), NumCalories = 120, CorrelationId = 2 };
            MenuEntryBO entry3 = new MenuEntryBO() { Text = "AliceThird", Moment = new DateTime(2016, 9, 10, 18, 0, 0), NumCalories = 50, CorrelationId = 3 };
            entry1.IsNew = true;
            entry2.IsNew = true;
            entry3.IsNew = true;

            aliceBO.Menus.Add(entry1);
            aliceBO.Menus.Add(entry2);

            CUDUsersRequest req2 = new CUDUsersRequest();
            req2.RequestorCredentials = credentials;

            req2.ObjectsToCUD.Add(aliceBO);
            var response2 = serviceUnderTest.CUDUsers(req2);
            Assert.IsTrue(response2.Error == null);
            Assert.IsTrue(response2.NumberOfEntriesWhichGeneratedChanges == 1);

            var returnedEntries = response2.NewlyCreatedObjectsWithIds.OfType<MenuEntryBO>().ToArray();
            Assert.IsTrue(returnedEntries.Length == 2);

            entry1 = returnedEntries.Where(entry => entry.CorrelationId == entry1.CorrelationId).FirstOrDefault();
            entry2 = returnedEntries.Where(entry => entry.CorrelationId == entry2.CorrelationId).FirstOrDefault();

            Assert.IsNotNull(entry1);
            Assert.IsNotNull(entry2);

            entry1.NumCalories = 120;
            Assert.IsFalse(entry1.IsNew);

            CUDMenusRequest req3 = new CUDMenusRequest();
            req3.RequestorCredentials = credentials;

            req3.ObjectsToCUD.Add(entry3);
            req3.ObjectsToCUD.Add(entry1);
            entry2.IsNew = false;
            req3.ObjectsToCUD.Add(entry2);

            var response3 = serviceUnderTest.CUDMenus(req3);
            Assert.IsTrue(response3.Error.Detail.ExceptionToString.StartsWith("MenusAPI.OperationException: Cannot add new Menu entries without a valid owner"));

            entry3.Owner = aliceBO;
            response3 = serviceUnderTest.CUDMenus(req3);
            Assert.IsNull(response3.Error);
            Assert.IsTrue(response3.NumberOfEntriesWhichGeneratedChanges == 2);
            Assert.IsTrue(response3.NewlyCreatedObjectsWithIds.Count == 1);

            var newEntry3 = response3.NewlyCreatedObjectsWithIds[0] as MenuEntryBO;
            Assert.AreEqual(entry3.CorrelationId, newEntry3.CorrelationId);
            entry3 = newEntry3;

            AuthenticateUserRequest req4 = new AuthenticateUserRequest(credentials, true);
            var response4 = serviceUnderTest.AuthenticateUser(req4);

            Assert.IsNull(response4.Error);

            var MenusReturned = (response4.ReadObjects[0] as UserBO).Menus;
            Assert.AreEqual(3, MenusReturned.Count);

            MenusReturned.Sort();

            Assert.AreEqual(MenusReturned[0].Text, "AliceFirst");
            Assert.AreEqual(MenusReturned[0].NumCalories, 120); //The new value
            Assert.AreEqual(MenusReturned[1].Text, "AliceSecond");
            Assert.AreEqual(MenusReturned[1].NumCalories, 120);
            Assert.AreEqual(MenusReturned[2].Text, "AliceThird");
            Assert.AreEqual(MenusReturned[2].NumCalories, 50);
        }

        void CreateSomeMenuEntriesAsBob()
        {
            UserCredentials credentials = new UserCredentials(BobUN, BobUN + "pass");

            AuthenticateUserRequest req = new AuthenticateUserRequest(credentials, false);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var bobBO = response.ReadObjects[0] as UserBO;

            bobBO.UserRoles.Clear();
            bobBO.Menus.Clear();

            MenuEntryBO entry1 = new MenuEntryBO() { Text = "BobFirst", Moment = new DateTime(2016, 8, 1, 10, 05, 0), NumCalories = 60, CorrelationId = 1 };
            MenuEntryBO entry2 = new MenuEntryBO() { Text = "BobSecond", Moment = new DateTime(2016, 8, 1, 13, 0, 0), NumCalories = 0, CorrelationId = 2 };
            MenuEntryBO entry3 = new MenuEntryBO() { Text = "BobThird", Moment = new DateTime(2016, 10, 5, 16, 00, 0), NumCalories = 360, CorrelationId = 3 };
            entry1.IsNew = true;
            entry2.IsNew = true;
            entry3.IsNew = true;

            bobBO.Menus.Add(entry1);
            bobBO.Menus.Add(entry2);
            bobBO.Menus.Add(entry3);

            CUDUsersRequest req2 = new CUDUsersRequest();
            req2.RequestorCredentials = credentials;

            req2.ObjectsToCUD.Add(bobBO);
            var response2 = serviceUnderTest.CUDUsers(req2);
            Assert.IsTrue(response2.Error == null);
            Assert.IsTrue(response2.NumberOfEntriesWhichGeneratedChanges == 1);
            Assert.IsTrue(response2.NewlyCreatedObjectsWithIds.Count == 3);
        }

        void CUDMenuEntriesAsBobOwnAndAlices()
        {
            UserCredentials credentials = new UserCredentials(BobUN, BobUN + "pass");

            AuthenticateUserRequest req = new AuthenticateUserRequest(credentials, true);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var bobBO = response.ReadObjects[0] as UserBO;

            var req2 = new ReadMenusRequest();
            req2.RequestorCredentials = credentials;
            req2.OwnerOfMenuEntries = null; //Get all Menus

            var response2 = serviceUnderTest.ReadMenus(req2);
            Assert.IsNull(response2.Error);

            var MenusOfAlice = response2.ReadObjects.OfType<MenuEntryBO>().Where(entry => entry.Owner.UserCredentials.UserName == AliceUN && entry.Moment.Month != 8).ToList();
            var ownEntries = response2.ReadObjects.OfType<MenuEntryBO>().Where(entry => entry.Text.StartsWith("Bob") && entry.Moment.Month != 10);

            var req3 = new CUDMenusRequest();
            req3.RequestorCredentials = credentials;

            int count = 0;

            foreach (var entry in MenusOfAlice)
            {
                entry.IsDeleted = true;
                req3.ObjectsToCUD.Add(entry);
                count++;
            }

            foreach (var entry in ownEntries)
            {
                entry.Text = "EntryOfBob";
                req3.ObjectsToCUD.Add(entry);
                count++;
            }

            var response3 = serviceUnderTest.CUDMenus(req3);
            Assert.IsNull(response3.Error);
            Assert.AreEqual(count, response3.NumberOfEntriesWhichGeneratedChanges);
        }
        void CreateSomeMenuEntriesAsJoe()
        {
            UserCredentials credentials = new UserCredentials(JoeUN, JoeUN + "pass");

            AuthenticateUserRequest req = new AuthenticateUserRequest(credentials, false);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var joeBO = response.ReadObjects[0] as UserBO;

            joeBO.UserRoles.Clear();
            joeBO.Menus.Clear();

            MenuEntryBO entry1 = new MenuEntryBO() { Text = "AJoeFirst", Moment = new DateTime(2016, 8, 15, 10, 05, 0), NumCalories = 60, CorrelationId = 1 };
            MenuEntryBO entry2 = new MenuEntryBO() { Text = "JoeSecond", Moment = new DateTime(2016, 8, 1, 14, 05, 0), NumCalories = 0, CorrelationId = 2 };
            MenuEntryBO entry3 = new MenuEntryBO() { Text = "JoeThird", Moment = new DateTime(2016, 9, 5, 19, 45, 0), NumCalories = 560, CorrelationId = 3 };
            MenuEntryBO entry4 = new MenuEntryBO() { Text = "FourthByJoe", Moment = new DateTime(2016, 10, 3, 20, 01, 0), NumCalories = 460, CorrelationId = 3 };
            entry1.IsNew = true;
            entry2.IsNew = true;
            entry3.IsNew = true;
            entry4.IsNew = true;

            joeBO.Menus.Add(entry1);
            joeBO.Menus.Add(entry2);
            joeBO.Menus.Add(entry3);
            joeBO.Menus.Add(entry4);

            CUDUsersRequest req2 = new CUDUsersRequest();
            req2.RequestorCredentials = credentials;

            req2.ObjectsToCUD.Add(joeBO);
            var response2 = serviceUnderTest.CUDUsers(req2);
            Assert.IsTrue(response2.Error == null);
            Assert.IsTrue(response2.NumberOfEntriesWhichGeneratedChanges == 1);
            Assert.IsTrue(response2.NewlyCreatedObjectsWithIds.Count == 4);
        }

        void GetJoesMenuEntriesUsingFilters()
        {
            var bobUC = new UserCredentials(BobUN, BobUN + "pass");
            var joeUC = new UserCredentials(JoeUN, JoeUN + "pass");

            var authBobReq = new AuthenticateUserRequest(bobUC, false);
            var resp1 = serviceUnderTest.AuthenticateUser(authBobReq);
            Assert.IsNull(resp1.Error);
            var bobBO = resp1.ReadObjects[0] as UserBO;
            bobBO.UserRoles.Clear();

            Assert.IsNotNull(resp1.SecurityToken);
            bobUC.TokenID = resp1.SecurityToken.TokenId;
            bobUC.UserName = ":ignored";
            bobUC.Password = null;

            var authJoeReq = new AuthenticateUserRequest(joeUC, false);
            var resp2 = serviceUnderTest.AuthenticateUser(authJoeReq);
            Assert.IsNull(resp2.Error);
            var joeBO = resp2.ReadObjects[0] as UserBO;
            joeBO.UserRoles.Clear();

            var filter1specs = new TimeFilterSpecs();
            filter1specs.DateFrom = new DateTime(2016, 8, 1, 19, 05, 0); //Time in DateFrom/DateTo should be ignored.
            filter1specs.DateTo = new DateTime(2016, 10, 1, 10, 05, 0);
            filter1specs.MinuteInDayFrom = 14 * 60 + 6;
            filter1specs.MinuteInDayTo = 20 * 60 + 0; //Filter should read just one entries: the one in September.

            var req1 = new ReadMenusRequest();
            req1.RequestorCredentials = bobUC;
            req1.OwnerOfMenuEntries = joeBO;
            req1.TimeFilterSpecs = filter1specs;
            var resp3 = serviceUnderTest.ReadMenus(req1);
            Assert.IsNull(resp3.Error);
            Assert.IsTrue(resp3.ReadObjects.Count == 1);
            Assert.IsTrue((resp3.ReadObjects[0] as MenuEntryBO).Text == "JoeThird");

            filter1specs.MinuteInDayFrom = 0 * 60 + 0;
            filter1specs.MinuteInDayTo = 23 * 60 + 59;
            filter1specs.DateFrom = new DateTime(2016, 10, 1, 0, 0, 0);
            filter1specs.DateTo = new DateTime(2016, 11, 1, 0, 0, 0, 0);

            var req2 = new ReadMenusRequest();
            req2.RequestorCredentials = bobUC;
            req2.OwnerOfMenuEntries = joeBO;
            req2.TimeFilterSpecs = filter1specs;
            var resp4 = serviceUnderTest.ReadMenus(req2);
            Assert.IsNull(resp4.Error);
            Assert.IsTrue(resp4.ReadObjects.Count == 1);


            filter1specs.DateFrom = new DateTime(2016, 8, 1, 19, 05, 0); //Time in DateFrom/DateTo should be ignored.
            filter1specs.DateTo = new DateTime(2016, 10, 1, 10, 05, 0);
            filter1specs.MinuteInDayFrom = 14 * 60 + 3;
            filter1specs.MinuteInDayTo = 20 * 60 + 0; //Filter should read just one entries: the one in September.

            var req3 = new ReadMenusRequest();
            req3.RequestorCredentials = joeUC;
            req3.OwnerOfMenuEntries = joeBO;
            req3.TimeFilterSpecs = filter1specs;
            var resp5 = serviceUnderTest.ReadMenus(req3);
            Assert.IsNull(resp5.Error);
            Assert.IsTrue(resp5.ReadObjects.Count == 2);
        }

        [TestMethod]
        void GetJoesEntriesUsingPaginationAndTokenBasedAuthentication()
        {
            UserCreationScenarioTest1();

            CreateSomeMenuEntriesAsJoe();

            UserCredentials credentials = new UserCredentials(JoeUN, JoeUN + "pass");

            AuthenticateUserRequest req = new AuthenticateUserRequest(credentials, false);
            var response = serviceUnderTest.AuthenticateUser(req);
            Assert.IsTrue(response.Error == null);
            var joeBO = response.ReadObjects[0] as UserBO;

            joeBO.UserRoles.Clear();

            credentials.TokenID = response.SecurityToken.TokenId;
            credentials.UserName = "ancadamndck";
            credentials.Password = "jdkgfsgfshdkfjs";

            ReadMenusRequest req2 = new ReadMenusRequest();
            req2.bFillOutCaloricColorCoding = true;
            req2.OwnerOfMenuEntries = joeBO;
            req2.RequestorCredentials = credentials;
            req2.PageingInfo = new PagingInfo() { PageNumber = 2, ItemsPerPage = 1 };

            var response2 = serviceUnderTest.ReadMenus(req2);
            Assert.IsNull(response2.Error);
            Assert.IsTrue(response2.ReadObjects.Count == 1);
            Assert.IsTrue(response2.PagingInfo.TotalItems == 4);
            Assert.IsTrue(response2.PagingInfo.ItemsPerPage == 1);

            ClearMenuEntriesByJoe();
        }

        [TestMethod]
        public void UserCreationScenarioTest1()
        {
            var adminCredentials = new UserCredentials("admin", "adminpass");

            CreateUsersTest1();
            DeleteSomeRolesIfTheyExist(adminCredentials);
            CreateSomeRoles(adminCredentials);
            AssignUsersSomeRoles(adminCredentials);
            TestAliceAuthRoles();
            TestBobAuthRoles();
            TestJoeAuthRoles();
            TestAliceChangeCredentials();
        }

        [TestMethod]
        public void FullUsageTestScenario1()
        {
            UserCreationScenarioTest1();

            ClearMenuEntriesByAlice();
            CreateSomeMenuEntriesAsAliceAndVerifyCorrect();

            GetJoesEntriesUsingPaginationAndTokenBasedAuthentication();
        }

        [TestMethod]
        public void FullUsageTestScenario2()
        {
            FullUsageTestScenario1();
            ClearMenuEntriesByBob();
            CreateSomeMenuEntriesAsBob();
            CUDMenuEntriesAsBobOwnAndAlices();
        }


        [TestMethod]
        public void FullUsageTestScenario3()
        {
            UserCreationScenarioTest1();

            ClearMenuEntriesByJoe();
            CreateSomeMenuEntriesAsJoe();
            GetJoesMenuEntriesUsingFilters();
        }


        [TestMethod]
        public void FullUsageTestScenario4ColorCoding()
        {
            UserCreationScenarioTest1();

            ClearMenuEntriesByAlice();
            CreateSomeMenuEntriesAsAliceAndVerifyCorrect();

            UserCredentials credentials = new UserCredentials(AliceUN, AliceUN + "pass");

            AuthenticateUserRequest req = new AuthenticateUserRequest(credentials, false);
            var response = serviceUnderTest.AuthenticateUser(req);

            Assert.IsTrue(response.Error == null);
            var aliceBO = response.ReadObjects[0] as UserBO;

            aliceBO.UserRoles.Clear();

            //Make first request with no caloric threshold set on Alice.
            var req2 = new ReadMenusRequest()
            {
                RequestorCredentials = credentials,
                bFillOutCaloricColorCoding = true,
                OwnerOfMenuEntries = aliceBO
            };
            var resp2 = serviceUnderTest.ReadMenus(req2);

            Assert.IsTrue(resp2.Error == null);
            Assert.IsTrue(resp2.ReadObjects.Count == 3);

            foreach (var obj in resp2.ReadObjects.OfType<MenuEntryBO>())
                Assert.IsNull(obj.ColorCodingWithRegardToOwnerSettings);

            //Set caloric threshold on Alice.
            aliceBO.ExpectedNumCalories = 150; //Two will be RED, one green from default entries created.

            CUDUsersRequest req3 = new CUDUsersRequest();
            req3.RequestorCredentials = credentials;
            req3.ObjectsToCUD.Add(aliceBO);

            var resp3 = serviceUnderTest.CUDUsers(req3);
            Assert.IsNull(resp3.Error);

            //Get color coded entries.
            var req4 = new ReadMenusRequest()
            {
                RequestorCredentials = credentials,
                bFillOutCaloricColorCoding = true,
                OwnerOfMenuEntries = aliceBO
            };
            var resp4 = serviceUnderTest.ReadMenus(req4);

            Assert.IsTrue(resp4.Error == null);
            Assert.IsTrue(resp4.ReadObjects.Count == 3);

            foreach (var obj in resp4.ReadObjects.OfType<MenuEntryBO>().Where(obj => obj.Moment.Date.Month == 8))
                Assert.IsTrue(obj.ColorCodingWithRegardToOwnerSettings < 0);


            foreach (var obj in resp4.ReadObjects.OfType<MenuEntryBO>().Where(obj => obj.Moment.Date.Month == 9))
                Assert.IsTrue(obj.ColorCodingWithRegardToOwnerSettings >= 0);

            //Clear caloric threshold on Alice.
            aliceBO.ExpectedNumCalories = null; //Two will be RED, one green from default entries created.

            CUDUsersRequest req5 = new CUDUsersRequest();
            req3.RequestorCredentials = credentials;
            req3.ObjectsToCUD.Add(aliceBO);

            var resp5 = serviceUnderTest.CUDUsers(req5);
            Assert.IsNull(resp3.Error);
        }


        [TestMethod]
        public void ReadLogAndIsDatabaseUpTest(bool bStandAlone = true)
        {
            var adminCredentials = new UserCredentials("admin", "adminpass");

            var req1 = new IsDatabaseUpRequest();
            req1.RequestorCredentials = adminCredentials;
            var resp1 = serviceUnderTest.IsDatabaseUp(req1);
            Assert.IsNull(resp1.Error);


            var req2 = new ReadLogRequest();
            req2.RequestorCredentials = adminCredentials;
            req2.NoLastDays = 2;
            var resp2 = serviceUnderTest.ReadLog(req2);
            Assert.IsNull(resp2.Error);

            if (bStandAlone)
                CreateUsersTest1();

            req1.RequestorCredentials = new UserCredentials(JoeUN, JoeUN + "pass");
            var resp3 = serviceUnderTest.IsDatabaseUp(req1);
            Assert.IsNull(resp3.Error);


            req2.RequestorCredentials = new UserCredentials(BobUN, BobUN + "pass");
            var resp4 = serviceUnderTest.ReadLog(req2);
            Assert.IsTrue(resp4.Error.Detail.ExceptionToString.StartsWith("MenusAPI.SecurityException: User is not authorized to perform requested operation."));
        }

        [TestMethod]
        public void CompleteScenarioTest()
        {
            UserCreationScenarioTest1();

            ClearMenuEntriesByAlice();
            CreateSomeMenuEntriesAsAliceAndVerifyCorrect();

            ReadLogAndIsDatabaseUpTest(false);

            CreateSomeMenuEntriesAsBob();
            ClearMenuEntriesByJoe();
            CreateSomeMenuEntriesAsJoe();
            CUDMenuEntriesAsBobOwnAndAlices();
            ClearMenuEntriesByJoe();
            CreateSomeMenuEntriesAsJoe();
            GetJoesMenuEntriesUsingFilters();
        }

        [TestMethod]
        public void InitializeDatabaseWithDemoData()
        {
            UserCreationScenarioTest1();

            CreateSomeMenuEntriesAsAliceAndVerifyCorrect();
            CreateSomeMenuEntriesAsBob();
            CreateSomeMenuEntriesAsJoe();
            GetJoesMenuEntriesUsingFilters();
            bLeaveDemoData = true;
        }
    }
}
