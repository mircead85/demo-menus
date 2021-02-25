/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public class UserBO : BusinessObject
    {
        [DataMember]
        public int? UserID { get; set; }
        
        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public int? ExpectedNumCalories { get; set; }

        [DataMember]
        public UserCredentials UserCredentials { get; protected set; }

        [DataMember]
        public virtual List<UserRoleBO> UserRoles { get; set; }
        
        [DataMember]
        public virtual List<MenuEntryBO> Menus { get; set; }

        public UserBO():base()
        {
            UserRoles = new List<UserRoleBO>();
            Menus = new List<MenuEntryBO>();
        }

        public UserBO(UserCredentials userCredentials):this()
        {
            UserCredentials = new UserCredentials(userCredentials);
        }

        public UserBO(int? userId, string userName = null, string userPassword = null, string displayName = null)
            : this()
        {
            UserID = userId;
            DisplayName = displayName;
            UserCredentials = new UserCredentials(userName, userPassword);
        }
    }
}
