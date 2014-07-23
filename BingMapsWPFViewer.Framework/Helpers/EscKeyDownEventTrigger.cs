using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace BingMapsWPFViewer.Framework
{
	
	public class EscKeyDownEventTrigger : EventTrigger
	{

		public EscKeyDownEventTrigger()
			: base("KeyDown")
		{
		}

		protected override void OnEvent(EventArgs eventArgs)
		{
			var e = eventArgs as KeyEventArgs;
			if (e != null && e.Key == Key.Escape)
				this.InvokeActions(eventArgs);
		}
	}
}
