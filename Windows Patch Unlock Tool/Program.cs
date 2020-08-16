using System;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace Windows_Patch_Unlock_Tool
{
    static class Program
    {
        static readonly string _patchPath = Environment.GetEnvironmentVariable("systemdrive") + @"\Windows\servicing\Packages";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Action<string> action = null;

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.WriteLine("This program needs to be run as an administrator.");
                Thread.Sleep(3000);
                return;
            }

            action = delegate (string path)
            {
                DirectoryInfo root = new DirectoryInfo(path);

                foreach (FileInfo fileInfo in root.GetFiles("*.mum"))
                {
                    string str = File.ReadAllText(fileInfo.FullName);

                    if (str.Contains("permanence=\"permanent\""))
                    {
                        str = str.Replace("permanence=\"permanent\"", "permanence=\"removable\"");

                        File.WriteAllText(fileInfo.FullName, str);
                        Console.WriteLine(fileInfo.FullName);
                    }
                }

                foreach (DirectoryInfo directoryInfo in root.GetDirectories())
                {
                    action.Invoke(directoryInfo.FullName);
                }
            };

            action.Invoke(_patchPath);

            Console.WriteLine("OK.");
            Thread.Sleep(3000);
        }
    }
}
