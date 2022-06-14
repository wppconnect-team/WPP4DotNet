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
using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace WPP4DotNet
{
    public abstract class IWpp
    {
        /// <summary>
        /// WppPath
        /// </summary>
        public string WppPath = "";

        /// <summary>
        /// JavaScript
        /// </summary>
        private JavaScript js = new JavaScript();

        /// <summary>
        /// DriverStarted
        /// </summary>
        private bool DriverStarted { get; set; }

        /// <summary>
        /// Internal WebDriver Interface
        /// </summary>
        private IWebDriver Driver;

        /// <summary>
        /// WebDriver Interface
        /// </summary>
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

        /// <summary>
        /// Internal Event Driver
        /// </summary>
        private EventFiringWebDriver _eventDriver;

        /// <summary>
        /// Event Driver
        /// </summary>
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

        /// <summary>
        /// This is the message class.
        /// </summary>
        public class Messenger : EventArgs
        {
            public Messenger(string id, string sender, string message)
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

        /// <summary>
        /// This method delegates.
        /// </summary>
        /// <param name="e">Set the messenger</param>
        public delegate void EventReceived(Messenger e);

        /// <summary>
        /// This is a received event method.
        /// </summary>
        public event EventReceived Received;

        /// <summary>
        /// This method checks incoming messages.
        /// </summary>
        /// <param name="id">Set the id</param>
        /// <param name="sender">Set the sender</param>
        /// <param name="message">Set the message</param>
        protected void CheckReceived(string id, string sender, string message)
        {
            Received?.Invoke(new Messenger(id, sender, message));
        }

        /// <summary>
        /// This method transforms the text into a qrcode image.
        /// </summary>
        /// <param name="width">Set the width</param>
        /// <param name="height">Set the height</param>
        /// <param name="text">Set the text</param>
        /// <param name="margin">Set the margin</param>
        /// <returns>Returns an object of type Bitmap</returns>
        private Bitmap CreateQRCode(int width, int height, string text, int margin = 0)
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

        /// <summary>
        /// This method starts the selenium session, allowing you to save the cache and hide the browser.
        /// </summary>
        /// <param name="cache">If necessary define the directory to save the session data.</param>
        /// <param name="hidden">Set true or false if you want to hide the browser.</param>
        public virtual void StartSession(string cache = "", bool hidden = false)
        {
            CheckDriverStarted();
            DriverStarted = true;
        }

        /// <summary>
        /// This method starts browsing and inserting JS scripts.
        /// </summary>
        /// <param name="driver">Insert the IWebDriver object.</param>
        public virtual void StartSession(IWebDriver driver)
        {
            this.Driver = driver;
            driver.Navigate().GoToUrl("https://web.whatsapp.com");
            _eventDriver = new EventFiringWebDriver(WebDriver);
            GetWppJS();
        }

        /// <summary>
        /// This method checks if the selenium driver is started.
        /// </summary>
        /// <param name="invert"></param>
        /// <exception cref="NotSupportedException"></exception>
        protected void CheckDriverStarted(bool invert = false)
        {
            if (DriverStarted ^ invert)
            {
                throw new NotSupportedException(String.Format("Driver has been {0} started.", invert ? "not" : ""));
            }
        }

        /// <summary>
        /// This method ends the selenium session.
        /// </summary>
        /// <returns>Return True or False</returns>
        public bool Finish()
        {
            try
            {
                Driver.Quit();
                Driver.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method in conjunction with "SearchMessage" allows sending a POST request to an external URL.
        /// </summary>
        /// <param name="url">Enter your external URL that will receive the POST.</param>
        /// <param name="msg">Insert the "Messenger" object returned by the "SearchMessage" task.</param>
        /// <returns>Return True or False</returns>
        public bool WebHook(string url, Messenger msg)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                RestClient client = new RestClient(url);
                RestRequest request = new RestRequest();
                request.Method = Method.Post;
                request.RequestFormat = DataFormat.Json;
                request.AddHeader("Content-Type", "application/json");
                request.AddJsonBody(new { Sender = msg.Sender, Message = msg.Message, Date = msg.Date });
                RestResponse response = client.PostAsync(request).Result;
                return response.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(response.Content) ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method is a task that validates if there are unread messages, if so, it captures the information and marks it as viewed.
        /// </summary>
        /// <returns>Returns a Task</returns>
        public async Task SearchMessage()
        {
            while (true)
            {
                List<Models.ChatModel> chats = await ChatList("unread");
                if(chats.Count > 0)
                {
                    foreach (var chat in chats)
                    {
                        foreach (var message in chat.Messages)
                        {
                            if(message.FromMe == false && message.Id == chat.LastMessage)
                            {
                                CheckReceived(message.Id,message.Sender,message.Message);
                                MarkIsRead(chat.Id);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method makes sending whatsapp messages.
        /// </summary>
        /// <param name="message">Enter the object (Models.MessageModels) of the message to be sent.</param>
        /// <param name="simulateTyping">Enter true or false if you want to simulate typing.</param>
        /// <returns>Returns the Models.SendReturnModels object</returns>
        public Task<Models.SendReturnModels> SendMessage(Models.MessageModels message, bool simulateTyping = false)
        {
            try
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                switch (message.Type)
                {
                    case Models.Enum.MessageType.chat:
                        dynamic response = js.ExecuteReturnObj(Driver, String.Format("return await WPP.chat.sendTextMessage('{0}','{1}')", message.Recipient, message.Message));
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
                    //case Models.Enum.MessageType.Reply:
                    //    break;
                    //case Models.Enum.MessageType.Sticker:
                    //    break;
                    //case Models.Enum.MessageType.Mentioned:
                    //    break;
                    //case Models.Enum.MessageType.Selection:
                    //    break;
                    //case Models.Enum.MessageType.Button:
                    //    break;
                    //case Models.Enum.MessageType.Contact:
                    //    break;
                    //case Models.Enum.MessageType.Ptt:
                    //    break;
                    //case Models.Enum.MessageType.Localization:
                    //    break;
                    //case Models.Enum.MessageType.Link:
                    //    break;
                    //case Models.Enum.MessageType.Audio:
                    //case Models.Enum.MessageType.Video:
                    //case Models.Enum.MessageType.Document:
                    //case Models.Enum.MessageType.Image:
                    //    break;
                    //case Models.Enum.MessageType.Payment:
                    //    break;
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

        /// <summary>
        /// This method checks if the WPPJS script has been entered in the browser.
        /// </summary>
        /// <returns>Return True or False</returns>
        public Task<bool> IsInjected()
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, "return await WPP.isInjected"));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method checks if the qr code authentication has already been done.
        /// </summary>
        /// <returns>Return True or False</returns>
        public Task<bool> IsAuthenticated()
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, "return await WPP.conn.isAuthenticated()"));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method checks if whatsapp Main has been loaded.
        /// </summary>
        /// <returns>Return True or False</returns>
        public Task<bool> IsMainLoaded()
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, "return await WPP.conn.isMainLoaded()"));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method takes the generated authentication code and creates a qrcode image of type "Image".
        /// </summary>
        /// <param name="width">Enter the width by default, it is already set to 300.</param>
        /// <param name="height">Enter the height by default, it is already set to 300.</param>
        /// <param name="refresh">Enter true or false if you want to reload the image.</param>
        /// <returns>Returns the Image object</returns>
        /// <exception cref="Exception"></exception>
        public async Task<Image> GetAuthImage(int width = 300, int height = 300, bool refresh = false)
        {
            try
            {
                if (await IsInjected())
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
                                    //throw new Exception("Image not found!");
                                    return null; 
                                }
                            });
                        }
                    }
                }
                else
                {
                    //throw new Exception("WPP not found!");
                    return null;
                }
            }
            catch (Exception)
            {
                //throw new Exception("Image not found!");
                return null;
            }
        }

        /// <summary>
        /// This method searches all chats and can be filtered by user, group, unread and label.
        /// </summary>
        /// <param name="filter">Use "group", "unread" or "label" to filter or leave blank to bring everything.</param>
        /// <param name="value">Enter an array of strings to filter the desired label.</param>
        /// <returns>Returns the Models.ChatModel object</returns>
        public Task<List<Models.ChatModel>> ChatList(string filter = "", List<string> value = null)
        {
            try
            {
                var label = "";
                if (value != null && filter == "label")
                {
                    foreach (var item in value)
                    {
                        label += string.Format(",'{0}'", item);
                    }
                    label = string.Format(",[{0}]", label.TrimStart(','));
                }
                IReadOnlyCollection<object> obj;
                switch (filter)
                {
                    case "user":
                    case "group":
                    case "unread":
                    case "label":
                        obj = js.ExecuteReturnListObj(Driver, string.Format("return await WPP.chatList('{0}'{1})", filter, label));
                        break;
                    default:
                        obj = js.ExecuteReturnListObj(Driver, "return await WPP.chatList()");
                        break;
                }
                List<Models.ChatModel> chats = new List<Models.ChatModel>();
                if(obj != null)
                {
                    Functions func = new Functions();
                    foreach (dynamic response in obj)
                    {
                        Models.ChatModel chat = new Models.ChatModel();
                        //Contact
                        chat.Id = response["contact"]["id"];
                        chat.Server = response["contact"]["server"];
                        chat.Name = response["contact"]["name"];
                        chat.PushName = response["contact"]["pushname"];
                        chat.Image = response["contact"]["image"];
                        chat.IsBroadcast = response["contact"]["isBroadcast"];
                        chat.IsBusiness = response["contact"]["isBusiness"];
                        chat.IsGroup = response["contact"]["isGroup"];
                        chat.IsMe = response["contact"]["isMe"];
                        chat.IsContact = response["contact"]["isMyContact"];
                        chat.IsUser = response["contact"]["isUser"];
                        chat.IsWAContact = response["contact"]["isWAContact"];
                        
                        //Messages
                        chat.LastMessage = response["lastMessage"]["_serialized"];

                        List<Models.MessageModels> messages = new List<Models.MessageModels>();
                        foreach (var item in response["messages"])
                        {
                            Models.MessageModels message = new Models.MessageModels();
                            message.Id = item["id"]["_serialized"];
                            message.FromMe = item["id"]["fromMe"];
                            message.Message = func.IsSet(item, "body") ? item["body"] : "";
                            message.Type = (Models.Enum.MessageType)Enum.Parse(typeof(Models.Enum.MessageType), item["type"], true);
                            message.Sender = item["from"]["user"];
                            message.Recipient = item["to"]["user"];
                            messages.Add(message);
                        }
                        chat.Messages = messages;

                        //Others
                        chat.HasUnread = response["hasUnread"];
                        chat.Type = (Models.Enum.ChatType)Enum.Parse(typeof(Models.Enum.ChatType), response["type"], true);
                        chats.Add(chat);
                    }
                }
                return Task.FromResult(chats);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new List<Models.ChatModel>());
            }
        }

        /// <summary>
        /// This method get the authentication code from the qr code.
        /// </summary>
        /// <returns>Returns STRING with authentication information.</returns>
        public Task<string> GetAuthCode()
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, "return await WPP.conn.getAuthCode()");
                return Task.FromResult(response["fullCode"]);
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method reloads the authentication code from the qr code.
        /// </summary>
        /// <returns>Returns STRING with authentication information.</returns>
        public Task<string> GetAuthCodeRefresh()
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, "return await WPP.conn.refreshQR()");
                return Task.FromResult(response["fullCode"]);
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method marks the chat as viewed messages.
        /// </summary>
        /// <param name="chat">Inform the chat you want to set as read.</param>
        /// <returns>Return True or False</returns>
        public bool MarkIsRead(string chat)
        {
            try
            {
                js.Execute(Driver, string.Format("return WPP.chat.markIsRead('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
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
        /// <summary>
        /// This method disconnects whatsapp web and ends the session.
        /// </summary>
        /// <returns>Return True or False</returns>
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
                        js.Execute(Driver, CustomJS());
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

        /// <summary>
        /// This method customizes some calls to WPP JS generating new JS functions.
        /// </summary>
        /// <returns>Returns STRING from JS functions.</returns>
        private string CustomJS()
        {
            string custom = "";
            custom += "window.WPP.chatList=async function(d,e){let a=[];switch(d){case\"user\":a=await window.WPP.chat.list({onlyUsers:!0});break;case\"group\":a=await window.WPP.chat.list({onlyGroups:!0});break;case\"label\":a=await window.WPP.chat.list({withLabels:e});break;case\"unread\":a=await window.WPP.chat.list({onlyWithUnreadMessage:!0});break;default:a=await window.WPP.chat.list()}let c=[];for(let b=0;b<a.length;b++)if(a[b]){let f=await WPP.contact.getProfilePictureUrl(a[b].id.user),g={hasUnread:a[b].hasUnread,type:a[b].kind,messages:a[b].msgs._models,lastMessage:a[b].lastReceivedKey,contact:{id:a[b].id.user,server:a[b].id.server,name:a[b].formattedTitle,pushname:a[b].contact.pushname,isUser:a[b].isUser,isGroup:a[b].isGroup,isBroadcast:a[b].isBroadcast,isMe:a[b].contact.isMe,isBusiness:a[b].contact.isBusiness,isMyContact:a[b].contact.isMyContact,isWAContact:a[b].contact.isWAContact,image:f}};c.push(g)}return c}";
            return custom;
        }
    }
}