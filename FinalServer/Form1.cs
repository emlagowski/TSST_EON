using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using ExtSrc;

namespace Cloud
{
    public partial class Form1 : Form
    {
        Cloud _server;
        public Form1(Cloud server)
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
