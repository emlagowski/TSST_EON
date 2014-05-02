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

namespace FinalServer
{
    public partial class Form1 : Form
    {
        Server _server;
        public Form1(Server server)
        {
            _server = server;
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            XmlNode node;
            node = _server.Doc.Clone();
            XmlNodeReader nr = new XmlNodeReader(node);
            DataSet ds = new DataSet();
            ds.ReadXml(nr);
            this.dataGridView1.DataSource = ds;
            nr.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            XmlNode node;
            node = _server.Doc.Clone();
            XmlNodeReader nr = new XmlNodeReader(node);
            DataSet ds = new DataSet();
            ds.ReadXml(nr);
            this.dataGridView1.DataSource = ds;
            nr.Close();
        }
    }
}
