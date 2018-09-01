using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Windows.Forms;

namespace CustomWebSearch
{
	public class OptionPage : DialogPage
	{
		readonly OptionPageControl optionPageControl;

        public const int QueryCount = 10;

		public WebBrowserType WebBrowserType { get; set; }
		public string CustomWebBrowserPath { get; set; } = string.Empty;
        public bool Initialized { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public QueryData[] Queries { get; set; } = new QueryData[QueryCount]
        {
            new QueryData(),
            new QueryData(),
            new QueryData(),
            new QueryData(),
            new QueryData(),
            new QueryData(),
            new QueryData(),
            new QueryData(),
            new QueryData(),
            new QueryData()
        };

        public QueryTemplateType TemplateType1 { get; set; }
        public QueryTemplateType TemplateType2 { get; set; }
        public QueryTemplateType TemplateType3 { get; set; }
        public QueryTemplateType TemplateType4 { get; set; }
        public QueryTemplateType TemplateType5 { get; set; }
        public QueryTemplateType TemplateType6 { get; set; }
        public QueryTemplateType TemplateType7 { get; set; }
        public QueryTemplateType TemplateType8 { get; set; }
        public QueryTemplateType TemplateType9 { get; set; }
        public QueryTemplateType TemplateType10 { get; set; }

        public string CustomTemplateName1 { get; set; } = string.Empty;
        public string CustomTemplateName2 { get; set; } = string.Empty;
        public string CustomTemplateName3 { get; set; } = string.Empty;
        public string CustomTemplateName4 { get; set; } = string.Empty;
        public string CustomTemplateName5 { get; set; } = string.Empty;
        public string CustomTemplateName6 { get; set; } = string.Empty;
        public string CustomTemplateName7 { get; set; } = string.Empty;
        public string CustomTemplateName8 { get; set; } = string.Empty;
        public string CustomTemplateName9 { get; set; } = string.Empty;
        public string CustomTemplateName10 { get; set; } = string.Empty;

        public string QueryFormat1 { get; set; } = string.Empty;
        public string QueryFormat2 { get; set; } = string.Empty;
        public string QueryFormat3 { get; set; } = string.Empty;
        public string QueryFormat4 { get; set; } = string.Empty;
        public string QueryFormat5 { get; set; } = string.Empty;
        public string QueryFormat6 { get; set; } = string.Empty;
        public string QueryFormat7 { get; set; } = string.Empty;
        public string QueryFormat8 { get; set; } = string.Empty;
        public string QueryFormat9 { get; set; } = string.Empty;
        public string QueryFormat10 { get; set; } = string.Empty;

        public OptionPage()
		{
			optionPageControl = new OptionPageControl(this);
		}

		public void SetQueryTemplateFormat(int queryIndex, QueryTemplateType type)
		{
			var queryData = Queries[queryIndex];
			queryData.TemplateType = type;
			if (type == QueryTemplateType.Custom) { return; }

			queryData.QueryFormat = Constants.QueryTemplateFormats[(int)type];
		}

		public void SetQueryFormat(int queryIndex, string queryFormat)
		{
			var queryData = Queries[queryIndex];
			queryData.QueryFormat = queryFormat;
			queryData.TemplateType = Constants.FindTemplateType(queryFormat);
		}

        public string GetTemplateTypeName(int queryIndex, QueryTemplateType type)
        {
            if (type == QueryTemplateType.Custom)
            {
                return Queries[queryIndex].CustomTemplateName;
            }

            return Constants.GetTemplateTypeName(type);
        }

		protected override IWin32Window Window
		{
			get { return optionPageControl; }
		}

		public override void LoadSettingsFromStorage()
		{
			base.LoadSettingsFromStorage();
            InitializeIfNot();
            OnAfterLoad();

			optionPageControl.UpdateProperties();
		}

        public override void SaveSettingsToStorage()
        {
            OnBeforeSave();
            base.SaveSettingsToStorage();
        }

        void InitializeIfNot()
        {
            if (!Initialized)
            {
                Queries = new QueryData[10]
                {
                    new QueryData(QueryTemplateType.GoogleSearch),
                    new QueryData(QueryTemplateType.GoogleTranslate),
                    new QueryData(QueryTemplateType.MicrosoftMSDN),
                    new QueryData(QueryTemplateType.StackOverflow),
                    new QueryData(QueryTemplateType.Unity3dDocument),
                    new QueryData(QueryTemplateType.Unity3dAnswer),
                    new QueryData(),
                    new QueryData(),
                    new QueryData(),
                    new QueryData()
                };
                Initialized = true;
                OnBeforeSave();
                SaveSettingsToStorage();
            }
        }

        void OnBeforeSave()
        {
            var type = GetType();
            for (int i = 0; i < QueryCount; i++)
            {
                var query = Queries[i];
                var indexString = (i + 1).ToString();
                type.GetProperty("TemplateType" + indexString).GetSetMethod().Invoke(this, new object[] { query.TemplateType });
                type.GetProperty("CustomTemplateName" + (i + 1).ToString()).GetSetMethod().Invoke(this, new object[] { query.CustomTemplateName });
                type.GetProperty("QueryFormat" + (i + 1).ToString()).GetSetMethod().Invoke(this, new object[] { query.QueryFormat });
            }
        }

        void OnAfterLoad()
        {
            var type = GetType();
            for (int i = 0; i < QueryCount; i++)
            {
                var query = Queries[i];
                var indexString = (i + 1).ToString();
                query.TemplateType = (QueryTemplateType)type.GetProperty("TemplateType" + indexString).GetGetMethod().Invoke(this, null);
                query.CustomTemplateName = (string)type.GetProperty("CustomTemplateName" + (i + 1).ToString()).GetGetMethod().Invoke(this, null);
                query.QueryFormat = (string)type.GetProperty("QueryFormat" + (i + 1).ToString()).GetGetMethod().Invoke(this, null);
            }
        }
    }
}
