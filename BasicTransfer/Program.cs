using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Threading;
using System.ComponentModel;
using System.IO;

namespace Basic_Transfer
{
    class Program
    {

        // Scan should not use classfull address, should instead specify the start and end address to scan EG 192.168.1.1-10 to scan hosts 1 to 10 on last octlet
        // Or use CDN addresses

        static void Main(string[] args)
        {
            // Basic arguments check
            if (args.Length < 2 || args.Length == 0 || args.Length == 1)
            {
                Console.WriteLine("Not enough arguments.");
                Console.WriteLine("Use either:");
                Console.WriteLine("BasicTransfer.exe /send *IP_ADDRESS* OR *IP_ADDRESS* *FILE_PATH*");
                Console.WriteLine("BasicTransfer.exe /recieve *TRANSFER_PATH* HACK:*LOCAL ADDRESS*");

                return;
            }

            // model selection via first argument
            if (args[0] == "/recieve")
                Recieve(args[1], args[2]);
            else if (args[0] == "/send" && args.Length == 3)
                Send(args[1], args[2]);

            // Drag and drop loop
            // NOTE: We capture drap and drop files as a series of ReadKey events
            Console.WriteLine("Drag and drop a file to send it...");
            Console.WriteLine("Press Ctrl-C to end");
            while (true)
            {
                string path = "";

                // Read all characters while keys are being pressed
                try
                {
                    do
                    {
                        ConsoleKeyInfo keyinfo = Console.ReadKey();
                        path += keyinfo.KeyChar;
                    }
                    while (Console.KeyAvailable);
                    Console.WriteLine();
                }
                catch (NotSupportedException)
                {
                    Error("Drag and drop not supported. Please use [BasicTransfer.exe /send *FILE_PATH* *IP_ADDRESS*] instead.");
                }

                // Strip illegal characters from string
                foreach (char c in Path.GetInvalidPathChars())
                    path = path.Replace(c.ToString(), "");

                // Once we have captured some text, send file
                if (path != "") 
                    Send(path.Trim('"'), args[1]);

                // Reset onsole
                if (path != "")
                {
                    Console.WriteLine();
                    path = "";
                }
            }

        }

        static void Send(string pathToFile, string endAddress)
        {
            // Create socket
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Create image of file
            FileImage fi = null;
            try
            { 
                fi = new FileImage(pathToFile);
            }
            // I know these are pretty sweaping catches but we really don't
            // need to handle specific exceptions we just need to keep going
            catch (Exception e)
            {
                Error(e.GetType().ToString() + " - " + e.Message, false);
                return;
            }

            // Setup network endpoint
            IPAddress ip = IPAddress.Parse(endAddress);
            IPEndPoint ep = new IPEndPoint(ip, 13450);

            try
            {
                // Try to connect to end point
                Console.Write("Connecting to [{0}]...", ep.ToString());
                LoadingSpinner.Start();
                sock.Connect(ep);
                LoadingSpinner.Stop();
            }
            catch (Exception e)
            {
                // Incase we exceptioned before we could stop the spinner
                LoadingSpinner.Stop();
                Error(e.GetType().ToString() + " - " + e.Message);
            }

            try
            {
                // Serialise FileImage object onto the socket's network stream
                Console.Write("Sending file [{0}]...", fi.Name);
                LoadingSpinner.Start();
                NetworkStream objStream = SerializeFileImage(fi, sock);
                LoadingSpinner.Stop();
                Console.WriteLine("Transfer complete. ");
            }
            catch (Exception e)
            {
                LoadingSpinner.Stop();
                Error(e.GetType().ToString() + " - " + e.Message, false);
            }

            // Clean up
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
            fi.Dispose();
            Console.WriteLine("Connection closed.");

        }

        static void Recieve(string transferPath, string address)
        {
            // Create listener for port 11000
            TcpListener listener = new TcpListener(IPAddress.Parse(address), 13450);
            listener.Start();
            Console.WriteLine("Listening on {0}...", listener.LocalEndpoint.ToString());

            // Connection loop
            while (true)
            {
                try
                {
                    using (var client = listener.AcceptTcpClient())
                    using (var stream = client.GetStream())
                    {
                        // Deserialise stream
                        Console.Write("[{0}] is sending a file, processing...", client.Client.RemoteEndPoint.ToString().Split(':')[0]);
                        LoadingSpinner.Start();
                        using (FileImage fi = DeserializeFileImage(stream))
                        {
                            LoadingSpinner.Stop();
                            Console.WriteLine("Transfer complete.");

                            // Create transfered file
                            Console.Write("Creating \"" + fi.Name + "\"...");
                            fi.CreateFile(transferPath);
                        }
                    }
                }
                catch (Exception e)
                {
                    Error(e.GetType().ToString() + " - " + e.Message, false);
                }
            }
        }

        static NetworkStream SerializeFileImage(FileImage fi, Socket sock)
        {
            // Create a stream for storing our serialized object
            NetworkStream netStream = new NetworkStream(sock);

            // Serialize the object onto the stream transport medium
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(netStream, fi);

            // Clean up
            netStream.Close();

            // Return the network stream containing our serialized object
            return netStream;
        }

        static FileImage DeserializeFileImage(NetworkStream netStream)
        {
            // Deseralize our object from the stream
            IFormatter formatter = new BinaryFormatter();
            FileImage fi = (FileImage)formatter.Deserialize(netStream);

            // Clean up
            netStream.Close();

            // Return the deserialized object
            return fi;
        }

        static String GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            return "";
        }

        public static void Error(string message, bool exitFlag = true)
        {
            // Basic error message formatting
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
            //Console.Read();

            // Terminate application if exit flag is set
            if (exitFlag)
                System.Environment.Exit(1);
        }
    }
}
