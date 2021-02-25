/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using MenusAPIlib.Model;
using MenusAPI;

namespace MenusAPIlib
{
    public static class UserBOExtensions
    {
        public static void ClearSensitiveDateFromUserBOObject(this UserBO userBO)
        {
            userBO.UserRoles = null;
            userBO.UserCredentials.UserName = null;
            userBO.UserCredentials.Password = null;
            userBO.UserCredentials.TokenID = null;
        }

        public static UserBO CreateBusinessObject(this User userDAL, bool translatePassword = false, bool getSatteliteData = false)
        {
            UserBO userBO = new UserBO(userDAL.Id, userDAL.Username, translatePassword ? userDAL.Password : null, userDAL.DisplayName);
            userBO.ExpectedNumCalories = userDAL.ExpectedNumCalories;

            userBO.UserRoles = userDAL.UserRoles.Select(userRoleDAL => userRoleDAL.CreateBusinessObject()).ToList();

            if (getSatteliteData)
            {
                userBO.Menus = userDAL.MenuEntries.Select(MenuEntryDAL => MenuEntryDAL.CreateBusinessObject()).ToList();
            }

            return userBO;
        }

        public static bool TransferDataToDALObject(this UserBO userBO, User userDAL, bool bIncludePassword)
        {
            if (bIncludePassword)
            {
                if (userBO.UserCredentials.Password == null)
                    throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Cannot add a user with no password. Operation is disallowed.");
                if (userBO.UserCredentials.Password.Length < 1 || userBO.UserCredentials.Password.Length > APIUtils.MaxNamesLength)
                    throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Invalid length of password.");
            }

            if (userBO.UserCredentials.UserName == null)
                throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Cannot add a user with no username. Operation is disallowed.");
            if (userBO.UserCredentials.UserName.Length < 1 || userBO.UserCredentials.UserName.Length > APIUtils.MaxNamesLength)
                throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Invalid length of username.");

            if (userBO.DisplayName == null)
                throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "Cannot add a user with no display name. Operation is disallowed.");
            if(bIncludePassword)
                if (userBO.UserCredentials.Password.Length > APIUtils.MaxNamesLength)
                    throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Invalid length of password.");

            if (userBO.ExpectedNumCalories < 0)
                throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Cannot permit a negative expected number of calories.");

            bool bChangesMade = false;

            if (userDAL.Username != userBO.UserCredentials.UserName)
                bChangesMade = true;

            if (bIncludePassword)
                bChangesMade = true;

            if (userDAL.DisplayName != userBO.DisplayName)
                bChangesMade = true;

            if (userDAL.ExpectedNumCalories != userBO.ExpectedNumCalories)
                bChangesMade = true;

            if (!bChangesMade)
                return false;

            userDAL.Username = userBO.UserCredentials.UserName;
            if(bIncludePassword)
                userDAL.Password = userBO.UserCredentials.Password.GetPasswordHash();

            userDAL.DisplayName = userBO.DisplayName;
            userDAL.ExpectedNumCalories = userBO.ExpectedNumCalories;

            return bChangesMade;
        }

        /// <summary>
        /// Updates the Data Access Layer context to reflect potential changes to this business object.
        /// </summary>
        /// <param name="contextToUpdate">The context to which changes are to be reflected.</param>
        /// <returns>True if changes were made to the context, false if not.</returns>
        public static bool UpdateDALWithChanges(this UserBO businessObject, MenusEntityModelContainer contextToUpdate, BusinessContext businessContext)
        {
            User userDAL = null;

            bool bChangesMade = false;

            if (businessObject.IsNew)
            {
                userDAL = contextToUpdate.Users.Create();
                bChangesMade = businessObject.TransferDataToDALObject(userDAL, true);

                if (contextToUpdate.Users.Any(user => user.Username == userDAL.Username))
                    throw new OperationException(System.Net.HttpStatusCode.Conflict, "Specified username already exists. Cannot have duplicate usernames.");
                if (contextToUpdate.Users.Local.Any(user => user.Username == userDAL.Username))
                    throw new OperationException(System.Net.HttpStatusCode.Conflict, "Specified username already exists. Cannot have duplicate usernames.");
                
                contextToUpdate.Users.Add(userDAL);
            }
            else
            {
                userDAL = businessContext[businessObject]?.DALObject as User;
                if (userDAL == null)
                {
                    userDAL = contextToUpdate.Users.Find(businessObject.UserID);
                }

                if (userDAL == null && !businessObject.IsDeleted)
                    throw new OperationException(System.Net.HttpStatusCode.NotFound, "Cannot alter an entry with an invalid id. Operation is attempted security breach.");

                if (!businessObject.IsDeleted)
                    if (userDAL.Username != businessObject.UserCredentials.UserName)
                    {
                        if (contextToUpdate.Users.Any(user => user.Username == businessObject.UserCredentials.UserName))
                            throw new OperationException(System.Net.HttpStatusCode.Conflict, "Specified username already exists. Cannot have duplicate usernames.");
                        if (contextToUpdate.Users.Local.Any(user => user.Username == businessObject.UserCredentials.UserName))
                            throw new OperationException(System.Net.HttpStatusCode.Conflict, "Specified username already exists. Cannot have duplicate usernames.");
                    }

                if(!businessObject.IsDeleted)
                    bChangesMade = businessObject.TransferDataToDALObject(userDAL, businessObject.UserCredentials?.Password != null);
            }

            if(userDAL != null)
                businessContext[businessObject, bRegisterIfNotPresent: true].DALObject = userDAL;
            if(!businessObject.IsNew)
                businessContext[businessObject, bRegisterIfNotPresent: true].DALObjectID = userDAL?.Id;

            if (businessObject.IsDeleted)
            {
                if (userDAL == null)
                    bChangesMade = false;
                else
                {
                    //Business Rule stating that there should be at least one admin is validated globally.
                    
                    contextToUpdate.MenuEntries.RemoveRange(userDAL.MenuEntries);
                    contextToUpdate.Users.Remove(userDAL);
                    bChangesMade = true;
                }
            }
            else
            {
                if (businessObject.UserRoles != null)
                    foreach (var userRoleBO in businessObject.UserRoles)
                    {
                        UserRole userRoleDAL;

                        if (userRoleBO.IsDeleted &&
                            ((businessContext.OriginalRequest as CUDUsersRequest)?.JustUpdateAssociationForReferencedRoles ?? false))
                        { //Just remove a role from user roles
                            if (userRoleBO.RoleID == null)
                                throw new OperationException(System.Net.HttpStatusCode.BadRequest, "User Role to remove from specified user must be specified by Id.");

                            userRoleDAL = contextToUpdate.UserRoles.Find(userRoleBO.RoleID);
                            if (userRoleDAL != null)
                            {
                                if (userDAL.UserRoles.Contains(userRoleDAL))
                                {
                                    userDAL.UserRoles.Remove(userRoleDAL);
                                    bChangesMade = true;
                                }
                            }
                        }
                        else
                        { //Also add/delete/update role
                            if ((businessContext.OriginalRequest as CUDUsersRequest)?.JustUpdateAssociationForReferencedRoles ?? false)
                            {
                                userRoleDAL = contextToUpdate.UserRoles.Find(userRoleBO.RoleID);
                            }
                            else
                            {
                                bChangesMade |= userRoleBO.UpdateDALWithChanges(contextToUpdate, businessContext);
                                userRoleDAL = businessContext[userRoleBO]?.DALObject as UserRole;
                            }
                            if (userRoleDAL != null && !userRoleBO.IsDeleted)
                            {
                                if (!userDAL.UserRoles.Contains(userRoleDAL))
                                {
                                    userDAL.UserRoles.Add(userRoleDAL);
                                    bChangesMade = true;
                                }
                            }
                        }
                    }

                if (businessObject.Menus != null)
                    foreach (var MenuEntryBO in businessObject.Menus)
                    {
                        MenuEntryBO.Owner = businessObject;
                        bChangesMade |= MenuEntryBO.UpdateDALWithChanges(contextToUpdate, businessContext);
                    }
            }

            return bChangesMade;
        }
   
    }
}
