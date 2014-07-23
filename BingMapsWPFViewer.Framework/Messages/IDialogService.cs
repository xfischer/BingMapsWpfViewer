using System;
using System.Windows;

namespace BingMapsWPFViewer.Framework
{
	public interface IDialogService
	{
		void OpenSaveCancelWindow<T>(string title,
				double height, double width,
				T data, Predicate<T> actionIfOK, Predicate<T> actionIfKO);

		void OpenDialogWindow<T>(string title,
					T data, Window view, Predicate<T> actionIfOK, Predicate<T> actionIfKO);

		void OpenDialogWindow<T>(string title,
						T data, Type viewWindowType, Predicate<T> actionIfOK, Predicate<T> actionIfKO);


		void OpenWindow<T>(string title,
					T data, Window view, bool showActivated, bool topMost, bool allowMultipleInstances, bool isChild);

		void OpenWindow<T>(string title,
						T data, Type viewWindowType, bool showActivated, bool topMost, bool allowMultipleInstances, bool isChild);

		bool AskConfirmation(string title, string message);
		void DisplayInformation(string title, string message);
		void DisplayWarning(string title, string message);

		string SetupSqlConnectionDialog();

		/// <summary>
		/// OpenFileDialog. Returns null if canceled, selected file name otherwise
		/// </summary>
		/// <param name="title"></param>
		/// <param name="filter"></param>
		/// <returns></returns>
		string OpenFileDialog(string title, string filter);
	}
}
