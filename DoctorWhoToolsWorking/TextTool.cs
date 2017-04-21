using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace DoctorWhoToolsWorking
{
    public partial class TextTool : Form
    {
        byte[] allblock_length = new byte[4];
        byte[] block_length = new byte[4];
        byte[] binContent = new byte[0];
        byte[] bytes = new byte[4];
        
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


        List<text_struct> text_header = new List<text_struct>();

        public void Add_head(byte[] _data_bytes, byte[] _string_index, int _index)
        {
            text_header.Add(new text_struct(_data_bytes, _string_index, _index));
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

        public static string GetNameOnly(int i)
        {
            return fi[i].Name.Substring(0, (fi[i].Name.Length - fi[i].Extension.Length));
        }


        uint pad_it(uint num, uint pad)
        {
            uint t;
            t = num % pad;

            if (Convert.ToBoolean(t)) num += pad - t;
            return (num);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            DirectoryInfo di = new DirectoryInfo(MainForm.settings.inputpath);
            if (di.Exists == true)
            {
                fi = di.GetFiles();
                for (int i = 0; i < fi.Length; i++)
                {
                    if (fi[i].Extension == ".dat")
                    {
                        offset = 0;
                        poz = 0;
                        text_header.Clear();
                        FileStream fs;
                        bool read;
                        try
                        {
                            fs = new FileStream(MainForm.settings.inputpath + "\\" + fi[i].Name, FileMode.Open);
                            binContent = Methods.ReadFull(fs);
                            fs.Close();
                            read = true;
                        }
                        catch
                        {
                            MessageBox.Show("Программа занята другим процессом", "Ошибка");
                            read = false;
                        }
                        if (read)
                        {
                            if (Methods.CheckHeader(binContent, offset, "TEXT") == true)
                            {
                                poz = 0;
                                offset = 0;
                                poz = Methods.FindStartOfStringSomething(binContent, offset, "TEXT");

                                offset = poz + 24;
                                poz += 16;
                                //Array.Copy(binContent, offset, str_counts, 0, 4);

                                ReadDatFile(poz, offset, binContent);


                                string newFileName = Methods.GetFileNameOnly(fi[i].Name, ".dat") + ".txt";
                                try
                                {
                                    fs = new FileStream(MainForm.settings.outputpath + "\\" + newFileName, FileMode.OpenOrCreate);
                                    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                                    int counter = 1;

                                    for (int j = str_collection.Count - 1; j >= 0; j--)
                                    {
                                        sw.WriteLine(counter + ")");
                                        sw.WriteLine(str_collection[j]);
                                        counter++;
                                    }

                                    sw.Close();
                                    fs.Close();

                                    text_header.Clear();
                                    str_collection.Clear();

                                    listBox1.Items.Add("File " + fi[i].Name + " exported.");
                                }
                                catch
                                {
                                    listBox1.Items.Add("Error: ");
                                }
                            }
                            else listBox1.Items.Add("File " + fi[i].Name + " is EMPTY!");
                        }
                    }

                }
            }
            else listBox1.Items.Add("Check paths in settings form!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DirectoryInfo dir = new DirectoryInfo(MainForm.settings.inputpath);
            fi = dir.GetFiles();

            for (int i = 0; i < fi.Length; i++)
            {
                if (i + 1 < fi.Count())
                {

                   if ((fi[i].Extension == ".dat" && fi[i + 1].Extension == ".txt") && GetNameOnly(i) == GetNameOnly(i + 1))
                   {

                       FileStream fs;
                       bool read;
                       offset = 0;
                       poz = 0;
                       try
                        {
                            fs = new FileStream(MainForm.settings.inputpath + "\\" + fi[i].Name, FileMode.Open);
                            binContent = Methods.ReadFull(fs);
                            fs.Close();
                            read = true;
                        }
                        catch
                        {
                            MessageBox.Show("Программа занята другим процессом", "Ошибка");
                            read = false;
                        }
                       if (read)
                       {
                           if (Methods.CheckHeader(binContent, offset, "TEXT") == true)
                           {
                               poz = 0;
                               offset = 0;
                               poz = Methods.FindStartOfStringSomething(binContent, offset, "TEXT");

                               byte[] NewHeader = new byte[28];
                               Array.Copy(binContent, poz, NewHeader, 0, 28);
                               byte[] new_binContent = new byte[poz];
                               Array.Copy(binContent, 0, new_binContent, 0, poz);
                               string enter = "\r\n";


                               offset = poz + 24;
                               poz += 16;
                               //Array.Copy(binContent, offset, str_counts, 0, 4);

                               ReadDatFile(poz, offset, binContent);

                               List<string> importString = new List<string>();

                               //StreamReader sr = new StreamReader(fi[i + 1].FullName, Encoding.UTF8);
                               //string curString;
                               bool FindNum = false;
                               int n_str = -1;
                               string[] test = File.ReadAllLines(fi[i + 1].FullName);
                               
                               for (int j = 0; j < test.Length; j++)
                               {
                                   if (test[j].IndexOf(")") > -1 && FindNum == false
                                       && Methods.IsNumber(test[j].Substring(0, test[j].IndexOf(")"))))
                                   {
                                           n_str++;
                                           importString.Add("");
                                           FindNum = true;
                                   }
                                   else
                                   {
                                       //string tempString = Methods.ConvertToLatin(test[j], MainForm.settings.WINcoding);
                                       importString[n_str] += enter + test[j];//tempString;
                                       FindNum = false;
                                   }
                               }

                                   /* while ((curString = sr.ReadLine()) != null)
                                    {
                                        if (curString.IndexOf(")") > -1 && FindNum == false)
                                        {
                                            if (Methods.IsNumber(curString.Substring(0, curString.IndexOf(")"))))
                                            {
                                                n_str++;
                                                importString.Add("");
                                                FindNum = true;
                                            }
                                            else
                                            {
                                                string tempString = Methods.ConvertToLatin(curString, MainForm.settings.WINcoding);
                                                importString[n_str] += enter + tempString;
                                                FindNum = false;
                                            }
                                        }
                                        else
                                        {
                                            string tempString = Methods.ConvertToLatin(curString, MainForm.settings.WINcoding);
                                            importString[n_str] += enter + tempString;
                                            FindNum = false;
                                        }
                                    }
                                    sr.Close();*/

                                   // listBox1.Items.Add("Old indexes");
                               
                               

                                   for (int j = 0; j < str_collection.Count; j++)
                                   {
                                       if (importString[j].Length > 0)
                                       {
                                           str_collection[str_collection.Count - j - 1] = importString[j].Substring(enter.Length, importString[j].Length - enter.Length);
                                           if (str_collection[str_collection.Count - j - 1].IndexOf("\r\n") > -1)
                                           {
                                               str_collection[str_collection.Count - j - 1] = str_collection[str_collection.Count - j - 1].Replace("\r\n", "\n");
                                           }
                                       }
                                       else
                                       {
                                           str_collection[str_collection.Count - j - 1] = importString[j];
                                           if (str_collection[str_collection.Count - j - 1].IndexOf("\r\n") > -1)
                                           {
                                               str_collection[str_collection.Count - j - 1] = str_collection[str_collection.Count - j - 1].Replace("\r\n", "\n");
                                           }
                                       }
                                   }

                                   List<text_struct> new_text_header = new List<text_struct>();
                                   new_text_header = text_header;

                                   for (int k = 0; k < new_text_header.Count(); k++)
                                   {
                                       for (int l = k + 1; l < new_text_header.Count(); l++)
                                       {
                                           if (BitConverter.ToInt32(new_text_header[l].string_index, 0) < BitConverter.ToInt32(new_text_header[k].string_index, 0))
                                           {
                                               byte[] temp_data = new byte[4];
                                               temp_data = new_text_header[k].data_bytes;

                                               byte[] temp_index = new byte[4];
                                               temp_index = new_text_header[k].string_index;

                                               string temp_string = str_collection[k];

                                               int temp_indx = new_text_header[k].index;

                                               new_text_header[k].data_bytes = new byte[4];
                                               new_text_header[k].data_bytes = new_text_header[l].data_bytes;

                                               new_text_header[k].string_index = new byte[4];
                                               new_text_header[k].string_index = new_text_header[l].string_index;

                                               str_collection[k] = str_collection[l];
                                               new_text_header[k].index = new_text_header[l].index;

                                               new_text_header[l].data_bytes = new byte[4];
                                               new_text_header[l].data_bytes = temp_data;

                                               new_text_header[l].string_index = new byte[4];
                                               new_text_header[l].string_index = temp_index;

                                               str_collection[l] = temp_string;
                                               new_text_header[l].index = temp_indx;
                                           }
                                       }
                                   }

                               
                                   offset = poz += 12;
                                   int tableLength = 8 * str_collection.Count;
                                   byte[] new_table = new byte[tableLength];
                                   //Array.Copy(binContent, offset, new_table, 0, tableLength);

                                       //fs.Write(new_table, 0, new_table.Length);

                                       for (int k = 0; k < str_collection.Count; k++)
                                       {
                                           if (k + 1 < str_collection.Count)
                                           {
                                               int index = BitConverter.ToInt32(new_text_header[k].string_index, 0);
                                               byte[] newstring = Encoding.UTF8.GetBytes(str_collection[k]);
                                               index += newstring.Length + 1;
                                               new_text_header[k + 1].string_index = new byte[4];
                                               new_text_header[k + 1].string_index = BitConverter.GetBytes(index);
                                           }
                                       }

                                   offset = 0;
                                   int str_index = 0; //Нужно больше костылей...

                                   while (str_index < str_collection.Count)
                                   {
                                       for (int f = 0; f < text_header.Count; f++)
                                       {
                                           if (text_header[f].index == str_index)
                                           {
                                               for (int j = new_text_header.Count - 1; j >= 0; j--)
                                               {
                                                   if ((new_text_header[j].index == text_header[f].index))
                                                   {
                                                       Array.Copy(new_text_header[j].data_bytes, 0, new_table, offset, 4);
                                                       offset += 4;
                                                       Array.Copy(new_text_header[j].string_index, 0, new_table, offset, 4);
                                                       offset += 4;
                                                       str_index++;
                                                       break;
                                                   }
                                               }
                                           }
                                       }
                                   }
                                   /*for (int f = 0; f < str_collection.Count; f++)
                                   {
                                       byte[] sampleData = new byte[4];
                                       Array.Copy(new_table, offset, sampleData, 0, 4);

                                       for (int g = 0; g < str_collection.Count; g++)
                                       {
                                           if (BitConverter.ToString(sampleData) == BitConverter.ToString(text_header[g].data_bytes))
                                           {
                                               offset += 4;
                                               Array.Copy(text_header[g].string_index, 0, new_table, offset, 4);
                                               offset += 4;
                                           }
                                       }
                                   }*/
                               

                                   byte[] emptyBytes = new byte[BitConverter.ToInt32(text_header[0].string_index, 0) - new_table.Length - 12];
                                   MemoryStream newTextBlock = new MemoryStream();
                                   newTextBlock.Write(NewHeader, 0, NewHeader.Length);
                                   newTextBlock.Write(new_table, 0, new_table.Length);
                                   newTextBlock.Write(emptyBytes, 0, emptyBytes.Length);

                                   for (int str = 0; str < str_collection.Count(); str++)
                                   {
                                       str_collection[str] += "\0";
                                       byte[] binString = new byte[str_collection[str].Length];
                                       binString = Encoding.UTF8.GetBytes(str_collection[str]);
                                       newTextBlock.Write(binString, 0, binString.Length);
                                   }
                                   byte[] result = newTextBlock.ToArray();
                                   newTextBlock.Close();
                                   uint textBlockSize = (uint)result.Length - 16;
                                   uint moduleBlockSize = pad_it((uint)result.Length, (uint)result.Length / 2);

                                   byte[] binModuleBlockSize = new byte[4];
                                   binModuleBlockSize = BitConverter.GetBytes(moduleBlockSize);
                                   Array.Copy(binModuleBlockSize, 0, result, 4, binModuleBlockSize.Length);

                                   byte[] binTextBlockSize = new byte[4];
                                   binTextBlockSize = BitConverter.GetBytes(textBlockSize);
                                   Array.Copy(binTextBlockSize, 0, result, 8, binTextBlockSize.Length);

                                   byte[] readyTextBlock = new byte[moduleBlockSize];
                                   Array.Copy(result, 0, readyTextBlock, 0, result.Length);

                                   fs = new FileStream(MainForm.settings.outputpath + "\\" + fi[i].Name, FileMode.Create);
                                   //MessageBox.Show("text Block: " + textBlockSize + "\r\nmodule block: " + moduleBlockSize);
                                   fs.Write(new_binContent, 0, new_binContent.Length);
                                   fs.Write(readyTextBlock, 0, readyTextBlock.Length);

                                   fs.Close();

                                   text_header.Clear();
                                   new_text_header.Clear();
                                   str_collection.Clear();
                                   importString.Clear();
                                   listBox1.Items.Add("File " + fi[i].Name + " imported.");
                               
                           }
                           else listBox1.Items.Add("File is EMPTY!");
                           }
                       }
                   }
               }
            }

        public void ReadDatFile(int poz, int offset, byte[] BinContent)
        {
            byte[] binCount = new byte[4];
            Array.Copy(binContent, offset, binCount, 0, 4);

            int count = BitConverter.ToInt32(binCount, 0);
            offset += 4;

            for (int i = 0; i < count; i++)
            {
                byte[] nil = new byte[4];
                Add_head(nil, nil, i);

                text_header[i].data_bytes = new byte[4];
                Array.Copy(binContent, offset, text_header[i].data_bytes, 0, 4);
                //MessageBox.Show("Шаг 1: " + BitConverter.ToString(text_header[0].data_bytes));
                offset += 4;

                text_header[i].string_index = new byte[4];
                Array.Copy(binContent, offset, text_header[i].string_index, 0, 4);
                offset += 4;

                int str_index = BitConverter.ToInt32(text_header[i].string_index, 0) + poz;
                byte temp_byte;
                int str_length = 0;

                bool reading = true;
                while (reading)
                {
                    temp_byte = BinContent[str_index];
                    str_index++;
                    str_length++;
                    if (temp_byte == 0x00)
                    {
                        str_length--;
                        str_index -= str_length + 1;
                        reading = false;
                    }
                }

                byte[] temp_bytes = new byte[str_length];
                Array.Copy(binContent, str_index, temp_bytes, 0, str_length);
                string temp_string = Methods.ConvertHexToString(temp_bytes, 0, str_length);
                temp_string = temp_string.Replace("\n", "\r\n");
                str_collection.Add(temp_string);
            }

            for (int k = 0; k < count; k++)
            {
                for (int l = 0; l < count - k - 1; l++)
                {
                    if (BitConverter.ToInt32(text_header[l].string_index, 0) < BitConverter.ToInt32(text_header[l + 1].string_index, 0))
                    {
                        byte[] tempData = text_header[l].data_bytes;
                        byte[] tempIndex = text_header[l].string_index;
                        string tempString = str_collection[l];
                        int temp_index = text_header[l].index;

                        text_header[l].data_bytes = text_header[l + 1].data_bytes;
                        text_header[l].string_index = text_header[l + 1].string_index;
                        str_collection[l] = str_collection[l + 1];
                        text_header[l].index = text_header[l + 1].index;

                        text_header[l + 1].data_bytes = tempData;
                        text_header[l + 1].string_index = tempIndex;
                        str_collection[l + 1] = tempString;
                        text_header[l + 1].index = temp_index;
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
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

                                if (File.Exists(MainForm.settings.outputpath + "\\" + GetNameOnly(i) + "_" + (t + 1).ToString() + ".dds")) File.Delete(MainForm.settings.outputpath + "\\" + GetNameOnly(i) + "_" + (t + 1).ToString() + ".dds");
                                fs = new FileStream(MainForm.settings.outputpath + "\\" + GetNameOnly(i) + "_" + (t + 1).ToString() + ".dds", FileMode.CreateNew);
                                fs.Write(texture, 0, texture.Length);
                                fs.Close();

                                temp = new byte[4];
                                Array.Copy(texture, 12, temp, 0, temp.Length);
                                height[t] = BitConverter.ToInt32(temp, 0);

                                temp = new byte[4];
                                Array.Copy(texture, 16, temp, 0, temp.Length);
                                width[t] = BitConverter.ToInt32(temp, 0);

                                info += GetNameOnly(i) + "_" + (t + 1).ToString() + ".dds\r\n";

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

                            StreamWriter sw = new StreamWriter(MainForm.settings.outputpath + "\\" + GetNameOnly(i) + ".txt");
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

    }
}