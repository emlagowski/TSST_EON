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

namespace Router
{
    public partial class RouterForm : Form
    {
        Router _router;
        public RouterForm(Router router)
        {
            _router = router;
            InitializeComponent();
            LabelText = _router.address;
            Console.SetOut(new TextBoxWriter(consoleOutput));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _router.closing();
            Console.WriteLine("papa");
        }

        public string LabelText
        {
            get
            {
                return this.labelName.Text;
            }
            set
            {
                this.labelName.Text = value;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        private void labelName_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    XmlReader xmlFile;
            //    xmlFile = XmlReader.Create(_router.logName, new XmlReaderSettings());
            //    DataSet ds = new DataSet();
            //    ds.ReadXml(xmlFile);
            //    dataGridView1.DataSource = ds.Tables[0];
            //    xmlFile.Close();
            //    XmlReader xmlFile2;
            //    xmlFile2 = XmlReader.Create(_router.wiresName, new XmlReaderSettings());
            //    DataSet ds2 = new DataSet();
            //    ds2.ReadXml(xmlFile2);
            //    dataGridView2.DataSource = ds2.Tables[0];
            //    xmlFile2.Close();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //} 
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}
