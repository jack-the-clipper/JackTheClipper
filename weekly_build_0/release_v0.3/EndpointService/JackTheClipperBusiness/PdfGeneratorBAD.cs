using System;
using System.Collections.Generic;
using System.IO;
using JackTheClipperCommon;
using JackTheClipperCommon.SharedClasses;
using PdfSharpCore.Drawing;
using PdfSharpCore.Fonts;
using PdfSharpCore.Pdf;

namespace JackTheClipperBusiness
{
    /// <summary>
    /// See classname
    /// </summary>
    internal static class PdfGeneratorBAD
    {
        private static readonly object lockObj = new object();
        static PdfGeneratorBAD()
        {
            GlobalFontSettings.FontResolver = new FontResolver();
        }

        public static MemoryStream GeneratePdf(List<ShortArticle> content)
        {
            try
            {
                using (new PerfTracer(nameof(GeneratePdf)))
                {
                    using (PdfDocument document = new PdfDocument())
                    {
                        var page = document.AddPage();

                        // Get an XGraphics object for drawing
                        using (var gfx = XGraphics.FromPdfPage(page))
                        {
                            // Create a font
                            XFont bigFont;
                            XFont normalFont;
                            lock (lockObj)
                            {
                                bigFont = new XFont("OpenSans", 14, XFontStyle.BoldItalic);
                                normalFont = new XFont("OpenSans", 11, XFontStyle.Regular);
                            }

                            int y = 10;
                            foreach (var shortArticle in content)
                            {
                                gfx.DrawString(shortArticle.Title + " (" + shortArticle.Published.ToLongDateString() + ")",
                                               bigFont, new XSolidBrush(XColor.FromKnownColor(XKnownColor.Blue)), 0, y);
                                y += (bigFont.Height + 3);
                                gfx.DrawString(shortArticle.ShortText, normalFont,
                                               new XSolidBrush(XColor.FromKnownColor(XKnownColor.Black)), 0, y);
                                y += (normalFont.Height + 3);
                                gfx.DrawString(shortArticle.Link, normalFont,
                                                new XSolidBrush(XColor.FromKnownColor(XKnownColor.Aqua)), 0, y);
                                y += (normalFont.Height + 3);
                            }

                            var stream = new MemoryStream();
                            document.Save(stream);
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
    }

    //From https://github.com/ststeiger/PdfSharpCore
    //TODO: Refacor (Performance)
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