/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    [DataContract]
    [KnownType(typeof(HttpStatusCode))]
    public class FaultDetails
    {
        [DataMember]
        public string ExceptionType { get; set; }

        [DataMember]
        public string ExceptionMessage { get; set; }

        [DataMember]
        public string ExceptionToString { get; set; }

        [DataMember]
        public int HttpStatusCode { get; set; }
    }
}
