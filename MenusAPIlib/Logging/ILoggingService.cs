/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenusAPI;

namespace MenusAPIlib
{
    public interface ILoggingService : IService
    {
        /// <summary>
        /// Logs the the specified exception.
        /// </summary>
        /// <param name="E"></param>
        /// <param name="requester"></param>
        /// <returns>True if the log operation was successful, false otherwise. Exception are suppressed.</returns>
        bool LogException(Exception Ex, UserCredentials requester = null);
        /// <summary>
        /// Logs the the specified message.
        /// </summary>
        /// <returns>True if the log operation was successful, false otherwise. Exception are suppressed.</returns>
        bool LogMessage(string msg, UserCredentials requester = null);
    }
}
