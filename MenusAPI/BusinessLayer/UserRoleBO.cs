/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public class UserRoleBO : BusinessObject
    {
        [DataMember]
        public int? RoleID { get; set; }
        [DataMember]
        public bool CanCRUDUsers { get; set; }
        [DataMember]
        public bool CanCRUDEntries { get; set; }
        [DataMember]
        public bool IsAdmin { get; set; }
        [DataMember]
        public string RoleName { get; set; }

        public UserRoleBO(int? roleID, string roleName, bool canCRUDUsers = false, bool canCRUDEntries = false, bool isAdmin = false)
        {
            if (roleName != null)
                if (roleName.Length > APIUtils.MaxNamesLength)
                    throw new ArgumentException("Role Name too long.");
            RoleName = roleName;

            CanCRUDUsers = canCRUDUsers;
            CanCRUDEntries = canCRUDEntries;
            IsAdmin = isAdmin;

            RoleID = roleID;
        }
    }
}
