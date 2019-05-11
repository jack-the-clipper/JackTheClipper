using System;
using System.Collections.Generic;
using System.Text;
using JackTheClipperBusiness;
using JackTheClipperCommon.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JackTheClipperBusinessTest
{
    [TestClass]
    public class OrganizationalUnitControllerTest
    {
        [TestMethod]
        public void GetOrganizationalUnitsTest()
        {
            var orgaController = Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>();

            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("i17029@hb.dhbw-stuttgart.de", "Passwort", Guid.Empty);

            Assert.IsNotNull(user);

            var orga = orgaController.GetOrganizationalUnits(user);

            Assert.IsNotNull(orga);

            Assert.IsTrue(orga.Count > 0);
        }
    }
}
