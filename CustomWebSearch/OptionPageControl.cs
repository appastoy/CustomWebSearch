using System;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;

namespace CustomWebSearch
{
    public partial class OptionPageControl : UserControl
	{
		public const int QueryCount = 10;

		OptionPage optionPage;
		CustomWebSearchPackage package;
		ComboBox[] dropdownQueries = new ComboBox[QueryCount];
		TextBox[] txtboxQueries = new TextBox[QueryCount];

		public OptionPageControl(OptionPage optionPage)
		{
			this.optionPage = optionPage;
			InitializeComponent();
			InitializeControls();
			VerifyPackage();
		}

		void InitializeControls()
		{
			txtboxQueries[0] = txtboxQuery1;
			txtboxQueries[1] = txtboxQuery2;
			txtboxQueries[2] = txtboxQuery3;
			txtboxQueries[3] = txtboxQuery4;
			txtboxQueries[4] = txtboxQuery5;
			txtboxQueries[5] = txtboxQuery6;
			txtboxQueries[6] = txtboxQuery7;
			txtboxQueries[7] = txtboxQuery8;
			txtboxQueries[8] = txtboxQuery9;
			txtboxQueries[9] = txtboxQuery10;

			dropdownQueries[0] = dropdownQuery1;
			dropdownQueries[1] = dropdownQuery2;
			dropdownQueries[2] = dropdownQuery3;
			dropdownQueries[3] = dropdownQuery4;
			dropdownQueries[4] = dropdownQuery5;
			dropdownQueries[5] = dropdownQuery6;
			dropdownQueries[6] = dropdownQuery7;
			dropdownQueries[7] = dropdownQuery8;
			dropdownQueries[8] = dropdownQuery9;
			dropdownQueries[9] = dropdownQuery10;

			dropdownWebBrowserType.Items.Clear();
			dropdownWebBrowserType.Items.AddRange(Constants.WebBrowserTypeNames);
			for (int i = 0; i < QueryCount; i++)
			{
				dropdownQueries[i].Items.Clear();
				dropdownQueries[i].Items.AddRange(Constants.QueryTemplateTypeNames);
			}
		}

		void VerifyPackage()
		{
			if (package != null) { return; }

            var vsShell = ServiceProvider.GlobalProvider.GetService(typeof(IVsShell)) as IVsShell;
			var result = vsShell.IsPackageLoaded(new Guid(CustomWebSearchPackage.PackageGuidString), out var foundPackage);
			if (result == VSConstants.S_OK)
			{
				package = foundPackage as CustomWebSearchPackage;
			}

			if (package == null)
			{
				throw new NullReferenceException();
			}
		}

		public void UpdateProperties()
		{
			txtboxCustomWebBrowserPath.Text = optionPage.CustomWebBrowserPath;
			dropdownWebBrowserType.SelectedIndex = (int)optionPage.WebBrowserType;
			UpdateCustomWebBrowserUI();

			for (int i = 0; i < QueryCount; i++)
			{
				dropdownQueries[i].SelectedIndex = (int)optionPage.Queries[i].TemplateType;
				txtboxQueries[i].Text = optionPage.Queries[i].QueryFormat;
			}
		}

		void UpdateCustomWebBrowserUI()
		{
			var isEnabled = optionPage.WebBrowserType == WebBrowserType.CustomWebBrowser;
			txtboxCustomWebBrowserPath.Enabled = isEnabled;
			btnCustomWebBrowserPathFileDialog.Enabled = isEnabled;
		}

		private void dropdownWebBrowserType_SelectedIndexChanged(object sender, EventArgs e)
		{
			var currentType = (WebBrowserType)dropdownWebBrowserType.SelectedIndex;
			if (optionPage.WebBrowserType == currentType) { return; }

			if (currentType == WebBrowserType.CustomWebBrowser &&
				string.IsNullOrEmpty(optionPage.CustomWebBrowserPath))
			{
				if (!WebBrowserUtility.TrySelectWebBrowserPath(optionPage.CustomWebBrowserPath, out var newPath))
				{
					optionPage.CustomWebBrowserPath = newPath;
					txtboxCustomWebBrowserPath.Text = optionPage.CustomWebBrowserPath;
					dropdownWebBrowserType.SelectedIndex = (int)optionPage.WebBrowserType;
					return;
				}
			}

			optionPage.WebBrowserType = currentType;
			UpdateCustomWebBrowserUI();
		}

		private void btnCustomWebBrowserPathFileDialog_Click(object sender, EventArgs e)
		{
			if(WebBrowserUtility.TrySelectWebBrowserPath(optionPage.CustomWebBrowserPath, out var newPath))
			{
				optionPage.CustomWebBrowserPath = newPath;
				txtboxCustomWebBrowserPath.Text = optionPage.CustomWebBrowserPath;
			}
		}
		
		private void dropdownQuery_SelectedIndexChanged(object sender, EventArgs e)
		{
			var comboBox = sender as ComboBox;
			var index = int.Parse((string)comboBox.Tag);

			optionPage.SetQueryTemplateFormat(index, (QueryTemplateType)comboBox.SelectedIndex);
			txtboxQueries[index].Text = optionPage.Queries[index].QueryFormat;
		}

		private void textboxQuery_TextChanged(object sender, EventArgs e)
		{
			var txtbox = sender as TextBox;
			var index = int.Parse((string)txtbox.Tag);

			optionPage.SetQueryFormat(index, txtbox.Text);
			dropdownQueries[index].SelectedIndex = (int)optionPage.Queries[index].TemplateType;
		}

		private void btnQueryTest_Click(object sender, EventArgs e)
		{
			var button = sender as Button;
			var index = int.Parse((string)button.Tag);
			package.QueryToWebBrowser(index, "Test", true);
		}
	}
}
