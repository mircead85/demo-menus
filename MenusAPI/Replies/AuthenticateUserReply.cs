using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    [KnownType(typeof(AppToken))]
    public class AuthenticateUserReply : ReadObjectsReply
    {
        [DataMember]
        public AppToken SecurityToken { get; set; }
    }
}
