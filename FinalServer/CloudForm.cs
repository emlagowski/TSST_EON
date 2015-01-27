#region

using System;
using System.ComponentModel;
using System.Windows.Forms;
using ExtSrc;

#endregion

namespace Cloud
{
    public partial class CloudForm : Form
    {
        Cloud _server;
        public CloudForm(Cloud server)
        {
            _server = server;
            InitializeComponent();
            Console.SetOut(new TextBoxWriter(consoleOutput));
        }

//        private void Form1_Paint(object sender, PaintEventArgs e)
//        {
//            System.Drawing.Graphics graphicsObj;
//
//            graphicsObj = this.CreateGraphics();
//
//            Pen myPen = new Pen(System.Drawing.Color.Red, 5);
//
//            graphicsObj.DrawLine(myPen, 20, 20, 200, 210);
//        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _server.Close();
        }
    }
}
