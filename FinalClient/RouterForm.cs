using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using ExtSrc;
using System.Windows.Forms.DataVisualization.Charting;


namespace Router
{
    public partial class RouterForm : Form
    {
        // to samo jest w NewWire
        static readonly int GUARD_BAND = 10;
        //blabla
        private Router _router;
        private Dictionary<int, Chart> chartsByWireIDs;
        private Random random;
        private List<System.Drawing.Color> colors;
        
        public string LabelText
        {
            get
            {
                return this.labelName.Text;
            }
            set
            {
                this.labelName.Text = value;
            }
        }

        public RouterForm(Router router)
        {
            _router = router;
            random = new Random();
            colors = new List<Color>(new Color[]{
                Color.Black,
                Color.Blue,
                Color.Green,
                Color.SaddleBrown,
                Color.Pink,
                Color.Yellow,
                Color.Cyan,
                Color.Gray,
                Color.DarkViolet,
            });
            InitializeComponent();
            LabelText = _router.address;
            Console.SetOut(new TextBoxWriter(consoleOutput));
            var t = new Timer { Enabled = true, Interval = 1 * 1000 };
            t.Tick += delegate { Bind(); };

            chartsByWireIDs = new Dictionary<int, Chart>();
            foreach (NewWire wire in _router.localPhysicalWires.Wires)
            {
                TabPage page = new TabPage();
                page.Text = "Wire " + wire.ID;
                tabs.TabPages.Add(page);
                Chart chart = new Chart();
                initChart(chart);
                page.Controls.Add(chart);
                
                chartsByWireIDs.Add(wire.ID, chart);
            }
             
        }

        private void initChart(Chart chart)
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
           // System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();

            chart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(211)))), ((int)(((byte)(223)))), ((int)(((byte)(240)))));
            chart.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.TopBottom;
            chart.BorderlineColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(59)))), ((int)(((byte)(105)))));
            chart.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chart.BorderlineWidth = 2;
            chart.BorderSkin.SkinStyle = System.Windows.Forms.DataVisualization.Charting.BorderSkinStyle.Emboss;

            chartArea1.AxisX.IsMarginVisible = false;
            chartArea1.AxisX.LabelStyle.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            chartArea1.AxisX.LabelStyle.IsEndLabelVisible = false;
            chartArea1.AxisX.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.AxisX.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.AxisY.LabelStyle.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            chartArea1.AxisY.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.AxisY.Title = "Spectrum Assigment [GHz]";
            chartArea1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(165)))), ((int)(((byte)(191)))), ((int)(((byte)(228)))));
            chartArea1.BackGradientStyle = System.Windows.Forms.DataVisualization.Charting.GradientStyle.TopBottom;
            chartArea1.BackSecondaryColor = System.Drawing.Color.Transparent;
            chartArea1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            chartArea1.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.Name = "Default";
            chartArea1.ShadowColor = System.Drawing.Color.Transparent;
            chart.ChartAreas.Add(chartArea1);

            legend1.BackColor = System.Drawing.Color.Transparent;
            legend1.Enabled = true;
            legend1.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold);
            legend1.IsTextAutoFit = false;
            legend1.Name = "Default";
            chart.Legends.Add(legend1);
            chart.Dock = System.Windows.Forms.DockStyle.Fill;
        }

        public void Bind()
        {
            //            connectedWiresTable;
            connectedWiresTable.DataSource = null;
            connectedWiresTable.DataSource = (_router.localPhysicalWires.Wires.Select(w => new
            {
                Wire_ID = w.ID,
                Distance = w.distance,
                Width = w.spectralWidth,
                FSUCount = w.FrequencySlotUnitList.Count,
                FSUDictCount = w.FrequencySlotDictionary.Count
            }));
            //            clientTable;
            clientTable.DataSource = null;
            clientTable.DataSource = (_router.clientSocketDictionary.Select(d => new
            {
                IDX = d.Key,
                Client_ID = d.Value.ID,
                LOCAL = d.Value.socket.LocalEndPoint,
                REMOTE = d.Value.socket.RemoteEndPoint
            })).ToList();
            //            messagesTable;
            messagesTable.DataSource = null;
            messagesTable.DataSource = (_router.waitingMessages.Select(d => new
            {
                UNIQUE = d.Key,
                CLIENT_ID = d.Value.ID,
                BANDWITDHT = d.Value.data.bandwidthNeeded,
                DATA = d.Value.data.info
            })).ToList();
            //            toClientTable;
            toClientTable.DataSource = null;
            toClientTable.DataSource = (_router.TOclientConnectionsTable.clientConnectionTable.Select(d => new
            {
                WireID = d.Key[0],
                FS_ID = d.Key[1],
                Client_ID = d.Value
            })).ToList();
            //            fromClientTable;
            fromClientTable.DataSource = null;
            fromClientTable.DataSource = (_router.FROMclientConnectionsTable.clientConnectionTable.Select(d => new
            {
                WireID = d.Key[0],
                FS_ID = d.Key[1],
                Client_ID = d.Value
            })).ToList();
            //            frequencySlotsTable;
            frequencySlotsTable.DataSource = null;
            frequencySlotsTable.DataSource = (_router.freqSlotSwitchingTable.freqSlotSwitchingTable.Select(d => new
            {
                WireID_A = d.Key[0],
                FS_ID_A = d.Key[1],
                WireID_B = d.Value[0],
                FS_ID_B = d.Value[1]
            })).ToList();

            foreach (NewWire wire in _router.localPhysicalWires.Wires)
            {
                Chart chart;
                if (chartsByWireIDs.TryGetValue(wire.ID, out chart))
                {
                    chart.Series.Clear();
                    chart.ChartAreas["Default"].Axes[1].Maximum = wire.spectralWidth.Length;
                     var slotsCount = wire.FrequencySlotDictionary.Values.Count;
                    if(colors.Count < slotsCount)
                        for (int i = 0; i < slotsCount - colors.Count; i++)
                        {
                            colors.Add(Color.FromArgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)));
                        }
                    var counter = 0;
                    foreach (var freqSlot in wire.FrequencySlotDictionary.Values)
                    {
                        var height = freqSlot.FSUList.Count * 20;
                        var start = freqSlot.startingFreq;
                        //Console.WriteLine("start = " + start + ", height = " + height);
                        var key =
                            _router.UniqueConnections.Where(d => d.WireAndFsu[0] == wire.ID && d.WireAndFsu[1] == freqSlot.ID).Select(d => d.UniqueKey).FirstOrDefault();
                        string name;
                        if (key != null)
                            name = "Bandwidth " + key;//Convert.ToString(start);
                        else
                        {
                            name = "Bandwidth not known" + Convert.ToString(start);
                            //
                        }
                        var guardName = "Guard Band " + Convert.ToString(start);
                        var seriesBand = new System.Windows.Forms.DataVisualization.Charting.Series
                        {
                            Name = name,
                            Color = colors[counter],
                            IsVisibleInLegend = true,
                            IsXValueIndexed = false,
                            ChartType = SeriesChartType.Range
                        };
                        var seriesGuardBand = new System.Windows.Forms.DataVisualization.Charting.Series
                        {
                            Name = guardName,
                            Color = Color.Crimson,
                            IsVisibleInLegend = true,
                            IsXValueIndexed = false,
                            ChartType = SeriesChartType.Range
                        };
                        chart.Series.Add(seriesBand);
                        chart.Series.Add(seriesGuardBand);

                        double[] yValue1 = { start + height, start + height };
                        double[] yValue2 = { start, start};
                        double[] yValue1Guard = { start + height + GUARD_BAND, start + height + GUARD_BAND};
                        double[] yValue2Guard = { start + height, start + height };

                        chart.Series[name].Points.DataBindY(yValue1, yValue2);
                        chart.Series[guardName].Points.DataBindY(yValue1Guard, yValue2Guard);

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
            this.BeginInvoke(new MethodInvoker(Close));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _router.closing();
        }
    }
}
