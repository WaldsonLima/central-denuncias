using System.Text.RegularExpressions;

namespace CentralDenuncias.Services
{
    public class DenunciaService
    {
        public static string ExtractFileId(string url)
        {
            var match = Regex.Match(url, @"/d/([a-zA-Z0-9_-]+)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

    }
}
