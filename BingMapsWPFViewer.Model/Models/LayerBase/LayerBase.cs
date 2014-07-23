using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Framework;

namespace BingMapsWPFViewer.Model
{
	public abstract class LayerBase : ModelBase, ICloneable
	{
		private string _displayName;
		public string DisplayName
		{
			get
			{
				if (_displayName == null)
					_displayName = this.GenerateDisplayNameProposal();
				return _displayName;
			}
			set
			{
				_displayName = value;
			}
		}

		public int ZIndex { get; set; }

		public abstract bool IsVectorLayer { get; }
		public abstract enLayerType LayerType { get; }
		public abstract string GenerateDisplayNameProposal();


		#region ICloneable Membres

		public virtual object Clone()
		{
			return this.MemberwiseClone();
		}

		#endregion
	}
}
