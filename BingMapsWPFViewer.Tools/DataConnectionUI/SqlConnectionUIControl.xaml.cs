using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.ComponentModel;

namespace BingMapsWPFViewer.Tools.DataConnectionUI
{
	/// <summary>
	/// Dialog window for Sql server connection setup
	/// WPF port of Data Connection Dialog http://archive.msdn.microsoft.com/Connection/
	/// </summary>
	public partial class SqlConnectionUIControl : UserControl
	{

		private object[] _servers;
		private object[] _databases;
		private Properties _properties;

		#region Connection properties class
		private class Properties
		{
			private bool _UseWindowsAuthentication = true;
			private string _ServerName;
			private string _Password;
			private string _UserName;
			private bool _SavePassword;
			private string _DatabaseName;
			private string _LogicalDatabaseName;
			private string _DatabaseFile;
			private bool _UserInstance;

			public bool UseWindowsAuthentication
			{
				get { return _UseWindowsAuthentication; }
				set { _UseWindowsAuthentication = value; RaiseChanged(); }
			}
			public string ServerName
			{
				get { return _ServerName; }
				set { _ServerName = value; RaiseChanged(); }
			}
			public string Password
			{
				get { return _Password; }
				set { _Password = value; RaiseChanged(); }
			}
			public string UserName
			{
				get { return _UserName; }
				set { _UserName = value; RaiseChanged(); }
			}
			public bool SavePassword
			{
				get { return _SavePassword; }
				set { _SavePassword = value; RaiseChanged(); }
			}
			public string DatabaseName
			{
				get { return _DatabaseName; }
				set { _DatabaseName = value; RaiseChanged(); }
			}
			public string LogicalDatabaseName
			{
				get { return _LogicalDatabaseName; }
				set { _LogicalDatabaseName = value; RaiseChanged(); }
			}
			public string DatabaseFile
			{
				get { return _DatabaseFile; }
				set { _DatabaseFile = value; RaiseChanged(); }
			}
			public bool UserInstance
			{
				get { return _UserInstance; }
				set { _UserInstance = value; RaiseChanged(); }
			}

			private void RaiseChanged()
			{
				if (PropertiesChanged != null)
					try
					{
						PropertiesChanged(this, new EventArgs());
					}
					finally
					{
					}
			}

			public bool IsComplete
			{
				get
				{
					if (string.IsNullOrEmpty(_ServerName))
					{
						return false;
					}
					if (!_UseWindowsAuthentication &&
						string.IsNullOrEmpty(_UserName))
					{
						return false;
					}
					return true;
				}
			}


			public event EventHandler PropertiesChanged;


		}
		#endregion Properties


		public SqlConnectionUIControl()
		{
			InitializeComponent();

			_properties = new Properties();
			_properties.PropertiesChanged += (o, e) => this.ConfigureButtons();
		}

		#region User Events

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			if (this.Parent is Window)
				(this.Parent as Window).DialogResult = true;
		}

		private void btnTest_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				using (SqlConnection con = new SqlConnection(this.GetConnectionString(_properties)))
				{
					con.Open();
				}

				MessageBox.Show("Test connection succeeded.", "Test results", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Test results: " + ex.Message, "Test results", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

		}


		#region Server

		private void cmbServer_DropDownOpened(object sender, EventArgs e)
		{
			EnumerateServers();
		}

		private void cmbServer_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Down)
			{
				EnumerateServers();
			}
		}

		private void cmbServer_LostFocus(object sender, RoutedEventArgs e)
		{
			cmbServer.Text = cmbServer.Text.Trim();
		}

		private void btnRefreshServers_Click(object sender, RoutedEventArgs e)
		{
			cmbServer.Items.Clear();
			EnumerateServers();
		}

		private void cmbServer_TextChanged(object sender, TextChangedEventArgs e)
		{
			SetServer();
		}

		#endregion

		#region Logon
		
		private void rdWindowsAuth_Checked(object sender, RoutedEventArgs e)
		{
			SetAuthenticationOption();
		}

		private void rdSqlAuth_Checked(object sender, RoutedEventArgs e)
		{
			SetAuthenticationOption();
		}

		private void txtUserName_LostFocus(object sender, RoutedEventArgs e)
		{
			txtUserName.Text = txtUserName.Text.Trim();
		}

		private void txtUserName_TextChanged(object sender, TextChangedEventArgs e)
		{
			SetUserName();
		}

		private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			SetPassword();
		}

		private void chkSavePassword_Checked(object sender, RoutedEventArgs e)
		{
			SetSavePassword();
		}

		#endregion

		#region Database

		private void rdSelectDatabase_Checked(object sender, RoutedEventArgs e)
		{
			SetDatabaseOption();
		}

		private void rdAttachDatabase_Checked(object sender, RoutedEventArgs e)
		{
			//MessageBox.Show("Not supported yet");
			SetDatabaseOption();
		}
				
		private void cmbDatabase_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Down)
			{
				EnumerateDatabases();
			}
		}

		private void cmbDatabase_DropDownOpened(object sender, EventArgs e)
		{
			EnumerateDatabases();
		}

		private void cmbDatabase_LostFocus(object sender, RoutedEventArgs e)
		{
			cmbDatabase.Text = cmbDatabase.Text.Trim();
		}

		private void cmbDatabase_TextChanged(object sender, TextChangedEventArgs e)
		{
			SetDatabase();
		}
		
		private void txtAttachDatabase_TextChanged(object sender, TextChangedEventArgs e)
		{
			SetAttachDatabase();
		}

		private void txtAttachDatabase_LostFocus(object sender, RoutedEventArgs e)
		{
			txtAttachDatabase.Text = txtAttachDatabase.Text.Trim();
		}

		private void btnBrowseAttach_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new OpenFileDialog();
			fileDialog.Title = "Browse database file";
			fileDialog.Multiselect = false;
			fileDialog.RestoreDirectory = true;
			fileDialog.Filter = "Microsoft SQL Server Databases (*.mdf)|*.mdf|All Files (*.*)|*.*";
			try
			{
				bool? result = fileDialog.ShowDialog();
				if (result.GetValueOrDefault(false))
				{
					txtAttachDatabase.Text = fileDialog.FileName.Trim();
				}
			}
			finally
			{

			}
		}

		private void txtLogicalName_TextChanged(object sender, TextChangedEventArgs e)
		{
			SetLogicalFilename();
		}

		private void txtLogicalName_LostFocus(object sender, RoutedEventArgs e)
		{
			txtLogicalName.Text = txtLogicalName.Text.Trim();
		}

		#endregion

		#endregion

		#region Server

		private void EnumerateServers()
		{
			if (cmbServer.Items.Count > 0)
				return;

			// Perform the enumeration
			DataTable dataTable = null;
			try
			{
				dataTable = System.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources();
			}
			catch
			{
				dataTable = new DataTable();
				dataTable.Locale = System.Globalization.CultureInfo.InvariantCulture;
			}

			// Create the object array of server names (with instances appended)
			_servers = new object[dataTable.Rows.Count];
			for (int i = 0; i < _servers.Length; i++)
			{
				string name = dataTable.Rows[i]["ServerName"].ToString();
				string instance = dataTable.Rows[i]["InstanceName"].ToString();
				if (instance.Length == 0)
				{
					_servers[i] = name;
				}
				else
				{
					_servers[i] = name + "\\" + instance;
				}
			}

			// Sort the list
			Array.Sort(_servers);

			PopulateServerComboBox();

		}

		private void PopulateServerComboBox()
		{
			if (cmbServer.Items.Count == 0)
			{
				if (_servers.Length > 0)
				{
					foreach (var server in _servers)
						cmbServer.Items.Add(server);
				}
				else
				{
					cmbServer.Items.Add(String.Empty);
				}
			}
		}		

		private void SetServer()
		{
			_properties.ServerName = cmbServer.Text;

			SetDatabaseGroupBoxStatus();
			cmbDatabase.Items.Clear(); // a server change requires a refresh here
		}

		private void SetDatabaseGroupBoxStatus()
		{
			if (cmbServer.Text.Trim().Length > 0 &&
				(rdWindowsAuth.IsChecked.GetValueOrDefault(false) ||
				txtUserName.Text.Trim().Length > 0))
			{
				grpDatabase.IsEnabled = true;
			}
			else
			{
				grpDatabase.IsEnabled = false;
			}
		}

		#endregion

		#region Logon

		private void SetAuthenticationOption()
		{
			if (_properties == null) // loading
				return;

			if (rdWindowsAuth.IsChecked.GetValueOrDefault(false))
			{
				_properties.UseWindowsAuthentication = true;
				_properties.UserName = null;
				_properties.Password = null;
				_properties.SavePassword = false;
				gridLogin.IsEnabled = false;
			}
			else /* if (rdSqlAuth.IsChecked) */
			{
				_properties.UseWindowsAuthentication = false;
				SetUserName();
				SetPassword();
				SetSavePassword();
				gridLogin.IsEnabled = true;
			}
			SetDatabaseGroupBoxStatus();
			cmbDatabase.Items.Clear(); // an authentication change requires a refresh here
		}

		private void SetUserName()
		{
			_properties.UserName = txtUserName.Text;
			SetDatabaseGroupBoxStatus();
			cmbDatabase.Items.Clear(); // a user name change requires a refresh here
		}

		private void SetPassword()
		{
			_properties.Password = txtPassword.Password;
			//txtPassword.Password = txtPassword.Password; // forces reselection of all text
			cmbDatabase.Items.Clear(); // a password change requires a refresh here
		}

		private void SetSavePassword()
		{
			_properties.SavePassword = chkSavePassword.IsChecked.GetValueOrDefault(false);
		}

		#endregion

		#region Database
		
		private void SetDatabaseOption()
		{
			if (_properties == null)
				return;

			if (rdSelectDatabase.IsChecked.GetValueOrDefault(false))
			{
				SetDatabase();
				SetAttachDatabase();
				cmbDatabase.IsEnabled = true;
				gridAttachDatabase.IsEnabled = false;
			}
			else /* if (attachDatabaseRadioButton.Checked) */
			{
				SetAttachDatabase();
				SetLogicalFilename();
				cmbDatabase.IsEnabled = false;
				gridAttachDatabase.IsEnabled = true;

			}
		}

		private void SetLogicalFilename()
		{
			if (rdSelectDatabase.IsChecked.GetValueOrDefault(false))
			{
				_properties.LogicalDatabaseName = null;
			}
			else /* if (attachDatabaseRadioButton.Checked) */
			{
				_properties.LogicalDatabaseName = txtLogicalName.Text;
			}
		}

		private void SetAttachDatabase()
		{
			if (_properties == null)
				return;

			if (rdSelectDatabase.IsChecked.GetValueOrDefault(false))
			{
				_properties.DatabaseFile = null;
			}
			else /* if (attachDatabaseRadioButton.Checked) */
			{
				_properties.DatabaseFile = txtAttachDatabase.Text;
			}
		}

		private void SetDatabase()
		{
			if (_properties == null)
				return;

			_properties.DatabaseName = cmbDatabase.Text;
			if (cmbDatabase.Items.Count == 0)
			{
				EnumerateDatabases();
			}
		}

		private void EnumerateDatabases()
		{
			// Perform the enumeration
			DataTable dataTable = null;
			IDbConnection connection = null;
			IDataReader reader = null;
			try
			{
				// Get a basic connection
				connection = GetBasicConnection(_properties);

				// Create a command to check if the database is on SQL AZure.
				IDbCommand command = connection.CreateCommand();
				command.CommandText = "SELECT CASE WHEN SERVERPROPERTY(N'EDITION') = 'SQL Data Services' OR SERVERPROPERTY(N'EDITION') = 'SQL Azure' THEN 1 ELSE 0 END";

				// Open the connection
				connection.Open();

				// SQL AZure doesn't support HAS_DBACCESS at this moment.
				// Change the command text to get database names accordingly
				if ((Int32)(command.ExecuteScalar()) == 1)
				{
					command.CommandText = "SELECT name FROM master.dbo.sysdatabases ORDER BY name";
				}
				else
				{
					command.CommandText = "SELECT name FROM master.dbo.sysdatabases WHERE HAS_DBACCESS(name) = 1 ORDER BY name";
				}

				// Execute the command
				reader = command.ExecuteReader();

				// Read into the data table
				dataTable = new DataTable();
				dataTable.Locale = System.Globalization.CultureInfo.CurrentCulture;
				dataTable.Load(reader);
			}
			catch
			{
				dataTable = new DataTable();
				dataTable.Locale = System.Globalization.CultureInfo.InvariantCulture;
			}
			finally
			{
				if (reader != null)
				{
					reader.Dispose();
				}
				if (connection != null)
				{
					connection.Dispose();
				}
			}

			// Create the object array of database names
			_databases = new object[dataTable.Rows.Count];
			for (int i = 0; i < _databases.Length; i++)
			{
				_databases[i] = dataTable.Rows[i]["name"];
			}

			if (cmbDatabase.Items.Count == 0)
			{
				if (_databases.Length > 0)
				{
					foreach (var db in _databases)
						cmbDatabase.Items.Add(db);
				}
				else
				{
					cmbDatabase.Items.Add(String.Empty);
				}
			}
		}

		private IDbConnection GetBasicConnection(Properties properties)
		{

			IDbConnection connection = null;

			string connectionString = String.Empty;
			connectionString += "Data Source='" + properties.ServerName.Replace("'", "''") + "';";
			if (properties.UserInstance)
			{
				connectionString += "User Instance=true;";
			}
			if (properties.UseWindowsAuthentication)
			{
				connectionString += "Integrated Security=" + true.ToString() + ";";
			}
			else
			{
				connectionString += "User ID='" + properties.UserName.Replace("'", "''") + "';";
				connectionString += "Password='" + properties.Password.Replace("'", "''") + "';";
			}
			connectionString += "Pooling=False;";



			connection = new SqlConnection(connectionString);

			return connection;
		}
		private string GetConnectionString(Properties properties)
		{

			if (properties == null || properties.ServerName == null)
				return string.Empty;

			string connectionString = String.Empty;
			try
			{
				connectionString += "Data Source='" + properties.ServerName.Replace("'", "''") + "';";
				if (!string.IsNullOrEmpty(properties.DatabaseFile))
				{
					connectionString += "AttachDbFilename=\"" + System.IO.Path.GetFullPath(properties.DatabaseFile) + "\";";
				}

				if (properties.UserInstance)
				{
					connectionString += "User Instance=true;";
				}
				if (properties.UseWindowsAuthentication)
				{
					connectionString += "Integrated Security=" + true.ToString() + ";";
				}
				else
				{
					connectionString += "User ID='" + properties.UserName.Replace("'", "''") + "';";
					connectionString += "Password='" + properties.Password.Replace("'", "''") + "';";
				}

				if (!string.IsNullOrEmpty(properties.LogicalDatabaseName))
				{
					connectionString += "Initial Catalog='" + properties.LogicalDatabaseName + "';";
				}
				else
				{
					connectionString += "Initial Catalog='" + properties.DatabaseName + "';";
				}
				connectionString += "Pooling=False;";
			}
			finally
			{

			}
			
			return connectionString;
		}
		
		#endregion

		public string ConnectionString
		{
			get { return this.GetConnectionString(_properties); }
		}

		private void ConfigureButtons()
		{
			try
			{
				btnOK.IsEnabled = (_properties != null) ? _properties.IsComplete : false;
			}
			catch
			{
				btnOK.IsEnabled = true;
			}
		}

		

	
	

	}
}

