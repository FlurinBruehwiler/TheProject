using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace TheProject;

public ref struct BinaryReader
{
    public int CurrentOffset;
    public ReadOnlySpan<byte> Data;
    public bool HasError;

    public Guid ReadGuid()
    {
        if (!HasEnoughBytesRemaining(16))
        {
            CurrentOffset += 16;
            HasError = true;
            return Guid.Empty;
        }

        var guid = MemoryMarshal.Read<Guid>(Data.Slice(CurrentOffset));
        CurrentOffset += 16;
        return guid;
    }

    public byte ReadByte()
    {
        if (!HasEnoughBytesRemaining(1))
        {
            CurrentOffset++;


            HasError = true;
            return 0;
        }
        CurrentOffset++;

        return Data[CurrentOffset];
    }

    public int ReadInt32()
    {
        if (BinaryPrimitives.TryReadInt32LittleEndian(Data.Slice(CurrentOffset), out var i))
        {
            CurrentOffset += 4;

            return i;
        }

        CurrentOffset += 4;
        HasError = true;
        return 0;
    }

    public ReadOnlySpan<char> ReadUtf16String()
    {
        var lengthOfStringInBytes = ReadInt32();

        if (!HasEnoughBytesRemaining(lengthOfStringInBytes))
        {
            HasError = true;
            CurrentOffset += lengthOfStringInBytes;
            return ReadOnlySpan<char>.Empty;
        }

        var originalOffset = CurrentOffset;
        CurrentOffset += lengthOfStringInBytes;

        return MemoryMarshal.Cast<byte, char>(Data.Slice(originalOffset, lengthOfStringInBytes));
    }

    private bool HasEnoughBytesRemaining(int byteCount)
    {
        return Data.Length - CurrentOffset >= byteCount;
    }
}