using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using ExtSrc;

namespace Client
{
    public partial class ClientForm : Form
    {
        Client _user;
        public ClientForm(Client user)
        {
            _user = user;
            InitializeComponent();
            labelName.Text = _user.localAddress;
            Console.SetOut(new TextBoxWriter(consoleOutput));
            band.Text = "0";
            var t = new Timer { Enabled = true, Interval = 1 * 1000 };
            t.Tick += delegate { Bind(); };
        }

        public void Bind()
        {
            messageHistory.DataSource = null;
            messageHistory.DataSource = _user.messages.Select(d => new
            {
                TYPE = d.Key,
                MESSAGE = d.Value
            });
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            _user.connect("127.0.1." + this.routerAddress.Text);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            _user.Send(Convert.ToInt32(this.band.Text), this.message.Text, "127.0.0." + this.targetAddress.Text);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _user.closing();
        }
    }
}
