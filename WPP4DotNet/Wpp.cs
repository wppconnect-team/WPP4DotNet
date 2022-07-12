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
using System.Collections.Generic;
using System.Net;
using System.Threading;
using RestSharp;

namespace WPP4DotNet
{
    public abstract class IWpp
    {
        #region WPP4DotNet - Library Functions
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
        public virtual void StartSession()
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
                return true;
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
        #endregion

        #region WPP4DotNet - JavaScript
        /// <summary>
        /// This method downloads and updates the latest version of wppconnect-wa.js.
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
                        js.Execute(Driver, "WPPConfig = {poweredBy: 'WPP4DotNet'}");
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
            return "window.WPP.chatList=async function(s,t){let a=[];switch(s){case\"user\":a=await window.WPP.chat.list({onlyUsers:!0});break;case\"group\":a=await window.WPP.chat.list({onlyGroups:!0});break;case\"label\":a=await window.WPP.chat.list({withLabels:t});break;case\"unread\":a=await window.WPP.chat.list({onlyWithUnreadMessage:!0});break;default:a=await window.WPP.chat.list()}let e=[];for(let s=0;s<a.length;s++)if(a[s]){let t={hasUnread:a[s].hasUnread,type:a[s].kind,messages:a[s].msgs._models,lastMessage:a[s].lastReceivedKey,contact:{id:a[s].id.user,server:a[s].id.server,name:a[s].formattedTitle,pushname:a[s].contact.pushname,isUser:a[s].isUser,isGroup:a[s].isGroup,isBroadcast:a[s].isBroadcast,isMe:a[s].contact.isMe,isBusiness:a[s].contact.isBusiness,isMyContact:a[s].contact.isMyContact,isWAContact:a[s].contact.isWAContact,image:\"\"}};e.push(t)}return e},window.WPP.chatFind=async function(s){let t=await window.WPP.chat.find(s);return{hasUnread:t.hasUnread,type:t.kind,messages:t.msgs._models,lastMessage:t.lastReceivedKey,contact:{id:t.id.user,server:t.id.server,name:t.formattedTitle,pushname:t.contact.pushname,isUser:t.isUser,isGroup:t.isGroup,isBroadcast:t.isBroadcast,isMe:t.contact.isMe,isBusiness:t.contact.isBusiness,isMyContact:t.contact.isMyContact,isWAContact:t.contact.isWAContact,image:\"\"}}},window.WPP.contactList=async function(s,t){let a=[];switch(s){case\"my\":a=await window.WPP.contact.list({onlyMyContacts:!0});break;case\"label\":a=await window.WPP.contact.list({withLabels:t});break;default:a=await window.WPP.contact.list()}let e=[];for(let s=0;s<a.length;s++)if(a[s]){let t={id:a[s].id.user,server:a[s].id.server,name:a[s].name,pushname:a[s].formattedName,isUser:a[s].isUser,isGroup:a[s].isGroup,isBroadcast:a[s].isBroadcast,isMe:a[s].isMe,isBusiness:a[s].isBusiness,isMyContact:a[s].isMyContact,isWAContact:a[s].isWAContact,image:\"\"};e.push(t)}return e};";
        }
        #endregion

        #region WPPJS CONN - Functions
        /// <summary>
        /// This method get the authentication code from the qr code.
        /// </summary>
        /// <returns>Returns STRING with authentication information.</returns>
        public Task<string> GetAuthCode()
        {
            try
            {
                Thread.Sleep(1000);
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
        /// This method return the current logged user ID with device id.
        /// </summary>
        /// <returns>Returns STRING with my id (Phone Number)</returns>
        public Task<string> GetMyDeviceId()
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, "return await WPP.conn.getMyDeviceId()");
                return Task.FromResult(string.Format("{0},{1},{2},{3}", response["device"], response["server"], response["user"], response["_serialized"]));
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }
        
        /// <summary>
        /// This method return the current logged user ID with device id.
        /// </summary>
        /// <returns>Returns STRING with my id (Phone Number)</returns>
        public Task<string> GetMyUserId()
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, "return await WPP.conn.getMyUserId()");
                return Task.FromResult(string.Format("{0},{1},{2}", response["server"], response["user"], response["_serialized"]));
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method return my number.
        /// </summary>
        /// <returns>Returns STRING with my id (Phone Number)</returns>
        public Task<string> GetMyNumber()
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, "return await WPP.conn.getMyUserId()");
                return Task.FromResult(string.Format("{0}", response["user"]));
            }
            catch (Exception)
            {
                return Task.FromResult("");
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
        /// This method checks is idle.
        /// </summary>
        /// <returns>Return True or False</returns>
        public Task<bool> IsIdle()
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, "return await WPP.conn.isIdle()"));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method checks is multi device.
        /// </summary>
        /// <returns>Return True or False</returns>
        public Task<bool> IsMultiDevice()
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, "return await WPP.conn.isMultiDevice()"));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method disconnects whatsapp web and ends the session.
        /// </summary>
        /// <returns>Return True or False</returns>
        public bool Logout()
        {
            try
            {
                js.Execute(Driver, "return WPP.conn.logout()");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method set keep alive state, that will force the focused and online state.
        /// </summary>
        /// <param name="status">Inform true or false.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> SetKeepAlive(bool status)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.conn.setKeepAlive({0})",status)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method if it's true, WhatsApp WEB will switch to MD. If it's false, WhatsApp WEB will switch to Legacy.
        /// </summary>
        /// <param name="status">Inform true or false.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> SetMultiDevice(bool status)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.conn.setMultiDevice({0})", status)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
        #endregion

        #region WPPJS CHAT - Chat Functions
        /// <summary>
        /// This method archives and unarchives the chats.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="status">Inform true or false.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> ChatArchive(string chat, bool status)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.chat.archive('{0}',{1})", chat, status)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method checks if is possible to mute this chat.
        /// </summary>
        /// <returns>Return True or False</returns>
        public Task<bool> CanMute()
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, "return await WPP.conn.canMute()"));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method clear a chat message.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return True or False</returns>
        public bool ChatClear(string chat)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.clear('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method delete a chat.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return True or False</returns>
        public bool ChatDelete(string chat)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.delete('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
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
        /// <param name="filter">Use "user", "group", "unread" or "label" to filter or leave blank to bring everything.</param>
        /// <param name="value">Enter an array of strings to filter the desired label.</param>
        /// <returns>Returns the List Models.ChatModel object</returns>
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
                if (obj != null)
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
                            message.Ack = (int)item["ack"];
                            message.FromMe = item["id"]["fromMe"];
                            message.Message = func.IsSet(item, "body") ? item["body"] : "";
                            message.Type = item["type"];
                            message.Sender = item["from"]["user"];
                            message.Recipient = item["to"]["user"];
                            messages.Add(message);
                        }
                        chat.Messages = messages;

                        //Others
                        chat.HasUnread = response["hasUnread"];
                        chat.Type = response["type"];
                        chats.Add(chat);
                    }
                }
                return Task.FromResult(chats);
            }
            catch (Exception)
            {
                return Task.FromResult(new List<Models.ChatModel>());
            }
        }

        /// <summary>
        /// This method searches all chats and can be filtered by user, group, unread and label.
        /// </summary>
        /// <param name="filter">Use "user", "group", "unread" or "label" to filter or leave blank to bring everything.</param>
        /// <param name="value">Enter an array of strings to filter the desired label.</param>
        /// <returns>Returns the object raw</returns>
        public Task<IReadOnlyCollection<object>> ChatListRaw(string filter = "", List<string> value = null)
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
                        obj = js.ExecuteReturnListObj(Driver, "return await WPP.chat.list({onlyUsers:true})");
                        break;
                    case "group":
                        obj = js.ExecuteReturnListObj(Driver, "return await WPP.chat.list({onlyGroups:true})");
                        break;
                    case "unread":
                        obj = js.ExecuteReturnListObj(Driver, "return await WPP.chat.list({onlyWithUnreadMessage:true})");
                        break;
                    case "label":
                        obj = js.ExecuteReturnListObj(Driver, "return await ({withLabels: " + label + "})");
                        break;
                    default:
                        obj = js.ExecuteReturnListObj(Driver, "return await WPP.chat.list()");
                        break;
                }
                return Task.FromResult(obj);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// This method find a chat by id.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Returns the Models.ChatModel object</returns>
        public Task<Models.ChatModel> ChatFind(string chat)
        {
            try
            {
                Models.ChatModel chatModel = new Models.ChatModel();
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chatFind('{0}')", chat));
                if(response != null)
                {
                    Functions func = new Functions();
                    //Contact
                    chatModel.Id = response["contact"]["id"];
                    chatModel.Server = response["contact"]["server"];
                    chatModel.Name = response["contact"]["name"];
                    chatModel.PushName = response["contact"]["pushname"];
                    chatModel.Image = response["contact"]["image"];
                    chatModel.IsBroadcast = response["contact"]["isBroadcast"];
                    chatModel.IsBusiness = response["contact"]["isBusiness"];
                    chatModel.IsGroup = response["contact"]["isGroup"];
                    chatModel.IsMe = response["contact"]["isMe"];
                    chatModel.IsContact = response["contact"]["isMyContact"];
                    chatModel.IsUser = response["contact"]["isUser"];
                    chatModel.IsWAContact = response["contact"]["isWAContact"];

                    //Messages
                    chatModel.LastMessage = response["lastMessage"]["_serialized"];

                    List<Models.MessageModels> messages = new List<Models.MessageModels>();
                    foreach (var item in response["messages"])
                    {
                        Models.MessageModels message = new Models.MessageModels();
                        message.Id = item["id"]["_serialized"];
                        message.Ack = (int)item["ack"];
                        message.FromMe = item["id"]["fromMe"];
                        message.Message = func.IsSet(item, "body") ? item["body"] : "";
                        message.Type = item["type"];
                        message.Sender = item["from"]["user"];
                        message.Recipient = item["to"]["user"];
                        messages.Add(message);
                    }
                    chatModel.Messages = messages;

                    //Others
                    chatModel.HasUnread = response["hasUnread"];
                    chatModel.Type = response["type"];
                }
                return Task.FromResult(chatModel);
            }
            catch (Exception)
            {
                return Task.FromResult(new Models.ChatModel());
            }
        }

        /// <summary>
        /// This method find a chat by id.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Returns the object raw</returns>
        public Task<object> ChatFindRaw(string chat)
        {
            try
            {
                Models.ChatModel chatModel = new Models.ChatModel();
                object response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chat.find('{0}')", chat));
                return Task.FromResult(response);
            }
            catch (Exception)
            {
                return Task.FromResult(new object());
            }
        }

        /// <summary>
        /// This method mark a chat to composing state and keep sending "is writting a message".
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="time">Enter milliseconds or leave empty.</param>
        /// <returns>Return True or False</returns>
        public bool MarkIsComposing(string chat, int time = 0)
        {
            try
            {
                var timer = time > 0 ? ", " + time : "";
                js.Execute(Driver, string.Format("return await WPP.chat.markIsComposing('{0}'{1})", chat, timer));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method mark a chat is paused state.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return True or False</returns>
        public bool MarkIsPaused(string chat)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.markIsPaused('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method mark a chat as read and send SEEN event.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return True or False</returns>
        public bool MarkIsRead(string chat)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.markIsRead('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method mark a chat to recording state and keep sending "is recording".
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="time">Enter milliseconds or leave empty.</param>
        /// <returns>Return True or False</returns>
        public bool MarkIsRecording(string chat, int time = 0)
        {
            try
            {
                var timer = time > 0 ? ", " + time : "";
                js.Execute(Driver, string.Format("return await WPP.chat.markIsRecording('{0}'{1})", chat, timer));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method mark a chat as unread.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return True or False</returns>
        public bool MarkIsUnread(string chat)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.markIsUnread('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method mute a chat, you can use duration.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="time">Inform the seconds.</param>
        /// <returns>Return True or False</returns>
        public bool Mute(string chat, int time = 0)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.mute('{0}',{duration: {1}})", chat, time));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method mute a chat, you can use expiration.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="date">Inform the DateTime.</param>
        /// <returns>Return True or False</returns>
        public bool Mute(string chat, DateTime date)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.mute('{0}',{expiration: {1}})", chat, date));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method unmute a chat.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return True or False</returns>
        public bool UnMute(string chat)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.unmute('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method open the chat in the WhatsApp interface in a specific message.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="messageID">Inform the message ID.</param>
        /// <returns></returns>
        public bool OpenChatAt(string chat, string messageID)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.openChatAt('{0}','{1}')", chat, messageID));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method open the chat in the WhatsApp interface in bottom position.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns></returns>
        public bool OpenChatBottom(string chat, string messageID)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.openChatBottom('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method open the chat in the WhatsApp interface from first unread message.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns></returns>
        public bool OpenChatFromUnread(string chat, string messageID)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.openChatFromUnread('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method pin the chat.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="status">Inform true or false.</param>
        /// <returns>Return True or False</returns>
        public bool Pin(string chat, bool status)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.pin('{0}'{1})", chat, status));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region WPPJS CHAT - Message Functions
        /// <summary>
        /// This method send a text message.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="message">Inform the Message.</param>
        /// <param name="simulateTyping">Inform true or false.</param>
        /// <returns>Returns the Models.SendReturnModels object</returns>
        public async Task<Models.SendReturnModels> SendMessage(string chat, string message, bool simulateTyping=false)
        {
            try
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                if (simulateTyping)
                {
                    MarkIsComposing(chat, 5000);
                }
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chat.sendTextMessage('{0}','{1}')", chat, message));
                if (!string.IsNullOrEmpty(response["id"]))
                {
                    ret.Id = response["id"];
                    ret.Sender = await GetMyNumber();
                    ret.Recipient = chat;
                    ret.Status = true;
                }
                else
                {
                    ret.Status = false;
                    ret.Error = "Error trying to send message.";
                }
                return ret;
            }
            catch (Exception ex)
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                ret.Error = ex.Message;
                ret.Status = false;
                return ret;
            }
        }

        /// <summary>
        /// This method send a create poll message (Note: This only works for groups).
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="name">Inform the Name.</param>
        /// <param name="choices">Inform the Choices.</param>
        /// <param name="options">Inform the Options List.</param>
        /// <param name="simulateTyping">Inform true or false.</param>
        /// <returns>Returns the Models.SendReturnModels object</returns>
        public async Task<Models.SendReturnModels> SendCreatePollMessage(string chat, string name, string choices, List<string> options, bool simulateTyping = false)
        {
            try
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                if (simulateTyping)
                {
                    MarkIsComposing(chat, 5000);
                }
                var option = "";
                foreach (var item in options)
                {
                    option += string.Format(",'{0}'", item);
                }
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chat.sendCreatePollMessage('{0}','{1}','{2}',[{3}])", chat, name, choices, option.TrimStart(',')));
                if (!string.IsNullOrEmpty(response["id"]))
                {
                    ret.Id = response["id"];
                    ret.Sender = await GetMyNumber();
                    ret.Recipient = chat;
                    ret.Status = true;
                }
                else
                {
                    ret.Status = false;
                    ret.Error = "Error trying to send message.";
                }
                return ret;
            }
            catch (Exception ex)
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                ret.Error = ex.Message;
                ret.Status = false;
                return ret;
            }
        }

        /// <summary>
        /// This method send a file message, that can be an audio, document, image, sticker or video.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="content">Inform the content.</param>
        /// <param name="options">Inform the Options List.</param>
        /// <param name="simulateTyping">Inform true or false.</param>
        /// <returns>Returns the Models.SendReturnModels object</returns>
        public async Task<Models.SendReturnModels> SendFileMessage(string chat, string content, List<string> options, bool simulateTyping = false)
        {
            try
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                if (simulateTyping)
                {
                    MarkIsComposing(chat, 5000);
                }
                var option = "";
                foreach (var item in options)
                {
                    option += string.Format(",{0}", item);
                }
                option = "{"+ option.TrimStart(',') + "}";
                var str = string.Format("return await WPP.chat.sendFileMessage('{0}','{1}',{2})", chat, content, option);
                dynamic response = js.ExecuteReturnObj(Driver, str);
                if (!string.IsNullOrEmpty(response["id"]))
                {
                    ret.Id = response["id"];
                    ret.Sender = await GetMyNumber();
                    ret.Recipient = chat;
                    ret.Status = true;
                }
                else
                {
                    ret.Status = false;
                    ret.Error = "Error trying to send message.";
                }
                return ret;
            }
            catch (Exception ex)
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                ret.Error = ex.Message;
                ret.Status = false;
                return ret;
            }
        }

        /// <summary>
        /// This method send a list message options.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="options">Inform the Options List.</param>
        /// <param name="simulateTyping">Inform true or false.</param>
        /// <returns>Returns the Models.SendReturnModels object</returns>
        public async Task<Models.SendReturnModels> SendListMessage(string chat,  List<string> options, bool simulateTyping = false)
        {
            try
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                if (simulateTyping)
                {
                    MarkIsComposing(chat, 5000);
                }
                var option = "";
                foreach (var item in options)
                {
                    option += string.Format(",{0}", item);
                }
                option = "{" + option.TrimStart(',') + "}";
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chat.sendListMessage('{0}',{1})", chat, option));
                if (!string.IsNullOrEmpty(response["id"]))
                {
                    ret.Id = response["id"];
                    ret.Sender = await GetMyNumber();
                    ret.Recipient = chat;
                    ret.Status = true;
                }
                else
                {
                    ret.Status = false;
                    ret.Error = "Error trying to send message.";
                }
                return ret;
            }
            catch (Exception ex)
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                ret.Error = ex.Message;
                ret.Status = false;
                return ret;
            }
        }

        /// <summary>
        /// This method send a location message.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="options">Inform the Options List.</param>
        /// <param name="simulateTyping">Inform true or false.</param>
        /// <returns>Returns the Models.SendReturnModels object</returns>
        public async Task<Models.SendReturnModels> SendLocationMessage(string chat, List<string> options, bool simulateTyping = false)
        {
            try
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                if (simulateTyping)
                {
                    MarkIsComposing(chat, 5000);
                }
                var option = "";
                foreach (var item in options)
                {
                    option += string.Format(",{0}", item);
                }
                option = "{" + option.TrimStart(',') + "}";
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chat.sendLocationMessage('{0}',{1})", chat, option));
                if (!string.IsNullOrEmpty(response["id"]))
                {
                    ret.Id = response["id"];
                    ret.Sender = await GetMyNumber();
                    ret.Recipient = chat;
                    ret.Status = true;
                }
                else
                {
                    ret.Status = false;
                    ret.Error = "Error trying to send message.";
                }
                return ret;
            }
            catch (Exception ex)
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                ret.Error = ex.Message;
                ret.Status = false;
                return ret;
            }
        }

        /// <summary>
        /// This method send a raw message.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="rawMessage">Inform the raw message.</param>
        /// <param name="options">Inform the Options List.</param>
        /// <param name="simulateTyping">Inform true or false.</param>
        /// <returns>Returns the Models.SendReturnModels object</returns>
        public async Task<Models.SendReturnModels> SendRawMessage(string chat, string rawMessage, List<string> options, bool simulateTyping = false)
        {
            try
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                if (simulateTyping)
                {
                    MarkIsComposing(chat, 5000);
                }
                var option = "";
                if(options != null)
                {
                    foreach (var item in options)
                    {
                        option += string.Format(",{0}", item);
                    }
                    option = ",{" + option.TrimStart(',') + "}";
                }
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chat.sendRawMessage('{0}','{1}'{2})", chat, rawMessage, option));
                if (!string.IsNullOrEmpty(response["id"]))
                {
                    ret.Id = response["id"];
                    ret.Sender = await GetMyNumber();
                    ret.Recipient = chat;
                    ret.Status = true;
                }
                else
                {
                    ret.Status = false;
                    ret.Error = "Error trying to send message.";
                }
                return ret;
            }
            catch (Exception ex)
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                ret.Error = ex.Message;
                ret.Status = false;
                return ret;
            }
        }

        /// <summary>
        /// This method send a reaction to a message.
        /// </summary>
        /// <param name="messageId">Inform the message ID.</param>
        /// <param name="reaction">Inform the reaction.</param>
        /// <returns>Return True or False</returns>
        public Task<string> SendReactionMessage(string messageId, string reaction)
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chat.sendReactionMessage('{0}','{1}')", messageId, reaction));
                return Task.FromResult(response["sendMsgResult"]);
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method send a file message, that can be an audio, document, image, sticker or video.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="contacts">Inform the contacts.</param>
        /// <param name="options">Inform the Options List.</param>
        /// <param name="simulateTyping">Inform true or false.</param>
        /// <returns>Returns the Models.SendReturnModels object</returns>
        public async Task<Models.SendReturnModels> SendVCardContactMessage(string chat, List<string> contacts, List<string> options, bool simulateTyping = false)
        {
            try
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                if (simulateTyping)
                {
                    MarkIsComposing(chat, 5000);
                }
                var contact = "";
                foreach (var item in contacts)
                {
                    contact += string.Format(",'{0}'", item);
                }
                var option = "";
                if (options != null)
                {
                    foreach (var item in options)
                    {
                        option += string.Format(",'{0}'", item);
                    }
                    option = ",{" + option.TrimStart(',') + "}";
                }
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chat.sendVCardContactMessage('{0}',[{1}]{2})", chat, contact, option));
                if (!string.IsNullOrEmpty(response["id"]))
                {
                    ret.Id = response["id"];
                    ret.Sender = await GetMyNumber();
                    ret.Recipient = chat;
                    ret.Status = true;
                }
                else
                {
                    ret.Status = false;
                    ret.Error = "Error trying to send message.";
                }
                return ret;
            }
            catch (Exception ex)
            {
                Models.SendReturnModels ret = new Models.SendReturnModels();
                ret.Error = ex.Message;
                ret.Status = false;
                return ret;
            }
        }

        /// <summary>
        /// This method send a reaction to a message.
        /// </summary>
        /// <param name="messageId">Inform the message ID.</param>
        /// <param name="reaction">Inform the false to remove.</param>
        /// <returns>Return True or False</returns>
        public Task<string> SendReactionMessage(string messageId, bool reaction)
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chat.sendReactionMessage('{0}','{1}')", messageId, reaction));
                return Task.FromResult(response["sendMsgResult"]);
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method star/unstar a message.
        /// </summary>
        /// <param name="messageId">Inform the message ID.</param>
        /// <param name="star">Inform true or false</param>
        /// <returns>Return True or False</returns>
        public Task<bool> StarMessage(string messageId, bool star = false)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.chat.starMessage('{0}',{1})", messageId, star));
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method delete message in chat.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="message">Inform the message ID list.</param>
        /// <param name="deleteMedia">Inform true or false</param>
        /// <param name="revoke">Inform true or false</param>
        /// <returns>Return True or False</returns>
        public Task<bool> DeleteMessage(string chat, List<string> message, bool deleteMedia=false, bool revoke=false)
        {
            try
            {
                var msgID = "";
                foreach (var item in message)
                {
                    msgID += string.Format(",'{0}'", item);
                }
                var deleteMediaString = "";
                if (deleteMedia)
                {
                    deleteMediaString = ", true";
                }
                var revokeString = "";
                if (revoke)
                {
                    revokeString = ", true";
                }
                js.Execute(Driver, string.Format("return await WPP.chat.deleteMessage('{0}',[{1}]{2}{3})", chat, msgID.TrimStart(','), deleteMediaString, revokeString));
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method download the blob of a media message.
        /// </summary>
        /// <param name="message">Inform the message ID.</param>
        /// <returns>Return String Blob</returns>
        public Task<string> DownloadMedia(string message)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnString(Driver, string.Format("return await WPP.chat.downloadMedia('{0}')", message)));
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method generate message id.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return String</returns>
        public Task<string> GenerateMessageID(string chat)
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chat.generateMessageID('{0}')", chat));
                return Task.FromResult(response["_serialized"]);
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method get message by a single ID.
        /// </summary>
        /// <param name="message">Inform the message ID.</param>
        /// <returns>Return Models.MessageModels Object</returns>
        public Task<Models.MessageModels> GetMessageById(string message)
        {
            try
            {
                Models.MessageModels msg = new Models.MessageModels();
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.chat.getMessageById('{0}')", message));
                if (response != null)
                {
                    Functions func = new Functions();
                    msg.Id = response["id"]["_serialized"];
                    msg.Ack = (int)response["ack"];
                    msg.FromMe = response["id"]["fromMe"];
                    msg.Message = func.IsSet(response, "body") ? response["body"] : "";
                    msg.Type = response["type"];
                    msg.Sender = response["from"]["user"];
                    msg.Recipient = response["to"]["user"];
                }
                return Task.FromResult(msg);
            }
            catch (Exception)
            {
                return Task.FromResult(new Models.MessageModels());
            }
        }

        /// <summary>
        /// This method fetch messages from a chat.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <param name="count">Inform the count.</param>
        /// <param name="onlyUnread">Inform the onlyUnread true or false.</param>
        /// <returns>Return List Models.MessageModels Object</returns>
        public Task<List<Models.MessageModels>> GetMessages(string chat, int count=20, bool onlyUnread=false)
        {
            try
            {
                var onlyUnreadString = "";
                if (onlyUnread)
                {
                    onlyUnreadString = ", onlyUnread: true";
                }
                List<Models.MessageModels> messages = new List<Models.MessageModels>();
                IReadOnlyCollection<object> response = js.ExecuteReturnListObj(Driver, string.Format("return await WPP.chat.getMessages('{0}', {count:{1}{2}})", chat, count, onlyUnreadString));
                if (response != null)
                {
                    Functions func = new Functions();
                    foreach (dynamic item in response)
                    {
                        Models.MessageModels msg = new Models.MessageModels();
                        msg.Id = item["id"]["_serialized"];
                        msg.Ack = (int)item["ack"];
                        msg.FromMe = item["id"]["fromMe"];
                        msg.Message = func.IsSet(item, "body") ? item["body"] : "";
                        msg.Type = item["type"];
                        msg.Sender = item["from"]["user"];
                        msg.Recipient = item["to"]["user"];
                        messages.Add(msg);
                    }
                }
                return Task.FromResult(messages);
            }
            catch (Exception)
            {
                return Task.FromResult(new List<Models.MessageModels>());
            }
        }
        #endregion

        #region WPPJS CONTACT - Functions
        /// <summary>
        /// This method get profile picture.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return String Url</returns>
        public Task<string> ContactGetProfilePictureUrl(string chat)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnString(Driver, string.Format("return await WPP.contact.getProfilePictureUrl('{0}')", chat)));
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method get status.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return String Status</returns>
        public Task<string> ContactGetStatus(string chat)
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.contact.getStatus('{0}')", chat));
                return Task.FromResult(response["status"]);
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method check contact exist.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> ContactExists(string chat)
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.contact.queryExists('{0}')", chat));
                return Task.FromResult(response==null?false:true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method searches all contacts and can be filtered by my and label.
        /// </summary>
        /// <param name="filter">Use "my" or "label" to filter or leave blank to bring everything.</param>
        /// <param name="value">Enter an array of strings to filter the desired label.</param>
        /// <returns>Returns the List Models.ChatModel object</returns>
        public Task<List<Models.ChatModel>> ContactList(string filter = "", List<string> value = null)
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
                    case "my":
                    case "label":
                        obj = js.ExecuteReturnListObj(Driver, string.Format("return await WPP.contactList('{0}'{1})", filter, label));
                        break;
                    default:
                        obj = js.ExecuteReturnListObj(Driver, "return await WPP.contactList()");
                        break;
                }
                List<Models.ChatModel> chats = new List<Models.ChatModel>();
                if (obj != null)
                {
                    Functions func = new Functions();
                    foreach (dynamic response in obj)
                    {
                        Models.ChatModel chat = new Models.ChatModel();
                        //Contact
                        chat.Id = response["id"];
                        chat.Server = response["server"];
                        chat.Name = response["name"];
                        chat.PushName = response["pushname"];
                        chat.Image = response["image"];
                        chat.IsBroadcast = response["isBroadcast"];
                        chat.IsBusiness = response["isBusiness"];
                        chat.IsGroup = response["isGroup"];
                        chat.IsMe = response["isMe"];
                        chat.IsContact = response["isMyContact"];
                        chat.IsUser = response["isUser"];
                        chat.IsWAContact = response["isWAContact"];
                        chats.Add(chat);
                    }
                }
                return Task.FromResult(chats);
            }
            catch (Exception)
            {
                return Task.FromResult(new List<Models.ChatModel>());
            }
        }

        /// <summary>
        /// This method searches all contacts and can be filtered by my and label.
        /// </summary>
        /// <param name="filter">Use "my" or "label" to filter or leave blank to bring everything.</param>
        /// <param name="value">Enter an array of strings to filter the desired label.</param>
        /// <returns>Returns the List object raw</returns>
        public Task<IReadOnlyCollection<object>> ContactListRaw(string filter = "", List<string> value = null)
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
                    case "my":
                        obj = js.ExecuteReturnListObj(Driver, "return await WPP.contact.list({onlyMyContacts:true})");
                        break;
                    case "label":
                        obj = js.ExecuteReturnListObj(Driver, "return await WPP.contact.list({withLabels: " + label + "})");
                        break;
                    default:
                        obj = js.ExecuteReturnListObj(Driver, "return await WPP.contact.list()");
                        break;
                }
                return Task.FromResult(obj);
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region WPPJS GROUP - Functions
        /// <summary>
        /// This method get information from invite code.
        /// </summary>
        /// <param name="code">Inform the invite code.</param>
        /// <returns>Return Models.GroupInfoModels Object</returns>
        public Task<Models.GroupInfoModels> GroupGetInfoFromInviteCode(string code)
        {
            try
            {
                Models.GroupInfoModels info = new Models.GroupInfoModels();
                List<Models.ParticipantsModels> participants = new List<Models.ParticipantsModels>();
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.group.getGroupInfoFromInviteCode('{0}')",code));
                foreach (var item in response["participants"])
                {
                    Models.ParticipantsModels participant = new Models.ParticipantsModels();
                    participant.Id = item["id"];
                    participant.IsAdmin = item["isAdmin"];
                    participant.IsSuperAdmin = item["isSuperAdmin"];
                    participants.Add(participant);
                }
                if (response != null)
                {
                    info.Id = response["id"];
                    info.Announce = response["announce"];
                    info.Creation = DateTimeOffset.FromUnixTimeSeconds(response["creation"]).DateTime;
                    info.DefaultSubgroup = response["defaultSubgroup"];
                    info.Desc = response["desc"];
                    info.DescID = response["descId"];
                    info.DescOwner = response["descOwner"];
                    info.DescTime = DateTimeOffset.FromUnixTimeSeconds(response["descTime"]).DateTime;
                    info.IsParentGroup = response["isParentGroup"];
                    info.NoFrequentlyForwarded = response["noFrequentlyForwarded"];
                    info.NumSubgroups = response["numSubgroups"];
                    info.Owner = response["owner"];
                    info.Participants = participants;
                    info.PvId = response["pvId"];
                    info.Restrict = response["restrict"];
                    info.Size = response["size"];
                    info.Status = response["status"];
                    info.Subject = response["subject"];
                    info.SubjectOwner = response["subjectOwner"];
                    info.SubjectTime = DateTimeOffset.FromUnixTimeSeconds(response["subjectTime"]).DateTime;
                    info.Support = response["support"];
                    info.Suspended = response["suspended"];
                }
                return Task.FromResult(info);
            }
            catch (Exception)
            {
                return Task.FromResult(new Models.GroupInfoModels());
            }
        }

        /// <summary>
        /// This method join group from invite code.
        /// </summary>
        /// <param name="code">Inform the invite code.</param>
        /// <returns>Return String Chat ID</returns>
        public Task<string> GroupJoin(string code)
        {
            try
            {
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.group.join('{0}')", code));
                return Task.FromResult(response["id"]);
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method leave from a group.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <returns>Return True or False</returns>
        public bool GroupLeave(string chat)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.group.leave('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method revoke the current invite code and generate new one.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <returns>Return String URL</returns>
        public Task<string> GroupRevokeInviteCode(string chat)
        {
            try
            {
                var code = js.ExecuteReturnString(Driver, string.Format("return await WPP.group.revokeInviteCode('{0}')", chat));
                return Task.FromResult(string.Format("https://chat.whatsapp.com/{0}", code));
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method define the group description.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="description">Inform the description.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupSetDescription(string chat, string description)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.setDescription('{0}','{1}')", chat, description)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method set the group property (announcement and restrict).
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="property">Inform the property name</param>
        /// <param name="value">Inform the value bool.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupSetProperty(string chat, string property, bool value)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.setProperty('{0}','{1}',{2})", chat, property, value)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method set the group property (ephemeral).
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="property">Inform the property name</param>
        /// <param name="value">Inform the value bool.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupSetProperty(string chat, string property, int value)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.setProperty('{0}','{1}',{2})", chat, property, value)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method define the group subject.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="subject">Inform the subject.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupSetSubject(string chat, string subject)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.setSubject('{0}','{1}')", chat, subject)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method add participants in group.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="participants">Inform the participants list.</param>
        /// <returns>Return True or False</returns>
        public bool GroupAddParticipants(string chat, List<string> participants)
        {
            try
            {
                var list = "";
                foreach (var item in participants)
                {
                    list += string.Format(",'{0}'", item);
                }
                js.Execute(Driver, string.Format("return await WPP.group.setSubject('{0}',[{1}])", chat, list.TrimStart(',')));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method check can add.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupCanAdd(string chat)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.canAdd('{0}')", chat)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method check can demote.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="participant">Inform the chat participant.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupCanDemote(string chat, string participant)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.canDemote('{0}','{1}')", chat, participant)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method check can promote.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="participant">Inform the chat participant.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupCanPromote(string chat, string participant)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.canPromote('{0}','{1}')", chat, participant)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method check can remove.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="participant">Inform the chat participant.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupCanRemove(string chat, string participant)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.canRemove('{0}','{1}')", chat, participant)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method create group.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="participants">Inform the participants list.</param>
        /// <returns>Return True or False</returns>
        public bool GroupCreate(string chat, List<string> participants)
        {
            try
            {
                var list = "";
                foreach (var item in participants)
                {
                    list += string.Format(",'{0}'", item);
                }
                js.Execute(Driver, string.Format("return await WPP.group.create('{0}',[{1}])", chat, list.TrimStart(',')));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method demote participants.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="participants">Inform the participants list.</param>
        /// <returns>Return True or False</returns>
        public bool GroupDemoteParticipants(string chat, List<string> participants)
        {
            try
            {
                var list = "";
                foreach (var item in participants)
                {
                    list += string.Format(",'{0}'", item);
                }
                js.Execute(Driver, string.Format("return await WPP.group.demoteParticipants('{0}',[{1}])", chat, list.TrimStart(',')));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method get the currend invite code of the group.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <returns>Return String URL</returns>
        public Task<string> GroupGetInviteCode(string chat)
        {
            try
            {
                var code = js.ExecuteReturnString(Driver, string.Format("return await WPP.group.getInviteCode('{0}')", chat));
                return Task.FromResult(string.Format("https://chat.whatsapp.com/{0}", code));
            }
            catch (Exception)
            {
                return Task.FromResult("");
            }
        }

        /// <summary>
        /// This method ensure group.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="checkIsAdmin">Inform true or false.</param>
        /// <returns>Return Models.MessageModels Object</returns>
        public Task<List<Models.MessageModels>> GetMyStatus(string chat, bool checkIsAdmin)
        {
            try
            {
                List<Models.MessageModels> messages = new List<Models.MessageModels>();
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.group.ensureGroup('{0}',{1})", chat, checkIsAdmin));
                if (response != null)
                {
                    Functions func = new Functions();
                    foreach (var item in response["msgs"])
                    {
                        Models.MessageModels message = new Models.MessageModels();
                        message.Id = item["id"]["_serialized"];
                        message.Ack = (int)item["ack"];
                        message.FromMe = item["id"]["fromMe"];
                        message.Message = func.IsSet(item, "body") ? item["body"] : "";
                        message.Type = item["type"];
                        message.Sender = item["from"]["user"];
                        message.Recipient = item["to"]["user"];
                        messages.Add(message);
                    }
                }
                return Task.FromResult(messages);
            }
            catch (Exception)
            {
                return Task.FromResult(new List<Models.MessageModels>());
            }
        }

        //----------------------------------------------
        //Implementar [GROUP] ensureGroupAndParticipants
        //----------------------------------------------

        /// <summary>
        /// This method get all participants.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <returns>Return Models.ParticipantsModels object</returns>
        public Task<List<Models.ParticipantsModels>> GroupGetParticipants(string chat)
        {
            try
            {
                List<Models.ParticipantsModels> participants = new List<Models.ParticipantsModels>();
                IReadOnlyCollection<object> obj = js.ExecuteReturnListObj(Driver, string.Format("return await WPP.group.getParticipants('{0}')", chat));
                foreach (dynamic response in obj)
                {
                    Models.ParticipantsModels participant = new Models.ParticipantsModels();
                    participant.Id = response["id"];
                    participant.IsAdmin = response["isAdmin"];
                    participant.IsSuperAdmin = response["isSuperAdmin"];
                    participants.Add(participant);
                }
                return Task.FromResult(participants);                    
            }
            catch (Exception)
            {
                return Task.FromResult(new List<Models.ParticipantsModels>());
            }
        }

        /// <summary>
        /// This method check i am admin.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupIAmAdmin(string chat)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.iAmAdmin('{0}')", chat)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method check i am member.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupIAmMember(string chat)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.iAmMember('{0}')", chat)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method check i am restricted member.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupIAmRestrictedMember(string chat)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.iAmRestrictedMember('{0}')", chat)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method check i am super admin.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <returns>Return True or False</returns>
        public Task<bool> GroupIAmSuperAdmin(string chat)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.group.iAmSuperAdmin('{0}')", chat)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// This method promote participants.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="participants">Inform the participants list.</param>
        /// <returns>Return True or False</returns>
        public bool GroupPromoteParticipants(string chat, List<string> participants)
        {
            try
            {
                var list = "";
                foreach (var item in participants)
                {
                    list += string.Format(",'{0}'", item);
                }
                js.Execute(Driver, string.Format("return await WPP.group.promoteParticipants('{0}',[{1}])", chat, list.TrimStart(',')));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method remove participants.
        /// </summary>
        /// <param name="chat">Inform the chat group ([number]@g.us).</param>
        /// <param name="participants">Inform the participants list.</param>
        /// <returns>Return True or False</returns>
        public bool GroupRemoveParticipants(string chat, List<string> participants)
        {
            try
            {
                var list = "";
                foreach (var item in participants)
                {
                    list += string.Format(",'{0}'", item);
                }
                js.Execute(Driver, string.Format("return await WPP.group.removeParticipants('{0}',[{1}])", chat, list.TrimStart(',')));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method Set Group image.
        /// </summary>
        /// <param name="chat"></param>
        /// <param name="image"></param>
        /// <returns></returns>
        public Task<bool> GroupSetIcon(string chat, string image)
        {
            try
            {
                var str = string.Format("return await WPP.group.setIcon('{0}','{1}')", chat, image);
                dynamic response = js.ExecuteReturnObj(Driver, str);
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false); 
            }
        }
        #endregion

        #region WPPJS LABELS - Functions

        #endregion

        #region WPPJS PROFILE - Functions

        #endregion

        #region WPPJS STATUS - Functions
        /// <summary>
        /// This method get my status messages.
        /// </summary>
        /// <returns>Return Models.MessageModels Object</returns>
        public Task<List<Models.MessageModels>> GetMyStatus()
        {
            try
            {
                List<Models.MessageModels> messages = new List<Models.MessageModels>();
                dynamic response = js.ExecuteReturnObj(Driver, string.Format("return await WPP.status.getMyStatus()"));
                if (response != null)
                {
                    Functions func = new Functions();
                    foreach (var item in response["msgs"])
                    {
                        Models.MessageModels message = new Models.MessageModels();
                        message.Id = item["id"]["_serialized"];
                        message.Ack = (int)response["ack"];
                        message.FromMe = item["id"]["fromMe"];
                        message.Message = func.IsSet(item, "body") ? item["body"] : "";
                        message.Type = item["type"];
                        message.Sender = item["from"]["user"];
                        message.Recipient = item["to"]["user"];
                        messages.Add(message);
                    }
                }
                return Task.FromResult(messages);
            }
            catch (Exception)
            {
                return Task.FromResult(new List<Models.MessageModels>());
            }
        }

        /// <summary>
        /// This method set the status in text.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public Task<bool> SendTextStatus(string content, List<string> options)
        {
            try
            {
                var option = "";
                foreach (var item in options)
                {
                    option += string.Format(",{0}", item);
                }
                option = "{" + option.TrimStart(',') + "}";
                var str = string.Format("return await WPP.status.sendTextStatus('{0}',{1})",content, option);
                dynamic response = js.ExecuteReturnObj(Driver, str);
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
        #endregion

        #region WPPJS BLOCKLIST - Functions
        /// <summary>
        /// This method returns a list of blocked contacts.
        /// </summary>
        /// <returns>Return String List.</returns>
        public Task<List<string>> BlockList()
        {
            try
            {
                IReadOnlyCollection<object> obj = js.ExecuteReturnListObj(Driver, "return await WPP.blocklist.all()");
                List<string> chats = new List<string>();
                if (obj != null)
                {
                    Functions func = new Functions();
                    foreach (dynamic response in obj)
                    {
                        chats.Add(String.Format("{0},{1},{2}", response["server"], response["user"], response["_serialized"]));
                    }
                }
                return Task.FromResult(chats);
            }
            catch (Exception)
            {
                return Task.FromResult(new List<string>());
            }
        }

        /// <summary>
        /// This method block a contact.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return True or False</returns>
        public bool BlockContact(string chat)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.blocklist.blockContact('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method unblock a contact.
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return True or False</returns>
        public bool UnBlockContact(string chat)
        {
            try
            {
                js.Execute(Driver, string.Format("return await WPP.blocklist.unblockContact('{0}')", chat));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// This method check if it's blocked
        /// </summary>
        /// <param name="chat">Inform the chat.</param>
        /// <returns>Return True or False</returns>
        public Task<bool> IsBlocked(string chat)
        {
            try
            {
                return Task.FromResult(js.ExecuteReturnBool(Driver, string.Format("return await WPP.blocklist.isBlocked('{0}')", chat)));
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
        #endregion
    }
}