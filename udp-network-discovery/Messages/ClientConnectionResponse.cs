using System;

namespace UdpNetworkDiscovery
{
	public class ClientConnectionResponse
	{
		public String Message { get; set; }

		public ClientConnectionResponse()
		{
			this.Message = "Client Connection Response";
		}
	}
}
