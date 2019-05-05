using System;
using JackTheClipperBusiness;
using JackTheClipperBusiness.CrawlerManagement;
using JackTheClipperBusiness.Notification;
using JackTheClipperCommon.Interfaces;
using JackTheClipperData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JackTheClipperBusinessTest
{
    [TestClass]
    public class FactoryTest
    {
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
