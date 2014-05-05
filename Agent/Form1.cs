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
        static System.Windows.Forms.Timer myTimer;
        List<String> bandItems = new List<string>(){"1 band", "2 band", "3 band", "4 band", "5 band", "6 band", "7 band", "8 band", "9 band", "10 band", "11 band", "12 band"};
        public Form1()
        {
            InitializeComponent();
            comm = new Communication();
            checkedListBox1.ItemCheck += checkedListBox1_ItemCheck;
            checkedListBox2.ItemCheck += checkedListBox2_ItemCheck;

            myTimer = new System.Windows.Forms.Timer();
            myTimer.Tick += new EventHandler(TimerEventProcessor);

            // Sets the timer interval to 5 seconds.
            myTimer.Interval = 5000;
            myTimer.Start();
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
        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            myTimer.Stop();
            //myTimer.Interval = 1000;

            BindingSource bs = new BindingSource();
            bs.DataSource = comm.agentData;
            comboBox1.DataSource = bs.DataSource;
            comboBox1.DisplayMember = "address";
            comboBox1.ValueMember = "address";

            ExtSrc.AgentData agTmp = comboBox1.SelectedItem as ExtSrc.AgentData;
            ExtSrc.AgentData agTmp1 = comboBox1.SelectedItem as ExtSrc.AgentData;


            BindingSource bs2 = new BindingSource();
            bs2.DataSource = agTmp.fibTable.Wires;
            BindingSource bs3 = new BindingSource();
            bs3.DataSource = agTmp1.fibTable.Wires;

            comboBox2.DataSource = bs2.DataSource;
            comboBox2.DisplayMember = "IDD";
            comboBox2.ValueMember = "IDD";

            comboBox3.BindingContext = new BindingContext();
            comboBox3.DataSource = bs3.DataSource;
            comboBox3.DisplayMember = "IDD";
            comboBox3.ValueMember = "IDD";


            this.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Form2 form2 = new Form2();
            //DialogResult dialogresult = form2.ShowDialog();
            //form2.Dispose();

            foreach (int indexChecked in checkedListBox1.CheckedIndices) 
            { 
                
            }

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
        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
          /*  System.Text.StringBuilder messageBoxCS = new System.Text.StringBuilder();
            messageBoxCS.AppendFormat("{0} = {1}", "Index", e.Index);
            messageBoxCS.AppendLine();
            messageBoxCS.AppendFormat("{0} = {1}", "NewValue", e.NewValue);
            messageBoxCS.AppendLine();
            messageBoxCS.AppendFormat("{0} = {1}", "CurrentValue", e.CurrentValue);
            messageBoxCS.AppendLine();
            MessageBox.Show(messageBoxCS.ToString(), "ItemCheck Event");*/
          /*  ExtSrc.AgentData item1 = comboBox1.SelectedItem as ExtSrc.AgentData;
            ExtSrc.Wire item2 = comboBox2.SelectedItem as ExtSrc.Wire;
            int id = item2.ID;
            ExtSrc.WireBand wire = item1.findWireOut(id);

            CheckState state = e.CurrentValue;
            for(int i=0; i<wire.lambdas.Length; i++){
            if ((wire.lambdas[i] == false) && e.Index == i ) e.NewValue = state;
            }*/
            if (e.CurrentValue == CheckState.Indeterminate) e.NewValue = e.CurrentValue;

        }
        private void checkedListBox2_ItemCheck(object sender, ItemCheckEventArgs e) 
        {
            if (e.CurrentValue == CheckState.Indeterminate) e.NewValue = e.CurrentValue;
            
        }



        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ExtSrc.AgentData item1 = comboBox1.SelectedItem as ExtSrc.AgentData;
            ExtSrc.Wire item2 = comboBox2.SelectedItem as ExtSrc.Wire;
            int id = item2.ID;
            ExtSrc.WireBand wire = item1.findWireOut(id);
            int band = wire.lambdaCapactity;
            checkedListBox1.DataSource = bandItems.GetRange(0,band);
           
            
            for (int i = 0; i < wire.lambdas.Length; i++)
            {

                if (wire.lambdas[i] == false)
                    checkedListBox1.SetItemCheckState(i, CheckState.Indeterminate);
                else { checkedListBox1.SetItemCheckState(i, CheckState.Unchecked); }

            }
            this.Refresh(); 


        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void checkedListBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            ExtSrc.AgentData item1a = comboBox1.SelectedItem as ExtSrc.AgentData;
            ExtSrc.Wire item2a = comboBox3.SelectedItem as ExtSrc.Wire;
            int id1 = item2a.ID;
            ExtSrc.WireBand wire = item1a.findWireIN(id1);
            int band1 = wire.lambdaCapactity;
            checkedListBox2.DataSource = bandItems.GetRange(0, band1);


            for (int i = 0; i < wire.lambdas.Length; i++)
            {

                if (wire.lambdas[i] == false)
                    checkedListBox2.SetItemCheckState(i, CheckState.Indeterminate);
                else { checkedListBox2.SetItemCheckState(i, CheckState.Unchecked); }

            }
            this.Refresh(); 
        }
    }
}
