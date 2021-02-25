/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    public enum APIReplyTypes
    {
        Base = 0,
        Other = 1,
        ReadObjects,
        CUDObjects
    }

    [DataContract]
    [KnownType(typeof(FaultDetails))]
    public class APIReply
    {
        [IgnoreDataMember]
        public virtual APIReplyTypes TypeOfThisReply => APIReplyTypes.Base;

        [DataMember]
        public FaultException<FaultDetails> Error { get; set; }
    }
}
