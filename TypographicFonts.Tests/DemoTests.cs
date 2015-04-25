using System;
using System.Drawing;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace jnm2.TypographicFonts.Tests
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void TestGetInfo_SegoeUISemibold_Italic()
        {
            var font = new Font("Segoe UI Semibold", 20, FontStyle.Regular);
            var baseFont = font.GetTypographicFont();

            Assert.AreEqual("Segoe UI", baseFont.Family);
            Assert.AreEqual("Semibold", baseFont.SubFamily);
            Assert.AreEqual("Segoe UI Semibold", baseFont.Name);
            Assert.AreEqual(TypographicFontWeight.Semibold, baseFont.Weight);
            Assert.AreEqual(false, baseFont.Bold);

            // The font is not natively italic; since there is no natively italic font
            // named "Segoe UI Semibold", Windows uses this non-italic font and simulates the italic.
            Assert.AreEqual(false, baseFont.Italic);
        }

        [TestMethod]
        public void TestConvert_SegoeUIItalic_ToSemibold()
        {
            var semiboldVersion = new Font("Segoe UI", 20, FontStyle.Italic).With(TypographicFontWeight.Semibold);

            Assert.AreEqual("Segoe UI Semibold", semiboldVersion.Name);
            Assert.AreEqual(false, semiboldVersion.Bold);

            // This is preserved.
            Assert.AreEqual(true, semiboldVersion.Italic);
        }
    }
}
