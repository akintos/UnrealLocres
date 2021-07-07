using System;
using System.Collections.Generic;
using System.Text;

namespace UnrealLocres
{
    public class LocresString
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public uint SourceStringHash { get; }

        public LocresString(string key, string value, uint sourceStringHash)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            SourceStringHash = sourceStringHash;
        }

        public static implicit operator string(LocresString l)
        {
            return l.Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
