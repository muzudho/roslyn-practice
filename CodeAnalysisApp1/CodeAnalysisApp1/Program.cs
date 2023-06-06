using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysisApp1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // 読込対象のテキスト
            const string programText =
@"using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}";

            // テキストをパースして、ツリー作成
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);

            // ツリーから根を取得
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

            // 根のデータ
            Console.WriteLine($"The tree is a {root.Kind()} node.");
            Console.WriteLine($"The tree has {root.Members.Count} elements in it.");
            Console.WriteLine($"The tree has {root.Usings.Count} using statements. They are:");

            // 根にぶら下がっている Using 句を出力
            foreach (UsingDirectiveSyntax element in root.Usings)
            {
                Console.WriteLine($"\t{element.Name}");
            }

            // 根にぶら下がっている最初のものの種類
            MemberDeclarationSyntax firstMember = root.Members[0];
            Console.WriteLine($"The first member is a {firstMember.Kind()}.");
            // The first member is a NamespaceDeclaration.

            var helloWorldDeclaration = (NamespaceDeclarationSyntax)firstMember;

            //
            var programDeclaration = (ClassDeclarationSyntax)helloWorldDeclaration.Members[0];
            Console.WriteLine($"There are {programDeclaration.Members.Count} members declared in the {programDeclaration.Identifier} class.");
            // There are 1 members declared in the Program class.

            Console.WriteLine($"The first member is a {programDeclaration.Members[0].Kind()}.");
            // The first member is a MethodDeclaration.

            //
            // プログラム中の宣言メンバーの１つ目
            //
            var mainDeclaration = (MethodDeclarationSyntax)programDeclaration.Members[0];

            //
            // 返却値の型
            //
            Console.WriteLine($"The return type of the {mainDeclaration.Identifier} method is {mainDeclaration.ReturnType}.");
            // The return type of the Main method is void.

            //
            // 引数の個数
            //
            Console.WriteLine($"The method has {mainDeclaration.ParameterList.Parameters.Count} parameters.");
            // The method has 1 parameters.

            //
            // 各引数
            //
            foreach (ParameterSyntax item in mainDeclaration.ParameterList.Parameters)
            {
                //
                // 引数１つ分
                //
                Console.WriteLine($"The type of the {item.Identifier} parameter is {item.Type}.");
                // The type of the args parameter is string[].
            }

            //
            // 関数定義文
            //
            Console.WriteLine($"The body text of the {mainDeclaration.Identifier} method follows:");
            Console.WriteLine(mainDeclaration.Body?.ToFullString());
            // {
            //     Console.WriteLine("Hello, World!");
            // }

            var argsParameter = mainDeclaration.ParameterList.Parameters[0];
        }
    }
}
