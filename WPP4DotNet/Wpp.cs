using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;
using Polly;
using WPP4DotNet.WebDriver;
using WPP4DotNet.Utils;
using System;
using System.Threading.Tasks;
using System.IO;
using ZXing.QrCode;
using System.Threading;

namespace WPP4DotNet
{
    public abstract class IWpp
    {
        public string WebHook = "";
        public string WppPath = "";

        private JavaScript js = new JavaScript();
        private bool DriverStarted { get; set; }
        private IWebDriver Driver;
        public IWebDriver WebDriver
        {
            get
            {
                if (Driver != null)
                {
                    return Driver;
                }
                throw new NullReferenceException("Could not use WebDriver, you must start the StartDriver() class first!");
            }
        }

        private EventFiringWebDriver _eventDriver;

        public EventFiringWebDriver EventDriver
        {
            get
            {
                if (_eventDriver != null)
                {
                    return _eventDriver;
                }
                throw new NullReferenceException("Could not use WebDriver, you must start the StartDriver() class first!");
            }
        }
        public class Messenger : EventArgs
        {
            public Messenger(string id ,string sender, string message)
            {
                Date = DateTime.Now;
                Id = id;
                Sender = sender;
                Message = message;
            }
            public DateTime Date { get; }
            public string Id { get; }
            public string Message { get; }
            public string Sender { get; }
        }

        public delegate void EventReceived(Messenger e);

        public event EventReceived Received;
        protected void CheckReceived(string id, string sender, string message)
        {
            Received?.Invoke(new Messenger(id, sender, message));
        }
        private Bitmap CreateQRCode(int width, int height, string text,int margin = 0)
        {
            try
            {
                Bitmap bmp;
                var qrCodeWriter = new ZXing.BarcodeWriterPixelData
                {
                    Format = ZXing.BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Height = height,
                        Width = width,
                        Margin = margin
                    }
                };
                var pixelData = qrCodeWriter.Write(text);
                using (var bitmap = new Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
                {
                    using (var ms = new MemoryStream())
                    {
                        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        try
                        {
                            System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                        }
                        finally
                        {
                            bitmap.UnlockBits(bitmapData);
                        }
                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        bmp = new Bitmap(ms);
                    }
                }

                return bmp;
            }
            catch
            {
                throw;
            }
        }

        public virtual void StartSession(string cache = "", bool hidden = false)
        {
            CheckDriverStarted();
            DriverStarted = true;
        }

        public virtual void StartSession(IWebDriver driver)
        {
            this.Driver = driver;
            driver.Navigate().GoToUrl("https://web.whatsapp.com");
            _eventDriver = new EventFiringWebDriver(WebDriver);
            GetWppJS();
        }

        protected void CheckDriverStarted(bool invert = false)
        {
            if (DriverStarted ^ invert)
            {
                throw new NotSupportedException(String.Format("Driver has been {0} started.", invert ? "not" : ""));
            }
        }

        public void Finish()
        {
            try
            {
                Driver.Quit();
                Driver.Dispose();
            }
            catch (Exception)
            {
                return;
            }
        }

        public Task SearchMessage()
        {
            while (true)
            {

            }
        }

        public Task<Models.SendReturnModels> SendMessage(Models.MessageModels message, bool simulateTyping = false)
        {
            try
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                switch (message.Type)
                {
                    case Models.Enum.MessageType.Text:
                        dynamic response = js.ExecuteReturnObj(Driver, String.Format("return WPP.chat.sendTextMessage('{0}','{1}')", message.Recipient, message.Message));
                        if (!string.IsNullOrEmpty(response["id"]))
                        {
                            ret.Id = response["id"];
                            ret.Sender = "";
                            ret.Recipient = message.Recipient;
                            ret.Status = true;
                        }
                        else
                        {
                            ret.Status = false;
                            ret.Error = "Error trying to send message.";
                        }
                        break;
                    case Models.Enum.MessageType.Reply:
                        break;
                    case Models.Enum.MessageType.Sticker:
                        break;
                    case Models.Enum.MessageType.Mentioned:
                        break;
                    case Models.Enum.MessageType.Selection:
                        break;
                    case Models.Enum.MessageType.Button:
                        break;
                    case Models.Enum.MessageType.Contact:
                        break;
                    case Models.Enum.MessageType.Ptt:
                        break;
                    case Models.Enum.MessageType.Localization:
                        break;
                    case Models.Enum.MessageType.Link:
                        break;
                    case Models.Enum.MessageType.Audio:
                    case Models.Enum.MessageType.Video:
                    case Models.Enum.MessageType.Document:
                    case Models.Enum.MessageType.Image:
                        break;
                    case Models.Enum.MessageType.Payment:
                        break;
                    default:
                        break;
                }
                return Task.FromResult(ret);
            }
            catch (Exception ex)
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                ret.Error = ex.Message;
                ret.Status = false;
                return Task.FromResult(ret);
            }
        }

        public Task<bool> IsInjected()
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, "return WPP.isInjected"));
            }
            catch (Exception)
            {
                return Task.FromResult(false); 
            }
        }
        public Task<bool> IsAuthenticated()
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, "return WPP.conn.isAuthenticated()"));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public async Task<Image> GetAuthImage(int width = 300, int height = 300, bool refresh=false)
        {
            try
            {
                if(await IsInjected())
                {
                    var pol = Policy<Image>
                    .Handle<Exception>()
                    .WaitAndRetry(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(3)
                    });
                    while (true)
                    {
                        string qrcode;
                        if (!refresh)
                        {
                            qrcode = await GetAuthCode();
                        }
                        else
                        {
                            qrcode = await GetAuthCodeRefresh();
                        }
                        if (!string.IsNullOrEmpty(qrcode))
                        {
                            return pol.Execute(() =>
                            {
                                try
                                {
                                    Bitmap objBitmap = new Bitmap(CreateQRCode(width, height, qrcode));
                                    Image objImage = (Image)objBitmap;
                                    return objImage;
                                }
                                catch (Exception)
                                {
                                    throw new Exception("Image not found!");
                                }
                            });
                        }
                    }
                }
                else
                {
                    throw new Exception("WPP not found!");
                }
            }
            catch (Exception)
            {
                throw new Exception("Image not found!");
            }
        }

        public Task<string> GetAuthCode()
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, "return WPP.conn.getAuthCode()");
                return Task.FromResult(response["fullCode"]);
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        public Task<string> GetAuthCodeRefresh()
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, "return WPP.conn.refreshQR()");
                return Task.FromResult(response["fullCode"]);
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        public bool Logout()
        {
            try
            {
                js.Execute(Driver, "return WPP.conn.logout()");
                Thread.Sleep(1000);
                Finish();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool GetWppJS()
        {
            try
            {
                GitHub github = new GitHub();
                github.CheckUpdate(WppPath);
                string path = string.IsNullOrEmpty(WppPath) ? Path.Combine(Environment.CurrentDirectory, "WppConnect") : WppPath;
                string file = Path.Combine(path, "wppconnect-wa.js");
                if (File.Exists(file))
                {
                    using (StreamReader sr = new StreamReader(file))
                    {
                        string wppjs = sr.ReadToEnd();
                        js.Execute(Driver, wppjs);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}