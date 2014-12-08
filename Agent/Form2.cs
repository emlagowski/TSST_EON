﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
          
           

         
        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {//Console.WriteLine("dsd");
            
            var prevSelRouter = routerListBox.SelectedItem;
            
           // RouterComboBox.DataSource = cm.dijkstraDataList.Select(d => d.routerID).Distinct().ToList();
            routerListBox.DataSource = cm.dijkstraDataList.Select(d => d.routerID).Distinct().ToList();
            if(prevSelRouter != null && routerListBox.Items.Contains(prevSelRouter))
            routerListBox.SelectedItem = prevSelRouter;

            clientListBox.DataSource = cm.clientMap.Select(d => d.Key).ToList();

               var list = new BindingList<int>();
            if (cm.dijkstraDataList.Count != 0)
            {
                for (var i = 0; i < cm.dijkstraDataList.Count; i++)
                {
                    if (routerListBox.SelectedIndex == -1) continue;
                    if (cm.dijkstraDataList.ElementAt(i).routerID.Equals(routerListBox.SelectedItem))
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
            }
            if(cm.dijkstraDataList.Count != 0)
            foreach (var v in cm.dijkstraDataList.Where(v => v.wireID.Equals(WireComboBox.SelectedItem)))
            {
                banwidthTrackBar.Maximum = v.wireDistance > 300 ? 500 : 1000;
            }
            var connections = new List<String[]>();
            foreach (var d in cm.routeHistoryList.Values)
            {
                foreach (var d1 in d)
                {
                    if (d1[0] == (int)routerListBox.SelectedItem)
                    {
                        var tab = new string[3];
                        tab[0] = cm.routeHistoryList.FirstOrDefault(x => x.Value == d).Key[2];
                        tab[1] = d1[1].ToString();
                        tab[2] = d1[2].ToString();
                        connections.Add(tab);
                    }
                }
            }
            ConnDataGridView.DataSource = (connections.Select(d => new
            {
                HashKey = d[0],
                WireID = d[1],
                FSid = d[2]
            })).ToList();

          
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
            //RouterComboBox.DataSource = cm.dijkstraDataList.Select(d => d.routerID).Distinct().ToList();




            // RouterComboBox.DisplayMember = "routerID";
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
        private String generateUniqueKey()
        {
            Guid g = Guid.NewGuid();
            String str = Convert.ToBase64String(g.ToByteArray());
            str = str.Replace("=", "");
            str = str.Replace("+", "");
            str = str.Replace("/", "");
            return str;
        }
        private void SetConnButton_Click(object sender, EventArgs e)
        {
            if (clientATextBox.Text.Equals("") || clientBTextBox.Text.Equals("") || routeTextBox.Text.Equals(""))
            {
                MessageBox.Show("All fields must be filled.", "ERROR");
                return;
            }
            string pattern = @"\d*";
            string pattern1 = @"(\s*\d\s*)*";
            Regex rgx = new Regex(pattern);
            Regex rgx1 = new Regex(pattern1);
            if (!rgx.IsMatch(clientATextBox.Text) || !rgx.IsMatch(clientBTextBox.Text) || !rgx1.IsMatch(routeTextBox.Text))
            {
                Console.WriteLine("regex doesn't match");
                MessageBox.Show("Wrong text format int textboxes.", "ERROR");
                return;
            }
            String hashKey = generateUniqueKey();
            int[] route;
            string[] r = routeTextBox.Text.Split(' ');
            route = new int[r.Count()];
            for (var i = 0; i < r.Count(); i++)
            {
                route[i] = Convert.ToInt32(r[i].Trim());
            }
            cm.setRoute("127.0.0." + clientATextBox.Text, "127.0.0." + clientBTextBox.Text, banwidthTrackBar.Value, null, hashKey, route);

        }

        private void clientListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

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
