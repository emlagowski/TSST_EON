using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Agent
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Communication comm = new Communication();

            comm.agentData.ForEach(delegate(ExtSrc.AgentData adt)
            {
                comboBox1.Items.Add(adt.routerAddress);
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            DialogResult dialogresult = form2.ShowDialog();
            form2.Dispose();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }
    }
}
