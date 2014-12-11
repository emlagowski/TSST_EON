using System;
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
using System.Windows.Forms.DataVisualization.Charting;
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
            //cm.Initialize();
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

            initChart();

        }

        private void initChart()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 =
                chart1.ChartAreas["ChartArea1"];//new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = chart1.Series["Series1"];//new System.Windows.Forms.DataVisualization.Charting.Series();
           // System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
           // System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();

            this.chart1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            this.chart1.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.TopBottom;
            this.chart1.BorderlineColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(59)))), ((int)(((byte)(105)))));
            this.chart1.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            this.chart1.BorderlineWidth = 2;
            this.chart1.BorderSkin.SkinStyle = System.Windows.Forms.DataVisualization.Charting.BorderSkinStyle.Emboss;
            chartArea1.Area3DStyle.Inclination = 15;
            chartArea1.Area3DStyle.IsClustered = true;
            chartArea1.Area3DStyle.IsRightAngleAxes = false;
            chartArea1.Area3DStyle.Perspective = 10;
            chartArea1.Area3DStyle.Rotation = 10;
            chartArea1.Area3DStyle.WallWidth = 0;
            chartArea1.AxisX.IsMarginVisible = false;
            chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            chartArea1.AxisX.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.AxisY.LabelStyle.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            chartArea1.AxisY.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(191)))), ((int)(((byte)(0)))));
            chartArea1.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.TopBottom;
            chartArea1.BackSecondaryColor = System.Drawing.Color.Transparent;
            chartArea1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.Name = "Default";
            chartArea1.ShadowColor = System.Drawing.Color.Transparent;
           // this.chart1.ChartAreas.Add(chartArea1);
           // legend1.BackColor = System.Drawing.Color.Transparent;
           // legend1.Enabled = false;
           // legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
           // legend1.IsTextAutoFit = false;
           // legend1.Name = "Default";
           // this.chart1.Legends.Add(legend1);
            //this.chart1.Location = new System.Drawing.Point(16, 53);
            //chart1.AutoSize = true;
            
          //  this.chart1.Name = "chart1";
          /*  series1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(26)))), ((int)(((byte)(59)))), ((int)(((byte)(105)))));
            series1.ChartArea = "Default";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.SplineRange;
            series1.Color = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(224)))), ((int)(((byte)(64)))), ((int)(((byte)(10)))));
            series1.Legend = "Default";
            series1.Name = "Series2";
            series1.YValuesPerPoint = 2;
            series2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(26)))), ((int)(((byte)(59)))), ((int)(((byte)(105)))));
            series2.ChartArea = "Default";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.SplineRange;
            series2.Color = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(65)))), ((int)(((byte)(140)))), ((int)(((byte)(240)))));
            series2.Legend = "Default";
            series2.Name = "Default";
            series2.YValuesPerPoint = 2;*/
            //this.chart1.Series.Add(series1);
          //  this.chart1.Series.Add(series2);
          //  this.chart1.Size = new System.Drawing.Size(412, 296);
          //  this.chart1.TabIndex = 1;
            //chart1.Dock = System.Windows.Forms.DockStyle.Fill;
           // ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            // Populate series data with data
            double[] yValue1 = { 50, 50/*, 50, 50, 50, 87, 50, 87, 64, 34*/ };
            double[] yValue2 = { 30, 30/*, 30, 30, 30, 47, 40, 70, 34, 20 */};
            chart1.Series["Series1"].Points.DataBindY(yValue1, yValue2);

            // Set chart type 
           // chart1.Series["Default"].ChartType = SeriesChartType.SplineRange;

            // Set Spline Range line tension
            chart1.Series["Series1"]["LineTension"] = "0.5";

            // Disable X axis margin
           // chart1.ChartAreas["Default"].AxisX.IsMarginVisible = false;

            // Show as 3D
            //chart1.ChartAreas["Default"].Area3DStyle.Enable3D = true;
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
                
                ConHashComboBox.DataSource = list1;
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
            if (ConHashComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("You must select connection hashkey", "ERROR");
            }
            var routeHist = cm.routeHistoryList.Where(d => d.Key[2].Equals(ConHashComboBox.SelectedItem.ToString())).Select(d => d.Value).FirstOrDefault();
            if (routeHist != null)
            {
                cm.disroute(routeHist, ConHashComboBox.SelectedItem.ToString());
                String[] key = cm.routeHistoryList.FirstOrDefault(d => d.Value.Equals(routeHist)).Key;
                cm.routeHistoryList.Remove(key);
            }

            else
            {
                Console.WriteLine("error, no connection with this haskey in routeHistoryList");
            }
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
