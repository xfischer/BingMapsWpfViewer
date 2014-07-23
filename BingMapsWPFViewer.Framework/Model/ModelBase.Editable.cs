using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace BingMapsWPFViewer.Framework
{
	public partial class ModelBase : IEditableObject
	{

		private Dictionary<PropertyInfo, object> _valueCache;
		private readonly static Dictionary<Type, IEnumerable<PropertyInfo>> _reflectionCache
				= new Dictionary<Type, IEnumerable<PropertyInfo>>();
		private readonly static object _locker = new object();

		public bool WasModified { get; set; }
		public bool IsEditing { get; private set; }
		public bool IsReallyEditing { get; /* private */ set; }
		public bool AutoEdit { get; set; }

		private void Editable_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Updates IsReallyEditing
			if (IsEditing) IsReallyEditing = true;
		}
		private void Editable_PropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			//If Auto edit mode => begin edition
			if (AutoEdit && !IsEditing) BeginEdit();
		}

		/// <summary>
		/// Returns property list from class obtained by reflection
		/// They are cached for better performance
		/// </summary>
		private IEnumerable<PropertyInfo> GetClassProperties()
		{
			IEnumerable<PropertyInfo> classProperties;
			if (!_reflectionCache.TryGetValue(GetType(), out classProperties))
			{
				lock (_locker)
				{
					if (_reflectionCache.TryGetValue(GetType(), out classProperties))
						return classProperties;

					classProperties = GetType()
							.GetProperties(BindingFlags.Public | BindingFlags.Instance)
							.Where(p => p.CanRead && p.CanWrite);

					_reflectionCache.Add(GetType(), classProperties);

				}
			}

			return classProperties;
		}

		/// <summary>
		/// Begin of current object editing
		/// </summary>
		public void BeginEdit()
		{
			if (IsEditing) return;
			IsEditing = true;
			IEnumerable<PropertyInfo> classProperties
					= GetClassProperties();

			_valueCache =
					classProperties.ToDictionary<PropertyInfo, PropertyInfo, object>
					(p => p, p => p.GetValue(this, null));
		}

		/// <summary>
		/// Cancel all changes made
		/// </summary>
		public void CancelEdit()
		{
			if (IsEditing && IsReallyEditing)
			{
				WasModified = true;
				IEnumerable<PropertyInfo> classProperties
						= GetClassProperties();

				foreach (PropertyInfo item in classProperties)
				{
					item.SetValue(this, _valueCache[item], null);
				}
				_valueCache = null;
				IsEditing = IsReallyEditing = false;
			}
			else
			{
				EndEdit();
			}
		}

		/// <summary>
		/// Edition validation
		/// </summary>
		public void EndEdit()
		{
			if (!IsEditing) return;
			WasModified = true;
			_valueCache = null;
			IsEditing = IsReallyEditing = false;
		}
	}
}
