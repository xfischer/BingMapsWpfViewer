using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Framework.IOC;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using BingMapsWPFViewer.ViewModel.Properties;

namespace BingMapsWPFViewer.ViewModel
{
	public class SetupViewModel : ValidationViewModelBase
	{
		public ProxyCommand<SetupViewModel> SaveCommand { get; private set; }
		public ProxyCommand<SetupViewModel> CloseCommand { get; private set; }

		protected override void InitCommands()
		{
			CloseCommand = new ProxyCommand<SetupViewModel>((_) =>
			{
				this.DialogResult = false;
			});

			SaveCommand = new ProxyCommand<SetupViewModel>((_) =>
				{
					SaveAppSettings();
					this.DialogResult = true;
				});
		}

		protected override void InitServices()
		{

		}

		internal SetupViewModel()
		{
			LoadAppSettings();
		}

		#region Load / Save app settings
		private void LoadAppSettings()
		{
			this.APIKey = Settings.Default.BingKey;
		}
		private void SaveAppSettings()
		{
			Settings.Default.BingKey = this.APIKey;
			Settings.Default.Save();
		}
		#endregion Load / Save app settings

		#region Properties <-> AppSettings mappings

		private string _apiKey;
		public string APIKey
		{
			get
			{
				return _apiKey;
			}
			set
			{
				_apiKey = value;
				RaisePropertyChanged<string>(() => APIKey);
			}
		}

		public bool IsAPIKeyValid
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_apiKey))
					return false;
				else
					return !_apiKey.StartsWith("YOUR_API_KEY_HERE");
			}
		}

		#endregion

		private bool? _dialogResult;
		public bool? DialogResult
		{
			get
			{
				return _dialogResult;
			}
			set
			{
				_dialogResult = value;
				RaisePropertyChanged<bool?>(() => DialogResult);
			}
		}


	}
}
