using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace DoctorWhoToolsWorking
{
    public partial class TextTool : Form
    {
        byte[] allblock_length = new byte[4];
        byte[] block_length = new byte[4];
        byte[] binContent = new byte[0];
        byte[] bytes = new byte[4];

        string result;

        int poz, offset;

        List<string> str_collection = new List<string>();

        List<textureswork> texCollect = new List<textureswork>();

        public class textureswork
        {
            public float x_start = 0, y_start = 0, x_end = 0, y_end = 0;
            public uint texdata = 0;
            public int texnum = 0;

            public textureswork(uint _texdata, float _x_start, float _y_start, float _x_end,
                float _y_end, int _texnum)
            {
                this.texdata = _texdata;
                this.x_start = _x_start;
                this.y_start = _y_start;
                this.x_end = _x_end;
                this.y_end = _y_end;
                this.texnum = _texnum;
            }
        }

        public static FileInfo[] fi;

        public TextTool()
        {
            InitializeComponent();
        }

        public void SendMessage(string message)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new SendMessage(SendMessage), message);
                Thread.Sleep(5);
            }
            else
            {
                listBox1.Items.Add(message);
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                listBox1.SelectedIndex = -1;
            }
        }

        public class text_struct
        {
            public byte[] data_bytes;
            public byte[] string_index;
            public int index;

            public text_struct() { }
            public text_struct(byte[] _data_bytes, byte[] _string_index,
                int _index)
            {
                this.data_bytes = _data_bytes;
                this.string_index = _string_index;
                this.index = _index;
            }
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            var ExtractFiles = new Threads();
            ExtractFiles.SendMes += SendMessage;

            var threadExtract = new Thread(new ParameterizedThreadStart(ExtractFiles.ExportFiles));
            threadExtract.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var ReplaceFiles = new Threads();
            ReplaceFiles.SendMes += SendMessage;

            var threadReplace = new Thread(new ParameterizedThreadStart(ReplaceFiles.ImportFiles));
            threadReplace.Start();
        }

        

        private void button3_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(MainForm.settings.inputpath) && Directory.Exists(MainForm.settings.outputpath))
            {
                DirectoryInfo di = new DirectoryInfo(MainForm.settings.inputpath);
                fi = di.GetFiles();

                for (int i = 0; i < fi.Length; i++)
                {
                    if (fi[i].Extension == ".dat")
                    {
                        FileStream fs = new FileStream(fi[i].FullName, FileMode.Open);
                        byte[] binContent = Methods.ReadFull(fs);
                        fs.Close();

                        if (Methods.FindStartOfStringSomething(binContent, 0, "INFO") == 0
                            && Methods.FindStartOfStringSomething(binContent, 0, "PTEX") > 0
                             && Methods.FindStartOfStringSomething(binContent, 0, "DDS ") > 0)
                        {
                            bool work = false;

                            if (texCollect.Count != 0) texCollect.Clear();

                            int poz = 4;
                            byte[] temp = new byte[4];
                            Array.Copy(binContent, poz, temp, 0, temp.Length);
                            uint offset_info = BitConverter.ToUInt32(temp, 0);
                            uint offset = BitConverter.ToUInt32(temp, 0) + 16;
                            temp = new byte[4];
                            Array.Copy(binContent, offset, temp, 0, temp.Length);
                            int counttexblocks = BitConverter.ToInt32(temp, 0);
                            offset += 4;
                            temp = new byte[4];
                            Array.Copy(binContent, offset, temp, 0, temp.Length);
                            offset += 4;
                            int counttextures = BitConverter.ToInt32(temp, 0);
                            temp = new byte[4];
                            Array.Copy(binContent, offset, temp, 0, temp.Length);
                            offset += 4;
                            poz = BitConverter.ToInt32(temp, 0) + 16 + (int)offset_info;

                            for (int j = 0; j < counttexblocks; j++)
                            {
                                float x_start = 0, y_start = 0, x_end = 0, y_end = 0;
                                uint texdata = 0;
                                int texnum = 0;

                                temp = new byte[4];
                                Array.Copy(binContent, offset, temp, 0, temp.Length);
                                texdata = BitConverter.ToUInt32(temp, 0);
                                offset += 4;

                                temp = new byte[4];
                                Array.Copy(binContent, offset, temp, 0, temp.Length);
                                x_start = BitConverter.ToSingle(temp, 0);
                                offset += 4;

                                temp = new byte[4];
                                Array.Copy(binContent, offset, temp, 0, temp.Length);
                                y_start = BitConverter.ToSingle(temp, 0);
                                offset += 4;

                                temp = new byte[4];
                                Array.Copy(binContent, offset, temp, 0, temp.Length);
                                x_end = BitConverter.ToSingle(temp, 0);
                                offset += 12;

                                temp = new byte[4];
                                Array.Copy(binContent, offset, temp, 0, temp.Length);
                                y_end = BitConverter.ToSingle(temp, 0);
                                offset += 12;

                                temp = new byte[4];
                                Array.Copy(binContent, offset, temp, 0, temp.Length);
                                texnum = BitConverter.ToInt32(temp, 0);
                                offset += 4;

                                texCollect.Add(new textureswork(texdata, x_start, y_start, x_end, y_end, texnum));
                            }

                            if (offset == poz) work = true;

                            if (work)
                            {
                                uint tex_offset = 0;
                                uint tex_size = 0;

                                string info = null;
                                int[] width = new int[counttextures];
                                int[] height = new int[counttextures];

                                for (int t = 0; t < counttextures; t++)
                                {
                                    temp = new byte[4];
                                    Array.Copy(binContent, offset, temp, 0, temp.Length);
                                    tex_offset = BitConverter.ToUInt32(temp, 0) + 16 + offset_info;
                                    offset += 4;

                                    temp = new byte[4];
                                    Array.Copy(binContent, offset, temp, 0, temp.Length);
                                    tex_size = BitConverter.ToUInt32(temp, 0);
                                    offset += 4;

                                    byte[] texture = new byte[tex_size];
                                    Array.Copy(binContent, tex_offset, texture, 0, texture.Length);

                                    if (File.Exists(MainForm.settings.outputpath + "\\" + Methods.GetFileNameOnly(fi[i].Name, fi[i].Extension) + "_" + (t + 1).ToString() + ".dds")) File.Delete(MainForm.settings.outputpath + "\\" + Methods.GetFileNameOnly(fi[i].Name, fi[i].Extension) + "_" + (t + 1).ToString() + ".dds");
                                    fs = new FileStream(MainForm.settings.outputpath + "\\" + Methods.GetFileNameOnly(fi[i].Name, fi[i].Extension) + "_" + (t + 1).ToString() + ".dds", FileMode.CreateNew);
                                    fs.Write(texture, 0, texture.Length);
                                    fs.Close();

                                    temp = new byte[4];
                                    Array.Copy(texture, 12, temp, 0, temp.Length);
                                    height[t] = BitConverter.ToInt32(temp, 0);

                                    temp = new byte[4];
                                    Array.Copy(texture, 16, temp, 0, temp.Length);
                                    width[t] = BitConverter.ToInt32(temp, 0);

                                    info += Methods.GetFileNameOnly(fi[i].Name, fi[i].Extension) + "_" + (t + 1).ToString() + ".dds\r\n";

                                    texture = null;
                                }

                                for (int l = 0; l < texCollect.Count; l++)
                                {
                                    info += "texdata\t" + texCollect[l].texdata.ToString()
                                        + "\tx_start\t" + (texCollect[l].x_start * width[texCollect[l].texnum])
                                        + "\ty_start\t" + (texCollect[l].y_start * height[texCollect[l].texnum])
                                        + "\tx_end\t" + (texCollect[l].x_end * width[texCollect[l].texnum])
                                        + "\ty_end\t" + (texCollect[l].y_end * height[texCollect[l].texnum])
                                        + "\tfontnum\t" + (texCollect[l].texnum + 1) + "\r\n";
                                }

                                info = info.Substring(0, (info.Length - 2));

                                StreamWriter sw = new StreamWriter(MainForm.settings.outputpath + "\\" + Methods.GetFileNameOnly(fi[i].Name, fi[i].Extension) + ".txt");
                                sw.Write(info);
                                sw.Close();

                                listBox1.Items.Add("File " + fi[i].Name + " exported");
                            }
                            else
                            {
                                listBox1.Items.Add("Error in file" + fi[i].Name + ". Please contact with me.");
                                binContent = null;
                            }
                        }
                        else binContent = null;
                    }
                }
            }
            else listBox1.Items.Add("Please check input/output paths in config file!");
        }

    }
}
