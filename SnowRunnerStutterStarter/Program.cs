using SnowRunnerStutterHook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnowRunnerStutterRemover
{
    class Program
    {
        static string ConfigFilename = "stutter.cfg";

        [STAThread]
        static void Main(string[] args)
        {
            // Will contain the name of the IPC server channel
            string channelName = null;

            // Process command line arguments or print instructions and retrieve argument value
            ProcessArgs(args, out var targetPid, out var targetExe);

            if (targetPid <= 0 && string.IsNullOrEmpty(targetExe))
                return;
            // Create the IPC server using the FileMonitorIPC.ServiceInterface class as a singleton
            EasyHook.RemoteHooking.IpcCreateServer<ServerInterface>(ref channelName,
                WellKnownObjectMode.Singleton);

            // Get the full path to the assembly we want to inject into the target process
            string injectionLibrary =
                Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "SnowRunnerStutterHook.dll");

            try
            {
                // Injecting into existing process by Id
                if (targetPid > 0)
                {
                    Console.WriteLine("Attempting to inject into process {0}", targetPid);

                    // inject into existing process
                    EasyHook.RemoteHooking.Inject(
                        targetPid, // ID of process to inject into
                        injectionLibrary, // 32-bit library to inject (if target is 32-bit)
                        injectionLibrary, // 64-bit library to inject (if target is 64-bit)
                        channelName // the parameters to pass into injected library
                        // ...
                    );
                }
                // Create a new process and then inject into it
                else if (!string.IsNullOrEmpty(targetExe))
                {
                    Console.WriteLine("Attempting to create and inject into {0}", targetExe);
                    // start and inject into a new process
                    EasyHook.RemoteHooking.CreateAndInject(
                        targetExe, // executable to run
                        "", // command line arguments for target
                        0, // additional process creation flags to pass to CreateProcess
                        EasyHook.InjectionOptions.DoNotRequireStrongName, // allow injectionLibrary to be unsigned
                        injectionLibrary, // 32-bit library to inject (if target is 32-bit)
                        injectionLibrary, // 64-bit library to inject (if target is 64-bit)
                        out targetPid, // retrieve the newly created process ID
                        channelName // the parameters to pass into injected library
                        // ...
                    );
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was an error while injecting into target:");
                Console.ResetColor();
                Console.WriteLine(e.ToString());
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("<Press any key to exit>");
            Console.ResetColor();
            Console.ReadKey();
        }

        static void ProcessArgs(string[] args, out int targetPID, out string targetExe)
        {
            targetPID = 0;
            targetExe = null;

            // Load any parameters
            while ((args.Length != 1) || !Int32.TryParse(args[0], out targetPID) || !File.Exists(args[0]))
            {
                if (targetPID > 0)
                {
                    break;
                }

                if (args.Length != 1 || !File.Exists(args[0]))
                {
                    var savedPath = LoadSnowRunnerPath();
                    if (savedPath != null)
                    {
                        targetExe = savedPath;
                        return;
                    }

                    MessageBox.Show("Please select SnowRunner.exe\nHere's the default path for a steam installation:\nC:\\Program Files (x86)\\Steam\\steamapps\\common\\SnowRunner\\Sources\\Bin");
                    
                    var fd = new OpenFileDialog()
                    {
                        CheckFileExists = true,
                        Filter = "Snowrunner Applicaton|SnowRunner.exe",
                        Title = "Select SnowRunner.exe",
                        RestoreDirectory = true
                    };
                    
                    if (fd.ShowDialog() == DialogResult.OK)
                    {
                        args = new[] {fd.FileName};
                        SaveSnowRunnerPath(fd.FileName);
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
                else
                {
                    targetExe = args[0];
                    break;
                }
            }
        }
        private static string LoadSnowRunnerPath()
        {
            try
            {
                var path = File.ReadAllText(ConfigFilename);
                return File.Exists(path) ? path : null;
            }
            catch
            {
                return null;
            }
        }
        private static void SaveSnowRunnerPath(string path)
        {
            File.WriteAllText(ConfigFilename, path);
        }
    }
}
