using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LocresLib.IO
{
    internal static class BinaryReaderExtensions
    {
        internal static string ReadUnrealString(this BinaryReader reader)
        {
            int length = reader.ReadInt32();
            string result;
            if (length > 0)
            {
                result = Encoding.ASCII.GetString(reader.ReadBytes(length));
            }
            else if (length < 0)
            {
                result = Encoding.Unicode.GetString(reader.ReadBytes(length * -2));
            }
            else // length == 0
            {
                return string.Empty;
            }
            return result.TrimEnd('\0');
        }
    }
}
