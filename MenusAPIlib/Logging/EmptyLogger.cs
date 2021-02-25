/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenusAPI;

namespace MenusAPIlib.Logging
{
    public class EmptyLogger : Service, ILoggingService
    {
        public bool LogException(Exception Ex, UserCredentials requester = null)
        {
            return true;
        }

        public bool LogMessage(string msg, UserCredentials requester = null)
        {
            return true;
        }
    }
}
