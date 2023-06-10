namespace CodeAnalysisApp1.Example2
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// 出力CSVの１行
    /// </summary>
    internal class Record
    {
        /// <summary>
        /// 生成
        /// </summary>
        /// <param name="kind">種類</param>
        /// <param name="codeLocation">コードのある場所</param>
        /// <param name="modifiers">修飾子</param>
        /// <param name="memberType">メンバー型</param>
        /// <param name="name">名前</param>
        /// <param name="value">値</param>
        /// <param name="summary">ドキュメント・コメントの概要</param>
        internal Record(string kind, string codeLocation, string modifiers, string memberType, string name, string value, string summary)
        {
            Kind = kind;
            CodeLocation = codeLocation;
            Modifiers = modifiers;
            MemberType = memberType;
            Name = name;
            Value = value;
            Summary = summary;
        }

        /// <summary>
        /// 種類
        /// </summary>
        internal string Kind { get; }

        /// <summary>
        /// コードのある場所
        /// </summary>
        internal string CodeLocation { get; }

        /// <summary>
        /// 修飾子
        /// </summary>
        internal string Modifiers { get; }

        /// <summary>
        /// メンバー型
        /// </summary>
        internal string MemberType { get; }

        /// <summary>
        /// 名前
        /// </summary>
        internal string Name { get; }

        /// <summary>
        /// 値
        /// </summary>
        internal string Value { get; }

        /// <summary>
        /// ドキュメント・コメントの概要
        /// </summary>
        internal string Summary { get; }

        /// <summary>
        /// CSV出力のためのテキスト作成
        /// </summary>
        /// <returns>CSV形式テキスト</returns>
        internal string ToCSV()
        {
            var list = new List<string>()
                {                    
                    CodeLocation,
                    Kind,
                    Modifiers,
                    MemberType,
                    Name,
                    Value,
                    Summary,
                };

            list = EscapeCSV(list);
            list = EscapeForGoogleSpreadSheet(list);

            return string.Join(",", list);
        }

        /// <summary>
        /// CSVエスケープ
        /// </summary>
        /// <param name="row">各列のデータ</param>
        /// <returns>変換後の各列のデータ</returns>
        internal static List<string> EscapeCSV(List<string> row)
        {
            var escapedValues = new List<string>();

            foreach (var cell in row)
            {
                // ダブル・クォーテーションは２つ重ねる
                var escapedValue = cell.Replace("\"", "\"\"");

                // カンマが含まれていれば、ダブル・クォーテーションで挟む
                if (escapedValue.Contains(","))
                {
                    escapedValue = $"\"{escapedValue}\"";
                }

                escapedValues.Add(escapedValue);
            }

            return escapedValues;
        }

        /// <summary>
        /// "=" で始まる
        /// </summary>
        static Regex RegexStartEquals = new Regex(@"^\s*=", RegexOptions.Multiline);

        /// <summary>
        /// `"=` で始まる
        /// </summary>
        static Regex RegexStartDquotEquals = new Regex(@"^\s*""\s*=", RegexOptions.Multiline);

        /// <summary>
        /// 両端のダブルクォーテーションで囲まれた内側
        /// </summary>
        static Regex RegexRemoveDquot = new Regex(@"^\s*""(.*)""\s*$", RegexOptions.Singleline);

        /// <summary>
        /// グーグル・スプレッドシート向けのエスケープ
        /// 
        /// - EscapeCSV を先に行っておく必要がある
        /// - Excel とは互換性が無くなる
        /// </summary>
        /// <param name="row">各列のデータ</param>
        /// <returns>変換後の各列のデータ</returns>
        internal static List<string> EscapeForGoogleSpreadSheet(List<string> row)
        {
            var escapedValues = new List<string>();

            foreach (var value in row)
            {
                string escapedValue;

                //
                // `=T("x")` の x が挟めているように、前後を挟む
                // =============================================
                //
                //- 空白を無視して = で始まるなら
                if (RegexStartEquals.IsMatch(value))
                {
                    escapedValue = value;

                    // これからダブルクォーテーションで挟むので、既存のダブルクォーテーションを二重にする
                    escapedValue = escapedValue.Replace("\"", "\"\"");

                    escapedValue = $"\"=T(\"\"{escapedValue}\"\")\"";
                }
                // 
                // - 空白を無視して "= で始まるなら
                else if (RegexStartDquotEquals.IsMatch(value))
                {
                    escapedValue = value;

                    // いったん両端のダブルクォーテーションを外す
                    var match = RegexRemoveDquot.Match(value);
                    if (match.Success)
                    {
                        escapedValue = match.Groups[1].Value;

                        // これからダブルクォーテーションで挟むので、既存のダブルクォーテーションを二重にする
                        escapedValue = escapedValue.Replace("\"", "\"\"");

                        escapedValue = $"\"=T(\"\"{escapedValue}\"\")\"";
                    }
                    else
                    {
                        // パース・エラー
                        escapedValue = $"[[Parse Error 125]] {value}";
                    }
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
