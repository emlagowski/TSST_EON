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
            myTimer.Interval = 1000;

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

            ExtSrc.AgentData agTmp3 = comboBox1.SelectedItem as ExtSrc.AgentData;

            dataGridView1.DataSource = agTmp3.fibTable.Wires;
            dataGridView2.DataSource = agTmp3.AbIN;
            dataGridView3.DataSource = agTmp3.AbOUT;
            BindingSource bs4 = new BindingSource();
                bs4.DataSource = agTmp3.Conn;
                dataGridView4.BindingContext = new BindingContext();

            dataGridView4.DataSource = bs4.DataSource;


            this.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Form2 form2 = new Form2();
            //DialogResult dialogresult = form2.ShowDialog();
            //form2.Dispose();

            if (comboBox4.SelectedIndex < 0)
            {
                MessageBox.Show("You have to chose bandwidth!", "ERROR");
                return;
            }
            int band = comboBox4.SelectedIndex +1;
            int IN = (comboBox2.SelectedItem as ExtSrc.Wire).distance;
            int OUT = (comboBox3.SelectedItem as ExtSrc.Wire).distance;

            int checkedCount1 = 0;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                var state = checkedListBox1.GetItemCheckState(i);

                if (state == CheckState.Checked) 
                {
                    checkedCount1++;
                }
            }
            int checkedCount2 = 0;
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
            {
                var state = checkedListBox2.GetItemCheckState(i);

                if (state == CheckState.Checked)
                {
                    checkedCount2++;
                }
            }

            if (IN < 1000 && OUT < 1000)
            {
                if ((checkedCount1 != checkedCount2) || (checkedCount1!=band))
                {
                    MessageBox.Show(String.Format("Number of IN/OUT checked boxes must be equal to {0}.", band), "ERROR");
                    return;
                }
                IN_label.Text = band.ToString();
                OUT_label.Text = band.ToString();
                IN_label.Refresh();
                OUT_label.Refresh();
            }
            else
            {

                if (IN < OUT)
                {
                    if (checkedCount1 != 2 * checkedCount2 || checkedCount1!=band)
                    {
                        MessageBox.Show(String.Format("Number of IN checked boxes must be equal to {0}.\nNumber of OUT checked boxes must be equal to {1} ", band, 2 * band), "ERROR");
                        return;
                    }
                    IN_label.Text = band.ToString();
                    OUT_label.Text = (2 * band).ToString();
                    IN_label.Refresh();
                    OUT_label.Refresh();
                }
                else if (OUT < IN)
                {
                    if (2 * checkedCount1 != checkedCount2 || checkedCount2 != band)
                    {
                        MessageBox.Show(String.Format("Number of IN checked boxes must be equal to {0}.\nNumber of OUT checked boxes must be equal to {1} ", 2 * band, band), "ERROR");
                        return;
                    }
                    IN_label.Text = (2*band).ToString();
                    OUT_label.Text = band.ToString();
                    IN_label.Refresh();
                    OUT_label.Refresh();
                }
                else
                {
                    if (checkedCount1 != checkedCount2 || checkedCount1 != band*2)
                    {
                        MessageBox.Show(String.Format("Number of IN/OUT checked boxes must be equal to {0}.", 2 * band), "ERROR");
                        return;
                    }
                    IN_label.Text = (2 * band).ToString();
                    OUT_label.Text = (2 * band).ToString();
                    IN_label.Refresh();
                    OUT_label.Refresh();
                }
            }

            int[] lambdasOut = new int[checkedListBox1.CheckedItems.Count];
            int[] lambdasIn = new int[checkedListBox2.CheckedItems.Count];

             
            checkedListBox1.CheckedIndices.CopyTo(lambdasOut, 0);
            checkedListBox2.CheckedIndices.CopyTo(lambdasIn, 0);
            int InWireID = (comboBox3.SelectedItem as ExtSrc.Wire).ID;
            int OutWireID = (comboBox2.SelectedItem as ExtSrc.Wire).ID;
            int Bandwidth = checkedListBox2.CheckedItems.Count;
            int cID;
            try
            {
                cID = Convert.ToInt32(textBox1.Text);
            }
            catch (Exception ex) 
            { 
                MessageBox.Show("Wrong Connection ID format. Must be Int32 value.", "ERROR");
                return;            
            }
            ExtSrc.Connection connection = new ExtSrc.Connection(lambdasIn, lambdasOut, InWireID, OutWireID, Bandwidth, cID);
            comm.Send((comboBox1.SelectedItem as ExtSrc.AgentData).address, new ExtSrc.AgentData(connection));

            //update local agent data
            foreach (ExtSrc.AgentData a in comm.agentData)
            {
                if(a.address == (comboBox1.SelectedItem as ExtSrc.AgentData).address)
                a.addConnection(connection);
            }
            myTimer.Start();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ExtSrc.AgentData agTmp3 = comboBox1.SelectedItem as ExtSrc.AgentData;

            dataGridView1.DataSource = agTmp3.fibTable.Wires;
            dataGridView2.DataSource = agTmp3.AbIN;
            dataGridView3.DataSource = agTmp3.AbOUT;
            dataGridView4.DataSource = agTmp3.Conn;
            dataGridView5.DataSource = agTmp3.unFib.addressList;
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
            ExtSrc.WireBand wire1 = item1a.findWireIN(id1);
            int band1 = wire1.lambdaCapactity;
            checkedListBox2.DataSource = bandItems.GetRange(0, band1);


            for (int i = 0; i < wire1.lambdas.Length; i++)
            {

                if (wire1.lambdas[i] == false)
                    checkedListBox2.SetItemCheckState(i, CheckState.Indeterminate);
                else { checkedListBox2.SetItemCheckState(i, CheckState.Unchecked); }

            }
            this.Refresh(); 
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView4_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            int IN = (comboBox2.SelectedItem as ExtSrc.Wire).distance;
            int OUT = (comboBox3.SelectedItem as ExtSrc.Wire).distance;
            int band = comboBox4.SelectedIndex + 1;

            if (IN < 1000 && OUT < 1000)
            {
                IN_label.Text = band.ToString();
                OUT_label.Text = band.ToString();
            }
            else
            {
                if (IN < OUT)
                {
                    IN_label.Text = band.ToString();
                    OUT_label.Text = (2 * band).ToString();
                }
                else if (OUT < IN)
                {
                    IN_label.Text = (2 * band).ToString();
                    OUT_label.Text = band.ToString();
                }
                else
                {
                    IN_label.Text = (2 * band).ToString();
                    OUT_label.Text = (2 * band).ToString();
                }
            }
            IN_label.Refresh();
            OUT_label.Refresh();
        }
    }
}
