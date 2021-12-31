namespace CustomWebSearch
{
    public enum QueryTemplateType
    {
        None,
        GoogleSearch,
        GoogleTranslate,
        MicrosoftMSDN,
        StackOverflow,
        Unity3dDocument,
        Unity3dAnswer,
        Custom
    }

    public enum WebBrowserType
    {
        SystemDefault,
        VisualStudio,
        CustomWebBrowser
    }

    internal static class Constants
    {
        public const int CommandIdMainMenu = 0x1100;

        public const int CommandIdOpenOption = 0x1300;

        public const int CommandIdQuery = 0x1200;

        public static readonly string[] QueryTemplateFormats = new string[]
        {
            "",
            "https://www.google.com/search?q={QUERY}",
            "https://translate.google.com/?hl=en#en/ko/{QUERY}",
            "https://social.msdn.microsoft.com/search/en-US?query={QUERY}",
            "https://stackoverflow.com/search?q={QUERY}",
            "https://docs.unity3d.com/ScriptReference/30_search.html?q={QUERY}",
            "https://answers.unity.com/search.html?q={QUERY}",
            ""
        };

        public static readonly string[] QueryTemplateTypeNames = new string[]
        {
            "-",
            "Google Search",
            "Google Translate",
            "Microsoft MSDN",
            "StackOverflow",
            "Unity3d Document",
            "Unity3d Answer",
            "Custom"
        };

        public static readonly string[] WebBrowserTypeNames = new string[]
                                                {
            "System Default",
            "Visual Studio",
            "Custom Web Browser"
        };

        public static QueryTemplateType FindTemplateType(string queryFormat)
        {
            QueryTemplateType result = QueryTemplateType.Custom;
            int maxCount = (int)result;
            for (int i = 0; i < maxCount; i++)
            {
                if (queryFormat == QueryTemplateFormats[i])
                {
                    result = (QueryTemplateType)i;
                    break;
                }
            }

            return result;
        }

        public static string GetTemplateTypeName(QueryTemplateType type) => QueryTemplateTypeNames[(int)type];
    }
}