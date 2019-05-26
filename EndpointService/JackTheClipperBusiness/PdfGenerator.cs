using System;
using System.Collections.Generic;
using System.IO;
using JackTheClipperCommon;
using JackTheClipperCommon.SharedClasses;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;


namespace JackTheClipperBusiness
{
    /// <summary>
    ///Generates a PDF file.
    /// </summary>
    public static class PdfGenerator
    {

        private static readonly object lockObj = new object();
        static PdfGenerator()
        {
            GlobalFontSettings.FontResolver = new FontResolver();
        }

        /// <summary>
        /// Generates a user specific PDF file with its name and the specific articles.
        /// </summary>
        /// <param name="content">The user specific articles.</param>
        /// <param name="userName">The users full name.</param>
        /// <returns>A (memory) stream build out of the PDF file.</returns>
        public static MemoryStream GeneratePdf(IEnumerable<ShortArticle> content, string userName)
        {

            try
            {
                using (new PerfTracer(nameof(GeneratePdf)))
                {
                    using (PdfDocument document = new PdfDocument())
                    {
                        var page = document.AddPage();

                        using (var gfx = XGraphics.FromPdfPage(page))
                        {
                            // Set position coordinates
                            double y = 30;
                            double x = 50;

                            // Create a font
                            XFont titleFont;
                            XFont bigFont;
                            XFont normalFont;
                            XFont smallFont;

                            // Create a Stringformat
                            XStringFormat format;

                            // Create a Textformatter
                            XTextFormatter tf;

                            lock (lockObj)
                            {
                                titleFont = new XFont("OpenSans", 18, XFontStyle.Bold);
                                bigFont = new XFont("OpenSans", 14, XFontStyle.Bold);
                                normalFont = new XFont("OpenSans", 11, XFontStyle.Regular);
                                smallFont = new XFont("OpenSans", 9, XFontStyle.Bold);
                                format = new XStringFormat();
                                tf = new XTextFormatter(gfx);
                            }

                            // Create header
                            CreateWelcomeTitle(y, x, gfx, userName, titleFont, bigFont, format);
                            y += ((titleFont.Height) * 2);
                            x += 5;

                            // Create each single article
                            foreach (var shortArticle in content)
                            {
                                // Check page size
                                if (CheckIfNewPageIsNeeded(y, page) == true)
                                {
                                    page = document.AddPage(new PdfPage(document));
                                    var gfx2 = XGraphics.FromPdfPage(page);
                                    y = 30;
                                    x = 50;
                                    CreateWelcomeTitle(y, x, gfx2, userName, titleFont, bigFont, format);
                                    tf = new XTextFormatter(gfx2);
                                    y += ((titleFont.Height) * 2);
                                    x += 5;
                                }

                                // Create article title
                                var lineCounter = GetLineCounter(shortArticle.Title, 64.00);

                                tf.DrawString(shortArticle.Title, bigFont,
                                            new XSolidBrush(XColor.FromKnownColor(XKnownColor.Black)),
                                            new XRect(x, y, 490, bigFont.Height * lineCounter));
                                y += ((bigFont.Height) * lineCounter);

                                // Create article date
                                lineCounter = GetLineCounter(shortArticle.Published.ToLongDateString(), 84.00);

                                tf.DrawString(shortArticle.Published.ToLongDateString(), smallFont,
                                    new XSolidBrush(XColor.FromKnownColor(XKnownColor.Gray)),
                                    new XRect(x, y, 490, smallFont.Height * lineCounter));
                                y += (smallFont.Height * lineCounter);

                                // Create short article
                                lineCounter = GetLineCounter(shortArticle.ShortText, 74.00);

                                tf.DrawString(shortArticle.ShortText, normalFont,
                                    new XSolidBrush(XColor.FromKnownColor(XKnownColor.Black)),
                                    new XRect(x, y, 490, normalFont.Height * lineCounter));
                                y += (normalFont.Height * lineCounter);

                                // Create article link
                                lineCounter = GetLineCounter(shortArticle.Link, 74.00);

                                tf.DrawString(shortArticle.Link, normalFont,
                                    new XSolidBrush(XColor.FromKnownColor(XKnownColor.Blue)),
                                    new XRect(x, y, 490, normalFont.Height * lineCounter));
                                y += (bigFont.Height * lineCounter);
                            }

                            var stream = new MemoryStream();
                            document.Save(stream);
                            //document.Save(@"C:\Users\Selina\Documents\GitHub\JackTheClipper_DEV\implementation\release_v0.3\EndpointService\Test\pdf.pdf"); //TODO: For testing, choose path.
                            document.Close();
                            return stream;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Generates a new welcome title.
        /// </summary>
        /// <param name="y">The y position.</param>
        /// <param name="x">The x position.</param>
        /// <param name="gfx">The used graphic object.</param>
        /// <param name="userName">The specific username</param>
        /// <param name="titleFont">The font for the title.</param>
        /// <param name="bigFont">The font for the secondary title.</param>
        /// <param name="format">The used string format.</param>
        private static void CreateWelcomeTitle(double y, double x, XGraphics gfx, string userName, XFont titleFont, XFont bigFont, XStringFormat format)
        {
            // Welcome rectangle
            XRect rectWelcome = new XRect(x, y, 490, 60);
            gfx.DrawRectangle(XPens.Transparent, rectWelcome);

            // Welcome title
            format.LineAlignment = XLineAlignment.Near;
            format.Alignment = XStringAlignment.Center;
            gfx.DrawString($"Hello {userName}!", titleFont, new XSolidBrush(XColor.FromKnownColor(XKnownColor.DarkRed)), rectWelcome, format);
            format.LineAlignment = XLineAlignment.Center;
            gfx.DrawString("Your Jack the Clipper news arrived", bigFont, new XSolidBrush(XColor.FromKnownColor(XKnownColor.Gray)), rectWelcome, format);
        }

        /// <summary>
        /// Checks if a new page is needed.
        /// </summary>
        /// <param name="usedPageHeight">The actual used page size.</param>
        /// <param name="page">Actual page.</param>
        /// <returns>True if new page is needed, false otherwise.</returns>
        private static bool CheckIfNewPageIsNeeded(double usedPageHeight, PdfPage page)
        {
            if (usedPageHeight >= (page.Height - 200))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the number of lines needed.
        /// </summary>
        /// <param name="contentLength">String who should be calculated.</param>
        /// <param name="divisor">Specific number for specific text fonts.</param>
        /// <returns>The calculated number of lines.</returns>
        private static double GetLineCounter(string contentLength, double divisor)
        {
            double division = contentLength.Length / divisor;
            double mod = contentLength.Length % divisor;

            //Get decimal
            double getDecimal = division - Math.Floor(division);

            if (mod != 0 && getDecimal >= 0.9)
            {
                division = Math.Floor(division + 2);
                return division;
            }
            else if (mod != 0 && getDecimal < 0.9)
            {
                division = Math.Floor(division + 1);
                return division;
            }
            return division;
        }
    }

    //From https://github.com/ststeiger/PdfSharpCore
    //This implementation is obviously not very good --> Though it should be enough for everyone to implement their own.
    public class FontResolver : IFontResolver
    {
        private static readonly object lockObj = new object();

        public byte[] GetFont(string faceName)
        {
            using (new PerfTracer(nameof(GetFont)))
            {
                using (var ms = new MemoryStream())
                {
                    lock (lockObj)
                    {
                        using (var fs = File.Open(faceName, FileMode.Open))
                        {
                            fs.CopyTo(ms);
                            ms.Position = 0;
                            return ms.ToArray();
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Generates preset fonts with specific characteristics.
        /// </summary>
        /// <param name="familyName">The name of the font family.</param>
        /// <param name="isBold">If the font is bold or not.</param>
        /// <param name="isItalic">If the font is italic or not.</param>
        /// <returns>A FontResolverInfo with the specific font.</returns>
        public string DefaultFontName => "OpenSans";
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("OpenSans", StringComparison.CurrentCultureIgnoreCase))
            {
                if (isBold && isItalic)
                {
                    return new FontResolverInfo("OpenSans-BoldItalic.ttf");
                }
                else if (isBold)
                {
                    return new FontResolverInfo("OpenSans-Bold.ttf");
                }
                else if (isItalic)
                {
                    return new FontResolverInfo("OpenSans-Italic.ttf");
                }
                else
                {
                    return new FontResolverInfo("OpenSans-Regular.ttf");
                }
            }
            return null;
        }
    }
}