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
        public static string ParseFile(FileInfo fi)
        {
            Stream s = null;
            try
            {
                s = TryOpenFile(fi);
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
                        return ReadFile(s);
                    }
                }
                else
                {
                    return ReadFile(s);
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
        private static FileStream TryOpenFile(FileInfo fi)
        {
            for (int i = 0; ; i++)
            {
                try
                {
                    return File.OpenRead(fi.FullName);
                }
                catch (Exception)
                {
                    if (i < 50)
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
            var bytes = new List<byte>();
            byte b;

            while ((b = (byte)stream.ReadByte()) != '\0')
            {
                bytes.Add(b);
            }

            string[] parts = ASCIIEncoding.ASCII.GetString(bytes.ToArray()).Split(' ');

            return new ObjectHeader
            {
                Type = (ObjectType)Enum.Parse(typeof(ObjectType), parts[0], true),
                Size = int.Parse(parts[1])
            };
        }
        private static string ReadFile(Stream s)
        {
            using (var sr = new StreamReader(s, Encoding.UTF8))
            {
                int n = 0;
                var buffer = new char[1024];

                StringBuilder sb = new StringBuilder();
                while ((n = sr.Read(buffer, 0, buffer.Length)) > 0)
                {
                    sb.Append(new string(buffer, 0, n)).Replace('\0', '\r');
                }

                return sb.ToString();
            }
        }

        private static string ReadTree(Stream s, ObjectHeader header)
        {
            var sb = new StringBuilder();
            //while (s.Position < header.Size)
            //{
            //    var bytes = new List<byte>();
            //    int b;

            //    while ((b = s.ReadByte()) != '\0' && b != -1)
            //    {
            //        bytes.Add((byte)b);
            //    }

            //    sb.AppendFormat("{0} {1}", ASCIIEncoding.ASCII.GetString(bytes.ToArray()), ReadTreeLineFileRef(s));
            //    sb.AppendLine();
            //}

            int fragment = 0;
            var bytes = new List<byte>();

            for (int i = 0; i < header.Size; i++)
            {
                int b;

                while ((b = s.ReadByte()) != '\0' && b > 0)
                {
                    sb.Append((char)b);
                }

                sb.Append(' ');

                for (int j = 0; j < 20 && i < header.Size; j++, i++)
                {
                    sb.Append(s.ReadByte().ToString("x2"));
                }
            }

            return sb.ToString();
        }

        private static System.String ReadTreeLineFileRef(Stream s)
        {
            StringBuilder sb = new StringBuilder();
            int b;
            while ((b = s.ReadByte()) != '\0' && b != -1 && b != '\r')
            {
                sb.AppendFormat("{0:x}", b);
            }

            if (b == '\r')
            {
                s.ReadByte();
            }

            return sb.ToString();
        }
    }
}
