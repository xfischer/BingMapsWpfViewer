using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Tools
{
	public class NetHelper
	{
		#region Empty Cache

		/// <summary>
		/// Method for clearing internet cache through code
		/// </summary>
		/// <param name="folder">Directory to empty</param>
		private static void ClearAllDirectoryFiles(DirectoryInfo folder)
		{
			//loop through all the files in the folder provided
			foreach (FileInfo file in folder.GetFiles())
			{
				//delete each file
				try
				{
					file.Delete();
				}
				catch
				{
				}

			}
			//now we loop through all the sub directories in the directory provided
			foreach (DirectoryInfo subfolder in folder.GetDirectories())
			{
				//recursively delete all files and folders
				//in each sub directory
				ClearAllDirectoryFiles(subfolder);
			}

			try
			{
				folder.Delete();
			}
			catch
			{
			}
		}

		/// <summary>
		/// Method passing the "internet cache" folder information
		/// to EmptyCacheFolder for emptying IE cache
		/// </summary>
		/// <returns></returns>
		public static bool TryClearCache()
		{
			//variable to hold our status
			bool isEmpty;
			try
			{
				//call EmptyCacheFolder passing the default internet cache
				//folder
				ClearAllDirectoryFiles(new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache)));
				//successful so return true
				isEmpty = true;
			}
			catch
			{
				//failed
				isEmpty = false;
			}
			//return status
			return isEmpty;
		}

		#endregion
	}
}
