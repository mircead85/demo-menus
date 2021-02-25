/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    public class WebReadyException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; protected set; }


        public WebReadyException(HttpStatusCode webErrorCode, string msg) : base(msg)
        {
            HttpStatusCode = webErrorCode;
        }

        public WebReadyException(HttpStatusCode webErrorCode, string msg, Exception innerException) : base(msg, innerException)
        {
            HttpStatusCode = webErrorCode;
        }
    }
}
