using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Text;

namespace WPP4DotNet.Utils
{
    internal class Functions
    {
        internal static string EncodeBase64(string stringtoencode)
        {
            byte[] data = Encoding.ASCII.GetBytes(stringtoencode);
            return Convert.ToBase64String(data);
        }
        internal static string DecodeBase64(string StringtoDecode)
        {
            byte[] data = Convert.FromBase64String(StringtoDecode);
            return ASCIIEncoding.ASCII.GetString(data);
        }
        internal string MD5(string input)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
        internal byte[] DownloadImageByte(string url)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    return webClient.DownloadData(url);
                }
            }
            catch (Exception)
            {

                return null;
            }
        }
        internal string DownloadImage(string url, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string file = path + Guid.NewGuid().ToString("N") + ".png";
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(url);
                using (MemoryStream mem = new MemoryStream(data))
                {
                    using (var img = Image.FromStream(mem))
                    {
                        img.Save(file, ImageFormat.Png);
                    }
                    mem.Dispose();
                }
                webClient.Dispose();
            }
            return file;
        }
        internal string ClearNumber(string number)
        {
            number = number.Replace("+", "").Trim();
            number = number.Replace("-", "");
            number = number.Replace("(", "");
            number = number.Replace(")", "");
            number = number.Replace(" ", "");
            number = number.Replace("/", "");
            number = number.Replace(@"\", "");
            return number.Trim();
        }
        internal string ConvertFileToBase64(string fileName)
        {
            string Extension = Path.GetExtension(fileName);
            string MimeType;
            switch (Extension.ToLower())
            {
                case ".jpg":
                    MimeType = "data:image/jpg;base64,";
                    break;
                case ".jpeg":
                    MimeType = "data:image/jpeg;base64,";
                    break;
                case ".gif":
                    MimeType = "data:image/gif;base64,";
                    break;
                case ".png":
                    MimeType = "data:image/png;base64,";
                    break;
                case ".bmp":
                    MimeType = "data:image/bmp;base64,";
                    break;
                case ".ico":
                    MimeType = "data:image/x-icon;base64,";
                    break;
                case ".pdf":
                    MimeType = "data:application/pdf;base64,";
                    break;
                case ".mp3":
                    MimeType = "data:audio/mp3;base64,";
                    break;
                case ".mp4":
                    MimeType = "data:video/mp4;base64,";
                    break;
                case ".mpeg":
                    MimeType = "data:application/mpeg;base64,";
                    break;
                case ".txt":
                    MimeType = "data:text/plain;base64,";
                    break;
                default:
                    MimeType = "data:application/octet-stream;base64,";
                    break;
            }
            return MimeType + Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
        }
    }
}
