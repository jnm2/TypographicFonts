# TypographicFonts
A .NET abstraction over typographical font families and subfamilies.

### What does this do?

This project was created because the .NET Framework does not have a way to group fonts by typographic family.
I needed to take an unknown font and apply a semibold style to it or the nearest lighter style if semibold
was not available, with a goal of catering to the default system UI font of Segoe UI.

However, System.Drawing has no such facilities. It treats Segoe UI and Segoe UI Semibold as completely different
font families. Microsoft Office has this shortcoming this too. Big deal? Well, Windows' native font picker seems
to magically know to group Semibold along with the other default styles for Segoe UI. There are certainly benefits
to knowing for sure which fonts are available when looking for different font weights in the same font family.

The difference lies in which pair of names you pick: font family name and font subfamily name, or *typographic family
name* and *typographic subfamily name*. (Specs: scroll down to see [Name IDs 1 and 2, 16 and 17](https://www.microsoft.com/typography/otspec/name.htm))
GDI and GDI+ and System.Drawing are all based on the first pair of names, but this results in isolating Segoe UI Semibold
into a single-font family. The system font dialog uses the typographic pair.

Oddly, there is no system API to find the typographic family name that the system dialog uses to group fonts into
families. If you want to be able to group families properly, it is necessary to retrieve a list of installed fonts
and parse the missing typographic family and subfamily from the files. This job turns out to be trivial once you have
[the specs](https://www.microsoft.com/typography/otspec/) in hand.

Thanks to [Todd Main](http://stackoverflow.com/a/3640445/521757) for pioneering.

### Getting started

[Take a look at the demo](https://github.com/jnm2/TypographicFonts/blob/master/TypographicFonts.Tests/DemoTests.cs) to get started. `TypographicFontFamily.InstalledFamilies` or the extension methods on `System.Drawing.Font` are the interesting parts.

Listing all fonts by typographic family:

```c#
foreach (var ff in TypographicFontFamily.InstalledFamilies)
{
    Console.WriteLine(ff.Name);

    foreach (var font in ff.Fonts)
    {
        Console.WriteLine(f.Subfamily);
        // var gdiPlusFont = new Font(f.Name, 16);
    }
}
```

Applying a style to an existing font object:

```c#
var semiboldFont = new Font("Segoe UI", 9).With(TypographicFontWeight.Semibold);
Console.WriteLine(semiboldFont.Name); // "Segoe UI Semibold"
```

You aren't restricted to installed fonts, either. To parse fonts from a font file, use `TypographicFont.FromFile`.

### Support

Start an issue if you have a bug or feature request. PRs welcome!

I took a dependency on System.Drawing, but the only file that needs it is FontExtensions.cs. I can easily make a WPF version of FontExtensions if anyone would use that.

I can see this project could expand to retrieve even more font information, if there was interest. It satisfies the goals of [those who need it now](https://stackoverflow.com/questions/3633000/net-enumerate-winforms-font-styles).
