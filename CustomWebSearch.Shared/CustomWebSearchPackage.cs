using EnvDTE;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace CustomWebSearch
{
    /// <summary>This is the class that implements the package exposed by this assembly.</summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio is to implement the IVsPackage interface and
    /// register itself with the shell. This package uses the helper classes defined inside the Managed Package Framework (MPF) to do it: it
    /// derives from the Package class that provides the implementation of the IVsPackage interface and uses the registration attributes
    /// defined in the framework to register itself and its components with the shell. These attributes tell the pkgdef creation utility
    /// what data to put into .pkgdef file.
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
        public const string PackageCmdSetGuidString = "db27c93a-fcc9-48e5-8448-9ffe640593b0";

        /// <summary>CustomWebSearchPackage GUID string.</summary>
        public const string PackageGuidString = "298c1395-10c9-4c1b-b6e5-d083ba50e266";

        /// <summary>Initializes a new instance of the <see cref="CustomWebSearchPackage"/> class.</summary>
        public CustomWebSearchPackage() =>
            // Inside this method you can place any initialization code that does not require any Visual Studio service because at this
            // point the package object is created but not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
            Instance = this;

        public static CustomWebSearchPackage Instance { get; private set; }

        public void QueryToWebBrowser(int index, string keyword, bool test = false)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OptionPage optionPage = GetDialogPage(typeof(OptionPage)) as OptionPage;
            QueryData queryData = optionPage.Queries[index];
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
                DTE dte = (DTE)GetService(typeof(DTE));
                string keywordEncoded = string.IsNullOrEmpty(keyword) ? string.Empty : WebUtility.UrlEncode(keyword);
                url = queryData.QueryFormat.Replace("{QUERY}", keywordEncoded);
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

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place where you can put
        /// all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">
        /// A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.
        /// </param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>
        /// A task representing the async work of package initialization, or an already completed task if there is none. Do not return null
        /// from this method.
        /// </returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point. Do any initialization that
            // requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            BindMenus();
        }

        private OleMenuCommand BindMenuHandler(OleMenuCommandService menuCommand, int cmdId, int index, EventHandler handler, bool bindEnableCheck = true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            CommandID command = new CommandID(new Guid(PackageCmdSetGuidString), cmdId);
            OleMenuCommand menuCmd = new OleMenuCommand(handler, command);
            menuCommand.AddCommand(menuCmd);
            if (bindEnableCheck)
            {
                menuCmd.Enabled = false;
                menuCmd.BeforeQueryStatus += (s, e) => MenuCmd_BeforeQueryStatus(s, index);
            }

            return menuCmd;
        }

        private void BindMenus()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OleMenuCommandService menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(menuCommandService);
            OleMenuCommand mainMenuCommand = new OleMenuCommand(null, new CommandID(new Guid(PackageCmdSetGuidString), Constants.CommandIdMainMenu));
            mainMenuCommand.BeforeQueryStatus += MainMenu_BeforeQueryStatus;
            menuCommandService.AddCommand(mainMenuCommand);

            for (int i = 0; i < OptionPageControl.QueryCount; i++)
            {
                int currentIndex = i;
                BindMenuHandler(menuCommandService, Constants.CommandIdQuery + i, i, (s, e) => OnClickQuery(currentIndex));
            }
            OleMenuCommand openMenu = BindMenuHandler(menuCommandService, Constants.CommandIdOpenOption, -1, OnOpenOption, false);
            openMenu.Text = "&Option Option...";
        }

        private string GetCurrentKeyword()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string keyword = string.Empty;
            DTE dte = (DTE)GetService(typeof(DTE));
            Assumes.Present(dte);
            if (dte?.ActiveWindow == null) { return keyword; }

            if (dte.ActiveWindow.Selection is TextSelection textSelection)
            {
                if (string.IsNullOrEmpty(textSelection.Text))
                {
                    VirtualPoint activePoint = textSelection.ActivePoint;
                    int absoluteCharOffset = activePoint.AbsoluteCharOffset;

                    textSelection.WordLeft();
                    textSelection.WordRight(true);

                    keyword = textSelection.Text.Trim();
                    textSelection.MoveToAbsoluteOffset(absoluteCharOffset);
                }
                else
                {
                    keyword = textSelection.Text.Trim();
                }
            }
            return keyword ?? string.Empty;
        }

        private void MainMenu_BeforeQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand cmd = (OleMenuCommand)sender;
            cmd.Text = "Cusom &Web Search";
        }

        private void MenuCmd_BeforeQueryStatus(object sender, int index)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OleMenuCommand cmd = (OleMenuCommand)sender;

            OptionPage optionPage = GetDialogPage(typeof(OptionPage)) as OptionPage;
            QueryData queryData = optionPage.Queries[index];
            if (queryData.TemplateType == QueryTemplateType.None ||
                string.IsNullOrEmpty(queryData.QueryFormat))
            {
                cmd.Visible = false;
                cmd.Enabled = false;
                return;
            }

            string keyword = GetCurrentKeyword();
            cmd.Enabled = !string.IsNullOrEmpty(keyword);
            cmd.Visible = true;

            int queryNumber = (index + 1) % 10;
            if (!cmd.Enabled)
            {
                cmd.Visible = true;
                cmd.Text = $"Query &{queryNumber} - {optionPage.GetTemplateTypeName(index, queryData.TemplateType)}";
            }
            else
            {
                cmd.Visible = true;
                cmd.Text = $"Query &{queryNumber} - \"{keyword}\" From {optionPage.GetTemplateTypeName(index, queryData.TemplateType)}";
            }
        }

        private void OnClickQuery(int index)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            QueryToWebBrowser(index, GetCurrentKeyword());
        }

        private void OnOpenOption(object sender, EventArgs e) => ShowOptionPage(typeof(OptionPage));
    }
}