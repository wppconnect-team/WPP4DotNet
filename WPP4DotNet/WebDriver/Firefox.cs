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
        /// <param name="hidden"></param>
        /// <param name="path"></param>
        public FirefoxWebApp(bool hidden = true, string path = "")
        {
            new DriverManager().SetUpDriver(new FirefoxConfig());
            FirefoxOpt = new FirefoxOptions();
            //FirefoxOpt.AddArguments("--window-size=800,600");
            //FirefoxOpt.AddArguments("--disable-infobars");
            if (!string.IsNullOrEmpty(path))
            {
                if (Directory.Exists(path))
                {
                    FirefoxOpt.Profile = new FirefoxProfile(path, false);
                }
                else
                {
                    FirefoxOpt = new FirefoxOptions();
                    FirefoxOpt.AddArguments("-profile", path);
                }
            }
            if (hidden)
            {
                FirefoxOpt.AddArguments("--headless");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void StartSession()
        {
            CheckDriverStarted();
            var driverService = FirefoxDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            var drive = new FirefoxDriver(driverService, FirefoxOpt);
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
