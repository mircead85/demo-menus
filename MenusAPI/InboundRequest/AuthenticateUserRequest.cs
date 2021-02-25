/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public class AuthenticateUserRequest : APIInboundRequest
    {
        public override UserRequestTypes TypeOfThisRequest
        {
            get
            {
                return UserRequestTypes.AuthenticateUser;
            }
        }

        [DataMember]
        public bool GetSatteliteData { get; set; }

        public AuthenticateUserRequest(UserCredentials credentials, bool getSatteliteData = false)
        {
            if ((credentials.UserName == null || credentials.Password == null) && !credentials.TokenID.HasValue)
                throw new ArgumentException("Credentials must be provided.");
            
            this.RequestorCredentials = credentials;
            GetSatteliteData = getSatteliteData;
        }
    }
}
