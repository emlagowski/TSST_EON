using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                XmlReader xmlFile;
                xmlFile = XmlReader.Create("log.xml", new XmlReaderSettings());
                DataSet ds = new DataSet();
                ds.ReadXml(xmlFile);
                dataGridView1.DataSource = ds.Tables[0];
                xmlFile.Close();
                XmlReader xmlFile2;
                xmlFile2 = XmlReader.Create("connections.xml", new XmlReaderSettings());
                DataSet ds2 = new DataSet();
                ds2.ReadXml(xmlFile2);
                dataGridView2.DataSource = ds2.Tables[0];
                xmlFile2.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            } 
        }
    }
}
