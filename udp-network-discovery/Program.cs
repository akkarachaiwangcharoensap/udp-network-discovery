using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace UdpNetworkDiscovery
{
    class Program
    {
        static void Main(string[] args)
        {

            Program myProgram = new Program();
            string inputOption = myProgram.AskFindOrHost();

            // Find other available hosts to connect
            if (inputOption.Equals("1"))
            {
                Client client = new Client();

                client.StartSearching();
                client.ListenForAvailableHosts();
            }
            // Host
            else if (inputOption.Equals("2"))
            {
                Server server = new Server();
                server.StartHost();
            }
        }

        public Program ()
        {
        }

        /**
         * <summary>
         * Output text messages to the screen and wait for an input
         * </summary>
         *
         * <returns>
         * String
         * </returns>
         */
        private string AskFindOrHost ()
        {
            Console.WriteLine("Welcome to Peer to Peer Connection.");
            Console.WriteLine("Would you like to \"Find\" or \"Host\"? `1` or `2`");
            return Console.ReadLine();
        }

        /**
         * <summary>
         * Host the TCP Connection
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        public void HostTCPConnection ()
        {

        }

        /**
         * <summary>
         * Get local IPv4 address
         * </summary>
         *
         * <returns>
         * string
         * </returns>
         */
        public static string GetLocalIPAddress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && ni.Name.Equals("Wi-Fi"))
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return ip.Address.ToString();
                        }
                    }
                }
            }

            return null;
        }

        /**
         * <summary>
         * Get local IPv4 address
         * </summary>
         *
         * <returns>
         * List<String>
         * </returns>
         */
        public static List<String> GetLocalIPAddresses()
        {
            List<String> addresses = new List<String>();
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            addresses.Add(ip.Address.ToString());
                        }
                    }
                }
            }

            return addresses;
        }

        /**
         * <summary>
         * Get current ip address that is used to connect to the internet
         * </summary>
         *
         * <returns>
         *  String
         * </returns>
         */
        public static String GetCurrentOutgoingIPAddress ()
        {
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    // Google DNS
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint.Address.ToString();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        /**
         * <summary>
         * Get the current outbound broadcast IP.
         * </summary>
         *
         * <returns>
         * String
         * </returns>
         */
        public static String GetBroadcastIPAddress ()
        {
            string currentIPAddress = GetCurrentOutgoingIPAddress();

            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (UnicastIPAddressInformation ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.ToString().Equals(currentIPAddress))
                        {
                            return GetBroadcastAddress(ip).ToString();
                        }
                    }
                }
            }

            return null;
        }

        /**
         * <summary>
         * Convert the Unicast IP Address into IPV4 based on its submask
         * </summary>
         *
         * <returns>
         * IPAddress
         * </returns>
         */
        public static IPAddress GetBroadcastAddress(UnicastIPAddressInformation unicastAddress)
        {
            return GetBroadcastAddress(unicastAddress.Address, unicastAddress.IPv4Mask);
        }

        /**
         * <summary>
         * Overloading
         * https://stackoverflow.com/questions/25281099/how-to-get-the-local-ip-broadcast-address-dynamically-c-sharp
         * 
         * Convert the Unicast IP Address into IPV4 based on its submask
         * </summary>
         *
         * <returns>
         * IPAddress
         * </returns>
         */
        public static IPAddress GetBroadcastAddress(IPAddress address, IPAddress mask)
        {
            uint ipAddress = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
            uint ipMaskV4 = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
            uint broadCastIpAddress = ipAddress | ~ipMaskV4;

            return new IPAddress(BitConverter.GetBytes(broadCastIpAddress));
        }
    }
}
