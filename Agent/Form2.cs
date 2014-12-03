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
    public partial class Form2 : Form
    {
        Communication cm;
        public Form2()
        {
            InitializeComponent();
            cm = new Communication();

            var list = new List<String> {"ssss", "seees"};
            comboBox1.DataSource = new BindingSource(list, null);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // refreshing comboBox after selecting
            var list = cm.routeHistoryList.Keys.Select(i => i[2]).ToList();
            comboBox1.DataSource = new BindingSource(list, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // send disroute by cm
            var key = comboBox1.SelectedItem as String;
            List<int[]> route = null;
            foreach (var i in cm.routeHistoryList.Keys.Where(i => i[2] == key))
            {
                route = cm.routeHistoryList[i];
            }
            cm.disroute(route, key);
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
