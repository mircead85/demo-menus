/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

using MenusAPI;
using System.Net;

namespace MenusWebsite.Controllers
{
    [DataContract]
    [KnownType(typeof(APIReply))]
    [KnownType(typeof(ReadObjectsReply))]
    [KnownType(typeof(CUDOperationsReply))]
    public class WebRequestReply
    {
        [DataMember]
        public bool HasError { get; set; }
        [DataMember]
        public int HttpStatusCode { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public object Result { get; set; }


        public WebRequestReply(HttpStatusCode httpStatusCode, bool bHasError, string message, object result)
        {
            HttpStatusCode = (int)httpStatusCode;
            HasError = bHasError;
            Message = message;
            Result = result;
        }
    }
}