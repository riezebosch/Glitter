using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Glitter.WPF
{
    public class ObjectFileParser
    {
        public static GitObject ParseFile(FileInfo fi)
        {
            Stream s = null;
            try
            {
                s = TryOpenFile(fi, 50);
                if (fi.Directory.Parent.Name == "objects")
                {
                    s = new Ionic.Zlib.ZlibStream(s, Ionic.Zlib.CompressionMode.Decompress);

                    var header = ReadHeader(s);
                    if (header.Type == ObjectType.Tree)
                    {
                        return ReadTree(s, header);
                    }
                    else
                    {
                        return ReadFile(s, header);
                    }
                }
                else
                {
                    return ReadFile(s, null);
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

        public static GitObject ReadFile(Stream s, ObjectHeader header)
        {
            StringBuilder sb = new StringBuilder();
            int b = 0;

            while ((b = s.ReadByte()) > 0)
            {
                sb.Append(b != '\0' ? (char)b : ' ');
            }

            return new GitObject
            {
                Header = header,
                Body = sb.ToString()
            };
        }

        private static GitObject ReadTree(Stream s, ObjectHeader header)
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
                // for the \0 that was read
                sb.Append(' ');
                i++;

                // Read the SHA1 id of the file or sub tree
                for (int j = 0; j < 20 && i < header.Size; j++, i++)
                {
                    sb.Append(s.ReadByte().ToString("x2"));
                }

                sb.AppendLine();
            }

            return new GitObject 
            { 
                Header = header, 
                Body = sb.ToString() 
            };
        }
    }
}
