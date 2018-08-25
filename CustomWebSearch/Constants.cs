
namespace CustomWebSearch
{
	public enum WebBrowserType
	{
		SystemDefault,
		VisualStudio,
		CustomWebBrowser
	}

	public enum QueryTemplateType
	{
		None,
		GoogleSearch,
		GoogleTranslate,
		StackOverflow,
		Unity3dDocument,
		Unity3dAnswer,
		Custom
	}

	static class Constants
	{
		public static readonly string[] WebBrowserTypeNames = new string[]
		{
			"System Default",
			"Visual Studio",
			"Custom Web Browser"

		};

		public static readonly string[] QueryTemplateTypeNames = new string[]
		{
			"-",
			"Google Search",
			"Google Translate",
			"StackOverflow",
			"Unity3d Document",
			"Unity3d Answer",
			"Custom"
		};

		public static readonly string[] QueryTemplateFormats = new string[]
		{
			"",
			"https://www.google.com/search?q={0}",
			"https://translate.google.co.kr/?hl=ko#en/ko/{0}",
			"https://stackoverflow.com/search?q={0}",
			"https://docs.unity3d.com/ScriptReference/30_search.html?q={0}",
			"https://answers.unity.com/search.html?q={0}",
			""
		};

		public const int CommandIdQuery = 0x1200;
		public const int CommandIdOpenOption = 0x1300;
	}
}
