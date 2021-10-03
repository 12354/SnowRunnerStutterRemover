using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Reloaded.Memory.Sigscan;

namespace SnowRunnerStutterPatcher
{
    class Program
    {

        /// <summary>
        /// Original code
        /// SnowRunner.exe+F7AF34 - cmp edx,00000219
        /// SnowRunner.exe+F7AF3A - jne SnowRunner.exe+F7B195
        /// 
        /// Patched code
        /// SnowRunner.exe+F7AF34 - or eax,-01
        /// SnowRunner.exe+F7AF37 - nop 
        /// SnowRunner.exe+F7AF38 - nop 
        /// SnowRunner.exe+F7AF39 - nop
        /// 
        /// The original code reloads all hid devices when a WM_DEVICECHANGE (0x219) message was found
        /// This patches this check to never find the message, therefore removing the lag and input loss
        /// A pattern search is used to hopefully make this compatible with future patches
        /// </summary>
        

        private static readonly string Pattern = "81 FA 19 02 00 00 0F 85"; //cmp edx,0x219;jne
        private static readonly byte[] Patch = { 0x83, 0xC8, 0xFF, 0x90, 0x90, 0x90 };//or eax,01;nop;nop;nop
        [STAThread]
        static void Main(string[] args)
        {
            
            MessageBox.Show("Please select SnowRunner.exe\nHere's the default path for a steam installation:\nC:\\Program Files (x86)\\Steam\\steamapps\\common\\SnowRunner\\Sources\\Bin");
                    
            var fd = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "Snowrunner Applicaton|SnowRunner.exe",
                Title = "Select SnowRunner.exe",
                RestoreDirectory = true
            };

            if (fd.ShowDialog() != DialogResult.OK)
            {
                Environment.Exit(0);
            }

            var fn = fd.FileName;
            var data = File.ReadAllBytes(fn);
            var scanner = new Scanner(data);
            var offset = scanner.CompiledFindPattern(Pattern);
            if (offset.Found)
            {
                for (var i = 0; i < Patch.Length; i++)
                {
                    data[offset.Offset+i] = Patch[i];
                }
            }

            File.WriteAllBytes(fn, data);
        }
    }
}
