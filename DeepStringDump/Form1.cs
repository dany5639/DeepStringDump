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

namespace DeepStringDump
{
    public partial class Form1 : Form
    {
        public string filepath = "";

        public Form1(string[] args)
        {
            InitializeComponent();

            if (args.Length == 1)
                filepath = args[0];

            if (!File.Exists(filepath))
                button1_Click(null, null);

            if (!File.Exists(filepath))
            {
                Console.WriteLine($"ERROR: file does not exist.");
                return;
            }

            richTextBox1.Text = "INFO: click scan to start scanning. Auto dump strings to strings1.log.";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog1.ShowDialog();
            filepath = openFileDialog1.FileName;

            if (!File.Exists(filepath))
            {
                Console.WriteLine($"ERROR: file does not exist.");
                return;
            }
        }

        private void Scan()
        {
            using (var reader = new BinaryReader(File.OpenRead(filepath)))
            {
                debugConsoleWrite = checkBox1.Checked;

                Log1($"INFO: Size 0x{reader.BaseStream.Length:X16}", true, false);

                var bufferSize = numericUpDown1.Value;
                var minWordCount = numericUpDown2.Value;

                var buffer = new byte[0];

                while (true)
                {
                    buffer = reader.ReadBytes((int)bufferSize);
                     
                    Log1($"INFO: read 0x{buffer.Length:X16} bytes at {reader.BaseStream.Position:X16} out of {reader.BaseStream.Length:X16}.", true, false);

                    var word = new List<char>();
                    var items = new List<List<char>>();

                    // pass 1, find words with sequential characters
                    for (int i = 1; i < buffer.Length - 1; i++)
                    {
                        var cchar = (char)buffer[i];

                        if (ValidCharacters.Contains(buffer[i]))
                        {
                            word.Add(cchar);
                            continue;
                        }

                        if (buffer[i] == 0)
                        {
                            if (word.Count >= minWordCount)
                                items.Add(word);

                            word = new List<char>();
                        }

                        if (word.Count >= minWordCount)
                            items.Add(word);
                        word = new List<char>();

                    }

                    foreach (var a in items)
                    {
                        // foreach (var b in a)
                        //     foreach (var c in a)
                        //         if (b != c)
                        //             goto ok;
                        // 
                        // continue;
                        // 
                        // ok:

                        var d = "";
                        foreach (var b in a)
                            d = $"{d}{(char)b}";

                        // richTextBox1.Text += $"{d}\n";
                        Log1($"{d}");
                    }

                    items = new List<List<char>>();

                    // pass 2, find words with characters separated by 0x00
                    for (int i = 1; i < buffer.Length - 1; i++)
                    {
                        var cchar = (char)buffer[i];

                        if (ValidCharacters.Contains(buffer[i]))
                        {
                            if (buffer[i - 1] != 0 || buffer[i + 1] != 0)
                                continue;
                    
                            word.Add(cchar);
                    
                            if (buffer[i + 1] == 0)
                                i++;
                    
                        }
                        else
                        {
                            if (word.Count >= minWordCount)
                                items.Add(word);

                            word = new List<char>();
                        }
                    
                    }

                    foreach (var a in items)
                    {
                        // foreach (var b in a)
                        //     foreach (var c in a)
                        //         if (b != c)
                        //             goto ok;
                        // 
                        // continue;
                        // 
                        // ok:

                        var d = "";
                        foreach (var b in a)
                            d = $"{d}{(char)b}";

                        // richTextBox1.Text += $"{d}\n";
                        Log2($"{d}");
                    }

                    if (reader.BaseStream.Position == reader.BaseStream.Length)
                        return;
                }
            }
        }

        private List<byte> ValidCharacters = new List<byte>
        {
            0x20, // 
            0x2E, // .
            0x2C, // ,
            0x3A, // :
            0x27, // '
            0x22, // "
            0x2D, // -
            0x5F, // _
            0x2F, // /
            0x5C, // \
            0x28, // (
            0x29, // )
            0x5B, // [
            0x5D, // ]
            0x40, // @
            0x30, // 0
            0x31, // 1
            0x32, // 2
            0x33, // 3
            0x34, // 4
            0x35, // 5
            0x36, // 6
            0x37, // 7
            0x38, // 8
            0x39, // 9
            0x41, // A
            0x42, // B
            0x43, // C
            0x44, // D
            0x45, // E
            0x46, // F
            0x47, // G
            0x48, // H
            0x49, // I
            0x4A, // J
            0x4B, // K
            0x4C, // L
            0x4D, // M
            0x4E, // N
            0x4F, // O
            0x50, // P
            0x51, // Q
            0x52, // R
            0x53, // S
            0x54, // T
            0x55, // U
            0x56, // V
            0x57, // W
            0x58, // X
            0x59, // Y
            0x5A, // Z
            0x61, // a
            0x62, // b
            0x63, // c
            0x64, // d
            0x65, // e
            0x66, // f
            0x67, // g
            0x68, // h
            0x69, // i
            0x6A, // j
            0x6B, // k
            0x6C, // l
            0x6D, // m
            0x6E, // n
            0x6F, // o
            0x70, // p
            0x71, // q
            0x72, // r
            0x73, // s
            0x74, // t
            0x75, // u
            0x76, // v
            0x77, // w
            0x78, // x
            0x79, // y
            0x7A, // z
        };

        private static List<byte> ValidCharacters_symbols = new List<byte>
        {
            0x20, //  
            0x21, // !
            0x22, // "
            0x23, // #
            0x24, // $
            0x25, // %
            0x26, // &
            0x27, // '
            0x28, // (
            0x29, // )
            0x2A, // *
            0x2B, // +
            0x2C, // ,
            0x2D, // -
            0x2E, // .
            0x2F, // /
            0x3A, // :
            0x3B, // ;
            0x3C, // <
            0x3D, // =
            0x3E, // >
            0x3F, // ?
            0x40, // @
            0x5B, // [
            0x5C, // \
            0x5D, // ]
            0x5E, // ^
            0x5F, // _
            0x60, // `
            0x7B, // {
            0x7C, // |
            0x7D, // }
        };

        #region Utilities
        private static bool debugConsoleWrite = true;
        private static List<string> LogQ1 = new List<string>();
        private static List<string> LogQ2 = new List<string>();

        public static void LogDumpToFile(List<string> in_, string file)
        {
            var fileOut = new FileInfo(file);
            if (File.Exists(file))
                File.Delete(file);

            int i = -1;
            try
            {
                using (var csvStream = fileOut.OpenWrite())
                using (var csvWriter = new StreamWriter(csvStream))
                {
                    foreach (var a in in_)
                    {
                        csvStream.Position = csvStream.Length;
                        csvWriter.WriteLine(a);
                        i++;
                    }
                }
            }
            catch
            { }

        }

        public static void Log1(string in_, bool force = false, bool write = true)
        {
            if (write)
                LogQ1.Add(in_);
            if (debugConsoleWrite || force)
                Console.WriteLine($"{in_}");
        }
        public static void Log2(string in_, bool force = false, bool write = true)
        {
            if (write)
                LogQ2.Add(in_);
            if (debugConsoleWrite || force)
                Console.WriteLine($"{in_}");
        }

        #endregion

        private void button2_Click(object sender, EventArgs e)
        { 
            Scan();
            LogDumpToFile(LogQ1, "strings1.log");
            LogDumpToFile(LogQ2, "strings2.log");

            richTextBox1.Text = "INFO: dumped strings to strings1.log and strings2.log";
        }
    }
}
