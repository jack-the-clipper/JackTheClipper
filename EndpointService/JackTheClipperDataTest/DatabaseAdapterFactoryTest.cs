using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.Interfaces;
using JackTheClipperData;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JackTheClipperDataTest
{
    [TestClass]
    public class DatabaseAdapterFactoryTest
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(new FileInfo(@"..\..\..\..\JackTheClipperRequestHandler\appsettings.json").FullName, optional: false)
                .Build();
            AppConfiguration.RegisterConfig(config);
        }

        [TestMethod]
        public void GetControllerTest()
        {
            var firstController = DatabaseAdapterFactory.GetControllerInstance<IIndexerService>();
            Assert.IsNotNull(firstController);

            var secondController = DatabaseAdapterFactory.GetControllerInstance<IClipperDatabase>();
            Assert.IsNotNull(secondController);
        }

        [TestMethod]
        public void InvalidControllerInterface()
        {
            Exception t = null;
            try
            {
                // ReSharper disable once UnusedVariable
                var tmp = DatabaseAdapterFactory.GetControllerInstance<IClipperUserAPI>();
            }
            catch (Exception e)
            {
                t = e;
            }

            Assert.IsTrue(t is ArgumentOutOfRangeException);
        }

    }
}
