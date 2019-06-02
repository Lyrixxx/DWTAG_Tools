using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace DoctorWhoToolsWorking
{
    public delegate void SendMessage(string message);

    class Threads
    {
        public event SendMessage SendMes;

        byte[] allblock_length = new byte[4];
        byte[] block_length = new byte[4];
        byte[] binContent = new byte[0];
        byte[] bytes = new byte[4];

        List<string> str_collection = new List<string>();

        List<TextTool.text_struct> text_header = new List<TextTool.text_struct>();

        public void Add_head(byte[] _data_bytes, byte[] _string_index, int _index)
        {
            text_header.Add(new TextTool.text_struct(_data_bytes, _string_index, _index));
        }

        private void ReadDatFile(int poz, int offset, byte[] BinContent)
        {
            byte[] binCount = new byte[4];
            Array.Copy(BinContent, offset, binCount, 0, 4);

            int count = BitConverter.ToInt32(binCount, 0);
            offset += 4;

            for (int i = 0; i < count; i++)
            {
                byte[] nil = new byte[4];
                Add_head(nil, nil, i);

                text_header[i].data_bytes = new byte[4];
                Array.Copy(binContent, offset, text_header[i].data_bytes, 0, 4);
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

        public void ImportFiles(object parameters)
        {
            if (Directory.Exists(MainForm.settings.inputpath) && Directory.Exists(MainForm.settings.outputpath))
            {
                DirectoryInfo dir = new DirectoryInfo(MainForm.settings.inputpath);
                FileInfo[] fi;
                fi = dir.GetFiles();

                int poz, offset;

                for (int i = 0; i < fi.Length; i++)
                {
                    if (i + 1 < fi.Count())
                    {

                        if ((fi[i].Extension == ".dat" && fi[i + 1].Extension == ".txt") && Methods.GetFileNameOnly(fi[i].Name, fi[i].Extension) == Methods.GetFileNameOnly(fi[i + 1].Name, fi[i + 1].Extension))//GetNameOnly(i) == GetNameOnly(i + 1))
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
                                SendMes("The tool is busy by another process");
                                //MessageBox.Show("The tool is busy by another process", "Error");
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

                                    ReadDatFile(poz, offset, binContent);

                                    List<string> importString = new List<string>();

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
                                            importString[n_str] += enter + test[j];
                                            FindNum = false;
                                        }
                                    }

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

                                    List<TextTool.text_struct> new_text_header = new List<TextTool.text_struct>();
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
                                    uint moduleBlockSize = Methods.alt_pad_it((uint)result.Length);//Methods.pad_it((uint)result.Length, (uint)result.Length / 2);

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
                                    SendMes("File " + fi[i].Name + " imported.");
                                    //listBox1.Items.Add("File " + fi[i].Name + " imported.");

                                }
                                else
                                {
                                    SendMes("File is EMPTY!");
                                    //listBox1.Items.Add("File is EMPTY!");
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                SendMes("Please check input/output paths in config file!");
                //listBox1.Items.Add("Please check input/output paths in config file!");
            }
        }

        public void ExportFiles(object parameters)
        {
            if (Directory.Exists(MainForm.settings.inputpath) && Directory.Exists(MainForm.settings.outputpath))
            {
                DirectoryInfo di = new DirectoryInfo(MainForm.settings.inputpath);
                FileInfo[] fi;
                fi = di.GetFiles();

                int poz, offset;

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
                            read = false;
                            SendMes("The tool is busy by another process");
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

                                    SendMes("File " + fi[i].Name + " exported.");
                                }
                                catch
                                {
                                    SendMes("Error: ");
                                }
                            }
                            else
                            {
                                SendMes("File " + fi[i].Name + " is EMPTY!");
                            }
                        }
                    }
                }
            }
            else SendMes("Please check input/output paths in config file!");
        }
    }
}
