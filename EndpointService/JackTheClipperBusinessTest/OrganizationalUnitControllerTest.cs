using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JackTheClipperBusiness;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using JackTheClipperData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JackTheClipperBusinessTest
{
    [TestClass]
    public class OrganizationalUnitControllerTest
    {
        internal static readonly Guid TestUnit = Guid.Parse("6d64d93a-7cad-11e9-910b-9615dc5f263c");
        internal static readonly Guid Malle = Guid.Parse("d7634aa6-7cad-11e9-910b-9615dc5f263c");
        internal static readonly Guid SystemUnit = Guid.Parse("00000000-BEEF-BEEF-BEEF-000000000000");

        [TestMethod]
        public void GetOrganizationalUnitsTest()
        {
            var orgaController = Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>();

            var userController = Factory.GetControllerInstance<IClipperUserAPI>();

            var user = userController.TryAuthenticateUser("SA", "123", SystemUnit);

            Assert.IsNotNull(user);

            var orga = orgaController.GetOrganizationalUnits(user);

            Assert.IsNotNull(orga);

            Assert.IsTrue(orga.Count > 0);
        }

        [TestMethod]
        public void AddOrganizationalUnitTest()
        {
            var orgaController = Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>();

            var result = orgaController.AddOrganizationalUnit("UnitTest", TestUnit);

            Assert.AreEqual(result.Status, SuccessState.Successful);
        }

        [TestMethod]
        public void DeleteOrganizationalUnitTest()
        {
            var orgaController = Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>();

            var toDelete = Factory.GetControllerInstance<IClipperDatabase>().GetOrganizationalUnitById(TestUnit);
            var x = toDelete.Children.FirstOrDefault(e => e.Name == "UnitTest");

            if (x==null)
            {
                Assert.IsTrue(true);
                return;
            }

            var result = orgaController.DeleteOrganizationalUnit(x.Id);

            Assert.AreEqual(result.Status, SuccessState.Successful);
        }

        [TestMethod]
        public void GetOrganizationalUnitSettingsTest()
        {
            var orgaController = Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>();

            var result = orgaController.GetOrganizationalUnitSettings(Malle);

            if (result == null)
            {
                Assert.IsTrue(true);
                return;
            }

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void SaveOrganizationalUnitSettingsTest()
        {
            var orgaController = Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>();

            var oldSettings = orgaController.GetOrganizationalUnitSettings(Malle);

            if (oldSettings == null)
            {
                Assert.IsTrue(true);
                return;
            }

            var list = oldSettings.BlackList as List<string>;

            var newSettings = new OrganizationalUnitSettings(oldSettings.Id, oldSettings.Feeds, oldSettings.AvailableSources,
                oldSettings.NotificationSettings, oldSettings.NotificationCheckIntervalInMinutes, list);

            var result = orgaController.SaveOrganizationalUnitSettings(newSettings);

            Assert.AreEqual(result.Status, SuccessState.Successful);
        }

        [TestMethod]
        public void UpdateOrganizationalUnitTest()
        {
            var orgaController = Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>();

            var toUpdate = Factory.GetControllerInstance<IClipperDatabase>().GetOrganizationalUnitById(TestUnit);

            var faked = toUpdate.AdminMailAddress+"fake";

            var newSettings = new OrganizationalUnit(toUpdate.Id, toUpdate.Name, toUpdate.IsPrincipalUnit, faked, SystemUnit, toUpdate.SettingsId);

            var result = orgaController.UpdateOrganizationalUnit(newSettings);

            Assert.AreEqual(result.Status, SuccessState.Successful);

            var result2 = orgaController.UpdateOrganizationalUnit(toUpdate);

            Assert.AreEqual(result2.Status, SuccessState.Successful);
        }

        [TestMethod]
        public void SetUserOrganizationalUnits()
        {
            var list = new List<Guid>
            {
                Malle
            };

            var user = Factory.GetControllerInstance<IClipperUserAPI>()
                .TryAuthenticateUser("timmaster121@web.de", "Passwort", TestUnit);
            var result = Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>()
                .SetUserOrganizationalUnits(user, list);

            Assert.AreEqual(SuccessState.Successful, result.Status);
        }
    }
}
