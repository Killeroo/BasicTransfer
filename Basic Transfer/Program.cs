using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.ComponentModel;

namespace Basic_Transfer
{
    class Program
    {

        // Scan should not use classfull address, should instead specify the start and end address to scan EG 192.168.1.1-10 to scan hosts 1 to 10 on last octlet
        // Or use CDN addresses

        static void Main(string[] args)
        {

            // Basic arguments check
            if (args.Length < 2)
            {
                Console.WriteLine("Not enough arguments.");
                Console.WriteLine("Use either:");
                Console.WriteLine("BasicTransfer.exe /send *FILE_PATH* *IP_ADDRESS*");
                Console.WriteLine("BasicTransfer.exe /recieve *TRANSFER_PATH*");

                return;
            }

            // model selection via first argument
            if (args[0] == "/recieve")
                Recieve(args[1]);
            else if (args[0] == "/send")
                Send(args[1], args[2]);
            else
            {
                Console.WriteLine("Not enough arguments.");
                Console.WriteLine("Use either:");
                Console.WriteLine("BasicTransfer.exe /send *FILE_PATH* *IP_ADDRESS*");
                Console.WriteLine("BasicTransfer.exe /recieve *TRANSFER_PATH*");
            }

            // Drag and drop loop //
            // Captured files that are dropped into the console
            // NOTE: When a file is dragged and dropped into the console,
            // the path to the file is inputted as a set of keyboard inputs.
            // We capture the keyboard inputs when they are avaliable to get
            // the dropped file's path.
            bool running = true;
            string path = "";
            while (running)
            {
                // Read all characters while keys are being pressed
                do
                {
                    ConsoleKeyInfo keyinfo = Console.ReadKey();
                    path += keyinfo.KeyChar;
                }
                while (Console.KeyAvailable);

                // Once we have captured some text
                if (path != "")
                {
                    // Reset console
                    Console.WriteLine();
                    path = "";

                    // Send(path, address);
                }
            }

        }

        static void Send(string pathToFile, string endAddress)
        {
            // Create socket
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Create image of file
            FileImage fi = new FileImage(pathToFile);

            // Setup network endpoint
            IPAddress ip = IPAddress.Parse(endAddress);
            IPEndPoint ep = new IPEndPoint(ip, 13450);

            try
            {
                // Try to connect to end point
                sock.Connect(ep);
                Console.WriteLine("Connected to {0}", ep.ToString());
            }
            catch (SocketException e)
            {
                Error("Socket Exception - " + e.Message);
            }

            // Serialise FileImage object onto the socket's network stream
            Console.WriteLine("Transfering file [{0}]...", fi.Name);
            NetworkStream objStream = serializeFileImage(fi, sock);
            Console.WriteLine("Transfer complete. ");

            // Clean up
            Console.WriteLine("Closing connection...");
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
            fi.Dispose();

        }

        static void Recieve(string transferPath)
        {
            // Create listener for port 11000
            TcpListener listener = new TcpListener(IPAddress.Parse(GetLocalIPAddress()), 13450);
            listener.Start();
            Console.WriteLine("Listening on {0}...", listener.LocalEndpoint.ToString());

            // Connection loop
            while (true)
            {

                using (var client = listener.AcceptTcpClient())
                using (var stream = client.GetStream())
                {
                    // Deserialise stream
                    Console.WriteLine("[{0}] connected, recieving file...", client.Client.RemoteEndPoint.ToString());
                    using (FileImage fi = deserializeFileImage(stream))
                    {
                        Console.WriteLine("Transfer complete.");

                        // Create transfered file
                        Console.WriteLine("Creating \"" + fi.Name + "\"...");
                        fi.CreateFile(transferPath);
                    }
                }
            }
        }

        static NetworkStream serializeFileImage(FileImage fi, Socket sock)
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

        static FileImage deserializeFileImage(NetworkStream netStream)
        {
            // Deseralize our object from the stream
            IFormatter formatter = new BinaryFormatter();
            FileImage fi = (FileImage)formatter.Deserialize(netStream);

            // Clean up
            netStream.Close();

            // Return the deserialized object
            return fi;
        }

        // Replace above functions with these:
        /*
          
        /// <summary>
        /// Conversion methods
        /// </summary>
        public static byte[] ToByteArray<T>(T obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public static T FromByteArray<T>(byte[] data)
        {
            if (data == null)
                return default(T);

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }
        public static T FromMemoryStream<T>(MemoryStream ms)
        {
            if (ms == null)
                return default(T);

            BinaryFormatter bf = new BinaryFormatter();
            ms.Seek(0, SeekOrigin.Begin);
            Object obj = bf.Deserialize(ms);
            ms.Close();
            return (T)obj;
        }
        */

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
            Console.Read();

            // Terminate application if exit flag is set
            if (exitFlag)
                System.Environment.Exit(1);
        }
    }
}

// MessageBox.Show("use this in the final project");
