using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Fonts;
using System.IO;

namespace WIS
{
    public class FontResolver : IFontResolver
    {
        // Кэш загруженных шрифтов
        private Dictionary<string, byte[]> fontData = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        public FontResolver()
        {
            LoadFont("Times New Roman", "times.ttf");       // обычный
            LoadFont("Times New Roman Bold", "timesbd.ttf"); // жирный
            LoadFont("Times New Roman Italic", "timesi.ttf"); // курсив
            LoadFont("Times New Roman Bold Italic", "timesbi.ttf"); // жирный курсив
        }

        private void LoadFont(string fontName, string fileName)
        {
            var fontsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            var path = Path.Combine(fontsFolder, fileName);
            if (File.Exists(path))
            {
                fontData[fontName] = File.ReadAllBytes(path);
            }
        }

        public byte[] GetFont(string faceName)
        {
            if (fontData.TryGetValue(faceName, out var data))
                return data;
            throw new InvalidOperationException($"Font '{faceName}' not found.");
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Times New Roman", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold && isItalic)
                    return new FontResolverInfo("Times New Roman Bold Italic");
                if (isBold)
                    return new FontResolverInfo("Times New Roman Bold");
                if (isItalic)
                    return new FontResolverInfo("Times New Roman Italic");
                return new FontResolverInfo("Times New Roman");
            }

            // Можно добавить другие шрифты или вернуть дефолтный
            return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
        }
    }
}
