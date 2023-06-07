namespace CodeAnalysisApp1.Example2
{
    using System.Collections.Generic;

    /// <summary>
    /// 出力CSVの１行
    /// </summary>
    internal class Record
    {
        internal Record(string type, string access, string memberType, string name, string value, string summary)
        {
            Type = type;
            Access = access;
            MemberType = memberType;
            Name = name;
            Value = value;
            Summary = summary;
        }

        internal string Type { get; }
        internal string Access { get; }
        internal string MemberType { get; }
        internal string Name { get; }
        internal string Value { get; }
        internal string Summary { get; }

        internal string ToCSV()
        {
            var list = new List<string>()
                {
                    Type,
                    Access,
                    MemberType,
                    Name,
                    Value,
                    Summary,
                };

            return EscapeCSV(list);
        }

        static string EscapeCSV(List<string> values)
        {
            var escapedValues = new List<string>();

            foreach (var value in values)
            {
                // ダブル・クォーテーションは２つ重ねる
                var escapedValue = value.Replace("\"", "\"\"");

                // カンマが含まれていれば、ダブル・クォーテーションで挟む
                if (escapedValue.Contains(","))
                {
                    escapedValue = $"\"{escapedValue}\"";
                }

                escapedValues.Add(escapedValue);
            }

            return string.Join(",", escapedValues);
        }
    }
}
