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
        [TestMethod]
        public void GeneratePdfTest()
        {
           var list = new List<ShortArticle>()
            {
                new ShortArticle(Guid.NewGuid(), "blub: Haben Politiker wirklich das Recht, über die Bildung von Studenten zu entscheiden? Und ist es gerechtfertigt, für den Diselskandal die genannten Studenten verantwortlich zu machen?",
                    "I Bims auf Malle und mir fällt kein text ein. Das ist aber kein Problem 12 3", "link.de", "imageLink.de",
                    DateTime.Now, DateTime.Now, Guid.NewGuid()),

                new ShortArticle(Guid.NewGuid(), "traktor: tucker tucker tucker.... TUT TUT! Oh da war eine Eìse` Bahn. Uff de schwäbsche Eisebähnle wollt a mol a kerle fahre....",
                    "I Bims auf Malle und mir fällt kein text ein. Haben Politiker wirklich das Recht, über die Bildung von Studenten zu entscheiden? Und ist es gerechtfertigt, für den Diselskandal die genannten Studenten verantwortlich zu machen?", "link.de", "imageLink.de",
                    DateTime.Now, DateTime.Now, Guid.NewGuid()),
                 new ShortArticle(Guid.NewGuid(), "bus: Fahrer, lieber bus fahrer, ...",
                    "I Bims auf Malle und mir fällt kein text ein. Zeilenumbruch? Gibts nicht im PDFSharper. Müll - dreckiger Müll.", "link.de", "imageLink.de",
                    DateTime.Now, DateTime.Now, Guid.NewGuid()),

                new ShortArticle(Guid.NewGuid(), "random: Got it. dunnow how",
                    "I Bims auf Malle und mir fällt kein text ein. Easy, calculated. NEUES YOUTUBE VIDEO: How to counter PDFSharper - NEW META?!", "link.de", "imageLink.de",
                    DateTime.Now, DateTime.Now, Guid.NewGuid()),
            };

           // Delete existing file with same name (pdf.pdf)
           if (System.IO.File.Exists(
               @"C: \Users\Selina\Documents\GitHub\JackTheClipper_DEV\implementation\release_v0.3\EndpointService\Test\pdf.pdf"))
           {
               System.IO.File.Delete(
                   @"C: \Users\Selina\Documents\GitHub\JackTheClipper_DEV\implementation\release_v0.3\EndpointService\Test\pdf.pdf");
           }

            var userName = "Gustav Grantelbart";
            var stream = PdfGeneratorBAD.GeneratePdf(list, userName);

            Assert.IsNotNull(stream);

            string pdf;
            using (var reader = new StreamReader(stream))
            {
                pdf = reader.ReadToEnd();
            }
            Assert.IsNotNull(pdf);
            
            Assert.IsTrue(true);
        }
    }

    
}
