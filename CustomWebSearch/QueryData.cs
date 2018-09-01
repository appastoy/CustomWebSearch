using System;
using System.ComponentModel;

namespace CustomWebSearch
{
    [Serializable]
    public class QueryData
    {
        public QueryTemplateType TemplateType { get; set; }
        public string CustomTemplateName { get; set; } = string.Empty;
        public string QueryFormat { get; set; } = string.Empty;

        public QueryData()
        {

        }

        public QueryData(QueryTemplateType type)
        {
            TemplateType = type;
            QueryFormat = Constants.QueryTemplateFormats[(int)TemplateType];
        }
    }
}
