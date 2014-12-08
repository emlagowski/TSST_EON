using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExtSrc;

namespace Agent
{
    public partial class Form2 : Form
    {
        Communication cm;
        private Action timerAction; 
        static System.Windows.Forms.Timer myTimer;
        private Action WireSourceSetter;
        public Form2()
        {

            InitializeComponent();
            Console.SetOut(new TextBoxWriter(consoleOutput));
            bandwidthTextBox.Text = "10";
            cm = new Communication(this);
            cm.Initialize();
            myTimer = new System.Windows.Forms.Timer();
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Interval = 1000;
            myTimer.Start();
            
          /*  WireSourceSetter = delegate
            {
                var list1 = new BindingList<int>();
                for (var i = 0; i < cm.dijkstraDataList.Count; i++)
                {
                    if (cm.dijkstraDataList.ElementAt(i).Equals(RouterComboBox.SelectedItem))
                        list1.Add(cm.dijkstraDataList.ElementAt(i).wireID);
                }
                var wiresBindingSource = new BindingSource
                {
                    DataSource = list1
                };
                WireComboBox.DataSource = wiresBindingSource.DataSource;
            };*/
            var list = new List<String> {"ssss", "seees"};
            comboBox1.DataSource = new BindingSource(list, null);
            BindingSource routerSource = new BindingSource
            {
                DataSource = cm.dijkstraDataList
            };
            RouterComboBox.DataSource = routerSource.DataSource;
          //  RouterComboBox.DataSource = new BindingSource(cm.dijkstraDataList, "routerID");
            RouterComboBox.DisplayMember = "routerID";
            RouterComboBox.ValueMember = "routerID";
           

         
        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {//Console.WriteLine("dsd");

            

            var prevSelRouter = routerListBox.SelectedItem;
            
           // RouterComboBox.DataSource = cm.dijkstraDataList.Select(d => d.routerID).Distinct().ToList();
            routerListBox.DataSource = cm.dijkstraDataList.Select(d => d.routerID).Distinct().ToList();
            if(prevSelRouter != null && routerListBox.Items.Contains(prevSelRouter))
            routerListBox.SelectedItem = prevSelRouter;
            
            /*   var list = new BindingList<int>();
            if (cm.dijkstraDataList.Count != 0)
            {
                for (var i = 0; i < cm.dijkstraDataList.Count; i++)
                {
                    if (RouterComboBox.SelectedIndex == -1) continue;
                    if (cm.dijkstraDataList.ElementAt(i).routerID.Equals(RouterComboBox.SelectedItem))
                        list.Add(cm.dijkstraDataList.ElementAt(i).wireID);
                }
            }
            var wiresBindingSource = new BindingSource
            {
                DataSource = list
            };
            var prevSelWire = WireComboBox.SelectedItem;
            WireComboBox.DataSource = wiresBindingSource.DataSource;
            if (prevSelWire != null && WireComboBox.Items.Contains(prevSelWire))
                WireComboBox.SelectedItem = prevSelWire;
            if (cm.routeHistoryList.Keys.Count != 0)
            {
                var list1 = new BindingList<string>();

                foreach (var s in cm.routeHistoryList.Keys)
                {
                    list1.Add(s[2]);
                }
                list1.Add("(New)");

                ConHashComboBox.DataSource = list1;
            }
            else
            {
                ConHashComboBox.DataSource = new BindingList<string> {"(New)"};
            }*/
            /*if(cm.dijkstraDataList.Count != 0)
            foreach (var v in cm.dijkstraDataList.Where(v => v.wireID.Equals(WireComboBox.SelectedItem)))
            {
                banwidthTrackBar.Maximum = v.wireDistance > 300 ? 1000 : 500;
            }*/


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
            Console.WriteLine("x");
//            var key = comboBox1.SelectedItem as String;
//            List<int[]> route = null;
//            foreach (var i in cm.routeHistoryList.Keys.Where(i => i[2] == key))
//            {
//                route = cm.routeHistoryList[i];
//            }
//            cm.disroute(route, key);
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void ConnDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void RemoveConnButton_Click(object sender, EventArgs e)
        {
            //RouterComboBox.BindingContext = new BindingContext();
           // RouterComboBox.DataSource = new BindingSource(cm.dijkstraDataList, "routerID");
           // RouterComboBox.ResetBindings();
            Console.WriteLine("click");
          //  RouterComboBox.DataSource = null;
          //  RouterComboBox.DataSource = cm.dijkstraDataList;
          //  RouterComboBox.DisplayMember = "routerID";
            //RouterComboBox.CreateControl();
            //RouterComboBox.CreateControl();
            RouterComboBox.DataSource = cm.dijkstraDataList.Select(d => d.routerID).Distinct().ToList();




            // RouterComboBox.DisplayMember = "routerID";
        }

        private void RouterComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
           // if (RouterComboBox.SelectedIndex != -1)
           // {
          //  if(RouterComboBox.InvokeRequired)
            //RouterComboBox.Invoke(WireSourceSetter);
           // else
           // {
                //SetWiresSource();
           // }
            // }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
          //  Console.WriteLine(trackBar1.Value);
            int tens = banwidthTrackBar.Value / 10;

            if (banwidthTrackBar.Value%10 >= 5)
            {
                bandwidthTextBox.Text = "" + (tens * 10 + 10);
            }
            else
            {
                bandwidthTextBox.Text = "" + tens * 10;
            }

        }
        /*void SetWiresSource()
        {
            BindingSource wiresBindingSource = new BindingSource
            {
                DataSource = cm.dijkstraDataList.
                    Where(d => d.routerID.Equals(RouterComboBox.SelectedItem)).Select(d => d.wireID).ToList()
            };
            WireComboBox.DataSource = wiresBindingSource.DataSource;
        }*/
    }
}
