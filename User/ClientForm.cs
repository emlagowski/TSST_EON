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
            _user.connect("127.0.1." + this.routerAddress.Text);
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            //_user.Send(Convert.ToInt32(this.band.Text), this.message.Text);
            _user.Send(Convert.ToInt32(this.band.Text), this.message.Text, "127.0.0." + this.targetAddress.Text);
        } 

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                XmlReader xmlFile;
                xmlFile = XmlReader.Create(_user.logName, new XmlReaderSettings());
                DataSet ds = new DataSet();
                ds.ReadXml(xmlFile);
                dataGridView1.DataSource = ds.Tables[0];
                xmlFile.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            } 
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
