using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnrealLocres;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

using LocresLib;

namespace LocresLib.Tests
{
    [TestClass()]
    public class LocalizationResourceFileTests
    {
        public static void LoadAndSaveTest(string filename, LocresVersion version)
        {
            var locres = TestHelper.LoadTestFile(filename);
            var totalCount = locres.TotalCount;

            byte[] bytes;

            using (var ms = new MemoryStream())
            {
                locres.Save(ms, version);

                bytes = ms.ToArray();
            }

            using (var ms2 = new MemoryStream(bytes))
            {
                var locres2 = new LocresFile();
                locres2.Load(ms2);

                Assert.AreEqual(totalCount, locres2.TotalCount);
                Assert.AreEqual(locres.Version, locres2.Version);
            }
        }

        [TestMethod()]
        public void LoadLegacyTest()
        {
            var locres = TestHelper.LoadTestFile("legacy.locres");
            Console.WriteLine(locres.TotalCount);
        }

        [TestMethod()]
        public void LoadCompactTest()
        {
            var locres = TestHelper.LoadTestFile("compact.locres");
            Console.WriteLine(locres.TotalCount);
        }

        [TestMethod()]
        public void LoadOptimizedTest()
        {
            var locres = TestHelper.LoadTestFile("optimized.locres");
            Console.WriteLine(locres.TotalCount);
        }

        [TestMethod()]
        public void LoadOptimizedCityhashTest()
        {
            var locres = TestHelper.LoadTestFile("optimized_cityhash.locres");
            Console.WriteLine(locres.TotalCount);
            Assert.AreEqual(LocresVersion.Optimized_CityHash64_UTF16, locres.Version);
        }

        [TestMethod()]
        public void SaveLegacyTest()
        {
            LoadAndSaveTest("legacy.locres", LocresVersion.Legacy);
        }

        [TestMethod()]
        public void SaveCompactTest()
        {
            LoadAndSaveTest("compact.locres", LocresVersion.Compact);
        }

        [TestMethod()]
        public void SaveOptimizedTest()
        {
            LoadAndSaveTest("optimized.locres", LocresVersion.Optimized);
        }

        [TestMethod()]
        public void SaveOptimizedCityHashTest()
        {
            LoadAndSaveTest("optimized_cityhash.locres", LocresVersion.Optimized_CityHash64_UTF16);
        }
    }
}