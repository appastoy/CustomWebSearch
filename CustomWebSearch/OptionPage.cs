using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Windows.Forms;

namespace CustomWebSearch
{
	[System.Serializable]
	public class QueryData
	{
		public QueryTemplateType TemplateType { get; set; }
		public string QueryFormat { get; set; }

		public QueryData()
		{

		}

		public QueryData(QueryTemplateType type)
		{
			TemplateType = type;
			QueryFormat = Constants.QueryTemplateFormats[(int)TemplateType];
		}
	}

	public class OptionPage : DialogPage
	{
		readonly OptionPageControl optionPageControl;

		public WebBrowserType WebBrowserType { get; set; }
		public string CustomWebBrowserPath { get; set; } = string.Empty;
		public bool IsInitialized { get; set; } = false;

		public QueryData[] Queries { get; set; } = new QueryData[10]
		{
			new QueryData(QueryTemplateType.GoogleSearch),
			new QueryData(QueryTemplateType.GoogleTranslate),
			new QueryData(QueryTemplateType.StackOverflow),
			new QueryData(QueryTemplateType.Unity3dDocument),
			new QueryData(QueryTemplateType.Unity3dAnswer),
			new QueryData(),
			new QueryData(),
			new QueryData(),
			new QueryData(),
			new QueryData()
		};

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
			queryData.TemplateType = FindTemplateType(queryFormat);
		}

		QueryTemplateType FindTemplateType(string queryFormat)
		{
			var result = QueryTemplateType.Custom;
			int maxCount = (int)result;
			for (int i = 0; i < maxCount; i++)
			{
				if (queryFormat == Constants.QueryTemplateFormats[i])
				{
					result = (QueryTemplateType)i;
					break;
				}
			}

			return result;
		}

		protected override IWin32Window Window
		{
			get { return optionPageControl; }
		}

		public override void LoadSettingsFromStorage()
		{
			base.LoadSettingsFromStorage();
			optionPageControl.UpdateProperties();
		}
	}
}
