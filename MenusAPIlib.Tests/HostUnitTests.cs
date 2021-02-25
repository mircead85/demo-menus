/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MenusAPIlib.Tests
{
    [TestClass]
    public class HostUnitTests
    {
        MenusServiceHost host;

        [TestInitialize]
        public void SetUp()
        {
            host = new MenusServiceHost();
        }

        [TestCleanup]
        public void CLeanup()
        {
            if (host != null)
                host.Stop();

            host = null;
        }

        [TestMethod]
        public void StartUpTest()
        {
            host.Start();
        }

    }
}
