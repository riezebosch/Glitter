using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;

namespace Glitter.Test
{
    [TestClass]
    public class ObjectFileParserTest
    {


        [TestMethod]
        public void DecodeTest()
        {
            var file = new System.IO.FileInfo(Path.Combine(Path.GetTempPath(), "objects", "08", Path.GetRandomFileName()));
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            
            File.WriteAllBytes(file.FullName, TestResources.multifiletree);

            try
            {
                var result = ObjectFileParser.ParseFile(file);
                StringAssert.Contains(result.Body, "b39e4ee91acfd62f10b9b931daed0b0344a438b1");
                StringAssert.Contains(result.Body, "12c4cf90fa0e83fa111446ef4fedf2fc37e4ebc1");
                StringAssert.Contains(result.Body, "test.txt");
                StringAssert.Contains(result.Body, "nog een file.txt");
            }
            finally
            {
                file.Delete();
            }
        }

        [TestMethod]
        public void TestReadHeader()
        {
            var file = new System.IO.FileInfo(Path.Combine(Path.GetTempPath(), "objects", Path.GetRandomFileName()));
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            File.WriteAllBytes(file.FullName, TestResources.singlefiletree);

            try
            {
                using (var stream = new Ionic.Zlib.ZlibStream(file.OpenRead(), Ionic.Zlib.CompressionMode.Decompress))
                {
                    ObjectHeader result = ObjectFileParser.ReadHeader(stream);
                    Assert.AreEqual(ObjectType.Tree, result.Type);
                    Assert.AreEqual(36, result.Size);
                }
            }
            finally
            {
                file.Delete();
            }
        }

        [TestMethod]
        public void SingleFileInTreeTest()
        {
            var file = new System.IO.FileInfo(Path.Combine(Path.GetTempPath(), "objects", "08", Path.GetRandomFileName()));
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            
            File.WriteAllBytes(file.FullName, TestResources.singlefiletree);

            try
            {
                var result = ObjectFileParser.ParseFile(file);
                StringAssert.Contains(result.Body, "d0f89fe97552ddd9cefcab879175436503bc9251");
            }
            finally
            {
                file.Delete();
            }
        }

        [TestMethod]
        public void TestExtractReferenceFromHeadToReference()
        {
            var result = ObjectFileParser.ExtractReferenceFromHead("ref: refs/heads/master");
            Assert.AreEqual("master", result);
        }

        [TestMethod]
        public void TestExtractReferenceFromHeadToCommit()
        {
            var result = ObjectFileParser.ExtractReferenceFromHead("987456");
            Assert.AreEqual("987456", result);
        }
    }
}
