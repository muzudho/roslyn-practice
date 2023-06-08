namespace CodeAnalysisApp1.Example2
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;

    internal class Example2
    {
        internal static void DoIt(
            string filePathToRead,
            LazyCoding.SetValue<List<RecordEx>> setRecordExList)
        {
            // コンソールに出力すると文字化けするのは、コンソールの方のエンコーディング設定（コードページ）が悪い
            // 📖 [How to get CMD/console encoding in C#](https://stackoverflow.com/questions/5910573/how-to-get-cmd-console-encoding-in-c-sharp)
            Console.OutputEncoding = Encoding.UTF8; // これでも絵文字は表示されない

            // 読込対象のテキスト
            string programText = File.ReadAllText(filePathToRead, Encoding.UTF8);

            //
            // テキストをパースして、ツリー作成
            // ツリーから根を取得
            // 根にぶら下がっている最初のものの種類
            //
            SyntaxTree tree = CSharpSyntaxTree.ParseText(
                text: programText,
                encoding: Encoding.UTF8);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            var recordExList = new List<RecordEx>();

            foreach (var rootMember in root.Members)
            {
                switch (rootMember.Kind())
                {
                    case SyntaxKind.NamespaceDeclaration:
                        {
                            var namespaceDeclaration = (NamespaceDeclarationSyntax)rootMember;

                            // クラスが２個定義されてるとか、列挙型が定義されてるとかに対応
                            foreach (var memberDeclaration in namespaceDeclaration.Members)
                            {
                                switch (memberDeclaration.Kind())
                                {
                                    case SyntaxKind.ClassDeclaration:
                                        {
                                            ParseClassDeclaration(
                                                setRecord: (record) =>
                                                {
                                                    recordExList.Add(new RecordEx(
                                                        recordObj: record,
                                                        filePathToRead: filePathToRead));
                                                },
                                                @namespace: namespaceDeclaration.Name.ToString(),
                                                programDeclaration: (ClassDeclarationSyntax)memberDeclaration);
                                        }
                                        break;

                                    case SyntaxKind.InterfaceDeclaration:
                                        {
                                            var programDeclaration = (InterfaceDeclarationSyntax)memberDeclaration;

                                            foreach (var programDeclarationMember in programDeclaration.Members)
                                            {
                                                switch (programDeclarationMember.Kind())
                                                {
                                                    // フィールドの宣言部なら
                                                    case SyntaxKind.FieldDeclaration:
                                                        {
                                                            //
                                                            // プログラム中の宣言メンバーの１つ目
                                                            //
                                                            var fieldDeclaration = (FieldDeclarationSyntax)programDeclarationMember;
                                                            //            fullString:         /// <summary>
                                                            //                                /// ?? 章Idの前に
                                                            //                                /// </summary>
                                                            //public int beforeChapterId;

                                                            var record = ParseField(
                                                                fieldDeclaration: fieldDeclaration,
                                                                @namespace: namespaceDeclaration.Name.ToString());
                                                            recordExList.Add(new RecordEx(
                                                                recordObj: record,
                                                                filePathToRead: filePathToRead));
                                                        }
                                                        break;

                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                        break;

                                    case SyntaxKind.EnumDeclaration:
                                        {
                                            ParseEnumDeclaration(
                                                setRecord: (record) =>
                                                {
                                                    recordExList.Add(new RecordEx(
                                                        recordObj: record,
                                                        filePathToRead: filePathToRead));
                                                },
                                                @namespace: string.Empty,
                                                programDeclaration: (EnumDeclarationSyntax)memberDeclaration);
                                        }
                                        break;

                                    default:
                                        {
                                            var message = $"[[What? 119]] memberDeclaration.Kind(): {memberDeclaration.Kind().ToString()}";

                                            recordExList.Add(new RecordEx(
                                                recordObj: new Record(
                                                    type: string.Empty,
                                                    access: string.Empty,
                                                    memberType: string.Empty,
                                                    name: string.Empty,
                                                    value: string.Empty,
                                                    summary: message),
                                                filePathToRead: filePathToRead));
                                            Console.WriteLine(message);
                                        }
                                        break;
                                }
                            }
                        }
                        break;

                    case SyntaxKind.ClassDeclaration:
                        {
                            var classDeclaration = (ClassDeclarationSyntax)rootMember;

                            ParseClassDeclaration(
                                setRecord: (record) =>
                                {
                                    recordExList.Add(new RecordEx(
                                        recordObj: record,
                                        filePathToRead: filePathToRead));
                                },
                                // トップ・レベルだから、ネームスペースは無い
                                @namespace: string.Empty,
                                programDeclaration: classDeclaration);
                        }
                        break;

                    default:
                        {
                            var message = $"[[What? 143]] rootMember.Kind(): {rootMember.Kind().ToString()}";

                            recordExList.Add(new RecordEx(
                                recordObj: new Record(
                                    type: string.Empty,
                                    access: string.Empty,
                                    memberType: string.Empty,
                                    name: string.Empty,
                                    value: string.Empty,
                                    summary: message),
                                filePathToRead: filePathToRead));
                            Console.WriteLine(message);
                        }
                        break;

                }

            }

            setRecordExList(recordExList);
        }

        /// <summary>
        /// class 宣言を解析
        /// </summary>
        static void ParseClassDeclaration(LazyCoding.SetValue<Record> setRecord, string @namespace, ClassDeclarationSyntax programDeclaration)
        {
            // サブ・クラスが２個定義されてるとか、サブ・列挙型が定義されてるとかに対応
            foreach (var programDeclarationMember in programDeclaration.Members)
            {
                switch (programDeclarationMember.Kind())
                {
                    // フィールドの宣言部なら
                    case SyntaxKind.FieldDeclaration:
                        {
                            var fieldDeclaration = (FieldDeclarationSyntax)programDeclarationMember;
                            //            fullString:         /// <summary>
                            //                                /// ?? 章Idの前に
                            //                                /// </summary>
                            //public int beforeChapterId;

                            var record = ParseField(
                                fieldDeclaration: fieldDeclaration,
                                // ネームスペース.親クラス名　とつなげる
                                @namespace: $"{@namespace}.{programDeclaration.Identifier.ToString()}");
                            setRecord(record);
                        }
                        break;

                    // メソッドの宣言部なら
                    case SyntaxKind.MethodDeclaration:
                        {
                            var methodDeclaration = (MethodDeclarationSyntax)programDeclarationMember;

                            var record = ParseMethod(
                                methodDeclaration: methodDeclaration,
                                // ネームスペース.親クラス名　とつなげる
                                @namespace: $"{@namespace}.{programDeclaration.Identifier.ToString()}");
                            setRecord(record);
                        }
                        break;

                    // サブ列挙型
                    case SyntaxKind.EnumDeclaration:
                        {
                            ParseEnumDeclaration(
                                setRecord: setRecord,
                                // ネームスペース.親クラス名.自列挙型名　とつなげる
                                @namespace: $"{@namespace}.{programDeclaration.Identifier.ToString()}.{((EnumDeclarationSyntax)programDeclarationMember).Identifier}",
                                programDeclaration: (EnumDeclarationSyntax)programDeclarationMember);
                        }
                        break;

                    default:
                        {
                            var message = $"[[What? 224]] programDeclarationMember.Kind(): {programDeclarationMember.Kind().ToString()}";

                            setRecord(new Record(
                                    type: string.Empty,
                                    access: string.Empty,
                                    memberType: string.Empty,
                                    name: string.Empty,
                                    value: string.Empty,
                                    summary: message));

                            Console.WriteLine(message);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 列挙型の定義を解析
        /// </summary>
        static void ParseEnumDeclaration(LazyCoding.SetValue<Record> setRecord, string @namespace, EnumDeclarationSyntax programDeclaration)
        {
            foreach (var programDeclarationMember in programDeclaration.Members)
            {
                switch (programDeclarationMember.Kind())
                {
                    // フィールドの宣言部なら
                    case SyntaxKind.EnumMemberDeclaration:
                        {
                            //
                            // プログラム中の宣言メンバーの１つ目
                            //
                            var fieldDeclaration = programDeclarationMember;

                            var record = ParseField(fieldDeclaration, @namespace);
                            setRecord(record);
                        }
                        break;

                    default:
                        {
                            var message = $"[[What? 265]] programDeclarationMember.Kind(): {programDeclarationMember.Kind().ToString()}";

                            setRecord(new Record(
                                    type: string.Empty,
                                    access: string.Empty,
                                    memberType: string.Empty,
                                    name: string.Empty,
                                    value: string.Empty,
                                    summary: message));

                            Console.WriteLine(message);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// class, interface のフィールド用
        /// 
        /// - プロパティにもヒットする
        /// </summary>
        /// <param name="fieldDeclaration"></param>
        /// <returns></returns>
        static Record ParseField(FieldDeclarationSyntax fieldDeclaration, string @namespace)
        {
            //
            // モディファイア
            // ==============
            //
            var modifiers = fieldDeclaration.Modifiers;
            // Modifiers:           public

            //
            // デクラレーション
            // ================
            //
            string declarationHeadText;
            string name;
            string value;
            if (fieldDeclaration.Declaration != null)
            {
                // Declaration:         int beforeChapterId
                //
                // List<string> AttackMotionImageLabel = new List<string> { "無し", "ダガー", "剣", "フレイル", "斧", "ウィップ", "杖", "弓", "クロスボウ", "銃", "爪", "グローブ", "槍", "メイス", "ロッド", "こん棒", "チェーン", "未来の剣", "パイプ", "ショットガン", "ライフル", "チェーンソー", "レールガン", "スタンロッド", "ユーザ定義1", "ユーザ定義2", "ユーザ定義3", "ユーザ定義4", "ユーザ定義5", "ユーザ定義6" }

                // 連続する空白を１つにしてみる
                // 📖 [Replace consecutive whitespace characters with a single space in C#](https://www.techiedelight.com/replace-consecutive-whitespace-by-single-space-csharp/)
                // var declarationText = Regex.Replace(fieldDeclaration.Declaration.ToString(), @"\s+", " ");
                var declarationText = Regex.Replace(fieldDeclaration.Declaration.ToString(), @"\s+", " ");

                // "=" を含むか？
                if (declarationText.Contains("="))
                {
                    // "=" より前だけ取るか
                    var tokenList = declarationText.Split('=').ToList();

                    declarationText = tokenList[0].TrimEnd();
                    tokenList.RemoveAt(0);
                    value = string.Join("=", tokenList);
                }
                else
                {
                    value = string.Empty;
                }

                // とりあえず半角スペースで区切ってみるか
                string[] list = declarationText.ToString().Split(' ');

                var declarationHead = new string[list.Length - 1];
                Array.Copy(list, 0, declarationHead, 0, list.Length - 1);
                declarationHeadText = string.Join(" ", declarationHead);
                name = list[list.Length - 1];
            }
            else
            {
                declarationHeadText = string.Empty;
                name = string.Empty;
                value = string.Empty;
            }

            //
            // 前トリビア
            // ==========
            //
            var leadingTrivia = fieldDeclaration.GetLeadingTrivia();
            //leadingTrivia:         /// <summary>
            //                       /// ?? 章Idの前に
            //                       /// </summary>

            var documentCommentBuilder = new StringBuilder();

            // `leadingTrivia.ToFullString()` は、 `#if UNITY_EDITOR` のようなものも、トリビアとして巻き込んで読取るから難しい。例えば以下は１つのトリビア
            /*
            
#if UNITY_EDITOR
        /// <summary>
        /// 😁 ファイルパス　＞　イベント共通の翻訳テキスト
        /// </summary>
        private const string JsonFileTranslation = "Assets/RPGMaker/Storage/Event/JSON/eventCommonTranslation.txt";
#endif

        /// <summary>
        /// 😁 イベント共通データ・モデル
        /// </summary>
             */


            //
            // ドキュメント・コメント
            // ======================
            //
            var documentCommentText = ChangeLeadingTriviaToDocumentCommentXMLText(leadingTrivia);

            string summaryText = ParseDocumentComment(documentCommentText);

            return new Record(
                type: @namespace,
                access: modifiers.ToString(),
                memberType: declarationHeadText,
                name: name,
                value: value,
                summary: summaryText);
        }

        /// <summary>
        /// メソッド解析
        /// </summary>
        /// <param name="methodDeclaration"></param>
        /// <returns></returns>
        static Record ParseMethod(MethodDeclarationSyntax methodDeclaration, string @namespace)
        {
            var builder = new StringBuilder();

            //
            // 引数の個数か？
            //
            // builder.AppendLine($"Arity:                         {methodDeclaration.Arity}");
            // Arity:                         1
            // Arity:                         0

            //
            // アノテーションか？
            //
            // builder.AppendLine($"AttributeLists:                {methodDeclaration.AttributeLists}");
            // AttributeLists:                
            // AttributeLists:                [Conditional(""____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____"")]
            // AttributeLists:                [Conditional(""DEBUG_UTIL_TEST_LOG"")]

            //
            // 本文だろうか？
            //
            // builder.AppendLine($"Body:                          {methodDeclaration.Body}");

            //
            // ジェネリック型への制約か？
            //
            // builder.AppendLine($"ConstraintClauses:             {methodDeclaration.ConstraintClauses}");
            // ConstraintClauses:             
            // ConstraintClauses:             where T : IComparable<T>
            // ConstraintClauses:             where TEnum : struct
            // ConstraintClauses:             where T : class

            //
            // なんだろう？
            //
            // builder.AppendLine($"ContainsAnnotations:           {methodDeclaration.ContainsAnnotations}");
            // ContainsAnnotations:           False

            //
            // なんだろう？
            //
            // builder.AppendLine($"ContainsDiagnostics:           {methodDeclaration.ContainsDiagnostics}");
            // ContainsDiagnostics:           False

            //
            // なんだろう？
            //
            // builder.AppendLine($"ContainsDirectives:            {methodDeclaration.ContainsDirectives}");
            // ContainsDirectives:            True
            // ContainsDirectives:            False

            //
            // なんだろう？
            //
            // builder.AppendLine($"ContainsSkippedText:           {methodDeclaration.ContainsSkippedText}");
            // ContainsSkippedText:           False

            //
            // なんだろう？
            //
            // builder.AppendLine($"ExplicitInterfaceSpecifier:    {methodDeclaration.ExplicitInterfaceSpecifier}");
            // ExplicitInterfaceSpecifier:    

            //
            // なんだろう？
            //
            // builder.AppendLine($"ExpressionBody:                {methodDeclaration.ExpressionBody}");
            // ExpressionBody:                

            //
            // コードの記述の開始文字、終了文字位置か？
            //
            // builder.AppendLine($"FullSpan:                      {methodDeclaration.FullSpan}");
            // FullSpan:                      [36254..36638)
            // FullSpan:                      [487..951)

            //
            // 多分、前方に付いている文字の固まりがあるかどうかだと思う
            //
            // builder.AppendLine($"HasLeadingTrivia:              {methodDeclaration.HasLeadingTrivia}");
            // HasLeadingTrivia:              True

            //
            // なんだろう？
            //
            // builder.AppendLine($"HasStructuredTrivia:           {methodDeclaration.HasStructuredTrivia}");
            // HasStructuredTrivia:           True

            //
            // なんだろう？
            //
            // builder.AppendLine($"HasTrailingTrivia:             {methodDeclaration.HasTrailingTrivia}");
            // HasTrailingTrivia:             True

            //
            // 関数名
            //
            // builder.AppendLine($"Identifier:                    {methodDeclaration.Identifier}");
            // Identifier:                    Start
            // Identifier:                    Stop
            // Identifier:                    OnDisable
            // Identifier:                    GetTextUnicodeHalfwidthCount

            //
            // なんだろう？
            //
            // builder.AppendLine($"IsMissing:                     {methodDeclaration.IsMissing}");
            // IsMissing:                     False

            //
            // なんだろう？
            //
            // builder.AppendLine($"IsStructuredTrivia:            {methodDeclaration.IsStructuredTrivia}");
            // IsStructuredTrivia:            False

            //
            // プログラミング言語の種類
            //
            // builder.AppendLine($"Language:                      {methodDeclaration.Language}");
            // Language:                      C#

            //
            // 修飾子
            //
            // builder.AppendLine($"Modifiers:                     {methodDeclaration.Modifiers}");
            // Modifiers:                     public static
            // Modifiers:                     private

            //
            // 引数のリストの記述
            //
            // builder.AppendLine($"ParameterList:                 {methodDeclaration.ParameterList}");
            // ParameterList:                 (IEnumerator routine)
            // ParameterList:                 (ref Coroutine coroutine)
            // ParameterList:                 (int lhs, int rhs, int min = int.MinValue, int max = int.MaxValue)
            // ParameterList:                 (
            // bool condition,
            // [CallerLineNumber] int sourceLineNumber = 0,
            // [CallerFilePath] string sourceFilePath = "",
            // [CallerMemberName] string memberName = "")

            //
            // これを含むソースコード本文か？
            //
            // builder.AppendLine($"Parent:                        {methodDeclaration.Parent}");

            //
            // なんだろう？
            //
            // builder.AppendLine($"ParentTrivia:                  {methodDeclaration.ParentTrivia}");
            // ParentTrivia:                  

            //
            // なんだろう？
            //
            // builder.AppendLine($"RawKind:                       {methodDeclaration.RawKind}");
            // RawKind:                       8875

            //
            // 戻り値の型の記述
            //
            // builder.AppendLine($"ReturnType:                    {methodDeclaration.ReturnType}");
            // ReturnType:                    Coroutine
            // ReturnType:                    void
            // ReturnType:                    IEnumerable<(T item, int index)>

            //
            // なんだろう？
            //
            // builder.AppendLine($"SemicolonToken:                {methodDeclaration.SemicolonToken}");
            // SemicolonToken:                

            //
            // 文字開始位置、終了位置か？
            //
            // builder.AppendLine($"Span:                          {methodDeclaration.Span}");
            // Span:                          [36415..36636)
            // Span:                          [476..667)

            //
            // 文字開始位置か？
            //
            // builder.AppendLine($"SpanStart:                     {methodDeclaration.SpanStart}");
            // SpanStart:                     36415
            // SpanStart:                     493

            //
            // 長いソースコード？
            //
            // builder.AppendLine($"SyntaxTree:                    {methodDeclaration.SyntaxTree}");

            //
            // 型パラメーターのリスト？
            //
            // builder.AppendLine($"TypeParameterList:             {methodDeclaration.TypeParameterList}");
            // TypeParameterList:             
            // TypeParameterList:             <T>
            // TypeParameterList:             <TEnum>

            //
            // ドキュメント・コメント
            // ======================
            //
            var leadingTrivia = methodDeclaration.GetLeadingTrivia();
            var documentCommentText = ChangeLeadingTriviaToDocumentCommentXMLText(leadingTrivia);
            string summaryText = ParseDocumentComment(documentCommentText);

            return new Record(
                type: @namespace,
                access: methodDeclaration.Modifiers.ToString(),         // 修飾子
                memberType: methodDeclaration.ReturnType.ToString(),    // 戻り値の型
                name: methodDeclaration.Identifier.ToString(),          // 関数名
                value: string.Empty,                                    // 値は空  
                summary: summaryText);                                  // ドキュメント・コメントの summary

            // テスト用 summary: builder.ToString()
        }

        /// <summary>
        /// enum 型のメンバー用
        /// </summary>
        /// <param name="enumMemberDeclaration"></param>
        /// <returns></returns>
        static Record ParseField(EnumMemberDeclarationSyntax enumMemberDeclaration, string @namespace)
        {
            var modifiers = enumMemberDeclaration.Modifiers;
            // Modifiers:           public

            var identifierText = enumMemberDeclaration.Identifier.ToString();

            var leadingTrivia = enumMemberDeclaration.GetLeadingTrivia();
            //leadingTrivia:         /// <summary>
            //                       /// ?? 章Idの前に
            //                       /// </summary>

            //
            // Enum 値
            //
            // 📖 [Roslyn CodeAnalysisでenumの値を取得したい](https://teratail.com/questions/290108?sort=1)
            // 静的には、取れないようだ？
            //
            // `= 値` が書かれているかどうかは取得できるようだ？
            //
            string enumValue;
            if (enumMemberDeclaration.EqualsValue != null)
            {
                var equalsValueText = enumMemberDeclaration.EqualsValue.ToString();
                var match = Regex.Match(equalsValueText, @"=\s*(.*)");
                if (match.Success)
                {
                    enumValue = match.Groups[1].ToString();
                }
                else
                {
                    enumValue = equalsValueText;
                }
            }
            else
            {
                enumValue = string.Empty;
            }

            //
            // ドキュメント・コメント
            // ======================
            //
            var documentCommentText = ChangeLeadingTriviaToDocumentCommentXMLText(leadingTrivia);
            string summaryText = ParseDocumentComment(documentCommentText);

            return new Record(
                type: @namespace,
                access: modifiers.ToString(),
                memberType: string.Empty,
                name: identifierText,
                value: enumValue,
                summary: summaryText);
        }

        /// <summary>
        /// ドキュメント・コメント文字列から、XML形式文字列を取得
        /// </summary>
        /// <param name="leadingTrivia">先行トリビア</param>
        /// <returns>XML形式文字列</returns>
        static string ChangeLeadingTriviaToDocumentCommentXMLText(SyntaxTriviaList leadingTrivia)
        {
            var documentCommentBuilder = new StringBuilder();
            var documentComment = leadingTrivia.ToFullString();
            // var documentComment2 = leadingTrivia.ToString(); // ToFullString() と同じじゃないか？

            // 改行は必ず `\r\n` （CRLF） とすること
            var documentCommentLines = documentComment.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            foreach (var line in documentCommentLines)
            {
                var match = Regex.Match(line, @"\s*/// ?(.*)");
                if (match.Success)
                {
                    var content = match.Groups[1];
                    documentCommentBuilder.AppendLine(content.ToString());
                }
            }

            return documentCommentBuilder.ToString();
            // documentCommentText: < summary >
            // 😁 章Idの前に
            // </ summary >
        }

        /// <summary>
        /// ドキュメント・コメントの解析
        /// </summary>
        /// <param name="leadingTriviaText">先行トリビア・テキスト</param>
        /// <returns></returns>
        static string ParseDocumentComment(string leadingTriviaText)
        {
            // ドキュメント・コメントには複数のルート要素が並ぶことがあるので、`<xml>` で囲んでやる
            var inputXml = $"<xml>{leadingTriviaText}</xml>";

            string summaryText;

            //
            // XMLパーサーが欲しい
            //
            // 📖 [How do I read and parse an XML file in C#?](https://stackoverflow.com/questions/642293/how-do-i-read-and-parse-an-xml-file-in-c)
            //
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(inputXml);

                XmlNode summaryNode = doc.DocumentElement.SelectSingleNode("/xml/summary");

                if (summaryNode != null)
                {
                    summaryText = summaryNode.InnerText;
                    //                    summaryText:
                    //?? 章Idの前に

                    // - 改行は必ず `\r\n` （CRLF） とすること
                    // - Excel に出力したいから、ワンライナーへ変換します
                    summaryText = summaryText.Replace("\r\n", "\\r\\n");
                }
                else
                {
                    summaryText = string.Empty;
                }
            }
            catch (XmlException ex)
            {
                // - 改行は必ず `\r\n` （CRLF） とすること
                // - Excel に出力したいから、ワンライナーへ変換します
                var source = inputXml.Replace("\r\n", "\\r\\n");

                // エラーではなく、パーサーの出来が悪い。
                // `エラーメッセージは気にしないでください。正確ではありません`
                summaryText = $"[[Don't worry about the error message. Not exactly]] {ex.Message} [[SOURCE]] {source}";
            }

            return summaryText;
        }
    }
}
