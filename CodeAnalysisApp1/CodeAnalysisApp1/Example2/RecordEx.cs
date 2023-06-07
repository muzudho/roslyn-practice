namespace CodeAnalysisApp1.Example2
{
    using System.Collections.Generic;
    using System.Text;

    internal class RecordEx
    {
        internal RecordEx(Record recordObj, string filePathToRead)
        {
            RecordObj = recordObj;
            FilePathToRead = filePathToRead;
        }

        // - プロパティ

        Record RecordObj { get; }

        string FilePathToRead { get; }

        // - メソッド

        internal string ToCSV()
        {
            var builder = new StringBuilder();

            var list = new List<string>()
            {
                FilePathToRead,
            };

            builder.Append($"{Record.EscapeCSV(list)},{this.RecordObj.ToCSV()}");

            return builder.ToString();
        }

    }
}
