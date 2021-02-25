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
    public static class MenuEntryBOExtensions
    {
        public static MenuEntryBO CreateBusinessObject(this MenuEntry MenuEntryDAL)
        {
            var result = new MenuEntryBO(MenuEntryDAL.Id);
            
            result.Moment = MenuEntryDAL.Moment;
            result.NumCalories = MenuEntryDAL.NumCalories;
            result.ColorCodingWithRegardToOwnerSettings = null;
            result.Text = MenuEntryDAL.Text;
            result.Owner = MenuEntryDAL.User.CreateBusinessObject(translatePassword: false, getSatteliteData: false);
            
            return result;
        }

        public static bool TransferDataToDALObject(this MenuEntryBO MenuEntryBO, MenuEntry MenuEntryDAL)
        {
            if (MenuEntryDAL.Moment != MenuEntryBO.Moment || MenuEntryDAL.NumCalories != MenuEntryBO.NumCalories || MenuEntryDAL.Text != MenuEntryBO.Text)
            {
                if (MenuEntryDAL.NumCalories < 0)
                    throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Cannot have a negative expected number of calories.", new ArgumentException("Cannot have a negative expected number of calories."));

                if(MenuEntryBO.Text.Length > APIUtils.MaxNamesLength)
                    throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Text of menu entry too long.", new ArgumentException("Text of menu entry too long."));

                MenuEntryDAL.Moment = MenuEntryBO.Moment;
                MenuEntryDAL.NumCalories= MenuEntryBO.NumCalories;
                MenuEntryDAL.Text = MenuEntryBO.Text;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the Data Access Layer context to reflect potential changes to this business object.
        /// </summary>
        /// <param name="contextToUpdate">The context to which changes are to be reflected.</param>
        /// <returns>True if changes were made to the context, false if not.</returns>
        public static bool UpdateDALWithChanges(this MenuEntryBO businessObject, MenusEntityModelContainer contextToUpdate, BusinessContext businessContext)
        {
            MenuEntry MenuEntryDAL = null;

            bool bChangesMade = false;

            if (businessObject.IsNew)
            {
                MenuEntryDAL = contextToUpdate.MenuEntries.Create();
                bChangesMade = businessObject.TransferDataToDALObject(MenuEntryDAL);

                if(businessObject.Owner == null)
                    throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Cannot add new Menu entries without a valid owner. Operation is disallowed.");
                
                User ownerDAL = businessContext[businessObject.Owner]?.DALObject as User;
                if (ownerDAL == null)
                    ownerDAL = contextToUpdate.Users.Find(businessObject.Owner.UserID);
                
                MenuEntryDAL.User = ownerDAL;

                if (MenuEntryDAL.User == null)
                    throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Cannot add new Menu entries without a valid owner. Operation is disallowed.");

                contextToUpdate.MenuEntries.Add(MenuEntryDAL);
                bChangesMade = true;
            }
            else
            {
                MenuEntryDAL = businessContext[businessObject]?.DALObject as MenuEntry;
                if(MenuEntryDAL == null)
                    MenuEntryDAL = contextToUpdate.MenuEntries.Find(businessObject.EntryID);

                if (MenuEntryDAL == null)
                    throw new OperationException(System.Net.HttpStatusCode.NotFound, "Cannot edit an entry with an invalid id. Operation might be attempted security breach.");

                if(businessObject.Owner?.UserID != null)
                    if (MenuEntryDAL.User?.Id != businessObject.Owner.UserID)
                        throw new SecurityException(System.Net.HttpStatusCode.Forbidden, "Cannot change owner of a Menu entry. Operation is disallowed.");

                if(!businessObject.IsDeleted)
                    bChangesMade = businessObject.TransferDataToDALObject(MenuEntryDAL);
            }

            if(MenuEntryDAL!=null)
                businessContext[businessObject, bRegisterIfNotPresent: true].DALObject = MenuEntryDAL;
            if (!businessObject.IsNew)
                businessContext[businessObject, bRegisterIfNotPresent: true].DALObjectID = MenuEntryDAL?.Id;

            if (businessObject.IsDeleted)
            {
                if (MenuEntryDAL == null)
                    bChangesMade = false;
                else
                {
                    contextToUpdate.MenuEntries.Remove(MenuEntryDAL);
                    bChangesMade = true;
                }
            }

            return bChangesMade;
        }
    }
}
