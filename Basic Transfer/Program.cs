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
        // Seperate UI and data into seperate threads, access UI through deligates
        // Multi thread file creation
        // Multi thread file sending
        // Implement dispose for FIleImage, remove GC call
        // Could send filename in first 20 bytes of stream then use file stream to write directly to file but loops and handling of stream too compicated cba
        // Scan should not use classfull address, should instead specify the start and end address to scan EG 192.168.1.1-10 to scan hosts 1 to 10 on last octlet

        static void Main(string[] args)
        {

            // Basic arguments check
            if (args.Length < 2)
            {
                Console.WriteLine("Not enough arguments.");
                Console.WriteLine("Use either:");
                Console.WriteLine("BasicTransfer.exe /send *FILE_PATH* *IP_ADDRESS*");
                Console.WriteLine("BasicTransfer.exe /listen *TRANSFER_PATH*");

                return;
            }

            // model selection via first argument
            if (args[0] == "/recieve")
                recieve(args[1]);
            else if (args[0] == "/send")
                send(args[1], args[2]);
            else
            {
                Console.WriteLine("Not enough arguments.");
                Console.WriteLine("Use either:");
                Console.WriteLine("BasicTransfer.exe /send *FILE_PATH* *IP_ADDRESS*");
                Console.WriteLine("BasicTransfer.exe /listen *TRANSFER_PATH*");
            }
        }

        static void send(string pathToFile, string endAddress)
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
                errorMsg("Socket Exception - " + e.Message);
            }

            // Serialise FileImage object onto the socket's network stream
            Console.WriteLine("Transfering file [{0}]...", fi.name);
            NetworkStream objStream = serializeFileImage(fi, sock);
            Console.WriteLine("Transfer complete. ");

            // Clean up
            Console.WriteLine("Closing connection...");
            sock.Shutdown(SocketShutdown.Both);
            sock.Close();
            fi.Dispose();

        }

        static void recieve(string transferPath)
        {
            // Create listener for port 11000
            TcpListener listener = new TcpListener(IPAddress.Parse(getLocalIPAddress()), 13450);
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
                        Console.WriteLine("Creating \"" + fi.name + "\"...");
                        fi.createFile(transferPath);
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

        static String getLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            return "";
        }

        public static void errorMsg(string message, bool exitFlag = true)
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
