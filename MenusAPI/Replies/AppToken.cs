/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public class AppToken
    {
        [DataMember]
        public int TokenId { get; set; }

        [DataMember]
        public DateTime ExpiryMomentServer { get; set; }

        public override int GetHashCode()
        {
            return TokenId.GetHashCode();
        }
    }
}
