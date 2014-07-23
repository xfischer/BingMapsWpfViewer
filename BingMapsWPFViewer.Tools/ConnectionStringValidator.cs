using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


namespace BingMapsWPFViewer.Tools
{
	public class ConnectionStringUtils
	{
		public static ValidationResult ValidateConnectionString(string connectionString)
		{
			if (connectionString == null)
			return new ValidationResult("ConnectionString is required");
			else
			{

				string serverName = ConnectionStringUtils.GetSqlServerName(connectionString);
				bool useWindowAuth = ConnectionStringUtils.IsAuthenticationWindows(connectionString);
				string userName = ConnectionStringUtils.GetUserName(connectionString);
				

				if (string.IsNullOrEmpty(serverName))
				{
					return new ValidationResult("Data source (server name) is required");
				}
				if (!useWindowAuth &&
					string.IsNullOrEmpty(userName))
				{
					return new ValidationResult("User ID is required");
				}
			}

			return ValidationResult.Success;
		}

		public static bool IsValid(string connectionString)
		{
			return ConnectionStringUtils.ValidateConnectionString(connectionString) == ValidationResult.Success;
		}

		#region Connection string parsing

		public static string GetSqlDatabaseName(string connectionString)
		{
			return GetConnectionStringPart(connectionString, "Initial Catalog");
		}
		public static string GetSqlServerName(string connectionString)
		{
			return GetConnectionStringPart(connectionString, "Data Source");
		}
		public static string GetUserName(string connectionString)
		{
			return GetConnectionStringPart(connectionString, "User ID");
		}

		public static bool IsSqlConnectionAsync(string connectionString)
		{
			string ret = GetConnectionStringPart(connectionString, "Asynchronous Processing");
			return ret != null && ret.ToUpper() == "TRUE";
		}

		public static bool IsAuthenticationWindows(string connectionString)
		{
			string ret = GetConnectionStringPart(connectionString, "Integrated Security");
			return ret != null && ret.ToUpper() == "TRUE";
		}

		public static string GetConnectionStringPart(string connectionString, string NamedPart)
		{
			string regexDataSource = NamedPart.Replace(" ", "\\s") + "(\\s)*=(\\s)*(?<Value>([^;]*))";
			RegexOptions options = RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.Compiled;
			Regex reg = new Regex(regexDataSource, options);
			MatchCollection v_matchCol = reg.Matches(connectionString);
			if (v_matchCol.Count > 0)
				return v_matchCol[0].Groups["Value"].Value;
			else
				return null;
		}

		#endregion

	}
}
