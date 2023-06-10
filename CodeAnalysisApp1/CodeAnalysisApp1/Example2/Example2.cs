namespace CodeAnalysisApp1.Example2
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

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

            // トップ・レベルだから、ネームスペースは無い
            var codeLocation = string.Empty;

            void setRecord(Record record)
            {
                recordExList.Add(new RecordEx(
                    recordObj: record,
                    fileLocation: filePathToRead));
            }

            foreach (var memberDeclaration in root.Members)
            {
                switch (memberDeclaration.Kind())
                {
                    // ネームスペース宣言
                    case SyntaxKind.NamespaceDeclaration:
                        {
                            var namespaceDeclarationMember = (NamespaceDeclarationSyntax)memberDeclaration;

                            ParseNamespaceDeclaration(
                                setRecord: setRecord,
                                namespaceDeclaration: namespaceDeclarationMember);

                        }
                        break;

                    // クラス宣言
                    case SyntaxKind.ClassDeclaration:
                        {
                            var classDeclarationMember = (ClassDeclarationSyntax)memberDeclaration;

                            ParseClassDeclaration(
                                setRecord: setRecord,
                                codeLocation: codeLocation,
                                classDeclaration: classDeclarationMember);
                        }
                        break;

                    // TODO インターフェース宣言部

                    // 構造体宣言
                    case SyntaxKind.StructDeclaration:
                        {
                            var structDeclarationMember = (StructDeclarationSyntax)memberDeclaration;

                            var record = ParseStruct(
                                structDeclaration: structDeclarationMember,
                                codeLocation: codeLocation);

                            setRecord(record);
                        }
                        break;

                    // 列挙型宣言
                    case SyntaxKind.EnumDeclaration:
                        {
                            var enumsDeclarationMember = (EnumDeclarationSyntax)memberDeclaration;

                            ParseEnumDeclaration(
                                setRecord: setRecord,
                                codeLocation: codeLocation,
                                enumDeclaration: enumsDeclarationMember);
                        }
                        break;

                    // コンストラクター宣言
                    case SyntaxKind.ConstructorDeclaration:
                        {
                            var constructorDeclarationMember = (ConstructorDeclarationSyntax)memberDeclaration;

                            var record = ParseConstructor(
                                constructorDeclaration: constructorDeclarationMember,
                                codeLocation: codeLocation);

                            setRecord(record);
                        }
                        break;

                    // デストラクター宣言
                    case SyntaxKind.DestructorDeclaration:
                        {
                            var destructorDeclarationMember = (DestructorDeclarationSyntax)memberDeclaration;

                            var record = ParseDestructor(
                                destructorDeclaration: destructorDeclarationMember,
                                codeLocation: codeLocation);

                            setRecord(record);
                        }
                        break;

                    // フィールドの宣言部なら
                    case SyntaxKind.FieldDeclaration:
                        {
                            var fieldDeclarationMember = (FieldDeclarationSyntax)memberDeclaration;

                            ParseField(
                                fieldDeclaration: fieldDeclarationMember,
                                codeLocation: codeLocation,
                                setRecord: setRecord);
                        }
                        break;

                    // デリゲートの宣言部なら
                    case SyntaxKind.DelegateDeclaration:
                        {
                            var delegateDeclarationMember = (DelegateDeclarationSyntax)memberDeclaration;

                            ParseDelegate(
                                delegateDeclaration: delegateDeclarationMember,
                                codeLocation: codeLocation,
                                setRecord: setRecord);
                        }
                        break;

                    // イベント・フィールド宣言
                    case SyntaxKind.EventFieldDeclaration:
                        {
                            var eventFieldDeclarationMember = (EventFieldDeclarationSyntax)memberDeclaration;

                            ParseEventField(
                                eventFieldDeclaration: eventFieldDeclarationMember,
                                codeLocation: codeLocation,
                                setRecord: setRecord);
                        }
                        break;

                    // メソッドの宣言部なら
                    case SyntaxKind.MethodDeclaration:
                        {
                            var methodDeclarationMember = (MethodDeclarationSyntax)memberDeclaration;

                            var record = ParseMethod(
                                methodDeclaration: methodDeclarationMember,
                                codeLocation: codeLocation);

                            setRecord(record);
                        }
                        break;

                    // 不完全なメンバーなら
                    case SyntaxKind.IncompleteMember:
                        {
                            var incompleteMember = (IncompleteMemberSyntax)memberDeclaration;

                            var record = ParseIncompleteMember(
                                incompleteMember: incompleteMember,
                                codeLocation: codeLocation);

                            setRecord(record);
                        }
                        break;

                    default:
                        {
                            var message = $"[[What? 238]] rootMember.Kind(): {memberDeclaration.Kind().ToString()}";

                            setRecord(new Record(
                                    kind: "[[What?]]",
                                    codeLocation: codeLocation,
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

            setRecordExList(recordExList);
        }

        /// <summary>
        /// 名前空間解析
        /// </summary>
        /// <param name="setRecord"></param>
        /// <param name="namespaceDeclaration"></param>
        static void ParseNamespaceDeclaration(LazyCoding.SetValue<Record> setRecord, NamespaceDeclarationSyntax namespaceDeclaration)
        {
            var codeLocation = namespaceDeclaration.Name.ToString();

            // クラスが２個定義されてるとか、列挙型が定義されてるとかに対応
            foreach (var memberDeclaration in namespaceDeclaration.Members)
            {
                switch (memberDeclaration.Kind())
                {
                    // ネームスペース宣言
                    case SyntaxKind.NamespaceDeclaration:
                        {
                            var namespaceDeclarationMember = (NamespaceDeclarationSyntax)memberDeclaration;

                            ParseNamespaceDeclaration(
                                setRecord: setRecord,
                                namespaceDeclaration: namespaceDeclarationMember);

                        }
                        break;

                    // クラス宣言
                    case SyntaxKind.ClassDeclaration:
                        {
                            var classDeclarationMember = (ClassDeclarationSyntax)memberDeclaration;

                            ParseClassDeclaration(
                                setRecord: setRecord,
                                codeLocation: codeLocation,
                                classDeclaration: classDeclarationMember);
                        }
                        break;

                    // インターフェース宣言
                    case SyntaxKind.InterfaceDeclaration:
                        {
                            var interfaceDeclarationMember = (InterfaceDeclarationSyntax)memberDeclaration;

                            // TODO インターフェース宣言部

                            foreach (var programDeclarationMember in interfaceDeclarationMember.Members)
                            {
                                switch (programDeclarationMember.Kind())
                                {
                                    // フィールドの宣言部なら
                                    case SyntaxKind.FieldDeclaration:
                                        {
                                            var fieldDeclaration = (FieldDeclarationSyntax)programDeclarationMember;

                                            ParseField(
                                                fieldDeclaration: fieldDeclaration,
                                                codeLocation: codeLocation,
                                                setRecord: setRecord);
                                        }
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                        break;

                    // 構造体宣言
                    case SyntaxKind.StructDeclaration:
                        {
                            var structDeclarationMember = (StructDeclarationSyntax)memberDeclaration;

                            var record = ParseStruct(
                                structDeclaration: structDeclarationMember,
                                codeLocation: codeLocation);

                            setRecord(record);
                        }
                        break;

                    // 列挙型宣言
                    case SyntaxKind.EnumDeclaration:
                        {
                            var enumDeclarationMember = (EnumDeclarationSyntax)memberDeclaration;

                            ParseEnumDeclaration(
                                setRecord: setRecord,
                                codeLocation: codeLocation,
                                enumDeclaration: enumDeclarationMember);
                        }
                        break;

                    // コンストラクター宣言
                    case SyntaxKind.ConstructorDeclaration:
                        {
                            var constructorDeclarationMember = (ConstructorDeclarationSyntax)memberDeclaration;

                            var record = ParseConstructor(
                                constructorDeclaration: constructorDeclarationMember,
                                // ネームスペース.親クラス名　とつなげる
                                codeLocation: codeLocation);

                            setRecord(record);
                        }
                        break;

                    // デストラクター宣言
                    case SyntaxKind.DestructorDeclaration:
                        {
                            var destructorDeclarationMember = (DestructorDeclarationSyntax)memberDeclaration;

                            var record = ParseDestructor(
                                destructorDeclaration: destructorDeclarationMember,
                                // ネームスペース.親クラス名　とつなげる
                                codeLocation: codeLocation);

                            setRecord(record);
                        }
                        break;

                    // メソッドの宣言部なら
                    case SyntaxKind.MethodDeclaration:
                        {
                            var methodDeclarationMember = (MethodDeclarationSyntax)memberDeclaration;

                            var record = ParseMethod(
                                methodDeclaration: methodDeclarationMember,
                                // ネームスペース.親クラス名　とつなげる
                                codeLocation: codeLocation);

                            setRecord(record);
                        }
                        break;

                    // フィールドの宣言部なら
                    case SyntaxKind.FieldDeclaration:
                        {
                            var fieldDeclarationMember = (FieldDeclarationSyntax)memberDeclaration;

                            ParseField(
                                fieldDeclaration: fieldDeclarationMember,
                                // ネームスペース.親クラス名　とつなげる
                                codeLocation: codeLocation,
                                setRecord: setRecord);
                        }
                        break;


                    // 不完全なメンバーなら
                    case SyntaxKind.IncompleteMember:
                        {
                            var incompleteMember = (IncompleteMemberSyntax)memberDeclaration;

                            var record = ParseIncompleteMember(
                                incompleteMember: incompleteMember,
                                // ネームスペース.親クラス名　とつなげる
                                codeLocation: codeLocation);

                            setRecord(record);
                        }
                        break;

                    default:
                        {
                            var message = $"[[What? 392]] memberDeclaration.Kind(): {memberDeclaration.Kind().ToString()}";

                            setRecord(new Record(
                                    kind: "[[What?]]",
                                    codeLocation: codeLocation,
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
        /// class 宣言を解析
        /// </summary>
        static void ParseClassDeclaration(LazyCoding.SetValue<Record> setRecord, string codeLocation, ClassDeclarationSyntax classDeclaration)
        {
            // ネームスペース.クラス名　とつなげる
            codeLocation = $"{codeLocation}.{classDeclaration.Identifier.ToString()}";

            // サブ・クラスが２個定義されてるとか、サブ・列挙型が定義されてるとかに対応
            foreach (var memberDeclaration in classDeclaration.Members)
            {
                switch (memberDeclaration.Kind())
                {
                    // サブ・クラス
                    case SyntaxKind.ClassDeclaration:
                        {
                            var subClassDeclaration = (ClassDeclarationSyntax)memberDeclaration;

                            ParseClassDeclaration(
                                setRecord: setRecord,
                                codeLocation: codeLocation,
                                classDeclaration: subClassDeclaration);
                        }
                        break;

                    // サブ構造体
                    case SyntaxKind.StructDeclaration:
                        {
                            var structDeclaration = (StructDeclarationSyntax)memberDeclaration;

                            var record = ParseStruct(
                                structDeclaration: structDeclaration,
                                codeLocation: codeLocation);
                            setRecord(record);
                        }
                        break;

                    // サブ列挙型
                    case SyntaxKind.EnumDeclaration:
                        {
                            var enumDeclaration = (EnumDeclarationSyntax)memberDeclaration;

                            ParseEnumDeclaration(
                                setRecord: setRecord,
                                codeLocation: codeLocation,
                                enumDeclaration: enumDeclaration);
                        }
                        break;

                    // フィールドの宣言部なら
                    case SyntaxKind.FieldDeclaration:
                        {
                            var fieldDeclaration = (FieldDeclarationSyntax)memberDeclaration;

                            ParseField(
                                fieldDeclaration: fieldDeclaration,
                                codeLocation: codeLocation,
                                setRecord: setRecord);
                        }
                        break;

                    // プロパティの宣言部なら
                    case SyntaxKind.PropertyDeclaration:
                        {
                            var propertyDeclaration = (PropertyDeclarationSyntax)memberDeclaration;

                            var record = ParseProperty(
                                propertyDeclaration: propertyDeclaration,
                                codeLocation: codeLocation);
                            setRecord(record);
                        }
                        break;

                    // デリゲート宣言部なら
                    case SyntaxKind.DelegateDeclaration:
                        {
                            var delegateDeclaration = (DelegateDeclarationSyntax)memberDeclaration;

                            ParseDelegate(
                                delegateDeclaration: delegateDeclaration,
                                codeLocation: codeLocation,
                                setRecord: setRecord);
                        }
                        break;

                    // イベント・フィールド宣言
                    case SyntaxKind.EventFieldDeclaration:
                        {
                            var eventFieldDeclaration = (EventFieldDeclarationSyntax)memberDeclaration;

                            ParseEventField(
                                eventFieldDeclaration: eventFieldDeclaration,
                                codeLocation: codeLocation,
                                setRecord: setRecord);
                        }
                        break;

                    // コンストラクター宣言
                    case SyntaxKind.ConstructorDeclaration:
                        {
                            var constructorDeclaration = (ConstructorDeclarationSyntax)memberDeclaration;

                            var record = ParseConstructor(
                                constructorDeclaration: constructorDeclaration,
                                codeLocation: codeLocation);
                            setRecord(record);
                        }
                        break;

                    // デストラクター宣言
                    case SyntaxKind.DestructorDeclaration:
                        {
                            var destructorDeclaration = (DestructorDeclarationSyntax)memberDeclaration;

                            var record = ParseDestructor(
                                destructorDeclaration: destructorDeclaration,
                                codeLocation: codeLocation);
                            setRecord(record);
                        }
                        break;

                    // メソッドの宣言部なら
                    case SyntaxKind.MethodDeclaration:
                        {
                            var methodDeclaration = (MethodDeclarationSyntax)memberDeclaration;

                            var record = ParseMethod(
                                methodDeclaration: methodDeclaration,
                                codeLocation: codeLocation);
                            setRecord(record);
                        }
                        break;

                    // 不完全なメンバーなら
                    case SyntaxKind.IncompleteMember:
                        {
                            var incompleteMember = (IncompleteMemberSyntax)memberDeclaration;

                            var record = ParseIncompleteMember(
                                incompleteMember: incompleteMember,
                                // ネームスペース.親クラス名　とつなげる
                                codeLocation: codeLocation);

                            setRecord(record);
                        }
                        break;

                    default:
                        {
                            var message = $"[[What? 445]] programDeclarationMember.Kind(): {memberDeclaration.Kind().ToString()}";

                            setRecord(new Record(
                                kind: "[[What?]]",
                                codeLocation: codeLocation,
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
        /// 構造体解析
        /// </summary>
        /// <param name="structDeclaration">構造体宣言</param>
        /// <param name="codeLocation">コードのある場所</param>
        /// <returns>解析結果</returns>
        static Record ParseStruct(StructDeclarationSyntax structDeclaration, string codeLocation)
        {
            // var builder = new StringBuilder();

            //
            // 引数の個数か？
            //
            // builder.Append($" ■Arity:                   {structDeclaration.Arity}");
            // ■Arity:                   1

            //
            // 属性
            //
            // builder.Append($" ■AttributeLists:          {structDeclaration.AttributeLists}");
            // ■AttributeLists:          [Serializable]

            //
            // なんだろう？
            //
            // builder.Append($" ■BaseList:                {structDeclaration.BaseList}");
            // ■BaseList:                

            //
            // 閉じ波括弧
            //
            // builder.Append($" ■CloseBraceToken:         {structDeclaration.CloseBraceToken}");
            // ■CloseBraceToken:         }

            //
            // なんだろう？
            //
            // builder.Append($" ■ConstraintClauses:       {structDeclaration.ConstraintClauses}");
            // ■ConstraintClauses:       

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsAnnotations:     {structDeclaration.ContainsAnnotations}");
            // ■ContainsAnnotations:     False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDiagnostics:     {structDeclaration.ContainsDiagnostics}");
            // ■ContainsDiagnostics:     False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDirectives:      {structDeclaration.ContainsDirectives}");
            // ■ContainsDirectives:      False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsSkippedText:     {structDeclaration.ContainsSkippedText}");
            // ■ContainsSkippedText:     False

            //
            // 開始文字位置、終了文字位置か？
            //
            // builder.Append($" ■FullSpan:                {structDeclaration.FullSpan}");
            // ■FullSpan:                [2392..3120)

            //
            // なんだろう？
            //
            // builder.Append($" ■HasLeadingTrivia:        {structDeclaration.HasLeadingTrivia}");
            // ■HasLeadingTrivia:        True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasStructuredTrivia:     {structDeclaration.HasStructuredTrivia}");
            // ■HasStructuredTrivia:     True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasTrailingTrivia:       {structDeclaration.HasTrailingTrivia}");
            // ■HasTrailingTrivia:       True

            //
            // 構造体名
            //
            // builder.Append($" ■Identifier:              {structDeclaration.Identifier}");
            // ■Identifier:              DummyNode

            //
            // なんだろう？
            //
            // builder.Append($" ■IsMissing:               {structDeclaration.IsMissing}");
            // ■IsMissing:               False

            //
            // なんだろう？
            //
            // builder.Append($" ■IsStructuredTrivia:      {structDeclaration.IsStructuredTrivia}");
            // ■IsStructuredTrivia:      False

            //
            // なんだろう？
            //
            // builder.Append($" ■Keyword:                 {structDeclaration.Keyword}");
            // ■Keyword:                 struct

            //
            // プログラミング言語の種類
            //
            // builder.Append($" ■Language:                {structDeclaration.Language}");
            // ■Language:                C#

            //
            // フィールドの宣言文か？
            //
            // builder.Append($" ■Members:                 {structDeclaration.Members}");
            // ■Members:                 public const string RootName = nameof(array);

            //
            // 修飾子
            //
            // builder.Append($" ■Modifiers:               {structDeclaration.Modifiers}");
            // ■Modifiers:               private

            //
            // 開き波括弧
            //
            // builder.Append($" ■OpenBraceToken:          {structDeclaration.OpenBraceToken}");
            // ■OpenBraceToken:          {

            //
            // 出力長そう
            //
            // builder.Append($" ■Parent:                  {structDeclaration.Parent}");

            //
            // なんだろう？
            //
            // builder.Append($" ■ParentTrivia:            {structDeclaration.ParentTrivia}");
            // ■ParentTrivia:            

            //
            // なんだろう？
            //
            // builder.Append($" ■RawKind:                 {structDeclaration.RawKind}");
            // ■RawKind:                 8856

            //
            // なんだろう？
            //
            // builder.Append($" ■SemicolonToken:          {structDeclaration.SemicolonToken}");
            // ■SemicolonToken:          

            //
            // 開始文字位置、終了文字位置か？
            //
            // builder.Append($" ■Span:                    {structDeclaration.Span}");
            // ■Span:                    [2526..3118)

            //
            // 開始文字位置か？
            //
            // builder.Append($" ■SpanStart:               {structDeclaration.SpanStart}");
            // ■SpanStart:               2526

            //
            // 出力長そう
            //
            // builder.Append($" ■SyntaxTree:              {structDeclaration.SyntaxTree}");

            //
            // 型引数のリスト
            //
            // builder.Append($" ■TypeParameterList:       {structDeclaration.TypeParameterList}");
            // ■TypeParameterList:       <T>

            //
            // ドキュメント・コメント
            // ======================
            //
            var leadingTrivia = structDeclaration.GetLeadingTrivia();
            var documentCommentText = ChangeLeadingTriviaToDocumentCommentXMLText(leadingTrivia);
            string summaryText = ParseDocumentComment(documentCommentText);

            return new Record(
                kind: "Struct",
                codeLocation: codeLocation,
                access: structDeclaration.Modifiers.ToString(),         // 修飾子
                memberType: string.Empty,                               // 戻り値の型
                name: structDeclaration.Identifier.ToString(),          // 構造体名
                value: string.Empty,                                    // 値は空  
                summary: summaryText);                                  // ドキュメント・コメントの summary

            // テスト用 summary: builder.ToString()
        }

        /// <summary>
        /// 列挙型の定義を解析
        /// </summary>
        static void ParseEnumDeclaration(LazyCoding.SetValue<Record> setRecord, string codeLocation, EnumDeclarationSyntax enumDeclaration)
        {
            foreach (var programDeclarationMember in enumDeclaration.Members)
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

                            var record = ParseEnum(fieldDeclaration, codeLocation);
                            setRecord(record);
                        }
                        break;

                    default:
                        {
                            var message = $"[[What? 265]] programDeclarationMember.Kind(): {programDeclarationMember.Kind().ToString()}";

                            setRecord(new Record(
                                kind: "[[What?]]",
                                codeLocation: string.Empty,
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
        static void ParseField(
            FieldDeclarationSyntax fieldDeclaration,
            string codeLocation,
            LazyCoding.SetValue<Record> setRecord)
        {
            // var builder = new StringBuilder();

            //
            // なんだろう？
            //
            // builder.Append($" ■AttributeLists:                {fieldDeclaration.AttributeLists}");
            // ■AttributeLists:                

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsAnnotations:           {fieldDeclaration.ContainsAnnotations}");
            // ■ContainsAnnotations:           False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDiagnostics:           {fieldDeclaration.ContainsDiagnostics}");
            // ■ContainsDiagnostics:           False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDirectives:            {fieldDeclaration.ContainsDirectives}");
            // ■ContainsDirectives:            False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsSkippedText:           {fieldDeclaration.ContainsSkippedText}");
            // ■ContainsSkippedText:           False

            //
            // 宣言文丸ごとか？
            //
            // builder.Append($" ■Declaration:                   {fieldDeclaration.Declaration}");
            // ■Declaration:                   Dictionary<string, AssetEntity> _entities = new Dictionary<string, AssetEntity>()
            // ■Declaration:                   string Key
            {
                //
                // なんだろう？
                //
                // builder.Append($" ■Declaration.ContainsAnnotations:                   {fieldDeclaration.Declaration.ContainsAnnotations}");
                // ■Declaration.ContainsAnnotations:                   False

                //
                // なんだろう？
                //
                // builder.Append($" ■Declaration.ContainsDiagnostics:                   {fieldDeclaration.Declaration.ContainsDiagnostics}");
                // ■Declaration.ContainsDiagnostics:                   False

                //
                // なんだろう？
                //
                // builder.Append($" ■Declaration.ContainsDirectives:                    {fieldDeclaration.Declaration.ContainsDirectives}");
                // ■Declaration.ContainsDirectives:                    False

                //
                // なんだろう？
                //
                // builder.Append($" ■Declaration.ContainsSkippedText:                   {fieldDeclaration.Declaration.ContainsSkippedText}");
                // ■Declaration.ContainsSkippedText:                   False

                //
                // 開始文字位置、終了文字位置か？
                //
                // builder.Append($" ■Declaration.FullSpan:                              {fieldDeclaration.Declaration.FullSpan}");
                // ■Declaration.FullSpan:                              [1063..1144)
                // ■Declaration.FullSpan:                              [846..9399)

                //
                // なんだろう？
                //
                // builder.Append($" ■Declaration.HasLeadingTrivia:                      {fieldDeclaration.Declaration.HasLeadingTrivia}");
                // ■Declaration.HasLeadingTrivia:                      False

                //
                // なんだろう？
                //
                // builder.Append($" ■Declaration.HasStructuredTrivia:                   {fieldDeclaration.Declaration.HasStructuredTrivia}");
                // ■Declaration.HasStructuredTrivia:                   False

                //
                // なんだろう？
                //
                // builder.Append($" ■Declaration.HasTrailingTrivia:                     {fieldDeclaration.Declaration.HasTrailingTrivia}");
                // ■Declaration.HasTrailingTrivia:                     False

                //
                // なんだろう？
                //
                // builder.Append($" ■Declaration.IsMissing:                             {fieldDeclaration.Declaration.IsMissing}");
                // ■Declaration.IsMissing:                             False

                //
                // なんだろう？
                //
                // builder.Append($" ■Declaration.IsStructuredTrivia:                    {fieldDeclaration.Declaration.IsStructuredTrivia}");
                // ■Declaration.IsStructuredTrivia:                    False

                //
                // プログラミング言語の種類
                //
                // builder.Append($" ■Declaration.Language:                              {fieldDeclaration.Declaration.Language}");
                // ■Declaration.Language:                              C#

                //
                // ソースが長そう
                //
                // builder.Append($" ■Declaration.Parent:                                {fieldDeclaration.Declaration.Parent}");

                //
                // なんだろう？
                //
                // builder.Append($" ■Declaration.ParentTrivia:                          {fieldDeclaration.Declaration.ParentTrivia}");
                // ■Declaration.ParentTrivia:                          

                //
                // なんだろう？
                //
                // builder.Append($" ■Declaration.RawKind:                               {fieldDeclaration.Declaration.RawKind}");
                // ■Declaration.RawKind:                               8794

                //
                // 開始文字位置、終了文字位置か？
                //
                // builder.Append($" ■Declaration.Span:                                  {fieldDeclaration.Declaration.Span}");
                // ■Declaration.Span:                                  [1063..1144)
                // ■Declaration.Span:                                  [846..9399)

                //
                // 開始文字位置か
                //
                // builder.Append($" ■Declaration.SpanStart:                             {fieldDeclaration.Declaration.SpanStart}");
                // ■Declaration.SpanStart:                             1063

                //
                // ソースが長そう
                //
                // builder.Append($" ■Declaration.SyntaxTree:                            {fieldDeclaration.Declaration.SyntaxTree}");

                //
                // 型
                //
                // builder.Append($" ■Declaration.Type:                                  {fieldDeclaration.Declaration.Type}");
                // ■Declaration.Type:                                  Dictionary<string, AssetEntity>
                // ■Declaration.Type:                                  List<EventEnum>

                //
                // 値
                //
                // builder.Append($" ■Declaration.Variables:                             {fieldDeclaration.Declaration.Variables}");
                // ■Declaration.Variables:                             _entities = new Dictionary<string, AssetEntity>()
                // ■Declaration.Variables:                             Map = new List<EventEnum> { 
                // 空白
                // 0,
                // 以下略。長いソース
                {
                    //
                    // 変数は複数あるのでは
                    //

                    //
                    // 変数の個数
                    //
                    // builder.Append($" ■Declaration.Variables.Count:             {fieldDeclaration.Declaration.Variables.Count}");
                    // ■Declaration.Variables.Count:             1

                    //
                    // 開始文字位置、終了文字位置か？
                    //
                    // builder.Append($" ■Declaration.Variables.FullSpan:          {fieldDeclaration.Declaration.Variables.FullSpan}");
                    // ■Declaration.Variables.FullSpan:          [1095..1144)

                    //
                    // なんだろう？
                    //
                    // builder.Append($" ■Declaration.Variables.SeparatorCount:    {fieldDeclaration.Declaration.Variables.SeparatorCount}");
                    // ■Declaration.Variables.SeparatorCount:    0

                    //
                    // 開始文字位置、終了文字位置か？
                    //
                    // builder.Append($" ■Declaration.Variables.Span:              {fieldDeclaration.Declaration.Variables.Span}");
                    // ■Declaration.Variables.Span:              [1095..1144)

                    // インデクサ
                    // for (int i = 0; i < fieldDeclaration.Declaration.Variables.Count; i++)
                    {
                        // var variable = fieldDeclaration.Declaration.Variables[i];

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].ArgumentList:         {fieldDeclaration.Declaration.Variables[i].ArgumentList}");
                        // ■Declaration.Variables[0].ArgumentList:         

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].ContainsAnnotations:  {fieldDeclaration.Declaration.Variables[i].ContainsAnnotations}");
                        // ■Declaration.Variables[0].ContainsAnnotations:  False

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].ContainsDiagnostics:  {fieldDeclaration.Declaration.Variables[i].ContainsDiagnostics}");
                        // ■Declaration.Variables[0].ContainsDiagnostics:  False

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].ContainsDirectives:   {fieldDeclaration.Declaration.Variables[i].ContainsDirectives}");
                        // ■Declaration.Variables[0].ContainsDirectives:   False

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].ContainsSkippedText:  {fieldDeclaration.Declaration.Variables[i].ContainsSkippedText}");
                        // ■Declaration.Variables[0].ContainsSkippedText:  False

                        //
                        // 開始文字位置、終了文字位置か？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].FullSpan:             {fieldDeclaration.Declaration.Variables[i].FullSpan}");
                        // ■Declaration.Variables[0].FullSpan:             [1095..1144)

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].HasLeadingTrivia:     {fieldDeclaration.Declaration.Variables[i].HasLeadingTrivia}");
                        // ■Declaration.Variables[0].HasLeadingTrivia:     False

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].HasStructuredTrivia:  {fieldDeclaration.Declaration.Variables[i].HasStructuredTrivia}");
                        // ■Declaration.Variables[0].HasStructuredTrivia:  False

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].HasTrailingTrivia:    {fieldDeclaration.Declaration.Variables[i].HasTrailingTrivia}");
                        // ■Declaration.Variables[0].HasTrailingTrivia:    False

                        //
                        // 変数名
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].Identifier:           {fieldDeclaration.Declaration.Variables[i].Identifier}");
                        // ■Declaration.Variables[0].Identifier:           _entities

                        //
                        // 初期化子
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].Initializer:          {fieldDeclaration.Declaration.Variables[i].Initializer}");
                        // ■Declaration.Variables[0].Initializer:          = new Dictionary<string, AssetEntity>()

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].IsMissing:            {fieldDeclaration.Declaration.Variables[i].IsMissing}");
                        // ■Declaration.Variables[0].IsMissing:            False

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].IsStructuredTrivia:   {fieldDeclaration.Declaration.Variables[i].IsStructuredTrivia}");
                        // ■Declaration.Variables[0].IsStructuredTrivia:   False

                        //
                        // プログラミング言語の種類
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].Language:             {fieldDeclaration.Declaration.Variables[i].Language}");
                        // ■Declaration.Variables[0].Language:             C#

                        //
                        // ソースが長そう
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].Parent:               {fieldDeclaration.Declaration.Variables[i].Parent}");

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].ParentTrivia:         {fieldDeclaration.Declaration.Variables[i].ParentTrivia}");
                        // ■Declaration.Variables[0].ParentTrivia:         

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].RawKind:              {fieldDeclaration.Declaration.Variables[i].RawKind}");
                        // ■Declaration.Variables[0].RawKind:              8795

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].Span:                 {fieldDeclaration.Declaration.Variables[i].Span}");
                        // ■Declaration.Variables[0].Span:                 [1095..1144)

                        //
                        // なんだろう？
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].SpanStart:            {fieldDeclaration.Declaration.Variables[i].SpanStart}");
                        // ■Declaration.Variables[0].SpanStart:            1095

                        //
                        // ソースが長そう
                        //
                        // builder.Append($" ■Declaration.Variables[{i}].SyntaxTree:           {fieldDeclaration.Declaration.Variables[i].SyntaxTree}");
                    }
                }
            }

            //
            // 開始文字位置、終了文字位置か？
            //
            // builder.Append($" ■FullSpan:                      {fieldDeclaration.FullSpan}");
            // ■FullSpan:                      [945..1147)
            // ■FullSpan:                      [13930..14092)

            //
            // なんだろう？
            //
            // builder.Append($" ■HasLeadingTrivia:              {fieldDeclaration.HasLeadingTrivia}");
            // ■HasLeadingTrivia:              True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasStructuredTrivia:           {fieldDeclaration.HasStructuredTrivia}");
            // ■HasStructuredTrivia:           True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasTrailingTrivia:             {fieldDeclaration.HasTrailingTrivia}");
            // ■HasTrailingTrivia:             True

            //
            // なんだろう？
            //
            // builder.Append($" ■IsMissing:                     {fieldDeclaration.IsMissing}");
            // ■IsMissing:                     False

            //
            // なんだろう？
            //
            // builder.Append($" ■IsStructuredTrivia:            {fieldDeclaration.IsStructuredTrivia}");
            // ■IsStructuredTrivia:            False

            //
            // なんだろう？
            //
            // builder.Append($" ■Modifiers:                     {fieldDeclaration.Modifiers}");
            // ■Modifiers:                     private static readonly
            // ■Modifiers:                     public

            //
            // ソースが長そう
            //
            // builder.Append($" ■Parent:                        {fieldDeclaration.Parent}");

            //
            // なんだろう？
            //
            // builder.Append($" ■ParentTrivia:                  {fieldDeclaration.ParentTrivia}");
            // ■ParentTrivia:                  

            //
            // なんだろう？
            //
            // builder.Append($" ■RawKind:                       {fieldDeclaration.RawKind}");
            // ■RawKind:                       8873

            //
            // なんだろう？
            //
            // builder.Append($" ■SemicolonToken:                {fieldDeclaration.SemicolonToken}");
            // ■SemicolonToken:                ;

            //
            // なんだろう？
            //
            // builder.Append($" ■Span:                          {fieldDeclaration.Span}");
            // ■Span:                          [1039..1145)
            // ■Span:                          [14055..14090)

            //
            // なんだろう？
            //
            // builder.Append($" ■SpanStart:                     {fieldDeclaration.SpanStart}");
            // ■SpanStart:                     1039
            // ■SpanStart:                     14055

            //
            // ソースが長そう
            //
            // builder.Append($" ■SyntaxTree:                    {fieldDeclaration.SyntaxTree}");

            //
            // デクラレーション
            // ================
            //
            // string declarationHeadText;
            //string name;
            //string value;
            //if (fieldDeclaration.Declaration != null)
            //{
            //    // Declaration:         int beforeChapterId
            //    //
            //    // List<string> AttackMotionImageLabel = new List<string> { "無し", "ダガー", "剣", "フレイル", "斧", "ウィップ", "杖", "弓", "クロスボウ", "銃", "爪", "グローブ", "槍", "メイス", "ロッド", "こん棒", "チェーン", "未来の剣", "パイプ", "ショットガン", "ライフル", "チェーンソー", "レールガン", "スタンロッド", "ユーザ定義1", "ユーザ定義2", "ユーザ定義3", "ユーザ定義4", "ユーザ定義5", "ユーザ定義6" }

            //    // 連続する空白を１つにしてみる
            //    // 📖 [Replace consecutive whitespace characters with a single space in C#](https://www.techiedelight.com/replace-consecutive-whitespace-by-single-space-csharp/)
            //    // var declarationText = Regex.Replace(fieldDeclaration.Declaration.ToString(), @"\s+", " ");
            //    var declarationText = Regex.Replace(fieldDeclaration.Declaration.ToString(), @"\s+", " ");

            //    // "=" を含むか？
            //    if (declarationText.Contains("="))
            //    {
            //        // "=" より前だけ取るか
            //        var tokenList = declarationText.Split('=').ToList();

            //        declarationText = tokenList[0].TrimEnd();
            //        tokenList.RemoveAt(0);
            //        value = string.Join("=", tokenList);
            //    }
            //    else
            //    {
            //        value = string.Empty;
            //    }

            //    // とりあえず半角スペースで区切ってみるか
            //    string[] list = declarationText.ToString().Split(' ');

            //    // var declarationHead = new string[list.Length - 1];
            //    // Array.Copy(list, 0, declarationHead, 0, list.Length - 1);
            //    // declarationHeadText = string.Join(" ", declarationHead);
            //    name = list[list.Length - 1];
            //}
            //else
            //{
            //    // declarationHeadText = string.Empty;
            //    name = string.Empty;
            //    value = string.Empty;
            //}

            //
            // 前トリビア
            // ==========
            //
            var leadingTrivia = fieldDeclaration.GetLeadingTrivia();
            //leadingTrivia:         /// <summary>
            //                       /// ?? 章Idの前に
            //                       /// </summary>

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

            //
            // 変数は一度に複数個定義できる
            //
            for (int i = 0; i < fieldDeclaration.Declaration.Variables.Count; i++)
            {
                var variable = fieldDeclaration.Declaration.Variables[i];

                string initializerText;
                if (variable.Initializer != null)
                {
                    initializerText = variable.Initializer.ToString();
                }
                else
                {
                    initializerText = string.Empty;
                }

                setRecord(new Record(
                    kind: "Field",
                    codeLocation: codeLocation,
                    access: fieldDeclaration.Modifiers.ToString(),
                    memberType: fieldDeclaration.Declaration.Type.ToString(),
                    name: variable.Identifier.ToString(),
                    value: initializerText,
                    summary: summaryText));
                // summary: builder.ToString())); // テスト用
            }
        }

        /// <summary>
        /// デリゲート宣言の解析
        /// </summary>
        /// <param name="delegateDeclaration">デリゲート宣言</param>
        /// <param name="codeLocation">コードのある場所</param>
        /// <param name="setRecord">結果</param>
        static void ParseDelegate(
            DelegateDeclarationSyntax delegateDeclaration,
            string codeLocation,
            LazyCoding.SetValue<Record> setRecord)
        {
            // var builder = new StringBuilder();

            //
            // 引数の数か？
            //
            // builder.Append($" ■Arity:                       {delegateDeclaration.Arity}");
            // ■Arity:                       0

            //
            // アノテーションか？
            //
            // builder.Append($" ■AttributeLists:              {delegateDeclaration.AttributeLists}");
            // ■AttributeLists:              

            //
            // なんだろう？
            //
            // builder.Append($" ■ConstraintClauses:           {delegateDeclaration.ConstraintClauses}");
            // ■ConstraintClauses:           

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsAnnotations:         {delegateDeclaration.ContainsAnnotations}");
            // ■ContainsAnnotations:         False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDiagnostics:         {delegateDeclaration.ContainsDiagnostics}");
            // ■ContainsDiagnostics:         False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDirectives:          {delegateDeclaration.ContainsDirectives}");
            // ■ContainsDirectives:          False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsSkippedText:         {delegateDeclaration.ContainsSkippedText}");
            // ■ContainsSkippedText:         False

            //
            // デリゲート予約語
            //
            // builder.Append($" ■DelegateKeyword:             {delegateDeclaration.DelegateKeyword}");
            // ■DelegateKeyword:             delegate

            //
            // 開始文字位置、終了文字位置か？
            //
            // builder.Append($" ■FullSpan:                    {delegateDeclaration.FullSpan}");
            // ■FullSpan:                    [10617..10837)

            //
            // なんだろう？
            //
            // builder.Append($" ■HasLeadingTrivia:            {delegateDeclaration.HasLeadingTrivia}");
            // ■HasLeadingTrivia:            True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasStructuredTrivia:         {delegateDeclaration.HasStructuredTrivia}");
            // ■HasStructuredTrivia:         True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasTrailingTrivia:           {delegateDeclaration.HasTrailingTrivia}");
            // ■HasTrailingTrivia:           True

            //
            // デリゲート名
            //
            // builder.Append($" ■Identifier:                  {delegateDeclaration.Identifier}");
            // ■Identifier:                  ParameterTypeConvert

            //
            // なんだろう？
            //
            // builder.Append($" ■IsMissing:                   {delegateDeclaration.IsMissing}");
            // ■IsMissing:                   False

            //
            // なんだろう？
            //
            // builder.Append($" ■IsStructuredTrivia:          {delegateDeclaration.IsStructuredTrivia}");
            // ■IsStructuredTrivia:          False

            //
            // プログラミング言語の種類
            //
            // builder.Append($" ■Language:                    {delegateDeclaration.Language}");
            // ■Language:                    C#

            //
            // 修飾子
            //
            // builder.Append($" ■Modifiers:                   {delegateDeclaration.Modifiers}");
            // ■Modifiers:                   

            //
            // 引数のリスト
            //
            // builder.Append($" ■ParameterList:               {delegateDeclaration.ParameterList}");
            // ■ParameterList:               (string value)

            //
            // 出力長そう
            //
            // builder.Append($" ■Parent:                      {delegateDeclaration.Parent}");
            // 

            //
            // なんだろう？
            //
            // builder.Append($" ■ParentTrivia:                {delegateDeclaration.ParentTrivia}");
            // ■ParentTrivia:                

            //
            // なんだろう？
            //
            // builder.Append($" ■RawKind:                     {delegateDeclaration.RawKind}");
            // ■RawKind:                     8859

            //
            // 戻り値の型
            //
            // builder.Append($" ■ReturnType:                  {delegateDeclaration.ReturnType}");
            // ■ReturnType:                  object

            //
            // セミコロン
            //
            // builder.Append($" ■SemicolonToken:              {delegateDeclaration.SemicolonToken}");
            // ■SemicolonToken:              ;

            //
            // 開始文字位置、終了文字位置か？
            //
            // builder.Append($" ■Span:                        {delegateDeclaration.Span}");
            // ■Span:                        [10784..10835)

            //
            // 開始文字位置か
            //
            // builder.Append($" ■SpanStart:                   {delegateDeclaration.SpanStart}");
            // ■SpanStart:                   10784

            //
            // 出力長そう
            //
            // builder.Append($" ■SyntaxTree:                  {delegateDeclaration.SyntaxTree}");
            //

            //
            // 型パラメーターのリスト
            //
            // builder.Append($" ■TypeParameterList:           {delegateDeclaration.TypeParameterList}");
            // ■TypeParameterList:           

            //
            // ドキュメント・コメント
            // ======================
            //
            var documentCommentText = ChangeLeadingTriviaToDocumentCommentXMLText(delegateDeclaration.GetLeadingTrivia());
            string summaryText = ParseDocumentComment(documentCommentText);

            setRecord(new Record(
                kind: "Delegate",                                           // 種類
                codeLocation: codeLocation,                                 // コードのある場所
                access: delegateDeclaration.Modifiers.ToString(),           // 修飾子
                memberType: delegateDeclaration.ReturnType.ToString(),      // 戻り値の型
                name: delegateDeclaration.Identifier.ToString(),            // デリゲート名
                value: string.Empty,                                        // 値は無し
                summary: summaryText));
            // summary: builder.ToString())); // テスト用
        }

        /// <summary>
        /// イベント・フィールド宣言の解析
        /// </summary>
        /// <param name="eventFieldDeclaration">イベント・フィールド宣言</param>
        /// <param name="codeLocation">コードのある場所</param>
        /// <param name="setRecord">結果</param>
        static void ParseEventField(
            EventFieldDeclarationSyntax eventFieldDeclaration,
            string codeLocation,
            LazyCoding.SetValue<Record> setRecord)
        {
            var builder = new StringBuilder();

            //
            // アノテーション
            //
            // builder.Append($" ■AttributeLists:          {eventFieldDeclaration.AttributeLists}");
            // ■AttributeLists:          

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsAnnotations:     {eventFieldDeclaration.ContainsAnnotations}");
            // ■ContainsAnnotations:     False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDiagnostics:     {eventFieldDeclaration.ContainsDiagnostics}");
            // ■ContainsDiagnostics:     False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDirectives:      {eventFieldDeclaration.ContainsDirectives}");
            // ■ContainsDirectives:      False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsSkippedText:     {eventFieldDeclaration.ContainsSkippedText}");
            // ■ContainsSkippedText:     False

            //
            // 宣言文
            //
            // builder.Append($" ■Declaration:             {eventFieldDeclaration.Declaration}");
            // ■Declaration:             Action<string> ScenePlayEndEvent
            // ■Declaration:             Action<DebugToolButton> OnSelected
            // ■Declaration:             Action<int> onDragIndexChanged
            {
                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.ContainsAnnotations:     {eventFieldDeclaration.Declaration.ContainsAnnotations}");
                // ■Declaration.ContainsAnnotations:     False

                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.ContainsDiagnostics:     {eventFieldDeclaration.Declaration.ContainsDiagnostics}");
                // ■Declaration.ContainsDiagnostics:     False

                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.ContainsDirectives:      {eventFieldDeclaration.Declaration.ContainsDirectives}");
                // ■Declaration.ContainsDirectives:      False

                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.ContainsSkippedText:     {eventFieldDeclaration.Declaration.ContainsSkippedText}");
                // ■Declaration.ContainsSkippedText:     False

                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.FullSpan:                {eventFieldDeclaration.Declaration.FullSpan}");
                // ■Declaration.FullSpan:                [732..764)

                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.HasLeadingTrivia:        {eventFieldDeclaration.Declaration.HasLeadingTrivia}");
                // ■Declaration.HasLeadingTrivia:        False

                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.HasStructuredTrivia:     {eventFieldDeclaration.Declaration.HasStructuredTrivia}");
                // ■Declaration.HasStructuredTrivia:     False

                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.HasTrailingTrivia:       {eventFieldDeclaration.Declaration.HasTrailingTrivia}");
                // ■Declaration.HasTrailingTrivia:       False

                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.IsMissing:               {eventFieldDeclaration.Declaration.IsMissing}");
                // ■Declaration.IsMissing:               False

                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.IsStructuredTrivia:      {eventFieldDeclaration.Declaration.IsStructuredTrivia}");
                // ■Declaration.IsStructuredTrivia:      False

                //
                // プログラミング言語の種類
                //
                builder.Append($" ■Declaration.Language:                {eventFieldDeclaration.Declaration.Language}");
                // ■Declaration.Language:                C#

                //
                // 自身を含む全体の文字列
                //
                builder.Append($" ■Declaration.Parent:                  {eventFieldDeclaration.Declaration.Parent}");
                // ■Declaration.Parent:                  public static event Action<string> ScenePlayEndEvent;

                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.ParentTrivia:            {eventFieldDeclaration.Declaration.ParentTrivia}");
                // ■Declaration.ParentTrivia:            

                //
                // なんだろう？
                //
                builder.Append($" ■Declaration.RawKind:                 {eventFieldDeclaration.Declaration.RawKind}");
                // ■Declaration.RawKind:                 8794

                //
                // 開始文字位置、終了文字位置か？
                //
                builder.Append($" ■Declaration.Span:                    {eventFieldDeclaration.Declaration.Span}");
                // ■Declaration.Span:                    [732..764)

                //
                // 開始文字位置か？
                //
                builder.Append($" ■Declaration.SpanStart:               {eventFieldDeclaration.Declaration.SpanStart}");
                // ■Declaration.SpanStart:               732

                //
                // 出力長そう
                //
                // builder.Append($" ■Declaration.SyntaxTree:              {eventFieldDeclaration.Declaration.SyntaxTree}");
                //

                //
                // 型
                //
                builder.Append($" ■Declaration.Type:                    {eventFieldDeclaration.Declaration.Type}");
                // ■Declaration.Type:                    Action<string>

                //
                // 変数のリスト
                //
                builder.Append($" ■Declaration.Variables:               {eventFieldDeclaration.Declaration.Variables}");
                // ■Declaration.Variables:               ScenePlayEndEvent
            }

            //
            // event 予約語
            //
            // builder.Append($" ■EventKeyword:            {eventFieldDeclaration.EventKeyword}");
            // ■EventKeyword:            event

            //
            // 開始文字位置、終了文字位置か？
            //
            // builder.Append($" ■FullSpan:                {eventFieldDeclaration.FullSpan}");
            // ■FullSpan:                [627..767)

            //
            // なんだろう？
            //
            // builder.Append($" ■HasLeadingTrivia:        {eventFieldDeclaration.HasLeadingTrivia}");
            // ■HasLeadingTrivia:        True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasStructuredTrivia:     {eventFieldDeclaration.HasStructuredTrivia}");
            // ■HasStructuredTrivia:     True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasTrailingTrivia:       {eventFieldDeclaration.HasTrailingTrivia}");
            // ■HasTrailingTrivia:       True

            //
            // なんだろう？
            //
            // builder.Append($" ■IsMissing:               {eventFieldDeclaration.IsMissing}");
            // ■IsMissing:               False

            //
            // なんだろう？
            //
            // builder.Append($" ■IsStructuredTrivia:      {eventFieldDeclaration.IsStructuredTrivia}");
            // ■IsStructuredTrivia:      False

            //
            // プログラミング言語の種類
            //
            // builder.Append($" ■Language:                {eventFieldDeclaration.Language}");
            // ■Language:                C#

            //
            // 修飾子
            //
            // builder.Append($" ■Modifiers:               {eventFieldDeclaration.Modifiers}");
            // ■Modifiers:               public static

            //
            // 出力長そう
            //
            // builder.Append($" ■Parent:                  {eventFieldDeclaration.Parent}");
            //

            //
            // なんだろう？
            //
            // builder.Append($" ■ParentTrivia:            {eventFieldDeclaration.ParentTrivia}");
            // ■ParentTrivia:            

            //
            // なんだろう？
            //
            // builder.Append($" ■RawKind:                 {eventFieldDeclaration.RawKind}");
            // ■RawKind:                 8874

            //
            // セミコロン
            //
            // builder.Append($" ■SemicolonToken:          {eventFieldDeclaration.SemicolonToken}");
            // ■SemicolonToken:          ;

            //
            // 開始文字位置、終了文字位置か？
            //
            // builder.Append($" ■Span:                    {eventFieldDeclaration.Span}");
            // ■Span:                    [712..765)

            //
            // 開始文字位置か？
            //
            // builder.Append($" ■SpanStart:               {eventFieldDeclaration.SpanStart}");
            // ■SpanStart:               712

            //
            // 出力長そう
            //
            // builder.Append($" ■SyntaxTree:              {eventFieldDeclaration.SyntaxTree}");
            //


            //
            // ドキュメント・コメント
            // ======================
            //
            var documentCommentText = ChangeLeadingTriviaToDocumentCommentXMLText(eventFieldDeclaration.GetLeadingTrivia());
            string summaryText = ParseDocumentComment(documentCommentText);

            //
            // 変数は複数個ある
            //
            foreach (var variable in eventFieldDeclaration.Declaration.Variables)
            {
                setRecord(new Record(
                    kind: "EventField",                                                 // 種類
                    codeLocation: codeLocation,                                         // コードのある場所
                    access: eventFieldDeclaration.Modifiers.ToString(),                 // 修飾子
                    memberType: eventFieldDeclaration.Declaration.Type.ToString(),      // 戻り値の型
                    name: variable.Identifier.ToString(),                               // デリゲート名
                    value: string.Empty,                                                // 値は無し
                    summary: summaryText));
                // summary: builder.ToString())); // テスト用
            }
        }


        /// <summary>
        /// プロパティ解析
        /// </summary>
        /// <param name="propertyDeclaration">プロパティ宣言</param>
        /// <returns>解析結果</returns>
        static Record ParseProperty(PropertyDeclarationSyntax propertyDeclaration, string codeLocation)
        {
            // var builder = new StringBuilder();

            //
            // ゲッター、セッターの本文のソースコードが入ってる
            //
            // builder.Append($" ■AccessorList:                {propertyDeclaration.AccessorList}");

            //
            // なんだろう？
            //
            // builder.Append($" ■AttributeLists:              {propertyDeclaration.AttributeLists}");
            // ■AttributeLists:              

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsAnnotations:         {propertyDeclaration.ContainsAnnotations}");
            // ■ContainsAnnotations:         False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDiagnostics:         {propertyDeclaration.ContainsDiagnostics}");
            // ■ContainsDiagnostics:         False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDirectives:          {propertyDeclaration.ContainsDirectives}");
            // ■ContainsDirectives:          True

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsSkippedText:         {propertyDeclaration.ContainsSkippedText}");
            // ■ContainsSkippedText:         False

            //
            // なんだろう？
            //
            // builder.Append($" ■ExplicitInterfaceSpecifier:  {propertyDeclaration.ExplicitInterfaceSpecifier}");
            // ■ExplicitInterfaceSpecifier:   

            //
            // 式を使ったプロパティのときの本文。長いソースになることもある
            //
            // builder.Append($" ■ExpressionBody:              {propertyDeclaration.ExpressionBody}");
            // ■ExpressionBody:              
            // ■ExpressionBody:              => MapPrefabManagerForRuntime.layers

            //
            // 開始文字位置、終了文字位置か？
            //
            // builder.Append($" ■FullSpan:                    {propertyDeclaration.FullSpan}");
            // ■FullSpan:                    [37167..37723)
            // ■FullSpan:                    [881..1172)

            //
            // 先行トリビアが付いているか？
            //
            // builder.Append($" ■HasLeadingTrivia:            {propertyDeclaration.HasLeadingTrivia}");
            // ■HasLeadingTrivia:            True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasStructuredTrivia:         {propertyDeclaration.HasStructuredTrivia}");
            // ■HasStructuredTrivia:         True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasTrailingTrivia:           {propertyDeclaration.HasTrailingTrivia}");
            // ■HasTrailingTrivia:           True

            //
            // プロパティ名
            //
            // builder.Append($" ■Identifier:                  {propertyDeclaration.Identifier}");
            // ■Identifier:                  Instance
            // ■Identifier:                  LayersForRuntime

            //
            // 初期化子
            //
            // builder.Append($" ■Initializer:                 {propertyDeclaration.Initializer}");
            // ■Initializer:                 = false

            //
            // なんだろう？
            //
            // builder.Append($" ■IsMissing:                   {propertyDeclaration.IsMissing}");
            // ■IsMissing:                   False

            //
            // なんだろう？
            //
            // builder.Append($" ■IsStructuredTrivia:          {propertyDeclaration.IsStructuredTrivia}");
            // ■IsStructuredTrivia:          False
            // ■Initializer:                 = 0
            // ■Initializer:                 = new List<string>()

            //
            // プログラミング言語
            //
            // builder.Append($" ■Language:                    {propertyDeclaration.Language}");
            // ■Language:                    C#

            //
            // 修飾子
            //
            // builder.Append($" ■Modifiers:                   {propertyDeclaration.Modifiers}");
            // ■Modifiers:                   private static
            // ■Modifiers:                   public override

            //
            // 長いソース
            //
            // builder.Append($" ■Parent:                      {propertyDeclaration.Parent}");

            //
            // なんだろう？
            //
            // builder.Append($" ■ParentTrivia:                {propertyDeclaration.ParentTrivia}");
            // ■ParentTrivia:                

            //
            // なんだろう？
            //
            // builder.Append($" ■RawKind:                     {propertyDeclaration.RawKind}");
            // ■RawKind:                     8892

            //
            // なんだろう？
            //
            // builder.Append($" ■SemicolonToken:              {propertyDeclaration.SemicolonToken}");
            // ■SemicolonToken:              

            //
            // 開始文字位置、終了文字位置だろうか？
            //
            // builder.Append($" ■Span:                        {propertyDeclaration.Span}");
            // ■Span:                        [37308..37721)
            // ■Span:                        [957..1170)

            //
            // 開始文字位置だろうか？
            //
            // builder.Append($" ■SpanStart:                   {propertyDeclaration.SpanStart}");
            // ■SpanStart:                   37308
            // ■SpanStart:                   957

            //
            // 長いソース
            //
            // builder.Append($" ■SyntaxTree:                  {propertyDeclaration.SyntaxTree}");

            //
            // 型
            //
            // builder.Append($" ■Type:                        {propertyDeclaration.Type}");
            // ■Type:                        CoroutineAccessor
            // ■Type:                        ImageUtility
            // ■Type:                        List<Layer>

            //
            // ドキュメント・コメント
            // ======================
            //
            var leadingTrivia = propertyDeclaration.GetLeadingTrivia();
            var documentCommentText = ChangeLeadingTriviaToDocumentCommentXMLText(leadingTrivia);
            string summaryText = ParseDocumentComment(documentCommentText);

            //
            // 値
            // ====
            //
            string value;
            if (propertyDeclaration.Identifier != null)
            {
                // TODO 長くなったり、複数行になるかも。ワンライナーにしたい
                value = propertyDeclaration.Identifier.ToString();
            }
            else
            {
                value = string.Empty;
            }

            return new Record(
                kind: "Property",
                codeLocation: codeLocation,
                access: propertyDeclaration.Modifiers.ToString(),
                memberType: propertyDeclaration.Type.ToString(),
                name: propertyDeclaration.Identifier.ToString(),
                value: value,
                summary: summaryText);

            // テスト用 summary: builder.ToString()
        }

        /// <summary>
        /// コンストラクター解析
        /// </summary>
        /// <param name="constructorDeclaration">コンストラクター宣言</param>
        /// <param name="codeLocation">コードのある場所</param>
        /// <returns>解析結果</returns>
        static Record ParseConstructor(ConstructorDeclarationSyntax constructorDeclaration, string codeLocation)
        {
            // var builder = new StringBuilder();

            //
            // なんだろう？
            //
            // builder.Append($" ■AttributeLists:          {constructorDeclaration.AttributeLists}");
            // ■AttributeLists:          

            //
            // 出力ソース長い。関数定義本文なのでは
            //
            // builder.Append($" ■Body:                    {constructorDeclaration.Body}");

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsAnnotations:     {constructorDeclaration.ContainsAnnotations}");
            // ■ContainsAnnotations:     False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDiagnostics:     {constructorDeclaration.ContainsDiagnostics}");
            // ■ContainsDiagnostics:     False

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsDirectives:      {constructorDeclaration.ContainsDirectives}");
            // ■ContainsDirectives:      True

            //
            // なんだろう？
            //
            // builder.Append($" ■ContainsSkippedText:     {constructorDeclaration.ContainsSkippedText}");
            // ■ContainsSkippedText:     False

            //
            // なんだろう？
            //
            // builder.Append($" ■ExpressionBody:          {constructorDeclaration.ExpressionBody}");
            // ■ExpressionBody:          

            //
            // 開始文字位置、終了文字位置か？
            //
            // builder.Append($" ■FullSpan:                {constructorDeclaration.FullSpan}");
            // ■FullSpan:                [13327..14196)

            //
            // なんだろう？
            //
            // builder.Append($" ■HasLeadingTrivia:        {constructorDeclaration.HasLeadingTrivia}");
            // ■HasLeadingTrivia:        True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasStructuredTrivia:     {constructorDeclaration.HasStructuredTrivia}");
            // ■HasStructuredTrivia:     True

            //
            // なんだろう？
            //
            // builder.Append($" ■HasTrailingTrivia:       {constructorDeclaration.HasTrailingTrivia}");
            // ■HasTrailingTrivia:       True

            //
            // コンストラクター名か？
            //
            // builder.Append($" ■Identifier:              {constructorDeclaration.Identifier}");
            // ■Identifier:              IndentLog

            //
            // 初期化子
            //
            // builder.Append($" ■Initializer:             {constructorDeclaration.Initializer}");
            // ■Initializer:             

            //
            // なんだろう？
            //
            // builder.Append($" ■IsMissing:               {constructorDeclaration.IsMissing}");
            // ■IsMissing:               False

            //
            // なんだろう？
            //
            // builder.Append($" ■IsStructuredTrivia:      {constructorDeclaration.IsStructuredTrivia}");
            // ■IsStructuredTrivia:      False

            //
            // プログラミング言語の種類
            //
            // builder.Append($" ■Language:                {constructorDeclaration.Language}");
            // ■Language:                C#

            //
            // 修飾子
            //
            // builder.Append($" ■Modifiers:               {constructorDeclaration.Modifiers}");
            // ■Modifiers:               public

            //
            // 引数のリスト
            //
            // builder.Append($" ■ParameterList:           {constructorDeclaration.ParameterList}");
            // ■ParameterList:           (string stockMessage = null)

            //
            // 出力ソース長そう
            //
            // builder.Append($" ■Parent:                  {constructorDeclaration.Parent}");

            //
            // なんだろう？
            //
            // builder.Append($" ■ParentTrivia:            {constructorDeclaration.ParentTrivia}");
            // ■ParentTrivia:            

            //
            // なんだろう？
            //
            // builder.Append($" ■RawKind:                 {constructorDeclaration.RawKind}");
            // ■RawKind:                 8878

            //
            // なんだろう？
            //
            // builder.Append($" ■SemicolonToken:          {constructorDeclaration.SemicolonToken}");
            // ■SemicolonToken:          

            //
            // 開始文字位置、終了文字位置か？
            //
            // builder.Append($" ■Span:                    {constructorDeclaration.Span}");
            // ■Span:                    [13951..14195)

            //
            // 開始文字位置か？
            //
            // builder.Append($" ■SpanStart:               {constructorDeclaration.SpanStart}");
            // ■SpanStart:               13951

            //
            // 出力ソース長そう
            //
            // builder.Append($" ■SyntaxTree:              {constructorDeclaration.SyntaxTree}");

            // ドキュメント・コメント
            // ======================
            //
            var leadingTrivia = constructorDeclaration.GetLeadingTrivia();
            var documentCommentText = ChangeLeadingTriviaToDocumentCommentXMLText(leadingTrivia);
            string summaryText = ParseDocumentComment(documentCommentText);

            return new Record(
                kind: "Constructor",
                codeLocation: codeLocation,                                   // コードのある場所
                access: constructorDeclaration.Modifiers.ToString(),        // 修飾子
                memberType: string.Empty,                                   // 戻り値の型は無い
                name: constructorDeclaration.Identifier.ToString(),         // 関数名
                value: string.Empty,                                        // 値は空  
                summary: summaryText);                                      // ドキュメント・コメントの summary

            // テスト用 summary: builder.ToString()
        }

        /// <summary>
        /// デストラクター解析
        /// </summary>
        /// <param name="destructorDeclaration">デストラクター宣言</param>
        /// <param name="codeLocation">コードのある場所</param>
        /// <returns>解析結果</returns>
        static Record ParseDestructor(DestructorDeclarationSyntax destructorDeclaration, string codeLocation)
        {
            var builder = new StringBuilder();

            // builder.Append($"Arity:                         {methodDeclaration.Arity}");

            // ドキュメント・コメント
            // ======================
            //
            var leadingTrivia = destructorDeclaration.GetLeadingTrivia();
            var documentCommentText = ChangeLeadingTriviaToDocumentCommentXMLText(leadingTrivia);
            string summaryText = ParseDocumentComment(documentCommentText);

            return new Record(
                kind: "Constructor",
                codeLocation: codeLocation,                                 // コードのある場所
                access: destructorDeclaration.Modifiers.ToString(),         // 修飾子
                memberType: string.Empty,                                   // 戻り値の型は無い
                name: destructorDeclaration.Identifier.ToString(),          // 関数名
                value: string.Empty,                                        // 値は空  
                summary: summaryText);                                      // ドキュメント・コメントの summary

            // テスト用 summary: builder.ToString()
        }

        /// <summary>
        /// 不完全メンバーの解析
        /// </summary>
        /// <param name="incompleteMember">不完全メンバー</param>
        /// <param name="codeLocation">コードのある場所</param>
        /// <returns>解析結果</returns>
        static Record ParseIncompleteMember(IncompleteMemberSyntax incompleteMember, string codeLocation)
        {
            var builder = new StringBuilder();

            //
            // なんだろう？
            //
            builder.Append($" ■AttributeLists:                {incompleteMember.AttributeLists}");
            // ■AttributeLists:                

            //
            // なんだろう？
            //
            builder.Append($" ■ContainsAnnotations:           {incompleteMember.ContainsAnnotations}");
            // ■ContainsAnnotations:           False

            //
            // なんだろう？
            //
            builder.Append($" ■ContainsDiagnostics:           {incompleteMember.ContainsDiagnostics}");
            // ■ContainsDiagnostics:           True

            //
            // なんだろう？
            //
            builder.Append($" ■ContainsDirectives:            {incompleteMember.ContainsDirectives}");
            // ■ContainsDirectives:            False

            //
            // なんだろう？
            //
            builder.Append($" ■ContainsSkippedText:           {incompleteMember.ContainsSkippedText}");
            // ■ContainsSkippedText:           True

            //
            // なんだろう？
            //
            builder.Append($" ■FullSpan:                      {incompleteMember.FullSpan}");
            // ■FullSpan:                      [7416..7443)

            //
            // なんだろう？
            //
            builder.Append($" ■HasLeadingTrivia:              {incompleteMember.HasLeadingTrivia}");
            // ■HasLeadingTrivia:              False

            //
            // なんだろう？
            //
            builder.Append($" ■HasStructuredTrivia:           {incompleteMember.HasStructuredTrivia}");
            // ■HasStructuredTrivia:           True

            //
            // なんだろう？
            //
            builder.Append($" ■HasTrailingTrivia:             {incompleteMember.HasTrailingTrivia}");
            // ■HasTrailingTrivia:             True

            //
            // なんだろう？
            //
            builder.Append($" ■IsMissing:                     {incompleteMember.IsMissing}");
            // ■IsMissing:                     False

            //
            // なんだろう？
            //
            builder.Append($" ■IsStructuredTrivia:            {incompleteMember.IsStructuredTrivia}");
            // ■IsStructuredTrivia:            False

            //
            // プログラミング言語の種類
            //
            builder.Append($" ■Language:                      {incompleteMember.Language}");
            // ■Language:                      C#

            //
            // 修飾子
            //
            builder.Append($" ■Modifiers:                     {incompleteMember.Modifiers}");
            // ■Modifiers:                     

            //
            // 出力長そう
            //
            // builder.Append($" ■Parent:                        {incompleteMember.Parent}");
            //

            //
            // なんだろう？
            //
            builder.Append($" ■ParentTrivia:                  {incompleteMember.ParentTrivia}");
            // ■ParentTrivia:                  

            //
            // なんだろう？
            //
            builder.Append($" ■RawKind:                       {incompleteMember.RawKind}");
            // ■RawKind:                       8916

            //
            // なんだろう？
            //
            builder.Append($" ■Span:                          {incompleteMember.Span}");
            // ■Span:                          [1826..1829)

            //
            // なんだろう？
            //
            builder.Append($" ■SpanStart:                     {incompleteMember.SpanStart}");
            // ■SpanStart:                     1826

            //
            // 出力長そう
            //
            // builder.Append($" ■SyntaxTree:                    {incompleteMember.SyntaxTree}");
            //

            //
            // 型
            //
            builder.Append($" ■Type:                          {incompleteMember.Type}");
            // ■Type:                          int

            //
            // 全文字列
            //
            builder.Append($" ■ToFullString():                {incompleteMember.ToFullString()}");
            // 


            // ドキュメント・コメント
            // ======================
            //
            var leadingTrivia = incompleteMember.GetLeadingTrivia();
            var documentCommentText = ChangeLeadingTriviaToDocumentCommentXMLText(leadingTrivia);
            string summaryText = ParseDocumentComment(documentCommentText);

            return new Record(
                kind: "IncompleteMember",
                codeLocation: codeLocation,                                 // コードのある場所
                access: incompleteMember.Modifiers.ToString(),         // 修飾子
                memberType: incompleteMember.Type.ToString(),                                   // 型
                name: string.Empty,          // 関数名
                value: string.Empty,                                        // 値は空  
                                                                            // summary: summaryText);                                      // ドキュメント・コメントの summary

            summary: builder.ToString());   // テスト用
        }

        /// <summary>
        /// メソッド解析
        /// </summary>
        /// <param name="methodDeclaration"></param>
        /// <returns></returns>
        static Record ParseMethod(MethodDeclarationSyntax methodDeclaration, string codeLocation)
        {
            // var builder = new StringBuilder();

            //
            // 引数の個数か？
            //
            // builder.Append($"Arity:                         {methodDeclaration.Arity}");
            // Arity:                         1
            // Arity:                         0

            //
            // アノテーションか？
            //
            // builder.Append($"AttributeLists:                {methodDeclaration.AttributeLists}");
            // AttributeLists:                
            // AttributeLists:                [Conditional(""____DEBUG__UTIL__NEVER__DEFINED__SYMBOL__NAME____"")]
            // AttributeLists:                [Conditional(""DEBUG_UTIL_TEST_LOG"")]
            // ■AttributeLists:          [Serializable]

            //
            // 本文だろうか？
            //
            // builder.Append($"Body:                          {methodDeclaration.Body}");

            //
            // ジェネリック型への制約か？
            //
            // builder.Append($"ConstraintClauses:             {methodDeclaration.ConstraintClauses}");
            // ConstraintClauses:             
            // ConstraintClauses:             where T : IComparable<T>
            // ConstraintClauses:             where TEnum : struct
            // ConstraintClauses:             where T : class

            //
            // なんだろう？
            //
            // builder.Append($"ContainsAnnotations:           {methodDeclaration.ContainsAnnotations}");
            // ContainsAnnotations:           False

            //
            // なんだろう？
            //
            // builder.Append($"ContainsDiagnostics:           {methodDeclaration.ContainsDiagnostics}");
            // ContainsDiagnostics:           False

            //
            // なんだろう？
            //
            // builder.Append($"ContainsDirectives:            {methodDeclaration.ContainsDirectives}");
            // ContainsDirectives:            True
            // ContainsDirectives:            False

            //
            // なんだろう？
            //
            // builder.Append($"ContainsSkippedText:           {methodDeclaration.ContainsSkippedText}");
            // ContainsSkippedText:           False

            //
            // なんだろう？
            //
            // builder.Append($"ExplicitInterfaceSpecifier:    {methodDeclaration.ExplicitInterfaceSpecifier}");
            // ExplicitInterfaceSpecifier:    

            //
            // なんだろう？
            //
            // builder.Append($"ExpressionBody:                {methodDeclaration.ExpressionBody}");
            // ExpressionBody:                

            //
            // コードの記述の開始文字、終了文字位置か？
            //
            // builder.Append($"FullSpan:                      {methodDeclaration.FullSpan}");
            // FullSpan:                      [36254..36638)
            // FullSpan:                      [487..951)

            //
            // 多分、前方に付いている文字の固まりがあるかどうかだと思う
            //
            // builder.Append($"HasLeadingTrivia:              {methodDeclaration.HasLeadingTrivia}");
            // HasLeadingTrivia:              True

            //
            // なんだろう？
            //
            // builder.Append($"HasStructuredTrivia:           {methodDeclaration.HasStructuredTrivia}");
            // HasStructuredTrivia:           True

            //
            // なんだろう？
            //
            // builder.Append($"HasTrailingTrivia:             {methodDeclaration.HasTrailingTrivia}");
            // HasTrailingTrivia:             True

            //
            // 関数名
            //
            // builder.Append($"Identifier:                    {methodDeclaration.Identifier}");
            // Identifier:                    Start
            // Identifier:                    Stop
            // Identifier:                    OnDisable
            // Identifier:                    GetTextUnicodeHalfwidthCount

            //
            // なんだろう？
            //
            // builder.Append($"IsMissing:                     {methodDeclaration.IsMissing}");
            // IsMissing:                     False

            //
            // なんだろう？
            //
            // builder.Append($"IsStructuredTrivia:            {methodDeclaration.IsStructuredTrivia}");
            // IsStructuredTrivia:            False

            //
            // プログラミング言語の種類
            //
            // builder.Append($"Language:                      {methodDeclaration.Language}");
            // Language:                      C#

            //
            // 修飾子
            //
            // builder.Append($"Modifiers:                     {methodDeclaration.Modifiers}");
            // Modifiers:                     public static
            // Modifiers:                     private

            //
            // 引数のリストの記述
            //
            // builder.Append($"ParameterList:                 {methodDeclaration.ParameterList}");
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
            // builder.Append($"Parent:                        {methodDeclaration.Parent}");

            //
            // なんだろう？
            //
            // builder.Append($"ParentTrivia:                  {methodDeclaration.ParentTrivia}");
            // ParentTrivia:                  

            //
            // なんだろう？
            //
            // builder.Append($"RawKind:                       {methodDeclaration.RawKind}");
            // RawKind:                       8875

            //
            // 戻り値の型の記述
            //
            // builder.Append($"ReturnType:                    {methodDeclaration.ReturnType}");
            // ReturnType:                    Coroutine
            // ReturnType:                    void
            // ReturnType:                    IEnumerable<(T item, int index)>

            //
            // なんだろう？
            //
            // builder.Append($"SemicolonToken:                {methodDeclaration.SemicolonToken}");
            // SemicolonToken:                

            //
            // 文字開始位置、終了位置か？
            //
            // builder.Append($"Span:                          {methodDeclaration.Span}");
            // Span:                          [36415..36636)
            // Span:                          [476..667)

            //
            // 文字開始位置か？
            //
            // builder.Append($"SpanStart:                     {methodDeclaration.SpanStart}");
            // SpanStart:                     36415
            // SpanStart:                     493

            //
            // 長いソースコード？
            //
            // builder.Append($"SyntaxTree:                    {methodDeclaration.SyntaxTree}");

            //
            // 型パラメーターのリスト？
            //
            // builder.Append($"TypeParameterList:             {methodDeclaration.TypeParameterList}");
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
                kind: "Method",
                codeLocation: codeLocation,
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
        static Record ParseEnum(EnumMemberDeclarationSyntax enumMemberDeclaration, string codeLocation)
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
                kind: "EnumMember",
                codeLocation: codeLocation,
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
