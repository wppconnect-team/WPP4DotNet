using OpenQA.Selenium.Firefox;
using System.IO;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace WPP4DotNet.WebDriver
{
    public class FirefoxWebApp : IWpp
    {
        /// <summary>
        /// 
        /// </summary>
        FirefoxOptions FirefoxOpt;

        /// <summary>
        /// 
        /// </summary>
        public FirefoxWebApp()
        {
            new DriverManager().SetUpDriver(new FirefoxConfig());
            FirefoxOpt = new FirefoxOptions();
            //FirefoxOpt.AddArguments("--window-size=800,600");
            //FirefoxOpt.AddArguments("--disable-infobars");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="hidden"></param>
        public override void StartSession(string cache = "", bool hidden = true)
        {
            CheckDriverStarted();
            var driverService = FirefoxDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            if (!string.IsNullOrEmpty(cache))
            {
                if (Directory.Exists(cache))
                {
                    FirefoxOpt.Profile = new FirefoxProfile(cache, false);
                }
                else
                {
                    FirefoxOpt = new FirefoxOptions();
                    FirefoxOpt.AddArguments("-profile", cache);
                }
            }
            var drive = new FirefoxDriver(driverService, FirefoxOpt);
            if (!string.IsNullOrEmpty(cache))
            {
                drive.Navigate().GoToUrl("https://web.whatsapp.com/");
            }
            if (hidden)
            {
                drive.Manage().Window.Position = new System.Drawing.Point(-10000, -10000);
            }
            base.StartSession(drive);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void AddExtensao(string path)
        {
            CheckDriverStarted();
            FirefoxOpt.Profile.AddExtension(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void AddArgumentoInicial(params string[] args)
        {
            CheckDriverStarted();
            FirefoxOpt.AddArguments(args);
        }

    }
}
