/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MenusAPIlib.Model;
using MenusAPI;

namespace MenusAPIlib
{
    public static class LogEntryBOExtensions
    {
        public static LogEntryBO CreateBusinessObject(this LogEntry logEntryDAL)
        {
            var result = new LogEntryBO();

            result.CredentialsSummary = logEntryDAL.CredentialsSummary;
            result.Details = logEntryDAL.Details;
            result.Message = logEntryDAL.Message;
            result.Moment = logEntryDAL.Moment;

            return result;
        }
    }
}
