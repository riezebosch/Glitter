using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Glitter
{
    public static class ObjectFileParser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ignoring files that cause exceptions.")]
        public static GitObject ParseFile(FileInfo fi)
        {
            if (fi == null)
            {
                throw new ArgumentNullException("fi", "fi is null.");
            }

            if (fi.Extension == ".lock" ||
                fi.Name.StartsWith("tmp", StringComparison.OrdinalIgnoreCase) ||
                fi.Name.StartsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
                !fi.Exists)
            {
                return null;
            }

            Stream s = null;
            try
            {
                s = TryOpenFile(fi, 50);
                if (fi.Name == "index")
                {
                    return null;
                }

                if (fi.Name == "HEAD")
                {
                    return ReadHead(fi, s);
                }

                if (fi.Directory.Parent.Name == "refs")
                {
                    return ReadRef(fi, s);
                }

                if (fi.Directory.Name == "pack")
                {
                    return ReadPack(fi);
                }

                if (fi.Directory.Name == "info")
                {
                    return ReadInfo(fi, s);
                }

                if (fi.Directory.Parent.Name == "objects")
                {
                    return ReadObject(fi, ref s);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (s != null)
                {
                    s.Dispose();
                }
            }

            return null;
        }

        private static GitObject ReadHead(FileInfo fi, Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                var go = new GitObject
                {
                    Id = fi.Name,
                    ObjectType = ObjectType.Head,
                    Body = sr.ReadToEnd().Trim()
                };
                go.AddReference(ExtractReferenceFromHead(go.Body));

                return go;
            }
        }

        public static string ExtractReferenceFromHead(string content)
        {
            Match match = Regex.Match(content, "(?<=ref: refs/heads/).*");
            return match.Success ? match.Value : content;
        }

        private static GitObject ReadRef(FileInfo fi, Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                var go = new GitObject
                {
                    Id = fi.Name,
                    ObjectType = ObjectType.Branch,
                    Body = sr.ReadToEnd().Trim()
                };
                go.AddReference(go.Body);
                return go;
            }
        }

        private static GitObject ReadPack(FileInfo fi)
        {
            var go = new GitObject
            {
                Id = fi.Name,
                ObjectType = ObjectType.Pack
            };
            if (fi.Extension == ".idx")
            {
                go.AddReference(Path.ChangeExtension(fi.Name, ".pack"));
            }

            return go;
        }

        private static GitObject ReadInfo(FileInfo fi, Stream s)
        {
            GitObject result = new GitObject
            {
                Id = fi.Name,
                ObjectType = ObjectType.Info
            };
            if (fi.Name == "packs")
            {
                ReadPackInfo(s, result);
            }

            return result;
        }

        private static void ReadPackInfo(Stream s, GitObject go)
        {
            using (var sr = new StreamReader(s))
            {
                go.Body = sr.ReadToEnd();
            }

            var packs = new Regex(@"P (?<id>.*)");
            foreach (Match pack in packs.Matches(go.Body))
            {
                go.AddReference(pack.Groups["id"].Value);
            }
        }

        private static FileStream TryOpenFile(FileInfo fi, int retries)
        {
            for (int i = 0; ; i++)
            {
                try
                {
                    return File.OpenRead(fi.FullName);
                }
                catch (Exception)
                {
                    if (i < retries)
                    {
                        Thread.Sleep(500);
                    }
                    else
                    {
                        throw;
                    }

                }
            }
        }

        private static GitObject ReadObject(FileInfo fi, ref Stream s)
        {
            s = new Ionic.Zlib.ZlibStream(s, Ionic.Zlib.CompressionMode.Decompress);

            var go = new GitObject();
            var header = ReadHeader(s);
            go.ObjectType = header.Type;

            ReadBody(s, go, header);
            go.Id = fi.Directory.Name + fi.Name;

            return go;
        }
        public static ObjectHeader ReadHeader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream", "stream is null.");
            }

            var type = new StringBuilder();
            var size = new StringBuilder();

            int b;
            while ((b = (byte)stream.ReadByte()) != ' ')
            {
                type.Append((char)b);
            }

            while ((b = (byte)stream.ReadByte()) != '\0')
            {
                size.Append((char)b);
            }

            return new ObjectHeader
            {
                Type = (ObjectType)Enum.Parse(typeof(ObjectType), type.ToString(), true),
                Size = int.Parse(size.ToString(), CultureInfo.InvariantCulture)
            };
        }

        private static void ReadBody(Stream s, GitObject go, ObjectHeader header)
        {
            switch (header.Type)
            {
                case ObjectType.Tree:
                    ReadTree(s, go, header);
                    break;
                case ObjectType.Commit:
                    ReadCommit(s, go);
                    break;
                default:
                    go.Body = ReadFile(s);
                    break;
            }
        }

        private static void ReadCommit(Stream s, GitObject go)
        {
            go.Body = ReadFile(s);

            Regex parents = new Regex(@"parent (?<id>\w*)");
            foreach (Match match in parents.Matches(go.Body))
            {
                go.AddReference(match.Groups["id"].Value);
            }

            var tree = Regex.Match(go.Body, @"tree (?<id>\w*)");
            if (tree.Success)
            {
                go.AddReference(tree.Groups["id"].Value);
            }
        }

        private static string ReadFile(Stream s)
        {
            StringBuilder sb = new StringBuilder();
            int b = 0;

            while ((b = s.ReadByte()) > 0)
            {
                sb.Append(b != '\0' ? (char)b : ' ');
            }

            return sb.ToString();
        }

        private static void ReadTree(Stream s, GitObject go, ObjectHeader header)
        {
            var sb = new StringBuilder();
            
            // The counter is increased inside the loop
            // with every read operation.
            for (int i = 0; i < header.Size; )
            {
                var leading = ReadLeadingBytes(s);
                sb.Append(leading);
                sb.Append(' ');
                i += 7;

                var filename = ReadFileName(s);
                sb.Append(filename);
                sb.Append(' ');
                i += filename.Length + 1;

                var hash = ReadHash(s);
                sb.AppendLine(hash);
                i += 20;

                go.AddReference(hash, filename);
            }

            go.Body = sb.ToString();
        }

        private static string ReadLeadingBytes(Stream s)
        {
            var sb = new StringBuilder();
            for (int j = 0; j < 6; j++)
            {
                sb.Append((char)s.ReadByte());
            }

            return sb.ToString();
        }

        private static string ReadFileName(Stream s)
        {
            var filename = new StringBuilder();
            int b;

            while ((b = s.ReadByte()) != '\0')
            {
                filename.Append((char)b);
            }

            return filename.ToString();
        }
        private static string ReadHash(Stream s)
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < 20; j++)
            {
                sb.Append(s.ReadByte().ToString("x2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }
    }
}
