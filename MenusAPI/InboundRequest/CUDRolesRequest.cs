/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public class CUDRolesRequest : CUDRequest
    {
        public override UserRequestTypes TypeOfThisRequest
        {
            get
            {
                return UserRequestTypes.CUDRoles;
            }
        }

        [IgnoreDataMember]
        public IEnumerable<UserRoleBO> RolesToCUD
        {
            get
            {
                return ObjectsToCUD.OfType<UserRoleBO>();
            }
        }
    }
}
