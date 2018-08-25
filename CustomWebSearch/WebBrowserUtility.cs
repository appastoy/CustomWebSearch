using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace CustomWebSearch
{
	static class WebBrowserUtility
	{
		public static void NavigateUrl(EnvDTE.DTE dte, string url, WebBrowserType webBrowserType, string customWebBrowser)
		{
			switch (webBrowserType)
			{
				case WebBrowserType.SystemDefault:
					Process.Start(url);
					break;

				case WebBrowserType.VisualStudio:
					dte.ItemOperations.Navigate(url);
					break;

				case WebBrowserType.CustomWebBrowser:
					Process.Start(string.Format("\"{0}\"", customWebBrowser), url);
					break;
			}
		}

		public static bool TrySelectWebBrowserPath(string lastPath, out string newPath)
		{
			newPath = lastPath;

			var openFileDialog = new OpenFileDialog();
			openFileDialog.Title = "Select Web Browser Path.";
			openFileDialog.AddExtension = true;
			openFileDialog.CheckFileExists = true;
			openFileDialog.Multiselect = false;
			if (string.IsNullOrEmpty(lastPath))
			{
				openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			}
			else
			{
				openFileDialog.InitialDirectory = Path.GetDirectoryName(lastPath);
				openFileDialog.FileName = Path.GetFileName(lastPath);
			}
			openFileDialog.Filter = "Executable (*.exe) | *.exe";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				newPath = openFileDialog.FileName;
				return true;
			}

			return false;
		}
	}
}
