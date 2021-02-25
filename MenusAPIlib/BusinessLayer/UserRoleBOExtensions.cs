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
    public static class UserRoleBOExtensions
    {
        public static UserRoleBO CreateBusinessObject(this UserRole userRoleDAL)
        {
            return new UserRoleBO(userRoleDAL.Id, userRoleDAL.RoleName, userRoleDAL.CanCRUDUsers, userRoleDAL.CanCRUDOthersEntries, userRoleDAL.IsAdmin);
        }
        
        public static bool TransferDataToDALObject(this UserRoleBO userRoleBO, UserRole userRoleDAL)
        {
            if (userRoleDAL.CanCRUDOthersEntries != userRoleBO.CanCRUDEntries
                || userRoleDAL.CanCRUDUsers != userRoleBO.CanCRUDUsers
                || userRoleDAL.IsAdmin != userRoleBO.IsAdmin
                || userRoleDAL.RoleName != userRoleBO.RoleName)
            {
                userRoleDAL.CanCRUDOthersEntries = userRoleBO.CanCRUDEntries;
                userRoleDAL.CanCRUDUsers = userRoleBO.CanCRUDUsers;
                userRoleDAL.IsAdmin = userRoleBO.IsAdmin;
                userRoleDAL.RoleName = userRoleBO.RoleName;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the Data Access Layer context to reflect potential changes to this business object.
        /// </summary>
        /// <param name="contextToUpdate">The context to which changes are to be reflected.</param>
        /// <returns>True if changes were made to the context, false if not.</returns>
        public static bool UpdateDALWithChanges(this UserRoleBO businessObject, MenusEntityModelContainer contextToUpdate, BusinessContext businessContext)
        {
            UserRole userRoleDAL = null;
            
            bool bChangesMade = false;

            if (businessObject.IsNew)
            {
                userRoleDAL = contextToUpdate.UserRoles.Create();

                businessObject.TransferDataToDALObject(userRoleDAL);
                bChangesMade = true;

                contextToUpdate.UserRoles.Add(userRoleDAL);
            }
            else
            {
                userRoleDAL = businessContext[businessObject]?.DALObject as UserRole;
                if (userRoleDAL == null)
                {
                    userRoleDAL = contextToUpdate.UserRoles.Find(businessObject.RoleID);
                }
                
                if(userRoleDAL == null && !businessObject.IsDeleted)
                    throw new OperationException(System.Net.HttpStatusCode.NotFound, "Cannot alter an entry with an invalid id. Operation is attempted security breach.");
                
                if(!businessObject.IsDeleted)
                    bChangesMade = businessObject.TransferDataToDALObject(userRoleDAL);
            }

            if(userRoleDAL != null)
                businessContext[businessObject, bRegisterIfNotPresent: true].DALObject = userRoleDAL;
            if (!businessObject.IsNew)
                businessContext[businessObject, bRegisterIfNotPresent: true].DALObjectID = userRoleDAL?.Id;

            if (businessObject.IsDeleted)
            {
                if (userRoleDAL == null)
                    bChangesMade = false;
                else
                {
                    contextToUpdate.UserRoles.Remove(userRoleDAL);
                    businessContext.DeletedObjects.Add(businessObject);
                    bChangesMade = true;
                }
            }

            return bChangesMade;
        }
    }
}
