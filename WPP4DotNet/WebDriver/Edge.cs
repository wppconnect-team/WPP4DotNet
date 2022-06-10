using OpenQA.Selenium.Edge;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace WPP4DotNet.WebDriver
{
    public class EdgeWebApp : IWpp
    {
        /// <summary>
        /// 
        /// </summary>
        EdgeOptions EdgeOpt;

        /// <summary>
        /// 
        /// </summary>
        public EdgeWebApp()
        {
            new DriverManager().SetUpDriver(new EdgeConfig());
            EdgeOpt = new EdgeOptions();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="hidden"></param>
        public override void StartSession(string cache = "", bool hidden = true)
        {
            CheckDriverStarted();
            var driverService = EdgeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            if (!string.IsNullOrEmpty(cache))
            {
                EdgeOpt.AddAdditionalCapability("cache", cache);
            }
            var drive = new EdgeDriver(driverService, EdgeOpt);
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
            EdgeOpt.AddAdditionalCapability("path", path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="base64"></param>
        public void AddExtensaoBase64(string base64)
        {
            CheckDriverStarted();
            //EdgeOpt.AddExtensionPath(base64);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        public void AddArgumentoInicial(params string[] arg)
        {
            CheckDriverStarted();
            EdgeOpt.AddAdditionalCapability("init", arg);
        }
    }
}
