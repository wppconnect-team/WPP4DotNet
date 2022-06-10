using System.Collections.Generic;
using System.IO;

namespace WPP4DotNet.WebDriver
{
    public static class Extensoes
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inp"></param>
        /// <returns></returns>
        public static string EscreverWhatsAppTexto(this string inp)
        {
            return inp.Replace("\n", OpenQA.Selenium.Keys.Shift + OpenQA.Selenium.Keys.Enter + OpenQA.Selenium.Keys.LeftShift)
                .Replace(':', (char)0xFF1A);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToWrite"></param>
        /// <param name="filePath"></param>
        /// <param name="append"></param>
        public static void WriteBinaryFile<T>(this T objectToWrite, string filePath, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, objectToWrite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T ReadBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
                return (T)new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Cookies"></param>
        /// <param name="driver"></param>
        public static void UploadCookies(this IReadOnlyCollection<OpenQA.Selenium.Cookie> Cookies, OpenQA.Selenium.IWebDriver driver)
        {
            foreach (OpenQA.Selenium.Cookie cookie in Cookies)
            {
                driver.Manage().Cookies.AddCookie(cookie);
            }
        }
    }
}
