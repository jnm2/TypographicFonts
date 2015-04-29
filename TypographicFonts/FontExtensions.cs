using System;
using System.Drawing;
using System.Linq;

namespace jnm2.TypographicFonts
{
    public static class FontExtensions
    {
        public static Font With(this Font font, TypographicFontWeight weight)
        {
            return font.With(font.SizeInPoints, weight, font.Italic, font.Underline, font.Strikeout);
        }
        public static Font With(this Font font, float size, TypographicFontWeight weight)
        {
            return font.With(size, weight, font.Italic, font.Underline, font.Strikeout);
        }
        public static Font With(this Font font, float size, TypographicFontWeight weight, bool italic)
        {
            return font.With(size, weight, italic, font.Underline, font.Strikeout);
        }
        public static Font With(this Font font, float size, TypographicFontWeight weight, bool italic, bool underline)
        {
            return font.With(size, weight, italic, underline, font.Strikeout);
        }
        public static Font With(this Font font, float size, TypographicFontWeight weight, bool italic, bool underline, bool strikeout)
        {
            var selectedSubfamily = font.GetTypographicFamily().Fonts
                .Where(_ => italic || !_.Italic && underline || !_.Underlined) // Can simulate styles, but can't reverse them
                .SelectMany(_ => _.Bold ? new[]
                {
                    // Bold style is already set, so it cannot be set twice to simulate bolder
                    new { font = _, simulateBold = false, weight = (int)_.Weight }
                
                } : new[]
                {
                    // Bold style could be set. Consider both normal and simulated-bold versions
                    new { font = _, simulateBold = false, weight = (int)_.Weight },
                    new { font = _, simulateBold = true, weight = (int)_.Weight * (int)TypographicFontWeight.Bold / (int)TypographicFontWeight.Normal }
                })
                .OrderBy(_ => Math.Abs(_.weight - (int)weight)) // Get the closest available by weight
                .ThenByDescending(_ => _.font.Italic == italic) // Avoid simulating italic if possible
                .ThenByDescending(_ => _.font.Underlined == underline) // Avoid simulating underline if possible
                .ThenByDescending(_ => _.font.Strikeout == underline) // Avoid simulating strikethrough if possible
                .First();

            var fontStyle = FontStyle.Regular;
            if (selectedSubfamily.font.Bold || selectedSubfamily.simulateBold) 
                fontStyle |= FontStyle.Bold;
            if (selectedSubfamily.font.Italic || italic)
                fontStyle |= FontStyle.Italic;
            if (selectedSubfamily.font.Underlined || underline)
                fontStyle |= FontStyle.Underline;
            if (selectedSubfamily.font.Strikeout || strikeout)
                fontStyle |= FontStyle.Strikeout;

            return selectedSubfamily.font.Name == font.Name && fontStyle == font.Style 
                ? font
                : new Font(selectedSubfamily.font.Name, size, fontStyle);
        }
        
        /// <summary>
        /// Gets the base font that is used. If a style is being simulated in the GDI font, this returns the base font without the simulated style.
        /// </summary>
        public static TypographicFont GetTypographicFont(this Font font)
        {
            return TypographicFontFamily.InstalledFamilies
                .SelectMany(_ => _.Fonts)
                .Where(_ => _.Name == font.Name
                            && (font.Bold || !_.Bold) // If the GDI font doesn't have a style, neither can the base font. If it does have a style, the base might still not have the style- Windows simulates styles.
                            && (font.Italic || !_.Italic)
                            && (font.Underline || !_.Underlined)
                            && (font.Strikeout || !_.Strikeout))
                .OrderByDescending(_ => _.Bold == font.Bold)
                .ThenByDescending(_ => _.Italic == font.Italic)
                .ThenByDescending(_ => _.Underlined == font.Underline)
                .ThenByDescending(_ => _.Strikeout == font.Strikeout)
                .First(); // Get the closest match if there are multiple
        }
        /// <summary>
        /// Gets the installed typographic family of the GDI font.
        /// </summary>
        public static TypographicFontFamily GetTypographicFamily(this Font font)
        {
            foreach (var installedFamily in TypographicFontFamily.InstalledFamilies)
                foreach (var subfamilyFont in installedFamily.Fonts)
                    if (subfamilyFont.Name == font.Name) return installedFamily;

            return null;
        }
    }
}