using HashDepot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Threading;

namespace CrossSender
{
    public class Sender
    {
        public void Run()
        {
            var port = 13000;
            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            client.Connect(new IPEndPoint(IPAddress.Parse("192.168.1.74"), port));

            var stop = Stopwatch.StartNew();

            //client.NoDelay = true;
            client.SendBufferSize = 1024 * 128;

            //client.SendFile(@"C:\temp2.zip", new byte[0], new byte[0], TransmitFileOptions.UseSystemThread);

            var files = new List<FileInfo>();
            var rootDir = @"D:\Toolchains\sysroot-glibc-linaro-2.23-2017.05-arm-linux-gnueabihf-2";

            Traverse(rootDir, files);

            using (var stream = new NetworkStream(client, true))
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    foreach (var file in files)
                    {
                        archive.CreateEntryFromFile(file.FullName, Path.GetRelativePath(rootDir, file.FullName), CompressionLevel.Fastest);
                    }
                }

                stream.Flush();
            }

            client.Close();

            stop.Stop();
            Console.WriteLine("Time: " + stop.Elapsed);
            Console.Read();
        }

        public static void Traverse(string rootDirectory, IList<FileInfo> fileList)
        {
            var files = Enumerable.Empty<string>();
            var directories = Enumerable.Empty<string>();

            try
            {
                // The test for UnauthorizedAccessException.
                var permission = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, rootDirectory);
                permission.Demand();

                files = Directory.GetFiles(rootDirectory);
                directories = Directory.GetDirectories(rootDirectory);
            }
            catch
            {
            }

            foreach (var file in files)
            {
                fileList.Add(new FileInfo(file));
            }

            foreach (var directory in directories)
            {
                Traverse(directory, fileList);
            }
        }
    }

    public class Receiver
    {
        public void Run()
        {
            var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                var port = 13000;
                server.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), port));
                server.Listen(2);

                while (true)
                {
                    var socket = server.Accept();

                    var runner = new ReceiverClientThread(socket);
                    var thread = new Thread(runner.Run);

                    thread.Start();
                    thread.Join();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                server.Close();
            }
        }
    }

    public class ReceiverClientThread
    {
        private readonly Socket socket;

        public ReceiverClientThread(Socket socket)
        {
            this.socket = socket;
        }

        public void Run()
        {
            var stop = Stopwatch.StartNew();

            using (var fs = new FileStream("output.zip", FileMode.Create, FileAccess.Write))
            {
                try
                {
                    //socket.NoDelay = true;
                    socket.ReceiveBufferSize = 1024 * 128;
                    socket.ReceiveTimeout = 5000;

                    while (socket.Connected)
                    {
                        var buff = new byte[socket.ReceiveBufferSize];
                        var n = socket.Receive(buff);

                        if (n == 0)
                        {
                            break;
                        }

                        fs.Write(buff, 0, n);

                        Console.WriteLine("Rcv: " + n);
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    Console.WriteLine("Done recv: " + stop.Elapsed);
                    stop.Restart();
                }
            }

            using (var archive = ZipFile.OpenRead("output.zip"))
            {
                foreach (var entry in archive.Entries)
                {
                    var file = new FileInfo("out/" + entry.FullName.Replace('\\', '/'));

                    if (!file.Directory.Exists)
                    {
                        file.Directory.Create();
                    }

                    entry.ExtractToFile(file.FullName);
                }
            }

            stop.Stop();
            Console.WriteLine("Done all: " + stop.Elapsed);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var stop = Stopwatch.StartNew();

            var tree = new TreeNode(@"D:\Toolchains\sysroot-glibc-linaro-2.23-2017.05-arm-linux-gnueabihf-2", false);
            BuildFileSystemTree(tree);

            var bin = new BinaryFormatter
            {
                TypeFormat = FormatterTypeStyle.TypesWhenNeeded
            };

            using (var ms = new FileStream(@"serial.dat", FileMode.Create, FileAccess.Write))
            {
                bin.Serialize(ms, tree);
                ms.Close();
            }

            using (var ms = new FileStream(@"serial.dat", FileMode.Open, FileAccess.Read))
            {
                var de = bin.Deserialize(ms) as TreeNode;
            }

            Console.WriteLine("File discovery : " + stop.Elapsed);

            //Console.WriteLine("Hello World!");
            //var stop = Stopwatch.StartNew();

            //var files = Directory.EnumerateFiles(@"D:\Toolchains\sysroot-glibc-linaro-2.23-2017.05-arm-linux-gnueabihf-2", "*", new EnumerationOptions
            //{
            //    RecurseSubdirectories = true
            //});

            //Console.WriteLine("File discovery : " + stop.Elapsed);
            //stop.Restart();

            //var hashes2 = files.Select(x =>
            //{
            //    using (var fs = File.OpenRead(x))
            //    {
            //        var buffer = new byte[1024];
            //        fs.Read(buffer, 0, buffer.Length);
            //        fs.Close();

            //        return XXHash.Hash64(buffer);
            //    }
            //}).ToList();

            //foreach (var f in hashes2.Take(10))
            //{
            //    Console.WriteLine(f);
            //}

            //Console.WriteLine(stop.Elapsed);

            //if (args[0] == "s")
            //{
            //    new Receiver().Run();
            //}
            //else if (args[0] == "c")
            //{
            //    new Sender().Run();
            //}

            while (true) ;
        }

        public static void BuildFileSystemTree(TreeNode parent)
        {
            try
            {
                var path = parent.GetPath();

                // The test for UnauthorizedAccessException.
                var permission = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, path);
                permission.Demand();

                var files = Directory.GetFiles(path);
                var directories = Directory.GetDirectories(path);

                foreach (var file in files)
                {
                    var fileNode = new TreeNode(Path.GetRelativePath(path, file), true);

                    using (var fs = File.OpenRead(file))
                    {
                        var buffer = new byte[1024];
                        fs.Read(buffer, 0, buffer.Length);
                        fs.Close();

                        fileNode.BeginningHash = XXHash.Hash64(buffer);
                    }

                    parent.Add(fileNode);
                }

                foreach (var directory in directories)
                {
                    var dir = new TreeNode(Path.GetRelativePath(path, directory), false);
                    parent.Add(dir);

                    BuildFileSystemTree(dir);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    [Serializable]
    public class TreeNode
    {
        public string Name;
        public bool IsFile;

        public ulong BeginningHash;
        public ulong EntireHash;

        public TreeNode Parent;

        public IList<TreeNode> Children;

        public TreeNode() { }

        public TreeNode(string name, bool isFile)
        {
            Name = name;
            IsFile = isFile;

            Children = new List<TreeNode>();
        }

        public string GetPath()
        {
            var path = Name;

            for (var parent = Parent; parent != null; parent = parent.Parent)
            {
                path = parent.Name + Path.DirectorySeparatorChar + path;
            }

            return path;
        }

        public TreeNode Add(TreeNode node)
        {
            node.Parent = this;
            Children.Add(node);
            return node;
        }

        public TreeNode[] AddChildren(params TreeNode[] values)
        {
            return values.Select(Add).ToArray();
        }

        public bool RemoveChild(TreeNode node)
        {
            return Children.Remove(node);
        }
    }
}
