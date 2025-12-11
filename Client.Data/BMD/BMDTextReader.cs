using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Client.Data.BMD;

public static class BMDTextReader
{
    public static IList<T> Read<T>(string filePath, ushort key = 0, bool itemCounter = true, bool hasCrc = true) where T : struct
    {

        if (filePath == null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"{typeof(T)} file not found: {filePath}", filePath);
        }

        using var br = new BinaryReader(File.OpenRead(filePath));
        int count = -1;
        if (itemCounter)
        {
            count = br.ReadInt32();
        }

        IList<T> list = [];
        
        

        var bytesPerItem = Marshal.SizeOf<T>();
        while (br.BaseStream.Position < br.BaseStream.Length - (4 * (hasCrc ? 1 : 0)))
        {
            var itemBytes = br.ReadBytes(bytesPerItem);
            itemBytes = Decrypt(itemBytes, key);
            T @struct = ToStruct<T>(itemBytes);
            list.Add(@struct);
        }
        br.Close();
        br.Dispose();

        return list;
    }

    static byte[] XOR_3_KEY = { 0xFC, 0xCF, 0xAB };

    //extra is 0xDC normally
    public static byte[] Decrypt(byte[] data, ushort extra = 0)
    {
        byte[] buf = (byte[])data.Clone();
        for (int i = 0; i < buf.Length; i++)
        {
            buf[i] ^= XOR_3_KEY[i % 3];
            buf[i] ^= (byte)(extra & 0xFF);
        }
        return buf;
    }

    public static T ToStruct<T>(byte[] buffer) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

        try
        {
            IntPtr ptr = handle.AddrOfPinnedObject();
            return Marshal.PtrToStructure<T>(ptr);
        }
        finally
        {
            handle.Free();
        }
    }
}
