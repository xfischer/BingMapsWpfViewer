using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;


namespace BingMapsWPFViewer.Framework
{
	/// <summary>
	/// Code taken from http://weblogs.asp.net/alexeyzakharov/archive/2010/03/24/silverlight-commands-hacks-passing-eventargs-as-commandparameter-to-delegatecommand-triggered-by-eventtrigger.aspx
	/// </summary>
	public class EventArgsCommand : TriggerAction<DependencyObject>
	{
		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty CommandParameterProperty =
				DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventArgsCommand), null);

		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
				"Command", typeof(ICommand), typeof(EventArgsCommand), null);

		/// <summary>
		/// 
		/// </summary>
		public static readonly DependencyProperty InvokeParameterProperty = DependencyProperty.Register(
				"InvokeParameter", typeof(object), typeof(EventArgsCommand), null);

		private string commandName;

		/// <summary>
		/// 
		/// </summary>
		public object InvokeParameter
		{
			get
			{
				return this.GetValue(InvokeParameterProperty);
			}
			set
			{
				this.SetValue(InvokeParameterProperty, value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public ICommand Command
		{
			get
			{
				return (ICommand)this.GetValue(CommandProperty);
			}
			set
			{
				this.SetValue(CommandProperty, value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string CommandName
		{
			get
			{
				return this.commandName;
			}
			set
			{
				if (this.CommandName != value)
				{
					this.commandName = value;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public object CommandParameter
		{
			get
			{
				return this.GetValue(CommandParameterProperty);
			}
			set
			{
				this.SetValue(CommandParameterProperty, value);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameter"></param>
		protected override void Invoke(object parameter)
		{
			this.InvokeParameter = parameter;

			if (this.AssociatedObject != null)
			{
				ICommand command = this.ResolveCommand();
				if ((command != null) && command.CanExecute(this.CommandParameter))
				{
					command.Execute(this.CommandParameter);
				}
			}			
		}

		private ICommand ResolveCommand()
		{
			ICommand command = null;
			if (this.Command != null)
			{
				return this.Command;
			}
			var frameworkElement = this.AssociatedObject as FrameworkElement;
			if (frameworkElement != null)
			{
				object dataContext = frameworkElement.DataContext;
				if (dataContext != null)
				{
					PropertyInfo commandPropertyInfo = dataContext
							.GetType()
							.GetProperties(BindingFlags.Public | BindingFlags.Instance)
							.FirstOrDefault(
									p =>
									typeof(ICommand).IsAssignableFrom(p.PropertyType) &&
									string.Equals(p.Name, this.CommandName, StringComparison.Ordinal)
							);

					if (commandPropertyInfo != null)
					{
						command = (ICommand)commandPropertyInfo.GetValue(dataContext, null);
					}
				}
			}
			return command;
		}
	}
}
