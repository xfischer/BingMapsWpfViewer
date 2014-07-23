using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace BingMapsWPFViewer.Framework
{
	public partial class ModelBase : IDataErrorInfo
	{
		
		#region IDataErrorInfo implementation
		public void ReplaceGlobalValidationErrors(List<string> listErrors)
		{
			_errors = listErrors
					.Aggregate<string>((agg, err) => agg + Environment.NewLine + err);
			RaisePropertyChanged(() => Error);
		}

		private string _errors = String.Empty;
		public string Error
		{
			get
			{
				return validationErrors
							.Values.Aggregate(_errors
							, (agg, err) => agg + Environment.NewLine + err);
			}
		}

		private readonly Dictionary<string, string> validationErrors
				= new Dictionary<string, string>();
		private void ValidateProperty(object sender, PropertyChangedEventArgs e)
		{
			//Validation context creation 
			string propertyName = e.PropertyName;
			if (String.IsNullOrEmpty(propertyName)
					|| string.Equals(e.PropertyName, "HasErrors")) return;
			ValidationContext context =
					new ValidationContext(this, null, null)
					{
						MemberName = propertyName,
						DisplayName = propertyName,
					};

			ICollection<ValidationResult> validationResults
					= new List<ValidationResult>();

			//Get property value
			PropertyInfo propertyInfo = this.GetType()
					.GetProperty(propertyName);
			if (propertyInfo == null) return;
			object propertyValue = propertyInfo.GetValue(this, null);

			//Apply validation rules
			Validator
					.TryValidateProperty(propertyValue, context, validationResults);

			//build error message
			string errors =
					validationResults
					.Aggregate<ValidationResult, string>(string.Empty,
					(a, b) => a += Environment.NewLine + b.ErrorMessage);

			if (!String.IsNullOrEmpty(errors))
			{
				errors = String.Format("[{0}]:{1}", propertyName, errors);
				validationErrors[propertyName] = errors;
			}
			else
			{
				validationErrors.Remove(propertyName);
			}

			RaisePropertyChanged<Boolean>(() => HasErrors);
		}

		public string this[string columnName]
		{
			get
			{
				//Getting good value from validation error list
				string propertyErrorValidation = string.Empty;
				validationErrors.TryGetValue(columnName, out propertyErrorValidation);
				return propertyErrorValidation;
			}
		}

		/// <summary>
		/// Indicates if current model has errors
		/// </summary>
		public bool HasErrors
		{
			get
			{
				return !String.IsNullOrEmpty(Error)
						|| validationErrors.Values.Count(err => !String.IsNullOrEmpty(err)) > 0;
			}
		}

		#endregion //IDataErrorInfo
	
	}
}
