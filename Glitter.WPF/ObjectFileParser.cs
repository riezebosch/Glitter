using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Glitter.WPF
{
    public class ObjectFileParser
    {
        public static GitObject ParseFile(FileInfo fi)
        {
            if (fi.Extension == ".lock" ||
                fi.Name.StartsWith("tmp") ||
                fi.Name.StartsWith(".tmp") ||
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
                    //return ReadIndex(fi, s);
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

        private static GitObject ReadIndex(FileInfo fi, Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                var go = new GitObject { Id = fi.Name, Type = ObjectType.Index };
                go.References.Add(sr.ReadToEnd().Trim());
                return go;
            }
        }

        private static GitObject ReadHead(FileInfo fi, Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                var go = new GitObject
                {
                    Id = fi.Name,
                    Type = ObjectType.Head,
                    Body = sr.ReadToEnd().Trim()
                };
                go.References.Add(ExtractReferenceFromHead(go.Body));

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
                    Type = ObjectType.Branch,
                    Body = sr.ReadToEnd().Trim()
                };
                go.References.Add(go.Body);
                return go;
            }
        }

        private static GitObject ReadPack(FileInfo fi)
        {
            var go = new GitObject
            {
                Id = fi.Name,
                Type = ObjectType.Pack
            };
            if (fi.Extension == ".idx")
            {
                go.References.Add(Path.ChangeExtension(fi.Name, ".pack"));
            }

            return go;
        }

        private static GitObject ReadInfo(FileInfo fi, Stream s)
        {
            GitObject result = new GitObject
            {
                Id = fi.Name,
                Type = ObjectType.Info
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
                go.References.Add(pack.Groups["id"].Value);
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
            go.Type = header.Type;

            ReadBody(fi, s, go, header);
            go.Id = fi.Directory.Name + fi.Name;

            return go;
        }
        public static ObjectHeader ReadHeader(Stream stream)
        {
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
                Size = int.Parse(size.ToString())
            };
        }

        private static void ReadBody(FileInfo fi, Stream s, GitObject go, ObjectHeader header)
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
                go.References.Add(match.Groups["id"].Value);
            }

            var tree = Regex.Match(go.Body, @"tree (?<id>\w*)");
            if (tree.Success)
            {
                go.References.Add(tree.Groups["id"].Value);
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
                // Read the filename and the leading bytes
                int b;
                while ((b = s.ReadByte()) != '\0')
                {
                    sb.Append((char)b);
                    i++;
                }

                // Add a space and increase the counter 
                // for the \0 that was read and the hash
                sb.Append(' ');
                i += 21;

                var hash = ReadHash(s);
                go.References.Add(hash);
                sb.Append(hash);

                sb.AppendLine();
            }

            go.Body = sb.ToString();
        }

        private static string ReadHash(Stream s)
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < 20; j++)
            {
                sb.Append(s.ReadByte().ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
