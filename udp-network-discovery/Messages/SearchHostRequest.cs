using System;

namespace UdpNetworkDiscovery
{
	public class SearchHostRequest
	{
		public string Message { set; get; }
		public SearchHostRequest () 
		{
			this.Message = "Search Host Request";
		}
	}
}
