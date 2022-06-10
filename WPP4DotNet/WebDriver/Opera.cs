using OpenQA.Selenium.Opera;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace WPP4DotNet.WebDriver
{
    public class OperaWebApp : IWpp
    {
        /// <summary>
        /// 
        /// </summary>
        OperaOptions OperaOpt;

        /// <summary>
        /// 
        /// </summary>
        public OperaWebApp()
        {
            new DriverManager().SetUpDriver(new OperaConfig());
            OperaOpt = new OperaOptions();
            //OperaOpt.AddArguments("--window-size=800,600");
            //OperaOpt.AddArguments("--disable-infobars");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="hidden"></param>
        public override void StartSession(string cache = "", bool hidden = true)
        {
            CheckDriverStarted();
            var driverService = OperaDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            if (!string.IsNullOrEmpty(cache))
            {
                OperaOpt.AddAdditionalCapability("cache", cache);
            }
            var drive = new OperaDriver(driverService, OperaOpt);
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void AddArgumentoInicial(params string[] args)
        {
            CheckDriverStarted();
            OperaOpt.AddArguments(args);
        }
    }
}
