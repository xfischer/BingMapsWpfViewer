using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using Nancy.Hosting.Self;

namespace BingMapsWPFViewer.Tools.TinyHttpHost
{
	/// <summary>
	/// Small http host for local tiles
	/// Built using Nancy http://nancyfx.org/
	/// </summary>
	public class TinyHttpHost
	{
		private static NancyHost _nancyHost = null;
		private static int _portNumber = 0;

		/// <summary>
		/// Returns port currently used
		/// </summary>
		public static int PortNumber
		{
			get { return _portNumber; }
		}

		public static void Start()
		{
			if (_nancyHost == null)
			{
				_portNumber = GetAvailablePort();
				_nancyHost = new NancyHost(new Uri("http://localhost:" + _portNumber));
				_nancyHost.Start();
			}
		}

		public static void Stop()
		{
			if (_nancyHost != null)
				_nancyHost.Stop();
		}

		private static int GetAvailablePort()
		{
			HashSet<int> ret = new HashSet<int>(IPGlobalProperties
													.GetIPGlobalProperties()
													.GetActiveTcpConnections()
													.Select(inf => inf.LocalEndPoint.Port)
													.AsEnumerable());

			int port = 81;
			do
			{
				if (ret.Contains(port))
					port++;
				else
					break;
			}
			while (port < 32768);

			return port;
		}


	}
}
