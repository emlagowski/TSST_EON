using System;
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

namespace Router
{
    public partial class RouterForm : Form
    {
        Router _router;
        
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
            InitializeComponent();
            LabelText = _router.address;
            Console.SetOut(new TextBoxWriter(consoleOutput));
            var t = new Timer { Enabled = true, Interval = 1 * 1000 };
            t.Tick += delegate { Bind(); };
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
        }

        public void Finish()
        {
            this.BeginInvoke(new MethodInvoker(Close));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _router.closing();
            Console.WriteLine("papa");
        }
    }
}
