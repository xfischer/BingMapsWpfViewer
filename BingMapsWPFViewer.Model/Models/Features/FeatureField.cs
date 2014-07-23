using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model.Features
{
	public class FeatureField : IEquatable<FeatureField>
	{
		public string FieldName { get; private set; }
		public string TypeName { get; private set; }
		public enFeatureFieldUsage Usage { get; set; }
		public int Ordinal { get; private set; }
		public bool IsSpatial
		{
			get
			{
				return this.TypeName == "geography" || this.TypeName == "geometry";
			}
		}

		public FeatureField(string name, string type, int ordinal, enFeatureFieldUsage usage = enFeatureFieldUsage.None)
		{
			this.FieldName = name;
			this.TypeName = type;
			this.Ordinal = ordinal;
			this.Usage = usage;
		}

		#region IEquatable<FeatureField> Membres

		public bool Equals(FeatureField other)
		{
			return this.GetHashCode() == other.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj is FeatureField)
				return this.Equals((FeatureField)obj);
			else
				return false;
		}

		public override int GetHashCode()
		{
			return FieldName.GetHashCode() ^ Usage.GetHashCode();
		}

		#endregion

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}, [{3}]", Ordinal, FieldName, TypeName, Usage);
		}
				
		public FeatureField Clone()
		{
			return this.MemberwiseClone() as FeatureField;
		}

	}
}
