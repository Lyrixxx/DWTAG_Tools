using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DoctorWhoToolsWorking
{
    public partial class EpcViewer : Form
    {
        public EpcViewer()
        {
            InitializeComponent();
        }

        public List<listingFiles> ListFiles = new List<listingFiles>();
        public List<Textures> tex_files = new List<Textures>();
        public List<Tex_Headers> DDS_headers = new List<Tex_Headers>();
        string openfile = null;

        public class Tex_Headers
        {
            public int code;
            public byte[] header;

            public Tex_Headers() { }

            public Tex_Headers(int _code, byte[] _header)
            {
                this.code = _code;
                this.header = _header;
            }
        }

        public class Textures
        {
            public byte[] height;
            public byte[] width;
            public byte[] tex_length;
            public int tex_format;
            public byte[] tex_content;
            public string tex_name;

            public Textures() { }
            public Textures(byte[] _heigth, byte[] _width, byte[] _tex_length,
                int _tex_format, byte[] _tex_content, string _tex_name)
            {
                this.height = _heigth;
                this.width = _width;
                this.tex_length = _tex_length;
                this.tex_format = _tex_format;
                this.tex_content = _tex_content;
                this.tex_name = _tex_name;
            }
        }

        public class listingFiles
        {
            public byte[] header;
            public byte[] file_size;
            public byte[] file;

            public listingFiles() { }

            public listingFiles(byte[] _header, byte[] _file_size,
                byte[] _file)
            {
                this.header = _header;
                this.file_size = _file_size;
                this.file = _file;
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }



        private void openEpcFileepcToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Load Epc arhive!";
            ofd.Filter = "EPC files (*.epc) | *.epc";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tex_files.Clear();
                ListFiles.Clear();

                openfile = ofd.FileName;

                FileStream fs = new FileStream(openfile, FileMode.Open);
                byte[] archiveContent = Methods.ReadFull(fs);
                fs.Close();

                int fc = 0; //File Counter
                uint offset = 8;
                //uint file_size = 0;
                bool read = true;

                while (read)
                {

                    byte[] header = new byte[4];
                    Array.Copy(archiveContent, offset, header, 0, 4);
                    offset += 4;
                    byte[] f_size = new byte[4];
                    Array.Copy(archiveContent, offset, f_size, 0, 4);
                    offset -= 4;
                    byte[] file = new byte[BitConverter.ToInt32(f_size, 0)];
                    if (file.Length > archiveContent.Length - offset) file = new byte[archiveContent.Length - offset];
                    Array.Copy(archiveContent, offset, file, 0, file.Length);
                    offset += (uint)file.Length;

                    ListFiles.Add(new listingFiles(header, f_size, file));

                    if (Encoding.ASCII.GetString(header) == "EMTR")
                    {
                        int data_off = 107;
                        byte[] height = new byte[2];
                        byte[] width = new byte[2];
                        byte[] tex_type = new byte[2];
                        byte[] file_size = new byte[4];
                        
                        Array.Copy(file, data_off, tex_type, 0, 1);
                        int tex_code = BitConverter.ToInt16(tex_type, 0);
                        data_off++;
                        Array.Copy(file, data_off, width, 0, width.Length);
                        data_off += 2;
                        Array.Copy(file, data_off, height, 0, height.Length);
                        data_off += 6;
                        Array.Copy(file, data_off, file_size, 0, file_size.Length);
                        //data_off++;
                        /*byte[] counter = new byte[2];
                        Array.Copy(file, data_off, counter, 0, 1);
                        if(BitConverter.ToInt16(counter, 0) > 1)*/
                        data_off += 20;
                        byte[] content = new byte[BitConverter.ToInt32(file_size, 0)];
                        Array.Copy(file, data_off, content, 0, content.Length);
                        data_off += content.Length;

                        int str_length = 0;
                        int str_index = data_off;
                        byte[] get_file_name_string;
                        string get_string_name = "";

                        bool reading = true;
                        while (reading)
                        {
                            byte temp_byte = file[str_index];
                            str_length++;
                            str_index++;

                            if (temp_byte == 0x00)
                            {
                                str_length--;
                                str_index -= str_length + 1;
                                get_file_name_string = new byte[str_length];
                                Array.Copy(file, data_off, get_file_name_string, 0, str_length);
                                get_string_name = Encoding.ASCII.GetString(get_file_name_string);
                                reading = false;
                            }
                        }
                        tex_files.Add(new Textures(height, width, file_size, tex_code, content, get_string_name));
                    }
                    else
                    {
                        byte[] nil = new byte[1];

                        tex_files.Add(new Textures(nil, nil, nil, 0, nil, "not_texture"));
                    }

                    fc++;
                    
                    if (offset >= archiveContent.Length) read = false;
                }

                archiveContent = null;

                dataGridView1.RowCount = ListFiles.Count;

                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    dataGridView1[0, i].Value = i;
                    dataGridView1[1, i].Value = BitConverter.ToInt32(ListFiles[i].file_size, 0);

                    switch (Encoding.ASCII.GetString(ListFiles[i].header))
                    {
                        case "EMTR":
                            dataGridView1[2, i].Value = "Texture";
                            dataGridView1[3, i].Value = tex_files[i].tex_name;

                            if((tex_files[i].tex_format != 12) && (tex_files[i].tex_format != 16))
                            {
                                dataGridView1[4, i].Style.BackColor = System.Drawing.Color.YellowGreen;
                            }
                            dataGridView1[4, i].Value = tex_files[i].tex_format;
                            break;
                        default:
                            dataGridView1[2, i].Value = "Unknown format";
                            dataGridView1[3, i].Value = "";
                            break;
                    }
                }
                
                EpcViewer.ActiveForm.Text = "Epc viewer. Opened file " + ofd.FileName;

                extractToolStripMenuItem.Enabled = true;
                MenuStripUpload.Enabled = true;
                saveToolStripMenuItem.Enabled = true;
                saveAsToolStripMenuItem.Enabled = true;
            }
        }

        private void allFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                progressBar1.Minimum = 0;
                progressBar1.Maximum = ListFiles.Count - 1;

                for (int i = 0; i < ListFiles.Count; i++)
                {
                    FileStream fs = new FileStream(fbd.SelectedPath + "\\FileName" + i.ToString() + ".raw", FileMode.OpenOrCreate);
                    fs.Write(ListFiles[i].file, 0, ListFiles[i].file.Length);
                    fs.Close();

                    progressBar1.Value = i;
                }

                MessageBox.Show("All files extracted!", "Done");
            }
        }

        private void export_dds(byte[] width, byte[] height, int tex_type, string f_name, byte[] content, string path)
        {
            string error = null;
            for (int j = 0; j < DDS_headers.Count; j++)
            {
                if (tex_type == DDS_headers[j].code)
                {
                    byte[] header = new byte[DDS_headers[j].header.Length];
                    Array.Copy(DDS_headers[j].header, 0, header, 0, header.Length);
                    Array.Copy(height, 0, header, 12, height.Length);
                    Array.Copy(width, 0, header, 16, width.Length);

                    //int get_mip = 1;
                    int int_height = BitConverter.ToInt16(height, 0);
                    int int_width = BitConverter.ToInt16(width, 0);
                    int length = int_height * int_width;

                    if (DDS_headers[j].code == 0x0c) length = length / 2;

                    byte[] dds_length = new byte[4];
                    dds_length = BitConverter.GetBytes(length);
                    Array.Copy(dds_length, 0, header, 20, dds_length.Length);

                    /*while (length < tex_files[i].tex_content.Length)
                    {
                        heigth = heigth / 2;
                        width = width / 2;

                        int temp_length = heigth * width;
                        if (DDS_headers[j].code == 0x0c) temp_length = temp_length / 2;

                        length += temp_length;
                        get_mip++;

                        if (length >= tex_files[i].tex_content.Length)
                        {
                            break;
                        }
                    }*/

                    // byte[] mip_maps = new byte[2];
                    //mip_maps = BitConverter.GetBytes(get_mip);
                    //Array.Copy(mip_maps, 0, header, 28, mip_maps.Length);

                    string file_name = null;
                    if ((f_name.IndexOf('\\') > 0))
                    {
                        string[] temp_strings = f_name.Split('\\');
                        file_name = temp_strings[temp_strings.Length - 1];
                    }
                    else if ((f_name.IndexOf('/') > 0))
                    {
                        string[] temp_strings = f_name.Split('/');
                        file_name = temp_strings[temp_strings.Length - 1];
                    }
                    else file_name = f_name;

                    FileStream fs = new FileStream(path + "\\" + file_name + ".dds", FileMode.OpenOrCreate);
                    fs.Write(header, 0, header.Length);
                    fs.Write(content, 0, content.Length);
                    fs.Close();
                }
            }
        }

        private void texturesOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tex_files.Count > 0)
            {
                FolderBrowserDialog fdb = new FolderBrowserDialog();
                if (fdb.ShowDialog() == DialogResult.OK)
                {
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = tex_files.Count - 1;

                    for (int i = 0; i < tex_files.Count; i++)
                    {
                        string path = fdb.SelectedPath;
                        export_dds(tex_files[i].width, tex_files[i].height, tex_files[i].tex_format, tex_files[i].tex_name, tex_files[i].tex_content, path);
                        progressBar1.Value = i;
                    }

                    MessageBox.Show("Done.");
                }
            }
        }

        private void EpcViewer_Load(object sender, EventArgs e)
        {
            byte[] DXT5 = { 0x44, 0x44, 0x53, 0x20, 0x7C, 0x00, 0x00, 0x00,
                            0x07, 0x10, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,
                            0x04, 0x00, 0x00, 0x00, 0x44, 0x58, 0x54, 0x35,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            DDS_headers.Add(new Tex_Headers(0x10, DXT5));

            byte[] DXT1 = { 0x44, 0x44, 0x53, 0x20, 0x7C, 0x00, 0x00, 0x00,
                            0x07, 0x10, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,
                            0x04, 0x00, 0x00, 0x00, 0x44, 0x58, 0x54, 0x31,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            DDS_headers.Add(new Tex_Headers(0x0C, DXT1));

            extractToolStripMenuItem.Enabled = false;
            MenuStripUpload.Enabled = false;
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
        }

        private void EpcViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            DDS_headers.Clear();
            ListFiles.Clear();
            tex_files.Clear();
        }

        private void exportFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sel_file = dataGridView1.SelectedCells[0].RowIndex;
            

            if (Encoding.ASCII.GetString(ListFiles[sel_file].header) == "EMTR")
            {

                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Choose folder for saving chose textures. Texture name will add itself.";
                
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string path = fbd.SelectedPath;
                    export_dds(tex_files[sel_file].width, tex_files[sel_file].height, tex_files[sel_file].tex_format,
                    tex_files[sel_file].tex_name, tex_files[sel_file].tex_content, path);

                    MessageBox.Show("Texture extrated.", "Done");
                }
            }
            else
            {
                byte[] export_content = new byte[ListFiles[sel_file].file.Length];
                Array.Copy(ListFiles[sel_file].file, 0, export_content, 0, export_content.Length);
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Raw file (*.raw) | *.raw";
                sfd.FileName = "Raw_file_" + sel_file.ToString();

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.OpenOrCreate);
                    fs.Write(export_content, 0, export_content.Length);
                    fs.Close();

                    MessageBox.Show("File extracted.", "Done");
                }
            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridView1.Rows[e.RowIndex].Selected = true;
            }
            if (e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // получаем координаты
                Point pntCell = dataGridView1.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location;
                pntCell.X += e.Location.X;
                pntCell.Y += e.Location.Y;

                // вызываем менюшку
                MenuStripUpload.Show(dataGridView1, pntCell);
            }
        }

        private void uploadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int sel_file = dataGridView1.SelectedCells[0].RowIndex;

            if (Encoding.ASCII.GetString(ListFiles[sel_file].header) == "EMTR")
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "DDS file (*.dds) | *.dds";
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                    byte[] dds_content = Methods.ReadFull(fs);
                    fs.Close();

                    byte[] temp_content = new byte[dds_content.Length - 128];
                    Array.Copy(dds_content, 128, temp_content, 0, temp_content.Length);
                    
                    byte[] heigth = new byte[2];
                    byte[] width = new byte[2];

                    Array.Copy(dds_content, 12, heigth, 0, heigth.Length);
                    Array.Copy(dds_content, 16, width, 0, width.Length);

                    byte[] tex_type = new byte[4];
                    Array.Copy(dds_content, 0x54, tex_type, 0, 4);
                    if (Encoding.ASCII.GetString(tex_type) == "DXT5") tex_files[sel_file].tex_format = 0x10;
                    else if (Encoding.ASCII.GetString(tex_type) == "DXT1") tex_files[sel_file].tex_format = 0x0C;

                    if (temp_content.Length != tex_files[sel_file].tex_content.Length)
                    {
                        MessageBox.Show("Sizes of textures don't match.", "We're sorry.");
                    }
                    else
                    {

                        byte[] bin_tex = BitConverter.GetBytes(tex_files[sel_file].tex_format);

                        tex_files[sel_file].width = width;
                        tex_files[sel_file].height = heigth;
                        tex_files[sel_file].tex_content = temp_content;

                        Array.Copy(bin_tex, 0, ListFiles[sel_file].file, 0x6B, 1);
                        Array.Copy(width, 0, ListFiles[sel_file].file, 0x6C, 2);
                        Array.Copy(heigth, 0, ListFiles[sel_file].file, 0x6E, 2);
                        Array.Copy(tex_files[sel_file].tex_content, 0, ListFiles[sel_file].file, 0x88, tex_files[sel_file].tex_content.Length);
                    }
                }
            }
            else
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "RAW (*.raw) | *.raw";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    FileStream fs = new FileStream(ofd.FileName, FileMode.Open);
                    byte[] temp_content = Methods.ReadFull(fs);
                    fs.Close();


                    ListFiles[sel_file].file = temp_content;
                    byte[] header = new byte[4];
                    Array.Copy(temp_content, 0, header, 0, 4);
                    ListFiles[sel_file].header = header;
                    ListFiles[sel_file].file_size = new byte[4];
                    ListFiles[sel_file].file_size = BitConverter.GetBytes(temp_content.Length);

                    dataGridView1[1, sel_file].Value = BitConverter.ToInt32(ListFiles[sel_file].file_size, 0);
                }
            }
        }

        public void SaveEpcArchive(FileStream fs)
        {
            byte[] empty_block = new byte[8];
            fs.Write(empty_block, 0, empty_block.Length);

            for (int i = 0; i < ListFiles.Count; i++)
            {
                fs.Write(ListFiles[i].file, 0, ListFiles[i].file.Length);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "EPC archive (*.epc) | *.epc";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(sfd.FileName, FileMode.OpenOrCreate);
                SaveEpcArchive(fs);
                fs.Close();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(openfile))
            {
                FileStream fs = new FileStream(openfile, FileMode.OpenOrCreate);
                SaveEpcArchive(fs);
                fs.Close();
            }
        }
    }
}
