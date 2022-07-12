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
            ChromeOpt.AddArguments("--log-level=3");
            ChromeOpt.AddArguments("--no-default-browser-check");
            ChromeOpt.AddArguments("--disable-site-isolation-trials");
            ChromeOpt.AddArguments("--no-experiments");
            ChromeOpt.AddArguments("--ignore-gpu-blacklist");
            ChromeOpt.AddArguments("--ignore-ssl-errors");
            ChromeOpt.AddArguments("--ignore-certificate-errors");
            ChromeOpt.AddArguments("--ignore-certificate-errors-spki-list");
            ChromeOpt.AddArguments("--disable-gpu");
            ChromeOpt.AddArguments("--disable-extensions");
            ChromeOpt.AddArguments("--disable-default-apps");
            ChromeOpt.AddArguments("--enable-features=NetworkService");
            ChromeOpt.AddArguments("--disable-setuid-sandbox");
            ChromeOpt.AddArguments("--no-sandbox");
            ChromeOpt.AddArguments("--disable-webgl");
            ChromeOpt.AddArguments("--disable-threaded-animation");
            ChromeOpt.AddArguments("--disable-threaded-scrolling");
            ChromeOpt.AddArguments("--disable-in-process-stack-traces");
            ChromeOpt.AddArguments("--disable-histogram-customizer");
            ChromeOpt.AddArguments("--disable-gl-extensions");
            ChromeOpt.AddArguments("--disable-composited-antialiasing");
            ChromeOpt.AddArguments("--disable-canvas-aa");
            ChromeOpt.AddArguments("--disable-3d-apis");
            ChromeOpt.AddArguments("--disable-accelerated-2d-canvas");
            ChromeOpt.AddArguments("--disable-accelerated-jpeg-decoding");
            ChromeOpt.AddArguments("--disable-accelerated-mjpeg-decode");
            ChromeOpt.AddArguments("--disable-app-list-dismiss-on-blur");
            ChromeOpt.AddArguments("--disable-accelerated-video-decode");
            ChromeOpt.AddArguments("--disable-infobars");
            ChromeOpt.AddArguments("--ignore-certifcate-errors");
            ChromeOpt.AddArguments("--ignore-certifcate-errors-spki-list");
            ChromeOpt.AddArguments("--disable-dev-shm-usage");
            ChromeOpt.AddArguments("--disable-gl-drawing-for-tests");
            ChromeOpt.AddArguments("--incognito");
            ChromeOpt.AddArguments("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36");
            ChromeOpt.AddArguments("--disable-web-security");
            ChromeOpt.AddArguments("--aggressive-cache-discard");
            ChromeOpt.AddArguments("--disable-cache");
            ChromeOpt.AddArguments("--disable-application-cache");
            ChromeOpt.AddArguments("--disable-offline-load-stale-cache");
            ChromeOpt.AddArguments("--disk-cache-size=0");
            ChromeOpt.AddArguments("--disable-background-networking");
            ChromeOpt.AddArguments("--disable-sync");
            ChromeOpt.AddArguments("--disable-translate");
            ChromeOpt.AddArguments("--hide-scrollbars");
            ChromeOpt.AddArguments("--metrics-recording-only");
            ChromeOpt.AddArguments("--mute-audio");
            ChromeOpt.AddArguments("--no-first-run");
            ChromeOpt.AddArguments("--safebrowsing-disable-auto-update");
            ChromeOpt.AddArguments("--no-zygote");
            ChromeOpt.AddArguments("--window-size=800,600");
            ChromeOpt.AddExcludedArgument("disable-notifications");
            ChromeOpt.AddExcludedArgument("enable-automation");
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
