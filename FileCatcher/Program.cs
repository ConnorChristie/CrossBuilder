using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace FileCatcher.Core
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //var fakes = new List<FileInfo>()
            //{
            //    new FileInfo(@"D:\Git\CrossBuilder\FileCatcher\bin\netcoreapp3.1\deviare32.db"),
            //    new FileInfo(@"D:\Git\CrossBuilder\FileCatcher\bin\netcoreapp3.1\deviare64.db"),
            //    new FileInfo(@"D:\Git\CrossBuilder\FileCatcher\bin\netcoreapp3.1\DeviareCOM64.dll"),
            //    new FileInfo(@"D:\Git\CrossBuilder\FileCatcher\bin\netcoreapp3.1\DvAgent.dll"),
            //    new FileInfo(@"D:\Git\CrossBuilder\FileCatcher\bin\netcoreapp3.1\DvAgent64.dll"),
            //    new FileInfo(@"D:\Git\CrossBuilder\FileCatcher\bin\netcoreapp3.1\Nektra.Deviare2.dll"),
            //};

            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1(fakes));

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: FileCatcher.exe <program> [arguments]");
                return;
            }

            var program = args[0];
            var arguments = args.Length > 1 ? string.Join(' ', args.AsSpan().Slice(1).ToArray()) : "";
            var workingDir = Directory.GetCurrentDirectory();

            var autoEvent = new AutoResetEvent(false);
            IEnumerable<FileInfo> files = null;

            new ProcessCapture(program, arguments, workingDir).Capture(x =>
            {
                files = x;
                autoEvent.Set();
            });

            autoEvent.WaitOne();

            if (files != null)
            {
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1(files));
            }
        }
    }
}
