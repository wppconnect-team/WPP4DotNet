using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        public void StartService(IWpp wpp, string cache = "")
        {
            _wpp = wpp;
            _wpp.StartSession();
            panel2.Visible = true;
            label3.Text = "Waiting!";
            label3.ForeColor = Color.DodgerBlue;
            thr = new Thread(Service);
            thr.IsBackground = true;
            thr.Start();
        }

        public async void Service()
        {
            pictureBox1.Image = await _wpp.GetAuthImage();
            while (true)
            {
                if (await _wpp.IsAuthenticated())
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

        public void Messenger(IWpp.Messenger msg)
        {
            //_wpp.WebHook("https://webhook.site/78e1d4ef-d787-41be-bbff-e769b241b9d4", msg);
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
                    if (ret.Status)
                    {
                        textBox1.Text = "";
                        richTextBox1.Text = "";
                        ListViewItem l = new ListViewItem();
                        l.Tag = LstSent.Items.Count + 1;
                        l.Text = DateTime.Now.ToString();
                        l.SubItems.Add(msg.Message);
                        l.SubItems.Add(ret.Id);
                        LstSent.Items.Add(l);
                        LstSent.Sorting = SortOrder.Descending;
                    }
                    else
                    {
                        MessageBox.Show(ret.Error);
                    }

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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
