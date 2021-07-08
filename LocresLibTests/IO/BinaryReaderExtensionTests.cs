using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using LocresLib.IO;

namespace LocresLib.IO.Tests
{
    [TestClass()]
    public class BinaryReaderExtensionsTests
    {
        [TestMethod()]
        public void ReadUnrealStringTest()
        {
            var testdata = new byte[] { 0xFB, 0xFF, 0xFF, 0xFF, 0x20, 0xC7, 0xC8, 0xB2, 0x54, 0xCF, 0xDC, 0xB4, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x41, 0x73, 0x43, 0x69, 0x49 };

            using (var ms = new MemoryStream(testdata))
            using (var br = new BinaryReader(ms))
            {
                var unicode_actual = br.ReadUnrealString();
                Assert.AreEqual("유니코드", unicode_actual);

                var ascii_actual = br.ReadUnrealString();
                Assert.AreEqual("AsCiI", ascii_actual);
            }
        }
    }
}