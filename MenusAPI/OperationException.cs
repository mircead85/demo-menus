/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    public class OperationException : WebReadyException
    {
        public OperationException(HttpStatusCode webErrorCode, string msg)
            : base(webErrorCode, msg)
        {

        }

        public OperationException(HttpStatusCode webErrorCode, string msg, Exception innerException)
            : base(webErrorCode, msg, innerException)
        {

        }
    }
}
