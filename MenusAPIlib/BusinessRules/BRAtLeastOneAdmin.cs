/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenusAPIlib.Model;

using MenusAPI;

namespace MenusAPIlib.BusinessRules
{
    public class BRAtLeastOneAdmin : IGlobalBusinessRule
    {
        public bool IsRespectedBy(MenusEntityModelContainer context, BusinessContext businessContext)
        {
            var deletedRolesIds = businessContext.DeletedObjects.OfType<UserRoleBO>().Select(obj => businessContext[obj].DALObjectID);
            var deletedUsersIds = businessContext.DeletedObjects.OfType<UserBO>().Select(obj => businessContext[obj].DALObjectID);

            var result = context.UserRoles
                .Any(role => role.IsAdmin 
                && !deletedRolesIds.Contains(role.Id) 
                && role.Users.Any(user => !deletedUsersIds.Contains(user.Id)));

            return 
                result;
        }
    }
}
