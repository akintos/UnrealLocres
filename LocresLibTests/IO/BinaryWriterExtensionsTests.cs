using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using LocresLib.IO;

namespace UnrealLocres.IO.Tests
{
    [TestClass()]
    public class BinaryWriterExtensionsTests
    {
        [TestMethod()]
        public void WriteUnrealStringTest()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.WriteUnrealString("유니코드");
                var expected = new byte[] { 0xFB, 0xFF, 0xFF, 0xFF, 0x20, 0xC7, 0xC8, 0xB2, 0x54, 0xCF, 0xDC, 0xB4, 0x00, 0x00 };
                var actual = new byte[ms.Position];

                var buf = ms.GetBuffer();
                Buffer.BlockCopy(buf, 0, actual, 0, actual.Length);

                CollectionAssert.AreEqual(expected, actual);
            }
        }
    }
}