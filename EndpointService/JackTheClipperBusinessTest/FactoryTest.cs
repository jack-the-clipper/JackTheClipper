using System;
using System.IO;
using JackTheClipperBusiness;
using JackTheClipperBusiness.CrawlerManagement;
using JackTheClipperBusiness.Notification;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.Interfaces;
using JackTheClipperData;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JackTheClipperBusinessTest
{
    [TestClass]
    public class FactoryTest
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
            var firstController = Factory.GetControllerInstance<IClipperUserAPI>();
            Assert.IsNotNull(firstController);

            var secondController = Factory.GetControllerInstance<IClipperDatabase>();
            Assert.IsNotNull(secondController);

            var thirdController = Factory.GetControllerInstance<IClipperService>();
            Assert.IsNotNull(thirdController);

            var fourthController = Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>();
            Assert.IsNotNull(fourthController);

            var fifthController = Factory.GetControllerInstance<ICrawlerController>();
            Assert.IsNotNull(fifthController);

            var sixthController = Factory.GetControllerInstance<INotificationController>();
            Assert.IsNotNull(sixthController);
        }

        [TestMethod]
        public void InvalidControllerInterface()
        {
            Exception t = null;
            try
            {
                // ReSharper disable once UnusedVariable
                var tmp = Factory.GetControllerInstance<IIndexerService>();
            }
            catch (Exception e)
            {
                t = e;
            }

            Assert.IsTrue(t is ArgumentOutOfRangeException);
        }
    }
}
