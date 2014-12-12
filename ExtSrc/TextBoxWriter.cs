using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExtSrc
{
    public class TextBoxWriter : TextWriter
    {
        TextBox _output = null;

        public TextBoxWriter(TextBox output)
        {
            _output = output;
        }

        public override void Write(char value)
        {
            try
            {
                if (_output == null) return;
                if (this._output.InvokeRequired)
                {
                    this._output.BeginInvoke(new Action<char>(WriteLinePrivate), new object[] {value});
                    return;
                }
                else
                {
                    WriteLinePrivate(value);
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (Win32Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void WriteLinePrivate(char value)
        {
            try
            {
                base.Write(value);
                if (_output != null)
                    _output.AppendText(value.ToString());
            }
            catch (ObjectDisposedException e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
