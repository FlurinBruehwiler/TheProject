using System;
using System.Text;
using LightningDB;
using Shared.Database;
using Shared.Utils;

class Program
{
    static unsafe void Main()
    {
        var before = Guid.CreateVersion7(DateTimeOffset.UtcNow);
        var after = Guid.CreateVersion7(DateTimeOffset.UtcNow.AddMilliseconds(1));

        var res = BPlusTree.CompareLexicographic(before.AsSpan(), after.AsSpan());
        Console.WriteLine(res);
    }
}