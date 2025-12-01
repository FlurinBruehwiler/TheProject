using System;
using System.Text;
using LightningDB;

class Program
{
    static void Main()
    {
        var overlay = new SortedDictionary<byte[], string>(ByteArrayComparer.Instance);
    }


}

class ByteArrayComparer : IComparer<byte[]>
{
    public static ByteArrayComparer Instance { get; } = new();

    public int Compare(byte[]? a, byte[]? b)
    {
        if (ReferenceEquals(a, b)) return 0;
        if (a == null) return -1;
        if (b == null) return 1;

        int len = Math.Min(a.Length, b.Length);
        for (int i = 0; i < len; i++)
        {
            int diff = a[i].CompareTo(b[i]);
            if (diff != 0) return diff;
        }
        return a.Length.CompareTo(b.Length);
    }
}
