namespace CodeAnalysisApp1.Example2
{
    using System.Collections.Generic;
    using System.Text;

    internal class RecordEx
    {
        internal RecordEx(Record recordObj, string fileLocation)
        {
            RecordObj = recordObj;
            FileLocation = fileLocation;
        }

        // - プロパティ

        Record RecordObj { get; }

        /// <summary>
        /// ファイルのある場所
        /// </summary>
        string FileLocation { get; }

        // - メソッド

        internal string ToCSV()
        {
            var builder = new StringBuilder();

            var row = new List<string>()
            {
                FileLocation,
            };

            row = Record.EscapeCSV(row);

            builder.Append($"{string.Join(",", row)},{this.RecordObj.ToCSV()}");

            return builder.ToString();
        }

    }
}
