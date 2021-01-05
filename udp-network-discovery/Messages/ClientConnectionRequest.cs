using System;

namespace UdpNetworkDiscovery
{
	public class ClientConnectionRequest
	{
		public String Message { get; set; }

		public ClientConnectionRequest ()
		{
			this.Message = "Client Connection Request";
		}
	}
}
