using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace UdpNetworkDiscovery
{
	public class Server
	{
        /**
         * <summary>
         * Connection port
         * </summary>
         */
        private int port = 10002;

        /**
         * <summary>
         * Server
         * </summary>
         */
        private TcpListener server;

        public Server()
		{
		}

        /**
         * <summary>
         * Start host broadcast.
         * </summary>
         *
         * <returns>
         * void
         * </returns>
         */
        public void StartHost()
        {
            UdpClient host = null;

            try
            {
                // New host and broadcast endpoint.
                host = new UdpClient(this.port);
                IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, this.port);

                Console.WriteLine("Host Started: Listening for clients.");

                // Start broadcasting
                while (true)
                {
                    Byte[] incomingBytes = host.Receive(ref broadcastEndPoint);

                    Console.WriteLine("RECEIVED FROM: " + broadcastEndPoint.Address.ToString());
                    // Ignore if we are receiving the current local ip address.
                    if (broadcastEndPoint.Address.ToString().Equals(Program.GetCurrentOutgoingIPAddress()))
                    {
                        continue;
                    }

                    string jsonRequest = Encoding.ASCII.GetString(incomingBytes);

                    SearchHostRequest searchHostRequest = JsonSerializer.Deserialize<SearchHostRequest>(jsonRequest);
                    ClientConnectionRequest clientConnectionRequest = JsonSerializer.Deserialize<ClientConnectionRequest>(jsonRequest);

                    // Send the search host response back
                    if (searchHostRequest != null && searchHostRequest.Message.Equals("Search Host Request"))
                    {
                        SearchHostResponse searchHostResponse = new SearchHostResponse();
                        String searchHostResponseJson = JsonSerializer.Serialize(searchHostResponse);
                        Byte[] bytesToSend = Encoding.ASCII.GetBytes(searchHostResponseJson);

                        // Send response back to the client
                        host.Send(bytesToSend, bytesToSend.Length, new IPEndPoint(broadcastEndPoint.Address, this.port));
                        Console.WriteLine("Sent a search host response back to " + broadcastEndPoint.Address.ToString());
                    }
                    // Send the client connection response back
                    else if (clientConnectionRequest != null && clientConnectionRequest.Message.Equals("Client Connection Request"))
                    {
                        ClientConnectionResponse clientConnectionResponse = new ClientConnectionResponse();
                        String clientConnectionResponseJson = JsonSerializer.Serialize(clientConnectionResponse);
                        Byte[] bytesToSend = Encoding.ASCII.GetBytes(clientConnectionResponseJson);

                        // Send response back to the client
                        host.Send(bytesToSend, bytesToSend.Length, new IPEndPoint(broadcastEndPoint.Address, this.port));
                        Console.WriteLine("Sent a client connection response back to " + broadcastEndPoint.Address.ToString());
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                host.Close();
            }
        }
    }
}
