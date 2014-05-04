using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace User
{
    public partial class UserForm : Form
    {
        User _user;
        public UserForm(User user)
        {
            _user = user;
            InitializeComponent();
            labelName.Text = _user.localAddress;
            //connectButton, sendButton
            //routerAddres, targetAddres, message, labelName
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            _user.connect("127.0.0." + this.routerAddress.Text);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            _user.Send("127.0.0." + this.targetAddress.Text, Convert.ToInt32(this.band.Text), this.message.Text, Guid.NewGuid().GetHashCode());
        }
    }
}
