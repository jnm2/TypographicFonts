using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace jnm2.TypographicFonts
{
    partial class TypographicFont
    {
        // See http://www.microsoft.com/typography/otspec/otff.htm

        private static class FontReader
        {
            public static TypographicFont[] Read(string filename)
            {
                // Do not check the file extension. Files are often misnamed. Detect by contents.

                using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
                using (var br = new BigEndianBinaryReader(file))
                {
                    if (Encoding.ASCII.GetString(br.ReadBytes(4)) == "ttcf")
                    {
                        // Font collection detected.
                        
                        // This check is probably not necessary, since future versions are probably going to be back-compatible
                        var version = br.ReadUInt32();
                        // const uint knownVersion1 = 0x00010000, knownVersion2 = 0x00020000;
                        // if (version != knownVersion1 && version != knownVersion2) return new TypographicFont[0];

                        var numContainedFonts = br.ReadUInt32();
                        var fontStreamOffsets = new uint[numContainedFonts];
                        for (var fi = 0; fi < fontStreamOffsets.Length; fi++)
                            fontStreamOffsets[fi] = br.ReadUInt32();


                        var r = new List<TypographicFont>();

                        for (var fi = 0; fi < fontStreamOffsets.Length; fi++)
                        {
                            var font = ParseOpenTypeStream(br, fontStreamOffsets[fi], filename);
                            if (font != null) r.Add(font);
                        }

                        return r.ToArray();
                    }
                    else
                    {
                        // No collection detected, assume this is an OpenType stream.

                        var font = ParseOpenTypeStream(br, 0, filename);
                        return font == null ? new TypographicFont[0] : new[] { font };
                    }
                }
            }

            
            private static TypographicFont ParseOpenTypeStream(BigEndianBinaryReader br, long streamOffset, string filename)
            {
                br.BaseStream.Seek(streamOffset, SeekOrigin.Begin);

                // This version check is crucial, since there is no tag at the beginning of the stream.
                // If it doesn't match either of the known spec version IDs, it's probably a FON file which we don't parse.
                var version = br.ReadUInt32();
                const uint trueTypeOutlinesVersion = 0x00010000;
                const uint ccfVersion = 0x4F54544F; // "OTTO"
                if (version != trueTypeOutlinesVersion && version != ccfVersion) return null;

                var numTables = br.ReadUInt16();
                var searchRange = br.ReadUInt16();
                var entrySelector = br.ReadUInt16();
                var rangeShift = br.ReadUInt16();

                var familyNames = default(FamilyNamesInfo);
                var fontStyle = default(FontStyle);
                var weightClass = default(TypographicFontWeight);
                for (var ti = 0; ti < numTables; ti++)
                {
                    var tag = Encoding.ASCII.GetString(br.ReadBytes(4));
                    var checksum = br.ReadUInt32();
                    var offset = br.ReadUInt32();
                    var length = br.ReadUInt32();
                
                    var position = br.BaseStream.Position;
                    switch (tag)
                    {
                        case "name":
                            familyNames = ReadFamilyNames(br, offset);
                            break;
                        case "head":
                            fontStyle = ReadHead(br, offset);
                            break;
                        case "OS/2":
                            weightClass = ReadOS2(br, offset);
                            break;
                    }
                    br.BaseStream.Seek(position, SeekOrigin.Begin);
                }

                if (familyNames.FontName == null) return null;

                return new TypographicFont(
                    familyNames.TypographicFamily,
                    familyNames.TypographicSubfamily == string.Empty ? null : familyNames.TypographicSubfamily,
                    familyNames.FontName,
                    weightClass,
                    (fontStyle & FontStyle.Bold) != 0,
                    (fontStyle & FontStyle.Italic) != 0,
                    (fontStyle & FontStyle.Underline) != 0,
                    (fontStyle & FontStyle.Outline) != 0,
                    (fontStyle & FontStyle.Shadow) != 0,
                    (fontStyle & FontStyle.Condensed) != 0,
                    (fontStyle & FontStyle.Extended) != 0,
                    filename
                );
            }


            private struct FamilyNamesInfo
            {
                public readonly string TypographicFamily;
                public readonly string TypographicSubfamily;
                public readonly string FontName;

                public FamilyNamesInfo(string typographicFamily, string typographicSubfamily, string fontName)
                {
                    TypographicFamily = typographicFamily;
                    TypographicSubfamily = typographicSubfamily;
                    FontName = fontName;
                }
            }

            // http://www.microsoft.com/typography/otspec/name.htm
            private static FamilyNamesInfo ReadFamilyNames(BigEndianBinaryReader br, uint offset)
            {
                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                var fSelector = br.ReadUInt16();
                var numNameRecords = br.ReadUInt16();
                var storageOffset = br.ReadUInt16();
                    
                var fontFamilyName = (string)null;
                var fontSubfamilyName = (string)null;
                var typographicFamilyName = (string)null;
                var typographicSubfamilyName = (string)null;

                for (var ri = 0; ri < numNameRecords; ri++)
                {
                    var platformId = (PlatformId)br.ReadUInt16();
                    var encodingId = br.ReadUInt16();
                    var languageId = br.ReadUInt16();
                    var nameId = (NameId)br.ReadUInt16();
                    var stringLength = br.ReadUInt16();
                    var stringOffset = br.ReadUInt16();
                    
                    // Assume we are on Windows
                    switch (platformId)
                    {
                        case PlatformId.Unicode:
                            break;
                        case PlatformId.Windows:
                            // We only want en-US
                            if (languageId != 1033) continue;
                            break;
                        default:
                            continue;
                    }
                    
                    switch (nameId)
                    {
                        case NameId.TypographicFamilyName:
                        case NameId.TypographicSubfamilyName:
                        case NameId.FontFamilyName:
                        case NameId.FontSubfamilyName:
                            break;
                        default:
                            // Don't bother reading other strings
                            continue;
                    }

                    var position = br.BaseStream.Position;
                    br.BaseStream.Seek(offset + storageOffset + stringOffset, SeekOrigin.Begin);
                    var str = Encoding.BigEndianUnicode.GetString(br.ReadBytes(stringLength));
                    br.BaseStream.Seek(position, SeekOrigin.Begin);
                    
                    switch (nameId)
                    {
                        case NameId.TypographicFamilyName:
                            typographicFamilyName = str;
                            break;
                        case NameId.TypographicSubfamilyName:
                            typographicSubfamilyName = str;
                            break;
                        case NameId.FontFamilyName:
                            fontFamilyName = str;
                            break;
                        case NameId.FontSubfamilyName:
                            fontSubfamilyName = str;
                            break;
                    }
                }

                return typographicFamilyName == null
                    ? new FamilyNamesInfo(fontFamilyName, fontSubfamilyName, fontFamilyName)
                    : new FamilyNamesInfo(typographicFamilyName, typographicSubfamilyName, fontFamilyName);
            }


            // http://www.microsoft.com/typography/otspec/head.htm
            private static FontStyle ReadHead(BigEndianBinaryReader br, uint offset)
            {
                br.BaseStream.Seek(offset + 44, SeekOrigin.Begin);
                return (FontStyle)br.ReadUInt16();
            }


            // http://www.microsoft.com/typography/otspec/os2.htm
            private static TypographicFontWeight ReadOS2(BigEndianBinaryReader br, uint offset)
            {
                br.BaseStream.Seek(offset + 4, SeekOrigin.Begin);
                return (TypographicFontWeight)br.ReadUInt16();
            }


            private enum PlatformId : ushort
            {
                Unicode = 0,
                Macintosh = 1,
                ISO = 2,
                Windows = 3,
                Custom = 4
            }

            [Flags]
            private enum FontStyle : ushort
            {
                Bold = 1 << 0,
                Italic = 1 << 1,
                Underline = 1 << 2,
                Outline = 1 << 3,
                Shadow = 1 << 4,
                Condensed = 1 << 5,
                Extended = 1 << 6
            }

            private enum NameId : ushort
            {
                CopyrightNotice = 0,
                FontFamilyName = 1,
                FontSubfamilyName = 2,
                UniqueFontIdentifier = 3,
                FullFontName = 4,
                Version = 5,
                PostscriptName = 6,
                Trademark = 7,
                ManufacturerName = 8,
                Designer = 9,
                Description = 10,
                VendorUrl = 11,
                DesignerUrl = 12,
                LicenseDescription = 13,
                LicenseInfoUrl = 14,
                TypographicFamilyName = 16,
                TypographicSubfamilyName = 17,
                CompatibleFull = 18,
                SampleText = 19,
                PostScriptCID = 20,
                WWSFamilyName = 21,
                WWSSubfamilyName = 22,
                LightBackgroundPalette = 23,
                DarkBackgroundPalette = 24
            }
        }
    }
}
