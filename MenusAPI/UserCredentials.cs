/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace MenusAPI
{
    [DataContract]
    public class UserCredentials
    {
        [IgnoreDataMember]
        protected string _UserName;
        [IgnoreDataMember]
        protected string _Password;

        public UserCredentials(UserCredentials userCredentials)
        {
            _UserName = userCredentials.UserName;
            _Password = userCredentials.Password;
            TokenID = userCredentials.TokenID;
        }


        public UserCredentials(string userName = null, string userPassword = null)
        {
            if (_UserName?.Length > APIUtils.MaxUsernameLength)
                throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Invalid length of username.");
            if(_Password?.Length > APIUtils.MaxPasswordLength)
                throw new OperationException(System.Net.HttpStatusCode.BadRequest, "Invalid length of password.");
            
            _UserName = userName;
            _Password = userPassword;

            if (_UserName != null)
                if (_UserName.StartsWith(":"))
                {
                    int tokenId = -1;
                    if (int.TryParse(_UserName.Substring(1), out tokenId))
                        if (tokenId > 0)
                        {
                            this.TokenID = tokenId;
                        }
                }
        }

        [DataMember]
        public string UserName
        {
            get { return _UserName; }
            set
            {
                if (value != null)
                    if (value.Length > APIUtils.MaxUsernameLength)
                        throw new ArgumentException("Username too long");

                _UserName = value;
            }
        }


        [DataMember]
        public string Password
        {
            get { return _Password; }
            set
            {
                if (value != null)
                    if (value.Length > APIUtils.MaxPasswordLength)
                        throw new ArgumentException("Username too long");

                _Password = value;
            }
        }

        /// <summary>
        /// When set, specifies the ID of a token issued by the server as a result of an authentication request.
        /// </summary>
        /// <remarks>When set, the behaviour is that the specified Username and Password (if any) are ignored, UNLESS the token is invalid in which case the behaviour is as if the token were null.</remarks>
        [DataMember]
        public int? TokenID { get; set; }

        public override string ToString()
        {
            return string.Format("Username: {0}; Password: {1}", UserName ?? "null", "<omitted>" ?? "null");
        }
    }
}
