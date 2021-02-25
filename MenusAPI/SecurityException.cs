/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    public class SecurityException : WebReadyException
    {
        public SecurityException(HttpStatusCode webErrorCode, string message)
            : base(webErrorCode, message)
        {

        }
        
        public SecurityException(HttpStatusCode webErrorCode, string message, Exception innerException)
            : base(webErrorCode, message,innerException)
        {
        }
    }
}
