using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WPP4DotNet.WebDriver;

namespace WPP4DotNet.Exemple
{
    public partial class FrmDefault : Form
    {
        private IWpp _wpp;
        private Thread _thr;
        private bool qrcode;

        public FrmDefault()
        {
            InitializeComponent();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            qrcode = true;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    StartService(new ChromeWebApp(checkBox3.Checked));
                    break;
                case 1:
                    StartService(new EdgeWebApp(checkBox3.Checked));
                    break;
                case 2:
                    StartService(new FirefoxWebApp(checkBox3.Checked));
                    break;
                default:
                    MessageBox.Show("Please select a browser!");
                    break;
            }
        }

        public void StartService(IWpp wpp)
        {
            label3.Text = "Waiting!";
            label3.ForeColor = Color.DodgerBlue;
            _wpp = wpp;
            _wpp.StartSession();
            if (qrcode)
            {
                panel2.Visible = true;
            }
            _thr = new Thread(Service);
            _thr.IsBackground = true;
            _thr.Start();
        }

        public async void Service()
        {
            if (!string.IsNullOrEmpty(await _wpp.GetAuthCode()))
            {
                Image image = await _wpp.GetAuthImage();
                Action<Image> inv = QrCode;
                Invoke(inv, image);
            }
            while (true)
            {
                if (await _wpp.IsAuthenticated())
                {
                    Action<bool> inv = Connected;
                    Invoke(inv, true);
                    break;
                }
                if (await _wpp.IsMainLoaded())
                {
                    Action<bool> inv = Connected;
                    Invoke(inv, true);
                    break;
                }
            }
            _wpp.Received += Messenger;
            _ = Task.Run(() => _wpp.SearchMessage());
        }

        public void QrCode(Image image)
        {
            pictureBox1.Image = image;
        }

        public void Connected(bool status)
        {
            label3.Text = "Connected!";
            label3.ForeColor = Color.ForestGreen;
            panel2.Visible = false;
        }

        public void AddList(ListViewItem l)
        {
            LstReceived.Items.Add(l);
            LstReceived.Sorting = SortOrder.Descending;
        }

        public async void Messenger(IWpp.Messenger msg)
        {
            //WebHook
            if (checkBox1.Checked)
            {
                _wpp.WebHook(textBox4.Text, msg);
            }

            //Auto Answer
            if (checkBox2.Checked)
            {
                var message = (string)richTextBox4.Invoke(new Func<string>(() => richTextBox4.Text));
                var keyword = textBox6.Text.Split(',');
                foreach (var item in keyword)
                {
                    if (item.ToLower().Trim() == msg.Message.ToLower().Trim())
                    {
                        Models.SendReturnModels ret = await _wpp.SendMessage(msg.Sender, message);
                        Action<Models.SendReturnModels, string> inv2 = SaveSend;
                        Invoke(inv2, ret, message);
                    }
                }

            }

            //Add incoming messages to ListView
            ListViewItem l = new ListViewItem();
            l.Tag = msg.Id;
            l.Text = msg.Date.ToString();
            l.SubItems.Add(msg.Sender);
            l.SubItems.Add(msg.Message);
            l.SubItems.Add(msg.Id);
            Action<ListViewItem> inv = AddList;
            Invoke(inv, l);
        }

        private async void BtnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                if (await _wpp.IsAuthenticated())
                {
                    //Logout Session
                    _wpp.Logout();
                }

                //Closed Selenium
                _wpp.Finish();

                //Google Chrome Kill All Process
                if(comboBox1.SelectedIndex == 0)
                {
                    KillChromeDriverProcesses();
                }

                label3.Text = "Disconnected!";
                label3.ForeColor = Color.Firebrick;
                panel2.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //Use if you want to clear pending processes after logging out of selenium using Google Chrome.
        private void KillChromeDriverProcesses()
        {
            Process[] chromeDriverProcesses = Process.GetProcessesByName("chromedriver");
            foreach (var chromeDriverProcess in chromeDriverProcesses)
            {
                chromeDriverProcess.Kill();
            }
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (await _wpp.IsAuthenticated())
                {
                    Models.SendReturnModels ret = await _wpp.SendMessage(textBox1.Text, richTextBox1.Text);
                    SaveSend(ret, richTextBox1.Text);
                    textBox1.Text = "";
                    richTextBox1.Text = "";
                }
                else
                {
                    MessageBox.Show("To send messages you need to log in.");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("To send messages you need to log in.");
            }
        }

        private void SaveSend(Models.SendReturnModels ret, string message)
        {
            if (ret.Status)
            {
                ListViewItem l = new ListViewItem();
                l.Tag = LstSent.Items.Count + 1;
                l.Text = DateTime.Now.ToString();
                l.SubItems.Add(message);
                l.SubItems.Add(ret.Id);
                LstSent.Items.Add(l);
                LstSent.Sorting = SortOrder.Descending;
            }
            else
            {
                MessageBox.Show(ret.Error);
            }
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Image = await _wpp.GetAuthImage(300, 300, true);
            }
            catch (Exception)
            {
                MessageBox.Show("Error loading new image.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Session");
            var name = Guid.NewGuid().ToString("N");
            qrcode = false;
            StartService(new ChromeWebApp(checkBox3.Checked, Path.Combine(path, name)));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var path = textBox5.Text;
            qrcode = false;
            StartService(new ChromeWebApp(checkBox3.Checked, path));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var ext = Path.GetExtension(dialog.FileName);
                textBox3.Text = dialog.FileName;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (await _wpp.IsAuthenticated())
                {
                    var base64 = ConvertFileToBase64(textBox3.Text);
                    var type = FileType(textBox3.Text);
                    List<string> options = new List<string>();
                    options.Add(string.Format("type: '{0}'", type));
                    options.Add(string.Format("caption: '{0}'", richTextBox2.Text));
                    Models.SendReturnModels ret = await _wpp.SendFileMessage(textBox2.Text, base64, options);
                    SaveSend(ret, richTextBox1.Text);
                    textBox2.Text = "";
                    textBox3.Text = "";
                    richTextBox2.Text = "";
                }
                else
                {
                    MessageBox.Show("To send file messages you need to log in.");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("To send file messages you need to log in.");
            }
        }

        private string ConvertFileToBase64(string fileName)
        {
            string Extension = Path.GetExtension(fileName);
            string MimeType;
            switch (Extension.ToLower())
            {
                case ".jpg":
                    MimeType = "data:image/jpg;base64,";
                    break;
                case ".jpeg":
                    MimeType = "data:image/jpeg;base64,";
                    break;
                case ".gif":
                    MimeType = "data:image/gif;base64,";
                    break;
                case ".png":
                    MimeType = "data:image/png;base64,";
                    break;
                case ".bmp":
                    MimeType = "data:image/bmp;base64,";
                    break;
                case ".ico":
                    MimeType = "data:image/x-icon;base64,";
                    break;
                case ".pdf":
                    MimeType = "data:application/pdf;base64,";
                    break;
                case ".mp3":
                    MimeType = "data:audio/mp3;base64,";
                    break;
                case ".mp4":
                    MimeType = "data:video/mp4;base64,";
                    break;
                case ".mpeg":
                    MimeType = "data:application/mpeg;base64,";
                    break;
                case ".txt":
                    MimeType = "data:text/plain;base64,";
                    break;
                default:
                    MimeType = "data:application/octet-stream;base64,";
                    break;
            }
            return MimeType + Convert.ToBase64String(System.IO.File.ReadAllBytes(fileName));
        }

        private string FileType(string fileName)
        {
            string Extension = Path.GetExtension(fileName);
            switch (Extension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".png":
                case ".bmp":
                case ".ico":
                    return "image";
                case ".mp3":
                    return "audio";
                case ".mp4":
                case ".mpeg":
                    return "video";
                default:
                    return "document";
            }
        }

        private void FrmDefault_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            if(label3.Text == "Connected!")
            {
                List<Models.ChatModel> data = await _wpp.ContactList("my");
                FrmListContact frm = new FrmListContact("my", data);
                frm.Show();
            }
            else
            {
                MessageBox.Show("Please connect to whatsapp.");
            }
        }
    }
}
