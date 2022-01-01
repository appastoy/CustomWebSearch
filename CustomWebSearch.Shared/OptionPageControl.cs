using Microsoft.VisualStudio.Shell;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CustomWebSearch
{
    public partial class OptionPageControl : UserControl
    {
        public const int QueryCount = 10;
        private readonly ComboBox[] dropdownQueries = new ComboBox[QueryCount];
        private readonly OptionPage optionPage;
        private readonly TextBox[] txtboxCustomTemplateTypes = new TextBox[QueryCount];
        private readonly TextBox[] txtboxQueries = new TextBox[QueryCount];
        private int txtboxQueryOriginalLocationX;
        private Size txtboxQueryOriginalSize;

        public OptionPageControl(OptionPage optionPage)
        {
            this.optionPage = optionPage;
            InitializeComponent();
            InitializeControls();
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

        private void btnCustomWebBrowserPathFileDialog_Click(object sender, EventArgs e)
        {
            if (WebBrowserUtility.TrySelectWebBrowserPath(optionPage.CustomWebBrowserPath, out string newPath))
            {
                optionPage.CustomWebBrowserPath = newPath;
                txtboxCustomWebBrowserPath.Text = optionPage.CustomWebBrowserPath;
            }
        }

        private void btnQueryTest_Click(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Button button = sender as Button;
            int index = int.Parse((string)button.Tag);
            CustomWebSearchPackage.Instance.QueryToWebBrowser(index, "Test", true);
        }

        private void dropdownQuery_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            int index = int.Parse((string)comboBox.Tag);

            optionPage.SetQueryTemplateFormat(index, (QueryTemplateType)comboBox.SelectedIndex);
            txtboxQueries[index].Text = optionPage.Queries[index].QueryFormat;

            QueryData queryData = optionPage.Queries[index];
            TextBox txtboxCustomTemplateType = txtboxCustomTemplateTypes[index];
            TextBox txtboxQuery = txtboxQueries[index];
            if (queryData.TemplateType == QueryTemplateType.Custom)
            {
                int term = txtboxQueryOriginalLocationX - comboBox.Location.X - txtboxCustomTemplateType.Size.Width;
                txtboxQuery.Location = new Point(txtboxQueryOriginalLocationX + term, txtboxQuery.Location.Y);
                txtboxQuery.Size = new Size(txtboxQueryOriginalSize.Width - term, txtboxQueryOriginalSize.Height);
                txtboxCustomTemplateType.Enabled = true;
                txtboxCustomTemplateType.Visible = true;
                if (string.IsNullOrEmpty(queryData.CustomTemplateName))
                {
                    queryData.CustomTemplateName = "Custom";
                }
                txtboxCustomTemplateType.Text = queryData.CustomTemplateName;
            }
            else
            {
                txtboxQuery.Location = new Point(txtboxQueryOriginalLocationX, txtboxQuery.Location.Y);
                txtboxQuery.Size = txtboxQueryOriginalSize;
                txtboxCustomTemplateType.Enabled = false;
                txtboxCustomTemplateType.Visible = false;
                queryData.CustomTemplateName = string.Empty;
            }
        }

        private void dropdownWebBrowserType_SelectedIndexChanged(object sender, EventArgs e)
        {
            WebBrowserType currentType = (WebBrowserType)dropdownWebBrowserType.SelectedIndex;
            if (optionPage.WebBrowserType == currentType) { return; }

            if (currentType == WebBrowserType.CustomWebBrowser &&
                string.IsNullOrEmpty(optionPage.CustomWebBrowserPath))
            {
                if (!WebBrowserUtility.TrySelectWebBrowserPath(optionPage.CustomWebBrowserPath, out string newPath))
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

        private void InitializeControls()
        {
            txtboxQueryOriginalSize = txtboxQuery1.Size;
            txtboxQueryOriginalLocationX = txtboxQuery1.Location.X;

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
                int currentIndex = i;
                ComboBox dropdownQuery = dropdownQueries[i];
                dropdownQuery.Items.Clear();
                dropdownQuery.Items.AddRange(Constants.QueryTemplateTypeNames);
                TextBox txtboxCustomTemplateType = new TextBox
                {
                    Parent = dropdownQuery.Parent,
                    Location = txtboxQueries[i].Location,
                    Size = new Size(dropdownQuery.Size.Width >> 1, dropdownQuery.Size.Height),
                    Enabled = false,
                    Visible = false
                };
                txtboxCustomTemplateType.TextChanged += (s, e) => TxtboxCustomTemplateType_TextChanged(currentIndex);
                txtboxCustomTemplateTypes[i] = txtboxCustomTemplateType;
            }
        }

        private void textboxQuery_TextChanged(object sender, EventArgs e)
        {
            TextBox txtbox = sender as TextBox;
            int index = int.Parse((string)txtbox.Tag);

            optionPage.SetQueryFormat(index, txtbox.Text);
            dropdownQueries[index].SelectedIndex = (int)optionPage.Queries[index].TemplateType;
        }

        private void TxtboxCustomTemplateType_TextChanged(int index) => optionPage.Queries[index].CustomTemplateName = txtboxCustomTemplateTypes[index].Text;

        private void UpdateCustomWebBrowserUI()
        {
            bool isEnabled = optionPage.WebBrowserType == WebBrowserType.CustomWebBrowser;
            txtboxCustomWebBrowserPath.Enabled = isEnabled;
            btnCustomWebBrowserPathFileDialog.Enabled = isEnabled;
        }
    }
}