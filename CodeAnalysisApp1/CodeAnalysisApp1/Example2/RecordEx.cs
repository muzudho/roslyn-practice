namespace CodeAnalysisApp1.Example2
{
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
    }
}
