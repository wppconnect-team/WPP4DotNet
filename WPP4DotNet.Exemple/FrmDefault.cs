using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        private Thread thr;

        public FrmDefault()
        {
            InitializeComponent();
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            StartService(new ChromeWebApp());
        }

        public void StartService(IWpp wpp, string session = "")
        {
            _wpp = wpp;
            _wpp.StartSession(session, checkBox3.Checked);
            if (string.IsNullOrEmpty(session))
            {
                panel2.Visible = true;
            }
            label3.Text = "Waiting!";
            label3.ForeColor = Color.DodgerBlue;
            thr = new Thread(Service);
            thr.IsBackground = true;
            thr.Start();
        }

        public async void Service()
        {
            if (!string.IsNullOrEmpty(await _wpp.GetAuthCode()))
            {
                pictureBox1.Image = await _wpp.GetAuthImage();
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
                    if(item.ToLower().Trim() == msg.Message.ToLower().Trim())
                    {
                        Models.MessageModels model = new Models.MessageModels();
                        model.Recipient = msg.Sender;
                        model.Message = message;
                        model.Type = Models.Enum.MessageType.chat;
                        Models.SendReturnModels ret = await _wpp.SendMessage(model);
                        Action<Models.SendReturnModels,string> inv2 = SaveSend;
                        Invoke(inv2, ret,message);
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
                    _wpp.Logout();
                }
                else
                {
                    _wpp.Finish();
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

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (await _wpp.IsAuthenticated())
                {
                    Models.MessageModels msg = new Models.MessageModels();
                    msg.Recipient = textBox1.Text;
                    msg.Message = richTextBox1.Text;
                    msg.Type = Models.Enum.MessageType.chat;
                    Models.SendReturnModels ret = await _wpp.SendMessage(msg);
                    SaveSend(ret, msg.Message);
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

        private void SaveSend(Models.SendReturnModels ret,string message)
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
            StartService(new ChromeWebApp(), Path.Combine(path, name));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var path = textBox5.Text;
            StartService(new ChromeWebApp(), path);
        }
    }
}
