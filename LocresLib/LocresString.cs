using System;
using System.Collections.Generic;
using System.Text;

namespace LocresLib
{
    public class LocresString
    {
        public string Key { get; set; }
        public string Value { get; set; }

        /// <summary>
        /// <see cref="Crc.StrCrc32"/> hash of source string.
        /// </summary>
        public uint SourceStringHash { get; } 

        public LocresString(string key, string value, uint sourceStringHash)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            SourceStringHash = sourceStringHash;
        }
    }
}
