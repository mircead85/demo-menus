/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public class LogoutUserRequest : APIInboundRequest
    {
        public override UserRequestTypes TypeOfThisRequest
        {
            get
            {
                return UserRequestTypes.LogoutUser;
            }
        }
        
        /// <summary>
        /// If token based authentication is in use on the server, setting this value to true will cause all user login tokens to be invalidated.
        /// </summary>
        [DataMember]
        public bool LogOutAllTokens { get; set; }

        public LogoutUserRequest(UserCredentials credentials, bool logOutAllTokens = false)
        {
            if ((credentials.UserName == null || credentials.Password == null) && !credentials.TokenID.HasValue)
                throw new ArgumentException("Credentials must be provided.");
            this.RequestorCredentials = credentials;

            this.LogOutAllTokens = logOutAllTokens;
        }

    }
}
