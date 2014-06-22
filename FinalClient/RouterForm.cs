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
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            
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

        private void labelName_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                XmlReader xmlFile;
                xmlFile = XmlReader.Create(_router.logName, new XmlReaderSettings());
                DataSet ds = new DataSet();
                ds.ReadXml(xmlFile);
                dataGridView1.DataSource = ds.Tables[0];
                xmlFile.Close();
                XmlReader xmlFile2;
                xmlFile2 = XmlReader.Create(_router.wiresName, new XmlReaderSettings());
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

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
