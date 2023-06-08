namespace CodeAnalysisApp1.Example2
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

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

            list = EscapeCSV(list);
            list = EscapeForGoogleSpreadSheet(list);

            return string.Join(",", list);
        }

        internal static List<string> EscapeCSV(List<string> values)
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

            return escapedValues;
        }

        static Regex RegexStartEquals = new Regex(@"^\s*=", RegexOptions.Multiline);
        static Regex RegexStartDquotEquals = new Regex(@"^\s*""\s*=", RegexOptions.Multiline);
        // static Regex RegexRemoveDquot = new Regex(@"^\s*""(.*)""\s*$", RegexOptions.Multiline);

        /// <summary>
        /// グーグル・スプレッドシート向けのエスケープ
        /// 
        /// - EscapeCSV を先に行っておく必要がある
        /// - Excel とは互換性が無くなる
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        internal static List<string> EscapeForGoogleSpreadSheet(List<string> values)
        {
            var escapedValues = new List<string>();

            foreach (var value in values)
            {
                string escapedValue;

                //
                // `=T("x")` の x が挟めているように、前後を挟む
                // =============================================
                //
                //- 空白を無視して = で始まるなら
                if (RegexStartEquals.IsMatch(value))
                {
                    escapedValue = $"=T(\"{value}\")";
                }
                // 
                // - 空白を無視して "= で始まるなら
                else if (RegexStartDquotEquals.IsMatch(value))
                {
                    //if (value.Contains(","))
                    //{
                    //    // さらにカンマが含まれているケース
                    //    // CSV例： "=T(""= new Dictionary<string, AssetEntity>()"")"
                    //}
                    //else
                    //{
                    //    // CSV例： "=T(""= new Dictionary<string>()"")"
                    //}


                    escapedValue = $"\"=T(\"{value}\")\"";
                    //// 両端のダブルクォーテーションを外す
                    //var match = RegexRemoveDquot.Match(value);
                    //if (match.Success)
                    //{
                    //    escapedValue = $"=T({match.Groups[1]})";
                    //}
                    //else
                    //{
                    //    // パース・エラー。エスケープを止める
                    //    escapedValue = value;
                    //}
                }
                else
                {
                    escapedValue = value;
                }

                escapedValues.Add(escapedValue);
            }

            return escapedValues;
        }

    }
}
