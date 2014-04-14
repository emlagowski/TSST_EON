using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace Router
{
    class Router
    {
        String hostname;
        int port;
        int timeout_milliseconds;
        TcpClient tcpClient;
        bool connected;
        Exception exception;

        public Router(String hostname, int port, int timeout_milliseconds)
        {
            this.hostname = hostname;
            this.port = port;
            this.timeout_milliseconds = timeout_milliseconds;
            connected = false;
        }

        public TcpClient Connect()
        {
            // kick off the thread that tries to connect
            connected = false;
            exception = null;
            Thread thread = new Thread(new ThreadStart(BeginConnect));
            thread.IsBackground = true; // So that a failed connection attempt 
            // wont prevent the process from terminating while it does the long timeout
            thread.Start();

            // wait for either the timeout or the thread to finish
            thread.Join(timeout_milliseconds);

            if (connected == true)
            {
                // it succeeded, so return the connection
                thread.Abort();
                return tcpClient;
            }
            if (exception != null)
            {
                // it crashed, so return the exception to the caller
                thread.Abort();
                throw exception;
            }
            else
            {
                // if it gets here, it timed out, so abort the thread and throw an exception
                thread.Abort();
                string message = string.Format("TcpClient connection to {0}:{1} timed out",
                  hostname, port);
                throw new TimeoutException(message);
            }
        }

        protected void BeginConnect()
        {
            try
            {
                tcpClient = new TcpClient(hostname, port);
                // record that it succeeded, for the main thread to return to the caller
                connected = true;
            }
            catch (Exception ex)
            {
                // record the exception for the main thread to re-throw back to the calling code
                exception = ex;
            }
        }



    }
}
