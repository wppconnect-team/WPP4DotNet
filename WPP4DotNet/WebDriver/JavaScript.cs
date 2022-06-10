using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace WPP4DotNet.WebDriver
{
    public class JavaScript
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="script"></param>
        /// <param name="args"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="script"></param>
        /// <param name="args"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="script"></param>
        /// <param name="args"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="script"></param>
        /// <param name="args"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="script"></param>
        /// <param name="args"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="script"></param>
        /// <param name="igual"></param>
        /// <param name="args"></param>
        /// <returns></returns>
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
