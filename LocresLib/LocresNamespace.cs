using System;
using System.Collections.Generic;
using System.Text;

namespace LocresLib
{
    public class LocresNamespace : List<LocresString>
    {
        public LocresNamespace() { }

        public LocresNamespace(int capacity) : base(capacity) { }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name + ":" + base.ToString();
        }
    }
}
