/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPIlib
{
    public interface IService
    {
        bool IsStarted { get; }

        bool Start();
        void Stop();
    }
}
