using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace UdpNetworkDiscovery
{
    public class Client
    {
        /**
         * <summary>
         * Connection port
         * </summary>
         */
        private int port = 10002;

        /**
         * <summary>
         * Wait for connection flag
         * </summary>
         */
        private bool waitForConnection = true;

        /**
         * <summary>
         * Searching flag
         * </summary>
         */
        private bool searching = true;

        /**
         * <summary>
         * Host
         * </summary>
         */
        private TcpClient host;

        /**
         * <summary>
         * Available host ip addresses
         * </summary>
         */
        private List<String> availableHosts;

        /**
         * <summary>
         * Timer
         * </summary>
         */
        private Timer timer;

        public Client()
        {
            this.availableHosts = new List<String>();
        }

        /**
         * <summary>
         * Start searching for available hosts
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        public void StartSearching ()
        {
            this.timer = new Timer(2000);
            this.timer.Enabled = true;

            // Start the searching
            this.timer.Elapsed += (source, e) => this.OnSearchingEvent(source, e);
            this.timer.AutoReset = true;

        }

        /**
         * <summary>
         * When the client sends the message to the broadcast channel.
         * The client will listen for the hosts responses.
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        public void ListenForAvailableHosts()
        {
            UdpClient client = null;

            try
            {
                // New host and broadcast endpoint
                client = new UdpClient(this.port);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, this.port);

                Console.WriteLine("Client: Waiting for available hosts responses");

                // Start listening
                while (true)
                {
                    if (!this.searching)
                    {
                        return;
                    }

                    Byte[] incomingBytes = client.Receive(ref endPoint);

                    // Ignore if we are receiving the current local ip address.
                    if (Program.GetLocalIPAddresses().Contains(endPoint.Address.ToString()))
                    {
                        continue;
                    }

                    string jsonRequest = Encoding.ASCII.GetString(incomingBytes);

                    // Convert the json into a SearcHostResponse object
                    SearchHostResponse searchHostResponse = JsonSerializer.Deserialize<SearchHostResponse>(jsonRequest);
                    ClientConnectionResponse clientConnectionResponse = JsonSerializer.Deserialize<ClientConnectionResponse>(jsonRequest);

                    // Searching for available hosts
                    if (searchHostResponse != null && searchHostResponse.Message.Equals("Search Host Response"))
                    {
                        // Add the available hosts to the list.
                        this.availableHosts.Add(endPoint.Address.ToString());
                    }
                    // Listening for success response for TCP connection
                    else if (clientConnectionResponse != null && clientConnectionResponse.Message.Equals("Client Connection Response"))
                    {
                        Console.WriteLine("Communication Connection Established.");
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                client.Close();
            }
        }

        /**
         * <summary>
         * On searching
         * </summary>
         *
         * <param name="e"></param>
         * <param name="source"></param>
         * 
         * <returns>
         * void
         * </returns>
         */
        private void OnSearchingEvent (Object source, ElapsedEventArgs e)
        {
            this.FindAvailableHosts();
            this.OutputAvailableHosts();
        }

        /**
         * <summary>
         * Search and find the available hosts to connect.
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        public void FindAvailableHosts()
        {
            UdpClient client = null;

            try
            {
                // New host and broadcast endpoint
                client = new UdpClient();

                IPAddress broadcastIPAddress = IPAddress.Parse(Program.GetBroadcastIPAddress());
                IPEndPoint broadcastEndPoint = new IPEndPoint(broadcastIPAddress, this.port);

                // Data to send
                SearchHostRequest searchRequest = new SearchHostRequest();

                // Serialize/convert the object into json string
                string json = JsonSerializer.Serialize(searchRequest);

                // Convert the string into bytes
                Byte[] bytesToSend = Encoding.ASCII.GetBytes(json);

                // Send request to the broadcast channel to see available hosts
                client.Send(bytesToSend, bytesToSend.Length, broadcastEndPoint);

                Console.WriteLine("Searching...");
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                client.Close();
            }
        }

        /**
         * <summary>
         * Output available host ip addresses
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        public void OutputAvailableHosts()
        {
            Console.Clear();
            Console.WriteLine("Available Hosts(s)");

            String[] availableHosts = this.availableHosts.ToArray();

            for (int i = 1; i <= availableHosts.Length; i++)
            {
                String host = this.availableHosts[i - 1];
                Console.WriteLine(i + "." + " " + host);
            }

            this.availableHosts = new List<String>();

            if (availableHosts.Length <= 0)
            {
                return;
            }

            Console.WriteLine("Please select an available host to join. For example, type `1` for first option.");
            int option = Int32.Parse(Console.ReadLine());

            while (option <= 0 || option > this.availableHosts.ToArray().Length)
            {
                Console.WriteLine("ERR: out of range, please try to pick the option again.");
                option = Int32.Parse(Console.ReadLine());
            }

            string hostIPAddress = availableHosts[option - 1];
            this.RequestToConnectHost(hostIPAddress);
        }

        /**
         * <summary>
         * The client sends a request to ask the host, if the client can be connected with the host.
         * If yes, then, try to establish the TCP connection with the host.
         * </summary>
         *
         * <param name="ipAddress"></param>
         * 
         * <returns>
         * void
         * </returns>
         */
        public void RequestToConnectHost(String ipAddress)
        {
            UdpClient client = null;
            try
            {
                // New client and broadcast endpoint
                client = new UdpClient();
                IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), this.port);

                // Data to send
                ClientConnectionRequest clientConnectionRequest = new ClientConnectionRequest();

                // Serialize/convert the object into json string
                string json = JsonSerializer.Serialize(clientConnectionRequest);

                // Convert the string into bytes
                Byte[] bytesToSend = Encoding.ASCII.GetBytes(json);

                this.searching = false;

                // Send request to the broadcast channel to see available hosts
                client.Send(bytesToSend, bytesToSend.Length, broadcastEndPoint);

            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                client.Close();
            }
        }
    }
}