using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace WPP4DotNet.WebDriver
{
    public class JavaScript
    {
        public void Execute(IWebDriver driver, string script, params object[] args)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript(script, args);
            }
            catch (Exception)
            {
            }
        }

        public string ExecuteReturnString(IWebDriver driver, string script, params object[] args)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                return (string)js.ExecuteScript(script, args);
            }
            catch (Exception)
            {
                return "";
            }
        }
        public bool ExecuteReturnBool(IWebDriver driver, string script, params object[] args)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                return (bool)js.ExecuteScript(script, args);
            }
            catch (Exception)
            {
                return false;
            }
        }
        public object ExecuteReturnObj(IWebDriver driver, string script, params object[] args)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                return js.ExecuteScript(script, args);
            }
            catch (Exception)
            {
                return new object();
            }
        }
        public IReadOnlyCollection<object> ExecuteReturnListObj(IWebDriver driver, string script, params object[] args)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                return (IReadOnlyCollection<object>)js.ExecuteScript(script, args);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public bool ExecuteIgual(IWebDriver driver, string script, string igual, params object[] args)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                return js.ExecuteScript(script, args).Equals(igual);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
