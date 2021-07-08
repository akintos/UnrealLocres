using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LocresLib.IO
{
    public static class BinaryWriterExtensions
    {
        internal static bool IsAsciiString(string value)
        {
            for (int i = 0; i < value.Length; i++)
                if (value[i] > 127) return false;
            return true;
        }
        
        internal static void WriteUnrealString(this BinaryWriter writer, string value, bool forceUnicode = false)
        {
            value += "\x00";

            if (!forceUnicode && IsAsciiString(value)) // ASCII
            {
                var data = Encoding.ASCII.GetBytes(value);
                writer.Write(data.Length);
                writer.Write(data);
            }
            else // UTF-16-LE
            {
                var data = Encoding.Unicode.GetBytes(value);
                writer.Write(value.Length * -1);
                writer.Write(data);
            }
        }
    }
}
