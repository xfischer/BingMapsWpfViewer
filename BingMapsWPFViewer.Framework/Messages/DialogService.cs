using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BingMapsWPFViewer.Framework.Messages;
using BingMapsWPFViewer.Tools.DataConnectionUI;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;

namespace BingMapsWPFViewer.Framework
{
	public class DialogService : IDialogService
	{

		public void OpenSaveCancelWindow<T>(string title,
				double height, double width,
				T dataContext, Predicate<T> actionIfOK, Predicate<T> actionIfKO)
		{
			var win = new SaveOrCancelWindow
			{
				Title = title,
				DataContext = dataContext,
				WindowStartupLocation = WindowStartupLocation.CenterScreen,
				WindowStyle = WindowStyle.ToolWindow,
				Height = height,
				MaxHeight = height,
				MinHeight = height,
				Width = width,
				MaxWidth = width,
				MinWidth = width,
			};


			win.Closing += (_, arg) =>
			{
				Predicate<T> predicateToUse = null;
				switch (win.RaisonDeFermeture)
				{
					case MessageBoxResult.OK:
						if (actionIfOK != null)
							predicateToUse = actionIfOK;
						break;
					case MessageBoxResult.Cancel:
						if (actionIfKO != null)
							predicateToUse = actionIfKO;
						break;
					default:
						predicateToUse = o => true;
						break;
				}

				bool okToClose = predicateToUse == null || predicateToUse(dataContext);
				arg.Cancel = !okToClose;
			};

			win.Show();
		}

		public void OpenDialogWindow<T>(string title
			, T data, Window view, Predicate<T> actionIfOK, Predicate<T> actionIfKO)
		{
			view.Title = title;
			view.DataContext = data;
			view.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			view.WindowStyle = WindowStyle.SingleBorderWindow;

			if (view.ShowDialog().GetValueOrDefault(false))
			{
				if (actionIfOK != null)
					actionIfOK(data);
			}
			else
			{
				if (actionIfKO != null)
					actionIfKO(data);
			}
		}

		public void OpenDialogWindow<T>(string title
			, T data, Type viewWindowType, Predicate<T> actionIfOK, Predicate<T> actionIfKO)
		{
			Window view = Activator.CreateInstance(viewWindowType) as Window;
			this.OpenDialogWindow<T>(title, data, view, actionIfOK, actionIfKO);
		}


		public void OpenWindow<T>(string title
			, T data, Window view, bool showActivated, bool topMost, bool allowMultipleInstances, bool isChild)
		{

			Window wnd = this.FindWindowByTitle(title);
			if (wnd != null && !allowMultipleInstances)
				wnd.Activate();
			else
			{
				// Child window => child of the first window loaded
				// TODO: add owner by title of type (enhance viewTypeLocator to add a RegisterWindow with owner param)
				if (isChild)
					view.Owner = Application.Current.Windows[0];

				view.Title = title;
				view.DataContext = data;
				view.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				view.WindowStyle = WindowStyle.SingleBorderWindow;
				view.ShowActivated = showActivated;
				view.Topmost = topMost;

				view.Show();
			}
		}

		private Window FindWindowByTitle(string windowTitle)
		{
			foreach (Window window in Application.Current.Windows)
			{
				if (windowTitle == window.Title)
					return window;
			}

			return null;
		}

		public void OpenWindow<T>(string title
			, T data, Type viewWindowType, bool showActivated, bool topMost, bool allowMultipleInstances, bool isChild)
		{
			Window wnd = this.FindWindowByTitle(title);
			if (wnd != null && !allowMultipleInstances)
				wnd.Activate();
			else
			{
				Window view = Activator.CreateInstance(viewWindowType) as Window;
				this.OpenWindow<T>(title, data, view, showActivated, topMost, allowMultipleInstances, isChild);
			}
		}

		public bool AskConfirmation(string title, string message)
		{
			return MessageBoxResult.Yes ==
					MessageBox.Show(message, title,
					MessageBoxButton.YesNo, MessageBoxImage.Question);
		}

		public void DisplayInformation(string title, string message)
		{
			MessageBox.Show(message, title,
					MessageBoxButton.OK, MessageBoxImage.Information);
		}

		public void DisplayWarning(string title, string message)
		{
			MessageBox.Show(message, title,
					MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		public string SetupSqlConnectionDialog()
		{
			var ctl = new SqlConnectionUIControl();
			var win = new Window();
			ctl.Margin = new Thickness(3, 3, 3, 0);
			win.Content = ctl;
			win.Width = 370;
			win.Height = 500;
			win.ResizeMode = ResizeMode.CanResizeWithGrip;
			win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			win.WindowStyle = WindowStyle.SingleBorderWindow;
			win.ShowInTaskbar = false;
			win.Title = "Sql Server connection";
			win.ShowDialog();
			return ctl.ConnectionString;
		}

		string IDialogService.OpenFileDialog(string title, string filter)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = filter;
			dlg.RestoreDirectory = true;
			dlg.Title = title;
			dlg.Multiselect = false;

			bool? result = dlg.ShowDialog();
			return result.GetValueOrDefault(false) ? dlg.FileName : null;

		}

	}
}
