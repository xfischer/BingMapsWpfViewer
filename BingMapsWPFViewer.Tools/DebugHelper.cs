using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Tools
{
	public class DebugHelper
	{
		public static void WriteLine(object sender, string message)
		{
			Debug.WriteLine(sender.GetType().Name + ": " + message);
		}
	}
}
