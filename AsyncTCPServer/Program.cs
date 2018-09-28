using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncTCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = args.Length == 1 ? int.Parse(args[0]) : 7;
            Start(port);
            Console.ReadLine();
        }

        private async static void Start(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            List<TcpClient> clientList = clientList = new List<TcpClient>();

            Console.WriteLine("Server is running");
            Console.WriteLine("Listening on port " + port);

            while (true)
            {
                Console.WriteLine("Waiting for connections...");
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    if (!clientList.Contains(client))
                    {
                        HandleConnectionAsync(client);
                        NotifyClients(clientList, client);
                        clientList.Add(client);
                    }

                }
                catch (Exception)
                {
                    Console.WriteLine("Error occured");
                }
            }
        }

        private static async void NotifyClients(List<TcpClient> clientList, TcpClient client)
        {
            foreach (var item in clientList)
            {
                if(item != client)
                {
                    try
                    {
                        using (var netStream = item.GetStream())
                        {
                            using (var writer = new StreamWriter(netStream))
                            {
                                writer.AutoFlush = true;
                                await writer.WriteLineAsync("Client connected " + client.Client.RemoteEndPoint.ToString());
                            }
                        }
                    }
                    catch (Exception e )
                    {
                        Console.WriteLine(" Erro - " + e.ToString());
                    }
                 
                }
            }
        }

        private static async void HandleConnectionAsync(TcpClient client)
        {
            string clientInfo = client.Client.RemoteEndPoint.ToString();
            Console.WriteLine("Got connection request from " + clientInfo);

            try
            {
                using (var netStream = client.GetStream())
                {
                    using (var reader = new StreamReader(netStream))
                    {
                        using (var writer = new StreamWriter(netStream))
                        {
                            writer.AutoFlush = true;
                            while (true)
                            {
                                var data = await reader.ReadLineAsync();
                                if (string.IsNullOrEmpty(data))
                                {
                                    break;
                                }
                                await writer.WriteLineAsync("From Server - " + data);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                //Console.WriteLine("Closing the client connection " + clientInfo);
                //client.Close();
            }
        }
    }
}
