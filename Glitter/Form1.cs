using GitSharp.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Glitter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            watcher_Changed(this, new FileSystemEventArgs(WatcherChangeTypes.Created, @"C:\git\demo\.git\objects\1f", "d1d6b396322fe81a0b1ee91737c3cc4483ffef"));
            watcher_Changed(this, new FileSystemEventArgs(WatcherChangeTypes.Created, @"C:\git\demo\.git\objects\1c\", "1d7deed649fbecd66fab423ccd9d001bf9ff91"));
        }

        FileSystemWatcher watcher = new FileSystemWatcher(@"C:\git\demo\.git\objects");
        private void Form1_Load(object sender, EventArgs e)
        {
            watcher.Changed += watcher_Changed;
            watcher.Created += watcher_Changed;
            watcher.Renamed += watcher_Changed;

            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var fi = new FileInfo(e.FullPath);
            textBox1.Text += e.ChangeType + ", " + fi.FullName + Environment.NewLine;

            var temp = string.Format("{0}{1}", fi.Directory.Name, fi.Name);

            if (ObjectId.IsId(temp))
            {
                var id = ObjectId.FromString(temp);

                var repo = new GitSharp.Core.Repository(new DirectoryInfo(@"C:\git\demo\.git"));

                ObjectLoader o = null;

                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        o = repo.OpenObject(id);
                    }
                    catch (IOException)
                    {
                    }
                }
                string text = string.Empty;

                switch ((ObjectType)o.Type)
                {
                    case ObjectType.Bad:
                        break;
                    case ObjectType.Blob:
                        text = ASCIIEncoding.ASCII.GetString(repo.OpenBlob(id).Bytes);
                        break;
                    case ObjectType.Commit:
                        var commit = repo.MapCommit(id);
                        text = commit.ToString();
                        break;
                    case ObjectType.DeltaBase:
                        break;
                    case ObjectType.Extension:
                        break;
                    case ObjectType.ObjectType5:
                        break;
                    case ObjectType.OffsetDelta:
                        break;
                    case ObjectType.ReferenceDelta:
                        break;
                    case ObjectType.Tag:
                        var tag = repo.MapTag(fi.Name);
                        text = tag.ToString();
                        break;
                    case ObjectType.Tree:
                        var tree = repo.MapTree(id);
                        //tree.
                        text = tree.ToString();
                        break;
                    case ObjectType.Unknown:
                        break;
                    default:
                        break;
                }

                textBox1.Text += Environment.NewLine;
                textBox1.Text += text;
            }

            textBox1.Select(textBox1.Text.Length, 0);
        }

    }
}
