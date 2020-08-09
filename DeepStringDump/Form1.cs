using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeepStringDump
{
    public partial class Form1 : Form
    {
        #region Utilities
        private static List<string> LogQ1 = new List<string>();
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
            //if (write)
            LogQ1.Add(in_);
            //if (debugConsoleWrite || force)
            //    Console.WriteLine($"{in_}");
        }
        #endregion

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
            0x3C, // <
            0x3E, // >
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
        public string filepath = "";
        private static int threadsDone = 0;

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

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LogQ1 = new List<string>();

            openFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            if (filepath != "")
                openFileDialog1.InitialDirectory = Directory.GetParent(filepath).FullName;

            openFileDialog1.ShowDialog();
            filepath = openFileDialog1.FileName;

            if (!File.Exists(filepath))
            {
                Console.WriteLine($"ERROR: file does not exist.");
                return;
            }
        }

        public Thread StartTheThread(byte[] buffer, int start, int end)
        {
            var t = new Thread(() => Scan2(buffer, start, end));
            t.Start();
            return t;
        }

        private void Scan2(byte[] buffer, int start, int end)
        {
            var minCharCount = numericUpDown2.Value;
            var word = new List<char>();
            var items = new List<List<char>>();

            // pass 1, find words with sequential characters
            for (int i = start; i < end - 1; i++)
            {
                toolStripProgressBar1.Value = (i / buffer.Length) * 100;

                var cchar = (char)buffer[i];

                if (ValidCharacters.Contains(buffer[i]))
                {
                    word.Add(cchar);
                    continue;
                }

                if (buffer[i] == 0)
                {
                    if (word.Count > minCharCount - 1)
                        items.Add(word);

                    word = new List<char>();
                }

                if (word.Count > minCharCount - 1)
                    items.Add(word);
                word = new List<char>();

            }

            // pass 2, find words with characters separated by 0x00
            for (int i = start; i < end - 1; i++)
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
                    if (word.Count > minCharCount - 1)
                        items.Add(word);

                    word = new List<char>();
                }

            }

            foreach (var a in items)
            {
                var d = "";
                foreach (var b in a)
                    d = $"{d}{(char)b}";

                Log1($"{d}");
            }

            threadsDone++;
        }

        private void Scan()
        {
            using (var reader = new BinaryReader(File.OpenRead(filepath)))
            {
                // Log1($"INFO: Size 0x{reader.BaseStream.Length:X16}", true, false);

                var bufferLength = (int)reader.BaseStream.Length;
                if (reader.BaseStream.Length > int.MaxValue)
                    throw new Exception("ERROR: file too big, must edit code.");


                var buffer = reader.ReadBytes(bufferLength);

                var bufferLength2 = buffer.Length;
                if (bufferLength2 < 5)
                    return;
                var bufferLengthdiv4 = bufferLength2 / 4;

                StartTheThread(buffer, 1, bufferLengthdiv4 * 1);
                StartTheThread(buffer, bufferLengthdiv4 * 1 - (int)numericUpDown2.Value, bufferLengthdiv4 * 2);
                StartTheThread(buffer, bufferLengthdiv4 * 2 - (int)numericUpDown2.Value, bufferLengthdiv4 * 3);
                StartTheThread(buffer, bufferLengthdiv4 * 3 - (int)numericUpDown2.Value, bufferLengthdiv4 * 4);

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LogQ1 = new List<string>();
            var startTime = DateTime.Now;
            Scan();
            start:
            Thread.Sleep(100);
            if (threadsDone < 4)
                goto start;

            var endTime = DateTime.Now;

            var fileinfo = new FileInfo(filepath);
            var path = $"{fileinfo.DirectoryName}\\{fileinfo.Name.Substring(0, fileinfo.Name.Length - fileinfo.Extension.Length)}_strings.txt";

            label1.Text = $"threads:{threadsDone}; dT:{endTime - startTime} {path}";
            LogDumpToFile(LogQ1, $"{path}");

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var dir2 = Directory.EnumerateFiles(Directory.GetParent(filepath).FullName);

            foreach (var a in dir2)
            {
                LogQ1 = new List<string>();

                filepath = a;

                Scan();

                if (LogQ1.Count == 0)
                    continue;

                LogDumpToFile(LogQ1, $"{filepath.Split("\\".ToCharArray()).Last().Split(".".ToCharArray()).First()}.txt");

                Console.WriteLine($"Dumped strings to {filepath.Split("\\".ToCharArray()).Last().Split(".".ToCharArray()).First()}.txt");
            }
        }

        private void Button3_Click_1(object sender, EventArgs e)
        {
            filepath = @"D:\TEST.RAW";

            using (var reader = new BinaryReader(File.OpenRead(filepath)))
            {
                var bufferLength = (int)reader.BaseStream.Length;
                if (reader.BaseStream.Length > int.MaxValue)
                    throw new Exception("ERROR: file too big, must edit code.");

                var buffer = reader.ReadBytes(bufferLength);

                var minCharCount = numericUpDown2.Value;
                var items = new List<List<char>>();
                toolStripProgressBar1.Maximum = buffer.Length;
                // pass 1, find words with sequential characters
                var word = new List<char>();

                for (int i = 0; i < buffer.Length - 1; i++)
                {
                    if (buffer[i] == 0)
                    {
                        for (int j = i + 1; j < buffer.Length - 1; j++)
                        {
                            if ((buffer[j] < 0x20 || buffer[j] > 0x7D) && buffer[j] != 0x0)
                                goto invalidWord;
                            else
                                word.Add((char)buffer[j]);
                            if (buffer[j] == 0 && word.Count > minCharCount) // pick only words with 0x00 caps
                                goto foundWord;
                            if (buffer[j] == 0)
                                goto invalidWord;
                        }
                        foundWord:
                        items.Add(word);

                        var word2 = "";
                        foreach (var a in word)
                            word2 = $"{word2}{a}";
                        Console.WriteLine($"0x{i:X8} {word2}");

                        toolStripProgressBar1.Value = buffer.Length - i;
                    }
                    invalidWord:
                    word = new List<char>();

                }

                /*
                // pass 2, find words with characters separated by 0x00
                // for (int i = 0; i < buffer.Length - 1; i++)
                // {
                //     toolStripProgressBar1.Value = buffer.Length - i;
                // 
                //     var cchar = (char)buffer[i];
                // 
                //     if (ValidCharacters.Contains(buffer[i]))
                //     {
                //         if (buffer[i - 1] != 0 || buffer[i + 1] != 0)
                //             continue;
                // 
                //         word.Add(cchar);
                // 
                //         if (buffer[i + 1] == 0)
                //             i++;
                // 
                //     }
                //     else
                //     {
                //         if (word.Count > minCharCount - 1)
                //             items.Add(word);
                // 
                //         word = new List<char>();
                //     }
                // 
                // }
                */

                Console.WriteLine("000102030405060708090A0B0C0D0E0F");
                // WARNING IT FUCKS UP ON UNICODE OR WHATEVER ITS CALLED
                for (int i = 0; i < buffer.Length - 1; i++)
                {
                    if (i == 0xFF)
                        ;

                    if (i != 0 && i % 0x10 == 0)
                        Console.WriteLine();

                    Console.Write($"{(byte)buffer[i]:X2}");

                    if (buffer[i] == 0 && buffer[i + 1] == 0)
                    {
                        if (buffer[i + 1] == 0)
                            continue;

                        if (buffer[i + 2] != 0)
                            continue;

                        for (int j = i + 1; j < buffer.Length - 1; j++)
                        {
                            if ((buffer[j] < 0x20 || buffer[j] > 0x7D) && buffer[j] != 0x0)
                                goto invalidWord;
                            else
                                word.Add((char)buffer[j]);
                            if (word.Count > minCharCount)
                                goto foundWord;
                        }
                        foundWord:
                        items.Add(word);
                        var word2 = "";
                        foreach (var a in word)
                            word2 = $"{word2}{a}";
                        Console.WriteLine($"0x{i:X8} {word2}");
                        toolStripProgressBar1.Value = buffer.Length - i;
                    }
                    invalidWord:
                    word = new List<char>();

                }

                foreach (var a in items)
                {
                    var d = "";
                    foreach (var b in a)
                        d = $"{d}{(char)b}";

                    Log1($"{d}");
                }

            }

            if (LogQ1.Count == 0)
                return;

            LogDumpToFile(LogQ1, $"{filepath.Split("\\".ToCharArray()).Last().Split(".".ToCharArray()).First()}.txt");

            Console.WriteLine($"Dumped strings to {filepath.Split("\\".ToCharArray()).Last().Split(".".ToCharArray()).First()}.txt");
        }
    }

}
