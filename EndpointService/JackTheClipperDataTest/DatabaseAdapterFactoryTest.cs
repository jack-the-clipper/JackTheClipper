using System;
using System.Collections.Generic;
using System.Text;
using JackTheClipperCommon.Interfaces;
using JackTheClipperData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JackTheClipperDataTest
{
    [TestClass]
    public class DatabaseAdapterFactoryTest
    {
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
