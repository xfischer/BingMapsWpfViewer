using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BingMapsWPFViewer.Framework
{
	/// <summary>
	/// Base class for client model
	/// </summary>
	public partial class ModelBase
			: ObservedBase, INotifyPropertyChanged, INotifyPropertyChanging
	{


		public ModelBase()
		{
			//Subsribe to validation method
			this.PropertyChanged += ValidateProperty;
			this.PropertyChanged += Editable_PropertyChanged;
			//this.PropertyChanging += Editable_PropertyChanging;
		}

		private Guid _id = Guid.NewGuid();
		/// <summary>
		/// Entity id
		/// </summary>
		public Guid Id
		{
			get { return _id; }
			set
			{
				if (value != _id)
				{
					_id = value;
					RaisePropertyChanged(() => Id);
				}
			}
		}


	}
}
