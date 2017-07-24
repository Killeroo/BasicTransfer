using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace Basic_Transfer
{
    class Program
    {
        // Seperate UI and data into seperate threads, access UI through deligates
        // Multi thread file creation
        // Multi thread file sending
        // Scan should not use classfull address, should instead specify the start and end address to scan EG 192.168.1.1-10 to scan hosts 1 to 10 on last octlet

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

            // mode selection via first argument
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
        }

        static void Send(string pathToFile, string endAddress)
        {
            // Create socket
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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
                Error("SocketException - " + e.Message);
            }
            catch (Exception e)
            {
                Error(e.GetType().ToString().Split('.').Last() + " - " + e.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Drag & Drop file you want to transfer...");
            Console.WriteLine();
            string path = "";
            bool running = true;

            // Captured files that are dropped into the console
            // NOTE: When a file is dragged and dropped into the console,
            // the path to the file is inputted as a set of keyboard inputs.
            // We capture the keyboard inputs when they are avaliable to get
            // the dropped file's path.
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

                    // Create image of file
                    using (FileImage fi = new FileImage(pathToFile))
                    {
                        // Send FileImage object
                        try
                        {
                            // Serialise FileImage object onto the socket's network stream
                            Console.WriteLine("Transfering file [{0}]...", fi.name);
                            NetworkStream objStream = new NetworkStream(sock);
                            byte[] buffer = ToByteArray<FileImage>(fi);
                            objStream.Write(buffer, 0, buffer.Length);
                            Console.WriteLine("Transfer complete. ");
                        }
                        catch (IOException e)
                        {
                            Error("IOException - " + e.Message, false);
                        }
                        catch (Exception e)
                        {
                            Error(e.GetType().ToString().Split('.').Last() + " - " + e.Message, false);
                        }
                    }
                }
            }

            // Clean up
            Console.WriteLine("Closing connection...");
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();

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
                    Console.WriteLine("[{0}] connected", client.Client.RemoteEndPoint.ToString());
                    bool running = true;

                    while (running)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            byte[] buffer = new byte[4096 * 50];
                            int bytesRead;

                            Console.WriteLine("Recieving file...");
                            try
                            {
                                do
                                {
                                    // Read data from client stream
                                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                                    ms.Write(buffer, 0, bytesRead);
                                }
                                while (stream.DataAvailable);
                            }
                            catch (SerializationException e)
                            {
                                Error("SerialiazationException - " + e.Message, false);
                            }
                            catch (IOException e)
                            {
                                Error("IOException - " + e.Message, false);
                            }
                            catch (Exception e)
                            {
                                Error(e.GetType().ToString().Split('.').Last() + " - " + e.Message, false);
                            }

                            // Deserialise stream
                            using (FileImage fi = FromMemoryStream<FileImage>(ms))
                            {
                                Console.WriteLine("Transfer complete.");

                                // Create transfered file
                                Console.WriteLine("Creating \"" + fi.name + "\"...");
                                fi.createFile(transferPath);
                            }
                        }
                    }
                }
            }
        }

        static string GetLocalIPAddress()
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
    }
}

// MessageBox.Show("use this in the final project");
