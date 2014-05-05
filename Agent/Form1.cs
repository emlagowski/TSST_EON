using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Agent
{
    public partial class Form1 : Form
    {
        Communication comm;

        public Form1()
        {
            InitializeComponent();
            comm = new Communication();

            /**comm.agentData.ForEach(delegate(ExtSrc.AgentData adt)
            {
                comboBox1.Items.Add(adt.routerAddress);
            });**/

            //Thread.Sleep(5000);
            //BindingSource bs = new BindingSource();
            //bs.DataSource = comm.agentData;
            //comboBox1.DataSource = bs.DataSource;
            //comboBox1.DisplayMember = "address";
            //comboBox1.ValueMember = "address";

            //comboBox1.DataSource = new List<String> { "elo", "elo2" };



            comboBox1.Update();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Form2 form2 = new Form2();
            //DialogResult dialogresult = form2.ShowDialog();
            //form2.Dispose();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = comm.agentData;
            comboBox1.DataSource = bs.DataSource;
            comboBox1.DisplayMember = "address";
            comboBox1.ValueMember = "address";

            ExtSrc.AgentData agTmp = comboBox1.SelectedItem as ExtSrc.AgentData;

            BindingSource bs2 = new BindingSource();
            bs2.DataSource = agTmp.fibTable.Wires;
            comboBox2.DataSource = bs2.DataSource;
            comboBox2.DisplayMember = "IDD";
            comboBox2.ValueMember = "IDD";

            this.Refresh();
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
