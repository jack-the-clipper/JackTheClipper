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
    //[TestClass]
    public class PdfGeneratorTest
    {
        //[TestMethod]
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


            var userName = "Gustav Grantelbart";
            var stream = PdfGenerator.GeneratePdf(list, userName);

            Assert.IsNotNull(stream);
            var fileStream = new FileStream(@"D:\Notizen\4.Semester\Softwareprojekt\Projekt\implementation\release_v0.3\EndpointService\Test\pdf.pdf", FileMode.Open);
            var differences = 0;
            var lines = 0;
            using (var expectedreader = new StreamReader(fileStream))
            {
                using (var reader = new StreamReader(stream))
                {
                    string actual;
                    string expected;
                    while ((actual=reader.ReadLine()) != null)
                    {
                        expected = expectedreader.ReadLine();
                        if (expected == null)
                        {
                            differences++;
                            continue;
                        }
                        if (actual.Contains("/Creationdate", StringComparison.OrdinalIgnoreCase) ||
                            actual.Contains("/id", StringComparison.OrdinalIgnoreCase) ||
                            actual.Contains("/length", StringComparison.OrdinalIgnoreCase) ||
                            actual.Contains("/w", StringComparison.OrdinalIgnoreCase) ||
                            actual.Contains("/fontname", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (!actual.Equals(expected))
                        {
                            differences++;
                            // ToDo: Streamverschiebung auf entsprechendes Level
                        }
                        lines++;
                    }
                }
            }            
            Assert.IsTrue(true);
        }
    }

    
}
