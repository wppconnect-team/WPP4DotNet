using OpenQA.Selenium.Chrome;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace WPP4DotNet.WebDriver
{
    public class ChromeWebApp : IWpp
    {
        /// <summary>
        /// 
        /// </summary>
        ChromeOptions ChromeOpt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hidden"></param>
        /// <param name="path"></param>
        public ChromeWebApp(bool hidden = true, string path = "")
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            ChromeOpt = new ChromeOptions() { LeaveBrowserRunning = false };
            ChromeOpt.AddArguments("--window-size=800,600");
            ChromeOpt.AddExcludedArgument("disable-notifications");
            ChromeOpt.AddExcludedArgument("enable-automation");
            ChromeOpt.AddArguments("--hide-scrollbars");
            ChromeOpt.AddArguments("--disable-gpu");
            ChromeOpt.AddArguments("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36");
            if (!string.IsNullOrEmpty(path))
            {
                ChromeOpt.AddArguments("user-data-dir=" + path);
            }
            if (hidden)
            {
                ChromeOpt.AddArguments("--headless");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void StartSession()
        {
            CheckDriverStarted();
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
   
            var drive = new ChromeDriver(driverService, ChromeOpt);

            base.StartSession(drive);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public void AddExtensao(string path)
        {
            CheckDriverStarted();
            ChromeOpt.AddExtension(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="base64"></param>
        public void AddExtensaoBase64(string base64)
        {
            CheckDriverStarted();
            ChromeOpt.AddEncodedExtension(base64);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arg"></param>
        public void AddArgumentoInicial(params string[] arg)
        {
            CheckDriverStarted();
            ChromeOpt.AddArguments(arg);
        }
    }
}
