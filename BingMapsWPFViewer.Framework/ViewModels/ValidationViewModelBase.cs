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
	/// A base class for ViewModel classes which supports validation 
	/// using IDataErrorInfo interface. Properties must defines
	/// validation rules by using validation attributes defined in 
	/// System.ComponentModel.DataAnnotations.
	/// </summary>
	public class ValidationViewModelBase : ViewModelBase, IDataErrorInfo
	{
		private readonly Dictionary<string, Func<ValidationViewModelBase, object>> propertyGetters;
		private readonly Dictionary<string, ValidationAttribute[]> validators;

		/// <summary>
		/// Gets the error message for the property with the given name.
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		public string this[string propertyName]
		{
			get
			{
				if (this.propertyGetters.ContainsKey(propertyName))
				{
					ValidationContext ctx = new ValidationContext(this, null, null);

					var propertyValue = this.propertyGetters[propertyName](this);
					var errorMessages = (from v in this.validators[propertyName]
															 let result = v.GetValidationResult(propertyValue, ctx)
															 where result != ValidationResult.Success
															 select result.ErrorMessage)
															.ToArray();

					return string.Join(Environment.NewLine, errorMessages);
				}

				return string.Empty;
			}
		}

		/// <summary>
		/// Returns a boolean value indicating if this object is valid
		/// </summary>
		public bool IsValid
		{
			get
			{
				var errors = from validator in this.validators
										 let error = this[validator.Key]
										 where error != string.Empty
										 select error;

				return errors.Count() == 0;
			}
		}

		/// <summary>
		/// Gets an error message indicating what is wrong with this object.
		/// </summary>
		public string Error
		{
			get
			{
				var errors = from validator in this.validators
										 select this[validator.Key];

				return string.Join(Environment.NewLine, errors.ToArray());
			}
		}

		/// <summary>
		/// Gets the number of properties which have a 
		/// validation attribute and are currently valid
		/// </summary>
		public int ValidPropertiesCount
		{
			get
			{
				var query = from validator in this.validators
										where validator.Value.All(attribute =>
attribute.IsValid(this.propertyGetters[validator.Key](this)))
										select validator;

				var count = query.Count() - this.validationExceptionCount;
				return count;
			}
		}

		/// <summary>
		/// Gets the number of properties which have a validation attribute
		/// </summary>
		public int TotalPropertiesWithValidationCount
		{
			get
			{
				return this.validators.Count();
			}
		}

		public ValidationViewModelBase()
		{
			this.validators = this.GetType()
					.GetProperties()
					.Where(p => this.GetValidations(p).Length != 0)
					.ToDictionary(p => p.Name, p => this.GetValidations(p));

			this.propertyGetters = this.GetType()
					.GetProperties()
					.Where(p => this.GetValidations(p).Length != 0)
					.ToDictionary(p => p.Name, p => this.GetValueGetter(p));
		}

		private ValidationAttribute[] GetValidations(PropertyInfo property)
		{
			return (ValidationAttribute[])property.GetCustomAttributes
(typeof(ValidationAttribute), true);
		}

		private Func<ValidationViewModelBase, object>
	GetValueGetter(PropertyInfo property)
		{
			return new Func<ValidationViewModelBase, object>
(viewmodel => property.GetValue(viewmodel, null));
		}

		private int validationExceptionCount;

		public void ValidationExceptionsChanged(int count)
		{
			this.validationExceptionCount = count;
			this.RaisePropertyChanged<int>(() => ValidPropertiesCount);
		}
	}
}
