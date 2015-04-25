using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace jnm2.TypographicFonts
{
    [DebuggerDisplay("{ToString(),nq}")]
    public sealed class TypographicFontFamily
    {
        private static TypographicFontFamilyCollection families;
        public static TypographicFontFamilyCollection InstalledFamilies
        {
            get { return families ?? (families = CreateFamilyList()); }
        }
        
        private static TypographicFontFamilyCollection CreateFamilyList()
        {
            var subfamilesByFont = new Dictionary<string, List<TypographicFont>>();

            foreach (var installedFontFile in TypographicFont.GetInstalledFontFiles())
                foreach (var font in TypographicFont.FromFile(installedFontFile))
                {
                    List<TypographicFont> list;
                    if (!subfamilesByFont.TryGetValue(font.Family, out list))
                        subfamilesByFont.Add(font.Family, list = new List<TypographicFont>());

                    list.Add(font);
                }

            var r = new List<TypographicFontFamily>();

            foreach (var kvp in subfamilesByFont)
            {
                kvp.Value.Sort((a, b) => String.Compare(a.SubFamily, b.SubFamily, StringComparison.OrdinalIgnoreCase));
                r.Add(new TypographicFontFamily(kvp.Key, kvp.Value));
            }

            r.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            return new TypographicFontFamilyCollection(r);
        }


        public TypographicFontFamily(string name, IReadOnlyList<TypographicFont> fonts)
        {
            this.Name = name;
            this.Fonts = fonts;
        }

        /// <summary>
        /// Gets the name of this typographic font family.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a collection of installed fonts that belong to this typographic family.
        /// </summary>
        public IReadOnlyList<TypographicFont> Fonts { get; private set; }

        public override string ToString()
        {
            return this.Name;
        }
    }

    public sealed class TypographicFontFamilyCollection : ReadOnlyKeyedCollection<string, TypographicFontFamily>
    {
        public TypographicFontFamilyCollection(IEnumerable<TypographicFontFamily> initialItems) : base(initialItems, _ => _.Name, StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}