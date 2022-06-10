﻿using OpenQA.Selenium.Chrome;
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
        public ChromeWebApp()
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            ChromeOpt = new ChromeOptions() { LeaveBrowserRunning = false };
            ChromeOpt.AddArguments("--window-size=800,600");
            ChromeOpt.AddExcludedArgument("disable-notifications");
            ChromeOpt.AddExcludedArgument("enable-automation");
            //ChromeOpt.AddAdditionalCapability("useAutomationExtension", capabilityValue: false);
            ChromeOpt.AddArguments("--hide-scrollbars");
            //ChromeOpt.AddArguments("--headless");
            ChromeOpt.AddArguments("--disable-gpu");
            ChromeOpt.AddArguments("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.114 Safari/537.36");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="hidden"></param>
        public override void StartSession(string cache = "", bool hidden = true)
        {
            CheckDriverStarted();
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            if (!string.IsNullOrEmpty(cache))
            {
                ChromeOpt.AddArguments("user-data-dir=" + cache);
            }
            var drive = new ChromeDriver(driverService, ChromeOpt);
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
