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
        /// <param name="hidden"></param>
        /// <param name="path"></param>
        public EdgeWebApp(bool hidden = true, string path = "")
        {
            new DriverManager().SetUpDriver(new EdgeConfig());
            EdgeOpt = new EdgeOptions();
            if (!string.IsNullOrEmpty(path))
            {
                EdgeOpt.AddAdditionalEdgeOption("cache", path);
            }
            if (hidden)
            {
                EdgeOpt.AddArguments("--headless");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void StartSession()
        {
            CheckDriverStarted();
            var driverService = EdgeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            var drive = new EdgeDriver(driverService, EdgeOpt);
            base.StartSession(drive);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void AddExtensao(string path)
        {
            CheckDriverStarted();
            EdgeOpt.AddAdditionalEdgeOption("path", path);
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
            EdgeOpt.AddAdditionalEdgeOption("init", arg);
        }
    }
}
