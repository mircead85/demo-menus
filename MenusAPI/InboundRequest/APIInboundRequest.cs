/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public enum UserRequestTypes
    {
        Base = 0,
        Other = 1,
        ResetDatabase,
        IsDatabaseUp,
        AuthenticateUser,
        CUDUsers,
        CUDMenus,
        CUDRoles,
        ReadMenus,
        ReadUsers,
        ReadRoles,
        ReadLog,
        ClearLog,
        CreateNewAccount,
        LogoutUser
    }

    [DataContract]
    public class APIInboundRequest
    {
        [DataMember]
        public UserCredentials RequestorCredentials { get; set; }

        [IgnoreDataMember]
        public virtual UserRequestTypes TypeOfThisRequest => UserRequestTypes.Base;
    }
}
