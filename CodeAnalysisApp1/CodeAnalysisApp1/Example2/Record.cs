namespace CodeAnalysisApp1.Example2
{
    using System.Collections.Generic;

    /// <summary>
    /// 出力CSVの１行
    /// </summary>
    internal class Record
    {
        internal Record(string codeLocation, string access, string memberType, string name, string value, string summary)
        {
            CodeLocation = codeLocation;
            Access = access;
            MemberType = memberType;
            Name = name;
            Value = value;
            Summary = summary;
        }

        /// <summary>
        /// コードのある場所
        /// </summary>
        internal string CodeLocation { get; }

        internal string Access { get; }
        internal string MemberType { get; }
        internal string Name { get; }
        internal string Value { get; }
        internal string Summary { get; }

        internal string ToCSV()
        {
            var list = new List<string>()
                {
                    CodeLocation,
                    Access,
                    MemberType,
                    Name,
                    Value,
                    Summary,
                };

            return EscapeCSV(list);
        }

        internal static string EscapeCSV(List<string> values)
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
