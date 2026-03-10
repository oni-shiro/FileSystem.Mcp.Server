using System.Text;
using UglyToad.PdfPig;

namespace FileSystem.Mcp.Server.Services;

internal class PdfReader : IFileReader
{
    public string ReadPdf(string path)
    {
        var text = new StringBuilder();
        text.AppendLine($"# Title: {Path.GetFileName(path)}");
        using (PdfDocument document = PdfDocument.Open(path))
        {
            foreach (var page in document.GetPages())
            {
                text.AppendLine($"## Page Number: {page.Number}");
                text.AppendLine(page.Text);
            }

            return text.ToString();
        }
    }
}