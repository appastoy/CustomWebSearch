using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace CustomWebSearch
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the
	/// IVsPackage interface and uses the registration attributes defined in the framework to
	/// register itself and its components with the shell. These attributes tell the pkgdef creation
	/// utility what data to put into .pkgdef file.
	/// </para>
	/// <para>
	/// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
	/// </para>
	/// </remarks>
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
	[Guid(CustomWebSearchPackage.PackageGuidString)]
	[ProvideOptionPage(typeof(OptionPage), "Custom Web Search", "General", 0, 0, true)]
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	[ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
	public sealed class CustomWebSearchPackage : AsyncPackage
	{
		/// <summary>
		/// CustomWebSearchPackage GUID string.
		/// </summary>
		public const string PackageGuidString = "298c1395-10c9-4c1b-b6e5-d083ba50e266";

		public const string PackageCmdSetGuidString = "db27c93a-fcc9-48e5-8448-9ffe640593b0";

		/// <summary>
		/// Initializes a new instance of the <see cref="CustomWebSearchPackage"/> class.
		/// </summary>
		public CustomWebSearchPackage()
		{
			// Inside this method you can place any initialization code that does not require
			// any Visual Studio service because at this point the package object is created but
			// not sited yet inside Visual Studio environment. The place to do all the other
			// initialization is the Initialize method.
		}

		#region Package Members

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
		/// <param name="progress">A provider for progress updates.</param>
		/// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			// When initialized asynchronously, the current thread may be a background thread at this point.
			// Do any initialization that requires the UI thread after switching to the UI thread.
			await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			BindMenus(); 
		}

		void BindMenus()
		{
			var menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			for (int i = 0; i < OptionPageControl.QueryCount; i++)
			{
				int currentIndex = i;
				BindMenuHandler(menuCommandService, Constants.CommandIdQuery + i, (s, e) => OnClickQuery(currentIndex));
			}
			BindMenuHandler(menuCommandService, Constants.CommandIdOpenOption, OnOpenOption, false);
		}

		void BindMenuHandler(OleMenuCommandService menuCommand, int cmdId, EventHandler handler, bool bindEnableCheck = true)
		{
			var command = new CommandID(new Guid(PackageCmdSetGuidString), cmdId);
			OleMenuCommand menuCmd = new OleMenuCommand(handler, command);
			menuCommand.AddCommand(menuCmd);
			if (bindEnableCheck)
			{
				menuCmd.Enabled = false;
				menuCmd.BeforeQueryStatus += MenuCmd_BeforeQueryStatus;
			}
		}

		private void MenuCmd_BeforeQueryStatus(object sender, EventArgs e)
		{
			var cmd = (OleMenuCommand)sender;
			cmd.Enabled = HasTextSelection();
		}

		bool HasTextSelection()
		{
			var dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
			if (dte != null && dte.ActiveWindow != null)
			{
				return dte.ActiveWindow.Selection is EnvDTE.TextSelection;
			}

			return false;
		}

		void OnClickQuery(int index)
		{
			QueryToWebBrowser(index, GetCurrentKeyword());
		}

		string GetCurrentKeyword()
		{
			string queryString = null;
			var dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
			if (dte != null && dte.ActiveWindow != null)
			{
				if (dte.ActiveWindow.Selection is EnvDTE.TextSelection textSelection)
				{
					if (string.IsNullOrEmpty(textSelection.Text))
					{
						textSelection.WordLeft();
						textSelection.WordRight(true);
					}

					queryString = textSelection.Text.Trim();
					if (string.IsNullOrEmpty(queryString))
					{
						int absoluteOffset = textSelection.ActivePoint.AbsoluteCharOffset;
						textSelection.MoveToAbsoluteOffset(absoluteOffset);
					}
					else if (textSelection.Text.Length > queryString.Length)
					{
						textSelection.CharLeft(true, textSelection.Text.Length - queryString.Length);
					}
				}
			}
			return queryString;
		}

		public void QueryToWebBrowser(int index, string keyword, bool test = false)
		{
			var optionPage = GetDialogPage(typeof(OptionPage)) as OptionPage;
			var queryData = optionPage.Queries[index];
			if (queryData.TemplateType == QueryTemplateType.None ||
				string.IsNullOrEmpty(queryData.QueryFormat))
			{
				return;
			}

			string errorType = string.Empty;
			string errorMessage = null;
			string url = null;
			try
			{
				var dte = (EnvDTE.DTE)GetService(typeof(EnvDTE.DTE));
				var keywordEncoded = WebUtility.UrlEncode(keyword);
				url = string.Format(queryData.QueryFormat, keywordEncoded);
				WebBrowserUtility.NavigateUrl(dte, url, optionPage.WebBrowserType, optionPage.CustomWebBrowserPath);
			}
			catch (FormatException ex)
			{
				errorMessage = string.Format("Invalid Query Format.\r\n\r\n\"{0}\"", queryData.QueryFormat);
				errorType = ex.GetType().Name;
			}
			catch (Exception ex)
			{
				if (optionPage.WebBrowserType == WebBrowserType.CustomWebBrowser)
				{
					errorMessage = string.Format("Invalid Custom Web Browser.\r\n\r\n\"{0}\"", optionPage.CustomWebBrowserPath);
				}
				else
				{
					errorMessage = string.Format("Invalid Query.\r\n\r\n\"{0}\"", url);
				}
				errorType = ex.GetType().Name;
			}

			if (errorMessage != null)
			{
				MessageBox.Show(errorMessage, errorType);
				if (!test)
				{
					ShowOptionPage(typeof(OptionPage));
				}
			}
		}

		void OnOpenOption(object sender, EventArgs e)
		{
			ShowOptionPage(typeof(OptionPage));
		}

		#endregion
	}
}
