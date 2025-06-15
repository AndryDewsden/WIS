using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WIS
{
    public class PdfPageEvents : PdfPageEventHelper
    {
        private PdfContentByte cb;
        private BaseFont bf = null;

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                // Используем системный шрифт с поддержкой кириллицы
                string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "times.ttf");
                bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                cb = writer.DirectContent;
            }
            catch (DocumentException) { }
            catch (IOException) { }
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            int pageN = writer.PageNumber;
            string text = "Страница " + pageN;
            float len = bf.GetWidthPoint(text, 8);
            Rectangle pageSize = document.PageSize;
            cb.BeginText();
            cb.SetFontAndSize(bf, 8);
            cb.SetTextMatrix(pageSize.GetRight(100), pageSize.GetBottom(30));
            cb.ShowText(text);
            cb.EndText();
        }
    }
}
