using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtSrc
{
    public class Log
    {
        private const bool _debugState = false;
        private const bool _infoState = true;

        private const string _lrm = "LRM";
        private const string _ncc = "NCC";
        private const string _ccc = "CCC";
        private const string _cc = "CC";
        private const string _rc = "RC";
        private const string _info = "INFO";
        private const string _debug = "DEBUG";

        public static void i(string message)
        {
            if (_infoState)
                WriteLine(_info, message);
        }

        public static void d(string message)
        {
            if(_debugState)
                WriteLine(_debug, message);
        }

        public static void LRM(string message)
        {
            WriteLine(_lrm, message);
        }

        public static void NCC(string message)
        {
            WriteLine(_ncc, message);
        }

        public static void CCC(string message)
        {
            WriteLine(_ccc, message);
        }

        public static void RC(string message)
        {
            WriteLine(_rc, message);
        }

        public static void CC(string message)
        {
            WriteLine(_cc, message);
        }

        static void WriteLine(string tag, string message)
        {
            Console.WriteLine("["+tag+"] "+ message);
        }
    }
}
