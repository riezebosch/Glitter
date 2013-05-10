using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Glitter.WPF;
using System.IO;

namespace Glitter.WPF.Test
{
    [TestClass]
    public class UnitTest1
    {


        [TestMethod]
        public void DecodeTest()
        {
            var result = ObjectFileParser.ParseFile(new System.IO.FileInfo(@"C:\git\demo\.git\objects\3a\a6f0f2f1e892f92ae8275b6e4911d06266c0e7"));
            StringAssert.Contains(result, "83f60fde27663b047beddcdc84417ac391a5466a");
        }

        [TestMethod]
        public void TestReadHeader()
        {
            using (var stream = new Ionic.Zlib.ZlibStream(File.OpenRead(@"C:\git\demo\.git\objects\3a\a6f0f2f1e892f92ae8275b6e4911d06266c0e7"), Ionic.Zlib.CompressionMode.Decompress))
            {
                ObjectHeader result = ObjectFileParser.ReadHeader(stream);
                Assert.AreEqual(ObjectType.Tree, result.Type);
                Assert.AreEqual(112, result.Size);
            }
        }

        [TestMethod]
        public void SingleFileInTreeTest()
        {
            var result = ObjectFileParser.ParseFile(new System.IO.FileInfo(@"C:\git\demo\.git\objects\89\667aefa8dda468c5892954420831ba50ec366d"));
            StringAssert.Contains(result, "d0f89fe97552ddd9cefcab879175436503bc9251");
        }
    }
}
