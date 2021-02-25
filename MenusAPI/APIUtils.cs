/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

namespace MenusAPI
{
    public static class APIUtils
    {
        public static readonly int MaxUsernameLength = 30;
        public static readonly int MaxPasswordLength = 3000;
        public static readonly int MaxNamesLength = 100;
        public static readonly string RegularRoleName = "Regular";
        public static readonly string DefaultAdminRoleName = "Admin";

        static APIUtils()
        {
            AppSettingsReader settingsReader = new AppSettingsReader();
        }
    }
}
