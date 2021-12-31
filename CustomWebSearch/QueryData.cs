using System;

namespace CustomWebSearch
{
    [Serializable]
    public class QueryData
    {
        public QueryData()
        {
        }

        public QueryData(QueryTemplateType type)
        {
            TemplateType = type;
            QueryFormat = Constants.QueryTemplateFormats[(int)TemplateType];
        }

        public string CustomTemplateName { get; set; } = string.Empty;
        public string QueryFormat { get; set; } = string.Empty;
        public QueryTemplateType TemplateType { get; set; }
    }
}