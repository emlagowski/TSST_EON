using System;
using System.Collections.Generic;
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
            if (_output != null)
            {
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
        }

        public void WriteLinePrivate(char value)
        {
            base.Write(value);
            if (_output != null)
                _output.AppendText(value.ToString());
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}
