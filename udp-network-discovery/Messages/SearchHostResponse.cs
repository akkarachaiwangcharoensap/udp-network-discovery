using System;

namespace UdpNetworkDiscovery
{
	public class SearchHostResponse
	{
		public string Message { set; get; }
		public SearchHostResponse()
		{
			this.Message = "Search Host Response";
		}
	}
}
