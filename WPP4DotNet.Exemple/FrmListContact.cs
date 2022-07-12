using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WPP4DotNet.Exemple
{
    public partial class FrmListContact : Form
    {
        private string _type;
        private List<Models.ChatModel> _data;
        public FrmListContact(string type, List<Models.ChatModel> data)
        {
            _type = type;
            _data = data;
            InitializeComponent();
        }

        private void ListContact_Load(object sender, EventArgs e)
        {
            //Add columns
            listView1.Columns.Add("Id");
            listView1.Columns.Add("Name");
            listView1.Columns.Add("Number");

            //Add Data
            foreach (var item in _data)
            {
                ListViewItem l = new ListViewItem();
                int pos = listView1.Items.Count + 1;
                l.Tag = pos;
                l.Text = pos.ToString();
                l.SubItems.Add(item.Name);
                l.SubItems.Add(item.Id);
                listView1.Items.Add(l);
            }
        }
    }
}
