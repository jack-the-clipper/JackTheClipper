using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JackTheClipperBusiness;
using JackTheClipperCommon;
using JackTheClipperCommon.SharedClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace JackTheClipperBusinessTest
{
    [TestClass]
    public class PdfGeneratorTest
    {
        //[TestMethod]
        public void GeneratePdfTest()
        {
            var list = new List<ShortArticle>()
            {
                new ShortArticle(Guid.NewGuid(), "blub",
                    "I Bims auf Malle und mir fällt kein text ein", "link.de", "imageLink.de",
                    DateTime.Now, DateTime.Now, Guid.NewGuid()),

                new ShortArticle(Guid.NewGuid(), "traktor",
                    "I Bims auf Malle und mir fällt kein text ein", "link.de", "imageLink.de",
                    DateTime.Now, DateTime.Now, Guid.NewGuid()),

                new ShortArticle(Guid.NewGuid(), "böller",
                    "I Bims auf Malle und mir fällt kein text ein", "link.de", "imageLink.de",
                    DateTime.Now, DateTime.Now, Guid.NewGuid()),

                new ShortArticle(Guid.NewGuid(), "bus",
                    "I Bims auf Malle und mir fällt kein text ein", "link.de", "imageLink.de",
                    DateTime.Now, DateTime.Now, Guid.NewGuid()),

                new ShortArticle(Guid.NewGuid(), "random",
                    "I Bims auf Malle und mir fällt kein text ein", "link.de", "imageLink.de", 
                    DateTime.Now, DateTime.Now, Guid.NewGuid()),

            };

            var stream = new MemoryStream(); //PdfGeneratorBAD.GeneratePdf(list));

            Assert.IsNotNull(stream);

            string pdf;
            using (var reader = new StreamReader(stream))
            {
                pdf = reader.ReadToEnd();
            }

            Assert.IsNotNull(pdf);

            Assert.IsTrue(pdf.Contains("blub"));
        }
    }

    
}
