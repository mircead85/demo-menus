/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using MenusAPI;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace MenusWebsite.Controllers
{
    public class CustomTrivialIHttpActionResult : IHttpActionResult
    {
        protected HttpResponseMessage Result { get; set; }

        public CustomTrivialIHttpActionResult(HttpResponseMessage result)
        {
            Result = result;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task<HttpResponseMessage>.FromResult<HttpResponseMessage>(Result);
        }
    }
}