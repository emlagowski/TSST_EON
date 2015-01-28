#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ExtSrc;

#endregion

namespace Node
{
    public partial class NodeForm : Form
    {
        // to samo jest w NewWire
        private const int GuardBand = 10;
        private readonly Node _node;
        private readonly Dictionary<int, Chart> chartsByWireIDs;
        private readonly List<Color> colors;
        private readonly Random random;

        public NodeForm(Node node)
        {
            _node = node;
            random = new Random();
            colors = new List<Color>( /*new Color[]{
                Color.Black,
                Color.Blue,
                Color.Green,
                Color.SaddleBrown,
                Color.Pink,
                Color.Yellow,
                Color.Cyan,
                Color.Gray,
                Color.DarkViolet,
            }*/);

            foreach (object colorValue in Enum.GetValues(typeof (KnownColor)))
            {
                colors.Add(Color.FromKnownColor((KnownColor) colorValue));
            }
            colors.Remove(Color.Crimson);
            // Shuffle(colors);
            InitializeComponent();
            LabelText = _node.address.Substring(_node.address.Length - 1, 1);
            Text = "Node " + LabelText;
            Console.SetOut(new TextBoxWriter(consoleOutput));
            var t = new Timer {Enabled = true, Interval = 1*1000};
            t.Tick += delegate { Bind(); };

            chartsByWireIDs = new Dictionary<int, Chart>();
            foreach (NewWire wire in _node.LocalPhysicalWires.Wires)
            {
                var page = new TabPage();
                page.Text = "Wire " + wire.ID;
                tabs.TabPages.Add(page);
                var chart = new Chart();
                initChart(chart);
                page.Controls.Add(chart);

                chartsByWireIDs.Add(wire.ID, chart);
            }
        }

        public string LabelText
        {
            get { return labelName.Text; }
            set { labelName.Text = value; }
        }

        public static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private void initChart(Chart chart)
        {
            var chartArea1 = new ChartArea();
            var legend1 = new Legend();
            // System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();

            chart.BackColor = Color.FromArgb(211, 223, 240);
            chart.BackGradientStyle = GradientStyle.TopBottom;
            chart.BorderlineColor = Color.FromArgb(26, 59, 105);
            chart.BorderlineDashStyle = ChartDashStyle.Solid;
            chart.BorderlineWidth = 2;
            chart.BorderSkin.SkinStyle = BorderSkinStyle.Emboss;

            chartArea1.AxisX.IsMarginVisible = false;
            chartArea1.AxisX.LabelStyle.Font = new Font("Trebuchet MS", 8.25F, FontStyle.Bold);
            chartArea1.AxisX.LabelStyle.IsEndLabelVisible = false;
            chartArea1.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea1.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea1.AxisY.LabelStyle.Font = new Font("Trebuchet MS", 8.25F, FontStyle.Bold);
            chartArea1.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea1.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea1.AxisY.Title = "Spectrum Assigment [GHz]";
            chartArea1.BackColor = Color.FromArgb(64, 165, 191, 228);
            chartArea1.BackGradientStyle = GradientStyle.TopBottom;
            chartArea1.BackSecondaryColor = Color.Transparent;
            chartArea1.BorderColor = Color.FromArgb(64, 64, 64, 64);
            chartArea1.BorderDashStyle = ChartDashStyle.Solid;
            chartArea1.Name = "Default";
            chartArea1.ShadowColor = Color.Transparent;
            chart.ChartAreas.Add(chartArea1);

            legend1.BackColor = Color.Transparent;
            legend1.Enabled = true;
            legend1.Font = new Font("Trebuchet MS", 8.25F, FontStyle.Bold);
            legend1.IsTextAutoFit = false;
            legend1.Name = "Default";
            chart.Legends.Add(legend1);
            chart.Dock = DockStyle.Fill;
        }

        public void Bind()
        {
            //            messageHistoryTable;
            messageHistoryTable.DataSource = null;
            //Log.d(_node.messageHistory.Count);
            messageHistoryTable.DataSource = (_node.MessageHistory.Select(w => new
            {
                Type = w.Key,
                Message = w.Value.info
            })).ToList();
            //            clientTable;
//            clientTable.DataSource = null;
//            clientTable.DataSource = (_node.clientSocketDictionary.Select(d => new
//            {
//                IDX = d.Key,
//                Client_ID = d.Value.ID,
//                LOCAL = d.Value.socket.LocalEndPoint,
//                REMOTE = d.Value.socket.RemoteEndPoint
//            })).ToList();
            //            messagesTable;

            //            frequencySlotsTable;
            frequencySlotsTable.DataSource = null;
            frequencySlotsTable.DataSource = (_node.FreqSlotSwitchingTable.freqSlotSwitchingTable.Select(d => new
            {
                WireID_A = d.Key[0],
                FS_ID_A = d.Key[1],
                WireID_B = d.Value[0],
                FS_ID_B = d.Value[1]
            })).ToList();

            foreach (NewWire wire in _node.LocalPhysicalWires.Wires)
            {
                Chart chart;
                if (chartsByWireIDs.TryGetValue(wire.ID, out chart))
                {
                    chart.Series.Clear();
                    chart.ChartAreas["Default"].Axes[1].Maximum = wire.spectralWidth.Length;
                    int slotsCount = wire.FrequencySlotDictionary.Values.Count;
                    if (colors.Count < slotsCount)
                        for (int i = 0; i < slotsCount - colors.Count; i++)
                        {
                            colors.Add(Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)));
                        }
                    int counter = 0;
                    foreach (FrequencySlot freqSlot in wire.FrequencySlotDictionary.Values)
                    {
                        int height = freqSlot.FSUList.Count*10;
                        int start = freqSlot.startingFreq;
                        string name = "FSid = " + freqSlot.ID;
                        //Log.d("start = " + start + ", height = " + height);
                        /* KeyValuePair<int[], int> k = _node.FROMclientConnectionsTable.clientConnectionTable.FirstOrDefault(d => d.Key[0] == wire.ID && d.Key[1] == freqSlot.ID);
                       KeyValuePair<int[], int> s = _node.TOclientConnectionsTable.clientConnectionTable.FirstOrDefault(d => d.Key[0] == wire.ID && d.Key[1] == freqSlot.ID);
                                
                       
                       KeyValuePair<int[], int[]> z = _node.freqSlotSwitchingTable.freqSlotSwitchingTable.FirstOrDefault(d => d.Key[0] == wire.ID && d.Key[1] == freqSlot.ID);
                        if (k.Key != null)
                        {
                            name = "Start -> wireID = " + k.Key[0] + " FS_ID = " + k.Key[1];
                        }
                        else if (s.Key != null)
                        {
                            name = "FS_ID = " + s.Key[1]+" -> END";
                        }
                        else
                        {
                            name = "{FS_ID = " + z.Key[1] + "} -> {wireID = " + z.Value[0] + " FS_ID = " + z.Value[1]+"}";
                        }*/

                        /*  var key = "gg";
                        if (key != null)
                            name = "Bandwidth " + key;//Convert.ToString(start);
                        else
                        {
                            name = "Bandwidth not known" + Convert.ToString(start);
                            //
                        }*/
                        string guardName = "Guard Band " + Convert.ToString(freqSlot.ID);
                        var seriesBand = new Series
                        {
                            Name = name,
                            Color = colors[freqSlot.ID],
                            IsVisibleInLegend = true,
                            IsXValueIndexed = false,
                            ChartType = SeriesChartType.Range
                        };
                        var seriesGuardBand = new Series
                        {
                            Name = guardName,
                            Color = Color.Crimson,
                            IsVisibleInLegend = false,
                            IsXValueIndexed = false,
                            ChartType = SeriesChartType.Range
                        };
                        var seriesGuardBand1 = new Series
                        {
                            Name = guardName + "_1",
                            Color = Color.Crimson,
                            IsVisibleInLegend = false,
                            IsXValueIndexed = false,
                            ChartType = SeriesChartType.Range
                        };
                        chart.Series.Add(seriesBand);
                        chart.Series.Add(seriesGuardBand);
                        chart.Series.Add(seriesGuardBand1);

                        double[] yValue1 = {start + height - GuardBand, start + height - GuardBand};
                        double[] yValue2 = {start + GuardBand, start + GuardBand};
                        double[] yValue1Guard = {start + height, start + height};
                        double[] yValue2Guard = {start + height - GuardBand, start + height - GuardBand};
                        double[] yValue1Guard1 = {start + GuardBand, start + GuardBand};
                        double[] yValue2Guard1 = {start, start};

                        chart.Series[name].Points.DataBindY(yValue1, yValue2);
                        chart.Series[guardName].Points.DataBindY(yValue1Guard, yValue2Guard);
                        chart.Series[guardName + "_1"].Points.DataBindY(yValue1Guard1, yValue2Guard1);

                        /* var legendItem = new LegendItem();
                        legendItem.SeriesName = name;
                        legendItem.ImageStyle = LegendImageStyle.Rectangle;
                        legendItem.BorderColor = Color.Transparent;
                        legendItem.Name = name + "_legend_item";
                        legendItem.Color = seriesGuardBand.Color;
                        
                        chart.Legends["Default"].CustomItems.Add(legendItem);*/


                        counter++;
                        if (counter >= colors.Count) counter = 0;
                    }
                }
            }
        }

        public void Finish()
        {
            BeginInvoke(new MethodInvoker(Close));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _node.Closing();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (MsgTextBox.Text.Equals("") || PortTextBox.Text.Equals(""))
            {
                MessageBox.Show("All fields must be filled.", "ERROR");
                return;
            }
            string pattern = @"\d*";

            var rgx = new Regex(pattern);

            if (!rgx.IsMatch(PortTextBox.Text))
            {
                Log.d("regex doesn't match");
                MessageBox.Show("Wrong text format in textboxes.", "ERROR");
                return;
            }
            var data = new Data(MsgTextBox.Text.Length*Node.BandPerChar, MsgTextBox.Text);

            //var id = _node.address.Substring(_node.address.Length - 1, 1);

            _node.MessageToSend("127.0.1." + Convert.ToInt32(PortTextBox.Text), data);

//            _node.WaitingMsgs.Add(new KeyValuePair<int[], DataAndID>
//                (new int[] { Convert.ToInt32(PortTextBox.Text), Convert.ToInt32(FSTextBox.Text) }, new ExtSrc.DataAndID(data, Int32.Parse(id))));
        }

        private void connectedWiresTable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {
        }

        private void clientTable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void RouterForm_Load(object sender, EventArgs e)
        {
            if (!_node.isEdge)
            {
                MsgLabel.Visible = false;
                PortIdLabel.Visible = false;
                MsgTextBox.Visible = false;
                PortTextBox.Visible = false;
                SendButton.Visible = false;
                //groupBox2.Visible = false;
                //groupBox1.Location = new Point(0, 30);
            }
            else
            {
                RunButton.Visible = false;
            }
        }

        private void RunButton_CheckedChanged(object sender, EventArgs e)
        {
            if (RunButton.Checked)
            {
                RunButton.Text = "Enable";
                _node.Enabled = false;
                _node.UnregisterToAgent();
            }
            else
            {
                RunButton.Text = "Disable";
                _node.Enabled = true;
                _node.RegisterToAgent();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                _node.Disroute("127.0.1." + Convert.ToInt32(PortTextBox.Text));
            }
            catch (Exception)
            {
                
            }
        }
    }
}