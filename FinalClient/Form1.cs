using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FinalClient
{
    public partial class Form1 : Form
    {
        Client _client;
        public Form1(Client client)
        {
            _client = client;
            InitializeComponent();
            labelName.Text = _client.address;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
