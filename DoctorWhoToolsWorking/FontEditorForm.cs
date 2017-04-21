using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DoctorWhoToolsWorking
{
    public partial class FontEditorForm : Form
    {
        byte[] binContent = new byte[0];
        byte[] relContent = new byte[0];
        int offset, poz, dds_count, links_count;
        int count_fonts;
        bool edited, read, rel_exists, tex_count_changed;
        string font_path, rel_path;

        OpenFileDialog OpenFont = new OpenFileDialog();
        public FontEditorForm()
        {
            InitializeComponent();
        }

        public static class fnt_struct
        {
            public struct coords
            {
                public short char_id; //Символ
                public short x_start; //X
                public short y_start; //Y
                public short width; //Ширина
                public short height; //Высота
                public short x_offset; //Смещение по X
                public short y_offset; //Смещение по Y
                public short x_advance; //Параметр для X
                public int page; //Номер текстуры
                public bool visable; //Видимость символа
                public int font_no; //Номер шрифта
            }

            public struct kern
            {
                public short first_ch;
                public short second_ch;
                public int amount;
                public int font_no;
            }

            public struct texture_names
            {
                public string FileName;
                public int page_id;
                public int tex_num;
                public int font_num;
                public bool need_load;
            }

            public static bool TextureLoaded(ref Font_Structure newffs, List<fnt_struct.texture_names> temp_tex, string GetPath, uint offset)
            {
                for (int c = 0; c < temp_tex.Count; c++)
                {
                    if (File.Exists(GetPath + "\\" + temp_tex[c].FileName) && (temp_tex[c].need_load))
                    {
                        FileStream fs = new FileStream(GetPath + "\\" + temp_tex[c].FileName, FileMode.Open);
                        byte[] bin = Methods.ReadFull(fs);
                        fs.Close();

                        byte[] b_height = new byte[4];
                        byte[] b_width = new byte[4];
                        byte[] b_length = new byte[4];
                        Array.Copy(bin, 12, b_height, 0, b_height.Length);
                        Array.Copy(bin, 16, b_width, 0, b_width.Length);
                        Array.Copy(bin, 20, b_length, 0, b_length.Length);

                        if (BitConverter.ToInt32(b_length, 0) != bin.Length)
                        {
                            b_length = new byte[4];
                            b_length = BitConverter.GetBytes(bin.Length);
                        }

                        byte[] b_offset = new byte[4];
                        b_offset = BitConverter.GetBytes(offset);
                        newffs.dds_add(b_offset, b_length, b_height, b_width, bin);
                        offset += (uint)bin.Length;
                    }
                    else if (!File.Exists(GetPath + "\\" + temp_tex[c].FileName))
                    {
                        MessageBox.Show("Проверьте все файлы, прежде чем заменять шрифты!", "Ошибка");
                        return false;
                    }
                }

                return true;
            }

            private static bool AllRight(string[] strings, int font_count)
            {
                int check_font = 0;
                for (int i = 0; i < strings.Length; i++)
                {
                    if (strings[i].IndexOf("info face") >= 0) check_font++;
                }

                if (check_font == font_count) return true;
                else return false;
            }

            public static void GetData(string[] strings, int font_count, ref List<coords> coord, ref List<kern> kerns, ref int[] coords_count, ref int[] kern_count, ref List<texture_names> Textures)
            {
                if (AllRight(strings, font_count))
                {
                    coords temp = new coords();
                    kern temp2 = new kern();

                    int x_right = 0;
                    int x_left = 0;
                    int y_up = 0;
                    int y_down = 0;
                    int x_spacing = 0;
                    int y_spacing = 0;
                    int line_height = 0;
                    int page = 0;
                    string file_name = null;
                    //int kern_pair_count = 0;
                    //int coords_count = 0;
                    int font_c = 0;
                    coords_count = new int[font_count];
                    kern_count = new int[font_count];

                    int check_coords_count = 1;
                    int check_kern_count = 1;

                    for (int n = 0; n < strings.Length; n++) //Убираем дебильные кавычки и скобки от xml файла
                    {
                        if ((strings[n].IndexOf('<') >= 0) || (strings[n].IndexOf('<') >= 0 && strings[n].IndexOf('/') > 0))
                        {
                            strings[n] = strings[n].Remove(strings[n].IndexOf('<'), 1);
                            if (strings[n].IndexOf('/') >= 0) strings[n] = strings[n].Remove(strings[n].IndexOf('/'), 1);
                        }
                        if (strings[n].IndexOf('>') >= 0 || (strings[n].IndexOf('/') >= 0 && strings[n + 1].IndexOf('>') > 0))
                        {
                            strings[n] = strings[n].Remove(strings[n].IndexOf('>'), 1);
                            if (strings[n].IndexOf('/') >= 0) strings[n] = strings[n].Remove(strings[n].IndexOf('/'), 1);
                        }
                        if (strings[n].IndexOf('"') >= 0)
                        {
                            while (strings[n].IndexOf('"') >= 0) strings[n] = strings[n].Remove(strings[n].IndexOf('"'), 1);
                        }
                    }

                    for (int m = 0; m < strings.Length; m++)
                    {
                        if ((strings[m].IndexOf("info face") >= 0)
                            && (strings[m].IndexOf("padding") > 0)
                             && (strings[m].IndexOf("spacing") > 0))
                        {
                            check_coords_count = 0;
                            check_kern_count = 0;
                            coords_count[font_c] = check_coords_count;
                            kern_count[font_c] = check_kern_count;
                            font_c++;


                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            int ind = 0;
                            int ind2 = 0;

                            for (int t = 0; t < splitted.Length; t++)
                            {
                                if (splitted[t] == "padding")
                                {
                                    ind = t + 1;
                                    //break;
                                }
                                if (splitted[t] == "spacing")
                                {
                                    ind2 = t + 1;
                                }
                            }

                            x_right = Convert.ToInt32(splitted[ind]);
                            x_left = Convert.ToInt32(splitted[ind + 2]);
                            y_up = Convert.ToInt32(splitted[ind + 1]);
                            y_down = Convert.ToInt32(splitted[ind + 3]);

                            x_spacing = Convert.ToInt32(splitted[ind2]);
                            y_spacing = Convert.ToInt32(splitted[ind2 + 1]);
                        }

                        if ((strings[m].IndexOf("lineHeight") > 0))
                        {
                            string[] par = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            int indx = 0;

                            for (int t = 0; t < par.Length; t++)
                            {
                                if (par[t] == "lineHeight")
                                {
                                    indx = t + 1;
                                    break;
                                }
                            }

                            line_height = Convert.ToInt32(par[indx]);
                        }

                        /*if(strings[m].IndexOf("chars count") >= 0)
                        {
                            string[] par = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            for(int t = 0; t < par.Length; t++)
                            {
                                if (par[t] == "count")
                                {
                                    coords_count = Convert.ToInt32(par[t + 1]);
                                }
                            }
                        }*/

                        if(strings[m].IndexOf("page id") >= 0)
                        {
                            string[] par = strings[m].Split(new char[] { ' ', '=', ',', '\"' });

                            texture_names temp3 = new texture_names();
                            bool diff_tex = true;

                            for (int t = 0; t < par.Length; t++)
                            {
                                if(par[t] == "id")
                                {
                                    temp3.page_id = Convert.ToInt32(par[t + 1]);
                                    temp3.tex_num = page;
                                }
                                if(par[t] == "file")
                                {
                                    string[] temp_s = par[t + 1].Split('.');

                                    string t_s = temp_s[0] + ".dds";

                                    if(temp_s.Length > 2)
                                    {
                                        t_s = null;
                                        for(int d = 0; d < temp_s.Length - 1; d++)
                                        {
                                            t_s += temp_s[d];
                                            if (d < temp_s.Length - 2) t_s += ".";
                                        }
                                        t_s += ".dds";
                                    }

                                    temp3.FileName = t_s;
                                    temp3.font_num = font_c;
                                    temp3.need_load = true;

                                    for(int v = 0; v < Textures.Count; v++)
                                    {
                                        if(temp3.FileName == Textures[v].FileName)
                                        {
                                            temp3.tex_num = Textures[v].tex_num;
                                            diff_tex = false;
                                            temp3.need_load = false;
                                        }
                                    }

                                   if(diff_tex) page++;
                                } 
                            }

                            Textures.Add(temp3);
                        }

                        if (strings[m].IndexOf("char id") >= 0)
                        {
                            string[] par = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            bool hasnt_id = true;

                            for (int t = 0; t < par.Length; t++)
                            {
                                if (t <= par.Length - 1)
                                {
                                    if (par[t] == "id")
                                    {
                                        temp.char_id = Convert.ToInt16(par[t + 1]);
                                    }
                                    if (par[t] == "x")
                                    {
                                        temp.x_start = Convert.ToInt16(par[t + 1]);
                                    }
                                    if (par[t] == "y")
                                    {
                                        temp.y_start = Convert.ToInt16(par[t + 1]);
                                    }
                                    if (par[t] == "width")
                                    {
                                        temp.width = Convert.ToInt16(par[t + 1]);
                                    }
                                    if (par[t] == "height")
                                    {
                                        temp.height = Convert.ToInt16(par[t + 1]);
                                    }
                                    if (par[t] == "xoffset")
                                    {
                                        temp.x_offset = Convert.ToInt16(par[t + 1]);
                                        temp.x_offset += (short)(x_spacing + (x_right + x_left));
                                    }
                                    if (par[t] == "yoffset")
                                    {
                                        temp.y_offset = (short)line_height;
                                        temp.y_offset -= Convert.ToInt16(par[t + 1]);
                                        temp.y_offset += (short)(y_spacing + (y_up + y_down));
                                    }
                                    if (par[t] == "xadvance")
                                    {
                                        temp.x_advance = Convert.ToInt16(par[t + 1]);
                                    }
                                    if(par[t] == "page")
                                    {
                                        int temp_p = Convert.ToInt32(par[t + 1]);
                                        for(int r = 0; r < Textures.Count; r++)
                                        {
                                            if(Textures[r].font_num == font_c && temp_p == Textures[r].page_id)
                                            {
                                                temp.page = Textures[r].tex_num;
                                            }
                                        }
                                    }
                                    if (par[t] == "chnl")
                                    {
                                        if (Convert.ToInt16(par[t + 1]) != 15) temp.visable = false;
                                        else temp.visable = true;
                                    }
                                }
                            }

                            temp.font_no = font_c;

                            if (coord.Count != 0)
                            {
                                for (int c = 0; c < coord.Count; c++)
                                {
                                    if ((coord[c].char_id == temp.char_id) && (coord[c].font_no == temp.font_no)) hasnt_id = false;
                                }

                                if (hasnt_id)
                                {
                                    coord.Add(temp);
                                    check_coords_count++;
                                    coords_count[font_c - 1] = check_coords_count;
                                }
                            }
                            else
                            {
                                coord.Add(temp);
                                check_coords_count++;
                                coords_count[font_c - 1] = check_coords_count;
                            }
                        }

                        if (strings[m].IndexOf("kerning first") >= 0)
                        {
                            string[] par = strings[m].Split(new char[] { ' ', '=', '\"', ',' });

                            for (int c = 0; c < par.Length; c++)
                            {
                                if (par[c] == "first")
                                {
                                    temp2.first_ch = Convert.ToInt16(par[c + 1]);
                                }
                                if (par[c] == "second")
                                {
                                    temp2.second_ch = Convert.ToInt16(par[c + 1]);
                                }
                                if (par[c] == "amount")
                                {
                                    temp2.amount = Convert.ToInt16(par[c + 1]);
                                }
                            }

                            temp2.font_no = font_c;

                            kerns.Add(temp2);
                            check_kern_count++;
                            kern_count[font_c - 1] = check_kern_count;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Your file isn't correct!", "Error");
                }
            }
        }

        public class header_textures //Данные о нескольких текстурах
        {
            
            public byte[] tex_data; //Данные текстур, которые прописаны в координатах
            public byte[] x_start; //Какие-то нулевые значения
            public byte[] y_start; //Начало блока с шрифтами (Обычно это используется в мультишрифтах, которые нарисованы на 1 текстуре)
            public byte[] x_end; //Непонятные данные. Что они означают, так и не понял.
            public byte[] y_end; //Конец блока с шрифтами
            public byte[] tex_num; //Номер текстуры
            public int font_num; //Костыль для работы с нужными шрифтами
            public header_textures() { }
            public header_textures(byte[] _tex_data, byte[] _x_start, byte[] _y_start,
                byte[] _x_end, byte[] _y_end, byte[] _tex_num, int _font_num)
            {
                this.tex_data = _tex_data;
                this.x_start = _x_start;
                this.y_start = _y_start;
                this.x_end = _x_end;
                this.y_end = _y_end;
                this.tex_num = _tex_num;
                this.font_num = _font_num;
            }

            ~header_textures(){ }
        }

        public class header_fonts //Данные о нескольких шрифтах
        {
            public byte[] count_fonts; //Количество шрифтов
            public byte[] fonts_data; //4 байта данных о шрифтах
            public byte[] count_symbols; //количество символов в шрифте
            public byte[] count_kernings; //количество кернинга
            public byte[] sym_offset; //Смещение таблицы символов
            public byte[] kern_offset; //Смещение таблицы кернинга
            public uint kern_off; //Костыль для записи парного кернинга в файл
            public uint sym_off;

            public header_fonts(byte[] _count_fonts, byte[] _fonts_data, byte[] _count_symbols,
                byte[] _count_kernings, byte[] _sym_offset, byte[] _kern_offset, uint _kern_off, uint _sym_off)
            {
                this.count_fonts = _count_fonts;
                this.fonts_data = _fonts_data;
                this.count_symbols = _count_symbols;
                this.count_kernings = _count_kernings;
                this.sym_offset = _sym_offset;
                this.kern_offset = _kern_offset;
                this.kern_off = _kern_off;
                this.sym_off = _sym_off;
            }

            public header_fonts(header_fonts ob)
            {
                this.count_fonts = ob.count_fonts;
                this.fonts_data = ob.fonts_data;
                this.count_symbols = ob.count_symbols;
                this.count_kernings = ob.count_kernings;
                this.sym_offset = ob.sym_offset;
                this.kern_offset = ob.kern_offset;
                this.kern_off = ob.kern_off;
            }

            ~header_fonts() { }
        }

        public class Font_Structure //Структура шрифта
        {
            public byte[] links_count; //количество связей между текстурами и координатами
            public byte[] font_count; //количество шрифтов
            public byte[] tex_count; //количество текстур
            public byte[] tex_block; //размер блока с текстурами
            public byte[] tex_part; //размер куска блока с текстурами (От заголовка PTEX)
            public byte[] coord_block; //размер блока с координатами
            public byte[] coord_part; //размер куска блока с координатами (От заголовка FONT)
            public byte[] old_data; //старые данные для сравнения и правильной записи новых данных.

            //public List<byte[]> font_data = new List<byte[]>();
            //public List<byte[]> tex_num = new List<byte[]>();
            public List<dds> texture = new List<dds>();
            public List<header_fonts> font_head = new List<header_fonts>();
            public List<header_textures> tex_head = new List<header_textures>();
            public List<coordinates> font_coord = new List<coordinates>();
            public List<kernings> font_kern = new List<kernings>();

            public Font_Structure() { }
            public Font_Structure(byte[] _tex_block, byte[] _tex_part, byte[] _coord_block,
                byte[] _coord_part) {
                    this.tex_block = _tex_block;
                    this.tex_part = _tex_part;
                    this.coord_block = _coord_block;
                    this.coord_part = _coord_block;
            }

            public Font_Structure Copy()
            {
                Font_Structure other = (Font_Structure)this.MemberwiseClone();
                other.texture = new List<dds>();
                other.font_coord = new List<coordinates>();
                other.font_kern = new List<kernings>();

                return other;
            }

            public void texhead_add(byte[] _tex_data, byte[] _x_start, byte[] _y_start,
                byte[] _x_end, byte[] _y_end, byte[] _tex_num, int _font_num) //функция по добавлению заголовка текстур
            {
                tex_head.Add(new header_textures(_tex_data, _x_start, _y_start, _x_end, _y_end, _tex_num, _font_num));
            }

            public void dds_add(byte[] _tex_offset, byte[] _tex_length, byte[] _dds_height, byte[] _dds_width, //функция под добавлению текстур в список
                byte[] _dds_content)
            {
                texture.Add(new dds(_tex_offset, _tex_length, _dds_height, _dds_width, _dds_content));
            }

            public void head_add(byte[] _count_fonts, byte[] _texture_data, byte[] _count_symbols, 
                byte[] _count_kernings, byte[] _sym_offset, byte[] _kern_offset, uint _kern_off, uint _sym_off) //функция по добавлению заголовка шрифта
            {
                font_head.Add(new header_fonts(_count_fonts, _texture_data, _count_symbols,
                    _count_kernings, _sym_offset, _kern_offset, _kern_off, _sym_off));
            }

            public void coord_add(byte[] _texture_data, byte[] _symbol,
                byte[] _x_start, byte[] _y_start, byte[] _coord_width, byte[] _coord_height,
                byte[] _x_advanced, byte[] _x_offset, byte[] _y_offset,
                byte[] _last_unknown_data, int _index, bool _new_coordinates, byte[] _font_data) //функция по добавлению координат в список
            {
                font_coord.Add(new coordinates(_texture_data, _symbol, _x_start, _y_start,
                _coord_width, _coord_height, _x_advanced, _x_offset, _y_offset, _last_unknown_data, _index, _new_coordinates, _font_data));
            }

            public void kern_add(byte[] _first_char, byte[] _second_char, byte[] _amount,
                int _index, bool _new_kernings, byte[] _font_data)
            {
                font_kern.Add(new kernings(_first_char, _second_char, _amount, _index, _new_kernings, _font_data));
            }

            ~Font_Structure() { }
        }

        public class kernings
        {
            public byte[] first_char;
            public byte[] second_char;
            public byte[] amount;
            public byte[] font_data;
            public int kern_index; //индекс к данным о кернинге
            public bool new_kernings;

            public kernings() { }
            public kernings(byte[] _first_char,
                byte[] _second_char, byte[] _amount, int _kern_index, bool _new_kernings, byte[] _font_data) {
                    this.first_char = _first_char;
                    this.second_char = _second_char;
                    this.amount = _amount;
                    this.kern_index = _kern_index;
                    this.new_kernings = _new_kernings;
                    this.font_data = _font_data;
            }

            ~kernings() { }
        }

        public class coordinates
        {
            public byte[] texture_data;
            public byte[] symbol;
            public byte[] x_start;
            public byte[] y_start;
            public byte[] coord_width;
            public byte[] coord_height;
            public byte[] x_advanced;
            public byte[] x_offset;
            public byte[] y_offset;
            public byte[] last_unknown_data;
            public int index;
            public bool new_coordinates;
            public byte[] font_data; //для точного сохранения координат

            public coordinates() { }
            public coordinates(byte[] _texture_data, byte[] _symbol,
                byte[] _x_start, byte[] _y_start, byte[] _coord_width, byte[] _coord_height,
                byte[] _x_advanced, byte[] _x_offset, byte[] _y_offset, 
                byte[] _last_unknown_data, int _index, bool _new_coordinates, byte[] _font_data) {
                    this.texture_data = _texture_data;
                    this.symbol = _symbol;
                    this.x_start = _x_start;
                    this.y_start = _y_start;
                    this.coord_width = _coord_width;
                    this.coord_height = _coord_height;
                    this.x_advanced = _x_advanced;
                    this.x_offset = _x_offset;
                    this.y_offset = _y_offset;
                    this.last_unknown_data = _last_unknown_data;
                    this.index = _index;
                    this.new_coordinates = _new_coordinates;
                    this.font_data = _font_data;
                }

            ~coordinates() { }
        }

        public class dds
        {
            public byte[] tex_offset; //Смещение текстуры
            public byte[] tex_length; //Длина текстуры
            public byte[] dds_height; //высота текстур
            public byte[] dds_width; //ширина текстур
            public byte[] dds_content; //Содержимое текстуры           

            public dds() { }
            public dds(byte[] _tex_offset, byte[] _tex_length, byte[] _dds_height, byte[] _dds_width,
                 byte[] _dds_content)
            {
                this.tex_offset = _tex_offset;
                this.tex_length = _tex_length;
                this.dds_height = _dds_height;
                this.dds_width = _dds_width;
                this.dds_content = _dds_content;
            }

            ~dds() { }
        }

        Font_Structure ffs = new Font_Structure();

        public static void ResortTable(ref Font_Structure newffs)
        {
            for (int k = 0; k < newffs.font_coord.Count; k++)
            {
                for (int m = k - 1; m >= 0; m--)
                {
                    if (BitConverter.ToUInt16(newffs.font_coord[m].symbol, 0) >
                        BitConverter.ToUInt16(newffs.font_coord[m + 1].symbol, 0)
                        && newffs.font_coord[m].index - 1 == newffs.font_coord[m + 1].index - 1)
                    {
                        byte[] temp_texture_data = newffs.font_coord[m].texture_data;
                        byte[] temp_symbol = newffs.font_coord[m].symbol;
                        byte[] temp_x_start = newffs.font_coord[m].x_start;
                        byte[] temp_y_start = newffs.font_coord[m].y_start;
                        byte[] temp_coord_width = newffs.font_coord[m].coord_width;
                        byte[] temp_coord_height = newffs.font_coord[m].coord_height;
                        byte[] temp_x_advanced = newffs.font_coord[m].x_advanced;
                        byte[] temp_x_offset = newffs.font_coord[m].x_offset;
                        byte[] temp_y_offset = newffs.font_coord[m].y_offset;
                        byte[] temp_last_unknown_data = newffs.font_coord[m].last_unknown_data;
                        int temp_index = newffs.font_coord[m].index;
                        byte[] temp_font_data = newffs.font_coord[m].font_data;
                        bool temp_new_coordinates = newffs.font_coord[m].new_coordinates;

                        newffs.font_coord[m].texture_data = newffs.font_coord[m + 1].texture_data;
                        newffs.font_coord[m].symbol = newffs.font_coord[m + 1].symbol;
                        newffs.font_coord[m].x_start = newffs.font_coord[m + 1].x_start;
                        newffs.font_coord[m].y_start = newffs.font_coord[m + 1].y_start;
                        newffs.font_coord[m].coord_width = newffs.font_coord[m + 1].coord_width;
                        newffs.font_coord[m].coord_height = newffs.font_coord[m + 1].coord_height;
                        newffs.font_coord[m].x_advanced = newffs.font_coord[m + 1].x_advanced;
                        newffs.font_coord[m].x_offset = newffs.font_coord[m + 1].x_offset;
                        newffs.font_coord[m].y_offset = newffs.font_coord[m + 1].y_offset;
                        newffs.font_coord[m].last_unknown_data = newffs.font_coord[m + 1].last_unknown_data;
                        newffs.font_coord[m].index = newffs.font_coord[m + 1].index;
                        newffs.font_coord[m].font_data = newffs.font_coord[m + 1].font_data;
                        newffs.font_coord[m].new_coordinates = newffs.font_coord[m + 1].new_coordinates;

                        newffs.font_coord[m + 1].texture_data = temp_texture_data;
                        newffs.font_coord[m + 1].symbol = temp_symbol;
                        newffs.font_coord[m + 1].x_start = temp_x_start;
                        newffs.font_coord[m + 1].y_start = temp_y_start;
                        newffs.font_coord[m + 1].coord_width = temp_coord_width;
                        newffs.font_coord[m + 1].coord_height = temp_coord_height;
                        newffs.font_coord[m + 1].x_advanced = temp_x_advanced;
                        newffs.font_coord[m + 1].x_offset = temp_x_offset;
                        newffs.font_coord[m + 1].y_offset = temp_y_offset;
                        newffs.font_coord[m + 1].last_unknown_data = temp_last_unknown_data;
                        newffs.font_coord[m + 1].index = temp_index;
                        newffs.font_coord[m + 1].font_data = temp_font_data;
                        newffs.font_coord[m + 1].new_coordinates = temp_new_coordinates;
                    }
                }
            }
        }


        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }

        uint pad_it(uint num, uint pad)
        {
            uint t;
            t = num % pad;

            if (Convert.ToBoolean(t)) num += pad - t;
            return (num);
        }

        private void SaveRelFile(int count_tex, int links_count, byte[] content, string path)
        {
            uint texture_off = 12 + (uint)(40 * links_count);

            if(Methods.FindStartOfStringSomething(content, 0, "RELO") == 0 && Methods.FindStartOfStringSomething(content, 15, "PTEX") == 16)
            {
                uint size_part = 12 + 8 + (uint)(count_tex * 8);
                uint block_size = pad_it(size_part + 16, 16);
                byte[] binSize_part = new byte[4];
                byte[] binBlock_size = new byte[4];

                binSize_part = BitConverter.GetBytes(size_part);
                binBlock_size = BitConverter.GetBytes(block_size);


                byte[] temp = new byte[block_size];
                Array.Copy(content, 0, temp, 0, 28);
                Array.Copy(binBlock_size, 0, temp, 4, binBlock_size.Length);
                Array.Copy(binSize_part, 0, temp, 8, binSize_part.Length);

                uint offset = 28;

                for(int i = 0; i < count_tex; i++)
                {
                    byte[] bin_one = new byte[4];
                    bin_one = BitConverter.GetBytes(1);

                    byte[] tex_off = new byte[4];
                    tex_off = BitConverter.GetBytes(texture_off);

                    Array.Copy(bin_one, 0, temp, offset, bin_one.Length);
                    offset += 4;
                    Array.Copy(tex_off, 0, temp, offset, tex_off.Length);
                    offset += 4;

                    texture_off += 8;
                }

                int index = Methods.FindStartOfStringSomething(content, 28, "RELO");

                if (File.Exists(rel_path)) File.Delete(rel_path);

                FileStream fs = new FileStream(rel_path, FileMode.CreateNew);
                fs.Write(temp, 0, temp.Length);

                temp = new byte[content.Length - index];
                Array.Copy(content, index, temp, 0, temp.Length);
                fs.Write(temp, 0, temp.Length);
                fs.Close();
            }
            else
            {
                MessageBox.Show("Incorrect file! Please check file or change file via HEX-editor.", "Error");
            }
        }

        public void SaveFont(byte[] binContent, string font_path)
        {
            FileStream fs;
            fs = new FileStream(font_path, FileMode.Create);

            int poz = 32;

            if (edited == false)
            {
                for (int i = 0; i < ffs.font_kern.Count; i++)
                {
                    ffs.font_kern[i].new_kernings = true;
                }

                for (int j = 0; j < ffs.font_coord.Count; j++)
                {
                    ffs.font_coord[j].new_coordinates = true;
                }
            }

            int link_length = BitConverter.ToInt32(ffs.links_count, 0) * 40;
            int tex_info_length = BitConverter.ToInt32(ffs.tex_count, 0) * 8;
            //byte[] temptexture = new byte[tex_info_length];

            byte[] tempblock = new byte[28 + link_length + tex_info_length];

            int offset_tex = link_length + 12;
            int offset_tex1 = offset_tex + tex_info_length;

            for (int k = 0; k < ffs.texture.Count; k++)
            {
                ffs.texture[k].tex_offset = new byte[4];
                ffs.texture[k].tex_offset = BitConverter.GetBytes(offset_tex1);
                offset_tex1 += BitConverter.ToInt32(ffs.texture[k].tex_length, 0);
            }

            poz -= 4;

            Array.Copy(binContent, 0, tempblock, 0, poz);

            int offset = poz;

            for (int j = 0; j < ffs.tex_head.Count; j++)
            {
                Array.Copy(ffs.tex_head[j].tex_data, 0, tempblock, offset, ffs.tex_head[j].tex_data.Length);
                offset += 4;
                Array.Copy(ffs.tex_head[j].x_start, 0, tempblock, offset, ffs.tex_head[j].x_start.Length);
                offset += 4;
                Array.Copy(ffs.tex_head[j].y_start, 0, tempblock, offset, ffs.tex_head[j].y_start.Length);
                offset += 4;
                Array.Copy(ffs.tex_head[j].x_end, 0, tempblock, offset, ffs.tex_head[j].x_end.Length);
                offset += 4;
                Array.Copy(ffs.tex_head[j].y_start, 0, tempblock, offset, ffs.tex_head[j].y_start.Length);
                offset += 4;
                Array.Copy(ffs.tex_head[j].x_end, 0, tempblock, offset, ffs.tex_head[j].x_end.Length);
                offset += 4;
                Array.Copy(ffs.tex_head[j].y_end, 0, tempblock, offset, ffs.tex_head[j].y_end.Length);
                offset += 4;
                Array.Copy(ffs.tex_head[j].x_start, 0, tempblock, offset, ffs.tex_head[j].x_start.Length);
                offset += 4;
                Array.Copy(ffs.tex_head[j].y_end, 0, tempblock, offset, ffs.tex_head[j].y_end.Length);
                offset += 4;
                Array.Copy(ffs.tex_head[j].tex_num, 0, tempblock, offset, ffs.tex_head[j].tex_num.Length);
                offset += 4;
            }

            for (int m = 0; m < ffs.texture.Count; m++)
            {
                Array.Copy(ffs.texture[m].tex_offset, 0, tempblock, offset, ffs.texture[m].tex_offset.Length);
                offset += 4;
                Array.Copy(ffs.texture[m].tex_length, 0, tempblock, offset, ffs.texture[m].tex_length.Length);
                offset += 4;
            }


            byte[] bTexOff = new byte[4];
            bTexOff = BitConverter.GetBytes(offset_tex);
            Array.Copy(bTexOff, 0, tempblock, 24, bTexOff.Length);
            Array.Copy(ffs.links_count, 0, tempblock, 16, ffs.links_count.Length);
            Array.Copy(ffs.tex_count, 0, tempblock, 20, ffs.tex_count.Length);

               /* for (int i = 0; i < BitConverter.ToInt32(ffs.links_count, 0); i++)
                {
                    Array.Copy(ffs.tex_head[i].x_start, 0, binContent, poz, ffs.tex_head[i].x_start.Length);
                    poz += 4;
                    Array.Copy(ffs.tex_head[i].y_start, 0, binContent, poz, ffs.tex_head[i].y_start.Length);
                    poz += 4;
                    Array.Copy(ffs.tex_head[i].x_end, 0, binContent, poz, ffs.tex_head[i].x_end.Length);
                    poz += 4;
                    Array.Copy(ffs.tex_head[i].y_start, 0, binContent, poz, ffs.tex_head[i].y_start.Length);
                    poz += 4;
                    Array.Copy(ffs.tex_head[i].x_end, 0, binContent, poz, ffs.tex_head[i].x_end.Length);
                    poz += 4;
                    Array.Copy(ffs.tex_head[i].y_end, 0, binContent, poz, ffs.tex_head[i].y_end.Length);
                    poz += 4;
                    Array.Copy(ffs.tex_head[i].x_start, 0, binContent, poz, ffs.tex_head[i].x_start.Length);
                    poz += 4;
                    Array.Copy(ffs.tex_head[i].y_end, 0, binContent, poz, ffs.tex_head[i].y_end.Length);
                    poz += 4;
                    Array.Copy(ffs.tex_head[i].tex_num, 0, binContent, poz, ffs.tex_head[i].tex_num.Length);
                    poz += 8;
                }*/

            //offset = poz - 4;
            
            //poz += 8 * BitConverter.ToInt32(ffs.tex_count, 0) - 4;

            poz = offset;// - (8 * BitConverter.ToInt32(ffs.tex_count, 0) - 4);

            //Methods.FindStartOfStringSomething(binContent, 0, "DDS |");
            byte[] temp_header = new byte[poz];
            //Array.Copy(binContent, 0, temp_header, 0, poz);
            Array.Copy(tempblock, 0, temp_header, 0, poz);

            //offset -= 4;

            offset -= 8 * BitConverter.ToInt32(ffs.tex_count, 0);
            
            byte[] bin_tex_offset = new byte[4];
            Array.Copy(temp_header, offset, bin_tex_offset, 0, 4);
            int tex_offset = BitConverter.ToInt32(bin_tex_offset, 0);
            int tex_block_size = temp_header.Length - 16;
            int tex_common_size = tex_block_size;

            for (int i = 0; i < BitConverter.ToInt32(ffs.tex_count, 0); i++)
            {
                offset += 4;
                byte[] dds_length = new byte[4];
                dds_length = BitConverter.GetBytes(ffs.texture[i].dds_content.Length);
                Array.Copy(dds_length, 0, temp_header, offset, dds_length.Length);
                tex_block_size += ffs.texture[i].dds_content.Length;
                tex_common_size = tex_block_size;

                if ((BitConverter.ToInt32(ffs.tex_count, 0) != 1) && (i < BitConverter.ToInt32(ffs.tex_count, 0) - 1))
                {
                    offset += 4;
                    tex_offset += ffs.texture[i].dds_content.Length;
                    byte[] dds_offset = new byte[4];
                    dds_offset = BitConverter.GetBytes(tex_offset);
                    Array.Copy(dds_offset, 0, temp_header, offset, dds_offset.Length);
                }
            }
            tex_common_size += 16;


            tex_common_size = Convert.ToInt32(pad_it((uint)tex_common_size, 16));

            byte[] tex_size = new byte[4];
            tex_size = BitConverter.GetBytes(tex_common_size);
            Array.Copy(tex_size, 0, temp_header, 4, 4);
            tex_size = new byte[4];
            tex_size = BitConverter.GetBytes(tex_block_size);
            Array.Copy(tex_size, 0, temp_header, 8, 4);

            MemoryStream ms = new MemoryStream();
            ms.Write(temp_header, 0, temp_header.Length);

            for (int j = 0; j < BitConverter.ToInt32(ffs.tex_count, 0); j++)
            {
                ms.Write(ffs.texture[j].dds_content, 0, ffs.texture[j].dds_content.Length);
            }
            byte[] temp = ms.ToArray();
            ms.Close();
            byte[] tex_header = new byte[tex_common_size];
            Array.Copy(temp, 0, tex_header, 0, temp.Length);


            poz = Methods.FindStartOfStringSomething(binContent, 0, "FONT");

            //poz = Methods.FindStartOfStringSomething(binContent, poz, "FONT");
            int table_size = 28 + (32 * BitConverter.ToInt32(ffs.font_count, 0)) + 4;
            byte[] temp_table = new byte[table_size];
            Array.Copy(binContent, poz, temp_table, 0, table_size);

            int coordCount = 0;
            int kernCount = 0;

            for (int j = 0; j < BitConverter.ToInt32(ffs.font_count, 0); j++)
            {
                coordCount += BitConverter.ToInt16(ffs.font_head[j].count_symbols, 0);
                kernCount += BitConverter.ToInt16(ffs.font_head[j].count_kernings, 0);
            }

            uint coords_size = (4 + 2 + 2 + 2 + 2 + 2 + 2 + 2 + 2 + 4) * (uint)coordCount;
            uint kern_size = (2 + 2 + 4) * (uint)kernCount;

            //coords_size = pad_it(coords_size, 8);
            //kern_size = pad_it(kern_size, 8);

            //Удаление лишних координат
            /*for (int i = 0; i < ffs.font_kern.Count; i++)
            {
                if (ffs.font_kern[i].new_kernings == false)
                {
                    ffs.font_kern.RemoveAt(i);
                    i = 0;
                }
            }*/


              /*  for (int j = 0; j < BitConverter.ToInt32(ffs.font_count, 0); j++)
                {
                    uint table_kern_offset = coords_size + BitConverter.ToUInt32(ffs.font_head[j].sym_offset, 0) - BitConverter.ToUInt32(ffs.font_head[j].count_kernings, 0) * 8;
                    ffs.font_head[j].kern_offset = new byte[4];
                    ffs.font_head[j].kern_offset = BitConverter.GetBytes(table_kern_offset);
                }*/

            byte[] bin_coords = new byte[coords_size];
            byte[] bin_kerns = new byte[kern_size];

            int block_size = 0;
            int common_size = 0;

            offset = 28;
            int sym_offset = 0;
            uint kern_offset = 0;


                if (count_fonts > 1) //Если больше одного шрифта, то пускаем чистейший говнокод!
                {
                /*    List<header_fonts> temp_fonts = new List<header_fonts>();
                    foreach (header_fonts ob in ffs.font_head)
                    {
                        temp_fonts.Add(new header_fonts(ob));
                    }

                    for (int i = 0; i < count_fonts; i++)
                    {
                        for (int j = 0; j < count_fonts - i - 1; j++)
                        {
                            if (BitConverter.ToUInt32(temp_fonts[j].sym_offset, 0) < BitConverter.ToUInt32(temp_fonts[j + 1].sym_offset, 0))
                            {
                                byte[] temp_sym_offset = temp_fonts[j].sym_offset;
                                byte[] temp_kern_offset = temp_fonts[j].kern_offset;
                                byte[] temp_sym_count = temp_fonts[j].count_symbols;
                                byte[] temp_kern_count = temp_fonts[j].count_kernings;
                                byte[] temp_font_data = temp_fonts[j].fonts_data;

                                temp_fonts[j].sym_offset = temp_fonts[j + 1].sym_offset;
                                temp_fonts[j].kern_offset = temp_fonts[j + 1].kern_offset;
                                temp_fonts[j].count_symbols = temp_fonts[j + 1].count_symbols;
                                temp_fonts[j].count_kernings = temp_fonts[j + 1].count_kernings;
                                temp_fonts[j].fonts_data = temp_fonts[j + 1].fonts_data;

                                temp_fonts[j + 1].sym_offset = temp_sym_offset;
                                temp_fonts[j + 1].kern_offset = temp_kern_offset;
                                temp_fonts[j + 1].count_symbols = temp_sym_count;
                                temp_fonts[j + 1].count_kernings = temp_kern_count;
                                temp_fonts[j + 1].fonts_data = temp_font_data;
                            }
                        }
                    }
                    for (int i = 1; i < count_fonts; ++i)
                    {
                        byte[] temp_sym_offset = temp_fonts[i].sym_offset;
                        byte[] temp_kern_offset = temp_fonts[i].kern_offset;
                        byte[] temp_sym_count = temp_fonts[i].count_symbols;
                        byte[] temp_kern_count = temp_fonts[i].count_kernings;
                        byte[] temp_font_data = temp_fonts[i].fonts_data;
                        uint temp_kern_off = temp_fonts[i].kern_off;

                        for (int j = i - 1; j >= 0 && BitConverter.ToUInt32(temp_fonts[i].sym_offset, 0) > BitConverter.ToUInt32(temp_sym_offset, 0); --j)
                        {
                            temp_fonts[i].sym_offset = temp_fonts[i + 1].sym_offset;
                            temp_fonts[i].kern_offset = temp_fonts[i + 1].kern_offset;
                            temp_fonts[i].count_symbols = temp_fonts[i + 1].count_symbols;
                            temp_fonts[i].count_kernings = temp_fonts[i + 1].count_kernings;
                            temp_fonts[i].fonts_data = temp_fonts[i + 1].fonts_data;
                            temp_fonts[i].kern_off = temp_fonts[i + 1].kern_off;

                            temp_fonts[i + 1].sym_offset = temp_sym_offset;
                            temp_fonts[i + 1].kern_offset = temp_kern_offset;
                            temp_fonts[i + 1].count_symbols = temp_sym_count;
                            temp_fonts[i + 1].count_kernings = temp_kern_count;
                            temp_fonts[i + 1].fonts_data = temp_font_data;
                            temp_fonts[i + 1].kern_off = temp_kern_off;
                        }
                    }

                    int index = 0;
                    if (BitConverter.ToInt32(temp_fonts[0].sym_offset, 0) > BitConverter.ToInt32(temp_fonts[count_fonts - 1].sym_offset, 0)) index = count_fonts - 1;


                    //uint sym_offs = ((uint)table_size - 16);//BitConverter.ToUInt32(temp_fonts[index].sym_offset, 0) + (BitConverter.ToUInt32(temp_fonts[index].count_symbols, 0) * 24);
                    uint kerning_offset = BitConverter.ToUInt32(temp_fonts[index].sym_offset, 0) + coords_size;
                    uint kern_off = 0;
                    //uint sym_off = 0;

                    if (index > 0)
                    {
                        for (int i = count_fonts - 1; i >= 0; i--)
                        {
                            /*temp_fonts[i].sym_off = sym_off;
                            temp_fonts[i].sym_offset = new byte[4];
                            temp_fonts[i].sym_offset = BitConverter.GetBytes(sym_offs);
                            sym_offs += (uint)BitConverter.ToInt16(temp_fonts[i].count_symbols, 0) * 24;
                            sym_off += (uint)BitConverter.ToInt16(temp_fonts[i].count_symbols, 0) * 24;
                            */


                     /*       if (BitConverter.ToInt32(temp_fonts[i].count_kernings, 0) > 0)
                            {
                                temp_fonts[i].kern_off = kern_off;
                                temp_fonts[i].kern_offset = new byte[4];
                                temp_fonts[i].kern_offset = BitConverter.GetBytes(kerning_offset);
                                kerning_offset += (uint)BitConverter.ToInt16(temp_fonts[i].count_kernings, 0) * 8;
                                kern_off += (uint)BitConverter.ToInt16(temp_fonts[i].count_kernings, 0) * 8;
                            }
                        }*/
                    /*}
                    else
                    {
                        for (int i = 0; i < count_fonts; i++)
                        {
                            /* temp_fonts[i].sym_off = sym_off;
                             temp_fonts[i].sym_offset = new byte[4];
                             temp_fonts[i].sym_offset = BitConverter.GetBytes(sym_offs);
                             sym_offs += (uint)BitConverter.ToInt16(temp_fonts[i].count_symbols, 0) * 24;
                             sym_off += (uint)BitConverter.ToInt16(temp_fonts[i].count_symbols, 0) * 24;*/

                      /*      if (BitConverter.ToInt32(temp_fonts[i].count_kernings, 0) > 0)
                            {
                                temp_fonts[i].kern_off = kern_off;
                                temp_fonts[i].kern_offset = new byte[4];
                                temp_fonts[i].kern_offset = BitConverter.GetBytes(kerning_offset);
                                kerning_offset += (uint)BitConverter.ToInt16(temp_fonts[i].count_kernings, 0) * 8;
                                kern_off += (uint)BitConverter.ToInt16(temp_fonts[i].count_kernings, 0) * 8;
                            }
                        }
                    }

                    for (int j = 0; j < count_fonts; j++)
                    {
                        for (int i = 0; i < count_fonts - j - 1; i++)
                        {
                            if (BitConverter.ToString(temp_fonts[i].fonts_data) == BitConverter.ToString(ffs.font_head[i].fonts_data))
                            {
                                ffs.font_head[i].count_symbols = temp_fonts[i].count_symbols;
                                ffs.font_head[i].sym_offset = temp_fonts[i].sym_offset;
                                ffs.font_head[i].sym_off = temp_fonts[i].sym_off;

                                ffs.font_head[i].count_kernings = temp_fonts[i].count_kernings;
                                ffs.font_head[i].kern_offset = temp_fonts[i].kern_offset;
                                ffs.font_head[i].kern_off = temp_fonts[i].kern_off;
                            }
                        }
                    }*/
                }
                else
                {
                    uint kerning_offset = BitConverter.ToUInt32(ffs.font_head[0].sym_offset, 0) + coords_size;
                    ffs.font_head[0].kern_offset = new byte[4];
                    ffs.font_head[0].kern_offset = BitConverter.GetBytes(kerning_offset);
                    ffs.font_head[0].kern_off = 0;
                }


                for (int i = 0; i < BitConverter.ToInt32(ffs.font_count, 0); i++)
                {
                    byte[] check = new byte[4];

                    Array.Copy(temp_table, offset, check, 0, check.Length);

                    for (int j = 0; j < ffs.font_head.Count; j++)
                    {
                        if (BitConverter.ToString(ffs.font_head[j].fonts_data) == BitConverter.ToString(check))
                        {
                            Array.Copy(ffs.font_head[j].fonts_data, 0, temp_table, offset, 4);
                            offset += 4;
                            Array.Copy(ffs.font_head[j].count_symbols, 0, temp_table, offset, 2);
                            offset += 2;
                            Array.Copy(ffs.font_head[j].count_kernings, 0, temp_table, offset, 2);
                            offset += 2;
                            Array.Copy(ffs.font_head[j].sym_offset, 0, temp_table, offset, 4);
                            offset += 4;
                            Array.Copy(ffs.font_head[j].kern_offset, 0, temp_table, offset, 4);
                            offset += 20;
                        }
                    }
                }

                    /*for (int n = 0; n < BitConverter.ToInt32(ffs.font_count, 0); n++)
                    {
                        Array.Copy(ffs.font_head[n].fonts_data, 0, temp_table, offset, 4);
                        offset += 4;
                        Array.Copy(ffs.font_head[n].count_symbols, 0, temp_table, offset, 2);
                        offset += 2;
                        Array.Copy(ffs.font_head[n].count_kernings, 0, temp_table, offset, 2);
                        offset += 2;
                        Array.Copy(ffs.font_head[n].sym_offset, 0, temp_table, offset, 4);
                        offset += 4;
                        Array.Copy(ffs.font_head[n].kern_offset, 0, temp_table, offset, 4);
                        offset += 20;
                    }*/
            


           /*if (count_fonts > 1)
            {
                for (int i = 0; i < count_fonts; i++)
                {
                    for (int j = 0; j < count_fonts - i - 1; j++)
                    {
                        if (BitConverter.ToInt32(ffs.font_head[i].sym_offset, 0) > BitConverter.ToInt32(ffs.font_head[i + 1].sym_offset, 0))
                        {
                            byte[] temp_sym_offset = ffs.font_head[i].sym_offset;
                            byte[] temp_kern_offset = ffs.font_head[i].kern_offset;
                            byte[] temp_sym_count = ffs.font_head[i].count_symbols;
                            byte[] temp_kern_count = ffs.font_head[i].count_kernings;
                            byte[] temp_font_data = ffs.font_head[i].fonts_data;

                            ffs.font_head[i].sym_offset = ffs.font_head[i + 1].sym_offset;
                            ffs.font_head[i].kern_offset = ffs.font_head[i + 1].kern_offset;
                            ffs.font_head[i].count_symbols = ffs.font_head[i + 1].count_symbols;
                            ffs.font_head[i].count_kernings = ffs.font_head[i + 1].count_kernings;
                            ffs.font_head[i].fonts_data = ffs.font_head[i + 1].fonts_data;

                            ffs.font_head[i + 1].sym_offset = temp_sym_offset;
                            ffs.font_head[i + 1].kern_offset = temp_kern_offset;
                            ffs.font_head[i + 1].count_symbols = temp_sym_count;
                            ffs.font_head[i + 1].count_kernings = temp_kern_count;
                            ffs.font_head[i + 1].fonts_data = temp_font_data;
                        }
                    }
                }
            }*/

            for (int j = 0; j < count_fonts; j++)
            {
                sym_offset =  BitConverter.ToInt32(ffs.font_head[j].sym_offset, 0) - (table_size - 16);
                if (BitConverter.ToUInt32(ffs.font_head[j].kern_offset, 0) > 0) kern_offset = BitConverter.ToUInt32(ffs.font_head[j].kern_offset, 0) - (uint)bin_coords.Length - ((uint)table_size - 16);
                else kern_offset = 0;


                for (int i = 0; i < ffs.font_coord.Count(); i++)
                {
                    //if (BitConverter.ToString(ffs.font_head[j].fonts_data) == BitConverter.ToString(ffs.font_coord[i].font_data) && ffs.font_coord[i].new_coordinates == true)
                    if (ffs.font_coord[i].index - 1 == j && ffs.font_coord[i].new_coordinates == true)
                    {
                        Array.Copy(ffs.font_coord[i].texture_data, 0, bin_coords, sym_offset, ffs.font_coord[i].texture_data.Length);
                        sym_offset += 4;
                        Array.Copy(ffs.font_coord[i].symbol, 0, bin_coords, sym_offset, ffs.font_coord[i].symbol.Length);
                        sym_offset += 2;
                        Array.Copy(ffs.font_coord[i].x_start, 0, bin_coords, sym_offset, ffs.font_coord[i].x_start.Length);
                        sym_offset += 2;
                        Array.Copy(ffs.font_coord[i].y_start, 0, bin_coords, sym_offset, ffs.font_coord[i].y_start.Length);
                        sym_offset += 2;
                        Array.Copy(ffs.font_coord[i].coord_width, 0, bin_coords, sym_offset, ffs.font_coord[i].coord_width.Length);
                        sym_offset += 2;
                        Array.Copy(ffs.font_coord[i].coord_height, 0, bin_coords, sym_offset, ffs.font_coord[i].coord_height.Length);
                        sym_offset += 2;
                        Array.Copy(ffs.font_coord[i].x_offset, 0, bin_coords, sym_offset, ffs.font_coord[i].x_offset.Length);
                        sym_offset += 2;
                        Array.Copy(ffs.font_coord[i].x_advanced, 0, bin_coords, sym_offset, ffs.font_coord[i].x_advanced.Length);
                        sym_offset += 2;
                        Array.Copy(ffs.font_coord[i].y_offset, 0, bin_coords, sym_offset, ffs.font_coord[i].y_offset.Length);
                        sym_offset += 2;
                        Array.Copy(ffs.font_coord[i].last_unknown_data, 0, bin_coords, sym_offset, ffs.font_coord[i].last_unknown_data.Length);
                        sym_offset += 4;

                        ffs.font_coord[i].font_data = new byte[4];
                        ffs.font_coord[i].font_data = BitConverter.GetBytes(-1);
                        ffs.font_coord[i].new_coordinates = false;
                        ffs.font_coord[i].index = -1;
                    }
                }

                if (ffs.font_kern.Count != 0)
                {
                    for (int k = 0; k < ffs.font_kern.Count(); k++)
                    {
                        if ((ffs.font_kern[k].new_kernings == true) && (ffs.font_kern[k].kern_index - 1 == j))
                        {
                            Array.Copy(ffs.font_kern[k].first_char, 0, bin_kerns, kern_offset, ffs.font_kern[k].first_char.Length);
                            kern_offset += 2;
                            Array.Copy(ffs.font_kern[k].second_char, 0, bin_kerns, kern_offset, ffs.font_kern[k].second_char.Length);
                            kern_offset += 2;
                            Array.Copy(ffs.font_kern[k].amount, 0, bin_kerns, kern_offset, ffs.font_kern[k].amount.Length);
                            kern_offset += 4;

                            ffs.font_kern[k].new_kernings = false;
                            ffs.font_kern[k].kern_index = -1;
                        }
                    }
                }
            }

            

            byte[] empty = new byte[4];

            block_size = temp_table.Length + bin_coords.Length + bin_kerns.Length - 16;

            common_size = block_size + 16;
            common_size = Convert.ToInt32(pad_it((uint)common_size, 8));


            byte[] size = new byte[4];
            size = BitConverter.GetBytes(common_size);
            Array.Copy(size, 0, temp_table, 4, 4);


            size = new byte[4];
            size = BitConverter.GetBytes(block_size);
            Array.Copy(size, 0, temp_table, 8, 4);

            byte[] newTable = new byte[common_size];
            Array.Copy(temp_table, 0, newTable, 0, temp_table.Length);
            Array.Copy(bin_coords, 0, newTable, temp_table.Length, bin_coords.Length);
            Array.Copy(bin_kerns, 0, newTable, temp_table.Length + bin_coords.Length, bin_kerns.Length);

            fs.Write(tex_header, 0, tex_header.Length);
            fs.Write(newTable, 0, newTable.Length);


            poz = Methods.FindStartOfStringSomething(binContent, 0, "TEXT");

            byte[] temp2 = new byte[binContent.Length - poz];
            Array.Copy(binContent, poz, temp2, 0, temp2.Length);
            fs.Write(temp2, 0, temp2.Length);
            
            fs.Close();

            if (tex_count_changed)
            {
                if (rel_exists && (relContent.Length > 0))
                {
                    rel_path = font_path.Remove(font_path.Length - 3, 3);
                    rel_path += "rel";
                    if (File.Exists(rel_path)) File.Delete(rel_path);

                    SaveRelFile(BitConverter.ToInt32(ffs.tex_count, 0), BitConverter.ToInt32(ffs.links_count, 0), relContent, rel_path);
                }
                else
                {
                    OpenFileDialog rel_ofd = new OpenFileDialog();
                    rel_ofd.Filter = "REL-file (*.rel) | *.rel";
                    rel_ofd.Title = "Choose REL-file of your font.";

                    if (rel_ofd.ShowDialog() == DialogResult.OK)
                    {
                        fs = new FileStream(rel_ofd.FileName, FileMode.Open);
                        relContent = Methods.ReadFull(fs);
                        fs.Close();

                        rel_path = font_path.Remove(font_path.Length - 3, 3);
                        rel_path += "rel";

                        SaveRelFile(BitConverter.ToInt32(ffs.tex_count, 0), BitConverter.ToInt32(ffs.links_count, 0), relContent, rel_path);
                    }

                }
            }
        }

        private void FontEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (edited == true)
            {
                DialogResult status = MessageBox.Show("Save font?", "Exit", MessageBoxButtons.YesNoCancel);
                if (status == DialogResult.No)
                // если (состояние == DialogResult. Нет) 
                {
                    ffs = null;
                }
                else if (status == DialogResult.Yes)
                {
                    SaveFileDialog save_font = new SaveFileDialog();
                    save_font.Filter = "FONT | *.dat";
                    if(save_font.ShowDialog() == DialogResult.OK){
                    using (FileStream fs = new FileStream(save_font.FileName, FileMode.Create))
                    {
                        fs.Write(binContent, 0, binContent.Length);
                    }
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                ffs = null;
            }
        }

        private void FontEditorForm_Load(object sender, EventArgs e)
        {
            dataGridTextures.Enabled = false;
            comboBox1.Enabled = false; //Выбор шрифта
            edited = false;
            rel_exists = false;
            label2.Text = "";
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
        }

        public void openandreadfont(string filename)
        {
            //Очистка списков и comboBoxа.

            //ffs.font_data.Clear();
            ffs.font_head.Clear();
            ffs.font_coord.Clear();
            ffs.font_kern.Clear();
            ffs.tex_head.Clear();
            //ffs.tex_num.Clear();
            ffs.texture.Clear();
            comboBox1.Items.Clear();
            offset = 0;


            FileStream fs;
            try
            {
                fs = new FileStream(filename, FileMode.Open);
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
                if (Methods.CheckHeader(binContent, offset, "FONT") == true
                    && (Methods.CheckHeader(binContent, offset, "PTEX") == true
                     && Methods.CheckHeader(binContent, offset, "DDS") == true))
                {
                    byte[] nil = new byte[4];
                    ffs.tex_block = new byte[4];
                    ffs.tex_part = new byte[4];

                    offset = 4;
                    Array.Copy(binContent, offset, ffs.tex_block, 0, 4);
                    offset += 4;

                    Array.Copy(binContent, offset, ffs.tex_part, 0, 4);

                    offset += 8;


                    //работа с текстурами
                    #region
                    ffs.tex_count = new byte[4];//количество текстур
                    ffs.links_count = new byte[4]; //Количество количество связей

                    Array.Copy(binContent, offset, ffs.links_count, 0, 4);
                    offset += 4;
                    Array.Copy(binContent, offset, ffs.tex_count, 0, 4);
                    offset += 8;

                    links_count = BitConverter.ToInt32(ffs.links_count, 0);
                    dds_count = BitConverter.ToInt32(ffs.tex_count, 0);


                    string test = null;
                    for (int f = 0; f < links_count; f++) //Считываем заголовок PTEX
                    {
                        nil = new byte[4];
                        ffs.texhead_add(nil, nil, nil, nil, nil, nil, 0);

                        ffs.tex_head[f].tex_data = new byte[4];
                        Array.Copy(binContent, offset, ffs.tex_head[f].tex_data, 0, 4);
                        test += "font_data: " + BitConverter.ToString(ffs.tex_head[f].tex_data) + "\r\n";
                        offset += 4;

                        ffs.tex_head[f].x_start = new byte[4];
                        Array.Copy(binContent, offset, ffs.tex_head[f].x_start, 0, 4);
                        offset += 4;
                        test += ("x_start " + BitConverter.ToSingle(ffs.tex_head[f].x_start, 0) * 512.0).ToString() + "\r\n";

                        ffs.tex_head[f].y_start = new byte[4];
                        Array.Copy(binContent, offset, ffs.tex_head[f].y_start, 0, 4);
                        offset += 4;
                        test += ("y_start " + BitConverter.ToSingle(ffs.tex_head[f].y_start, 0) * 256.0).ToString() + "\r\n";

                        ffs.tex_head[f].x_end = new byte[4];
                        Array.Copy(binContent, offset, ffs.tex_head[f].x_end, 0, 4);
                        offset += 12;
                        test += ("x_end " + BitConverter.ToSingle(ffs.tex_head[f].x_end, 0) * 512.0).ToString() + "\r\n";

                        ffs.tex_head[f].y_end = new byte[4];
                        Array.Copy(binContent, offset, ffs.tex_head[f].y_end, 0, 4);
                        offset += 12;
                        test += ("y_end " + BitConverter.ToSingle(ffs.tex_head[f].y_end, 0) * 256.0).ToString() + "\r\n";

                        ffs.tex_head[f].tex_num = new byte[4];
                        Array.Copy(binContent, offset, ffs.tex_head[f].tex_num, 0, 4);
                        offset += 4;
                    }

                    //MessageBox.Show(test);

                    for (int t = 0; t < dds_count; t++)
                    {
                        ffs.dds_add(nil, nil, nil, nil, nil);
                        ffs.texture[t].tex_offset = new byte[4];
                        Array.Copy(binContent, offset, ffs.texture[t].tex_offset, 0, 4);
                        offset += 4;

                        ffs.texture[t].tex_length = new byte[4];
                        Array.Copy(binContent, offset, ffs.texture[t].tex_length, 0, 4);
                        offset += 4;

                        int texture_offset = BitConverter.ToInt32(ffs.texture[t].tex_offset, 0) + 16;
                        int texture_length = BitConverter.ToInt32(ffs.texture[t].tex_length, 0);

                        //byte[] dds_content = new byte[texture_length];

                        ffs.texture[t].dds_content = new byte[texture_length];
                        ffs.texture[t].dds_height = new byte[4];
                        ffs.texture[t].dds_width = new byte[4];

                        Array.Copy(binContent, texture_offset, ffs.texture[t].dds_content, 0, texture_length);
                        Array.Copy(ffs.texture[t].dds_content, 12, ffs.texture[t].dds_height, 0, 4);
                        Array.Copy(ffs.texture[t].dds_content, 16, ffs.texture[t].dds_width, 0, 4);
                    }
                    #endregion

                    //работа с координатами
                    #region
                    ffs.coord_block = new byte[4];
                    ffs.coord_part = new byte[4];
                    int sym_steper = 0;
                    int kern_steper = 0;

                    poz = Methods.FindStartOfStringSomething(binContent, offset, "FONT");
                    poz += 4;
                    Array.Copy(binContent, poz, ffs.coord_block, 0, 4);
                    poz += 4;
                    Array.Copy(binContent, poz, ffs.coord_part, 0, 4);
                    offset = poz + 16;

                    ffs.font_count = new byte[4];
                    Array.Copy(binContent, offset, ffs.font_count, 0, 4);
                    count_fonts = BitConverter.ToInt32(ffs.font_count, 0);
                    int sizeData = (4 + 2 + 2 + 4 + 20) * count_fonts;



                    //MessageBox.Show(Convert.ToString(BitConverter.ToInt32(ffs.font_count, 0)));
                    offset += 4;
                    byte[] empty_byte = new byte[2];
                    byte[] nil2 = new byte[4];

                    ffs.old_data = new byte[sizeData];
                    Array.Copy(binContent, offset, ffs.old_data, 0, sizeData);
                    //MessageBox.Show(BitConverter.ToString(ffs.old_data));

                    string result = null;

                    for (int i = 0; i < count_fonts; i++)
                    {

                        ffs.head_add(nil2, nil2, empty_byte, empty_byte, empty_byte, empty_byte, 0, 0);

                        ffs.font_head[i].fonts_data = new byte[4];
                        Array.Copy(binContent, offset, ffs.font_head[i].fonts_data, 0, 4);
                        offset += 4;

                        ffs.font_head[i].count_symbols = new byte[2];
                        Array.Copy(binContent, offset, ffs.font_head[i].count_symbols, 0, 2);
                        offset += 2;

                        ffs.font_head[i].count_kernings = new byte[2];
                        Array.Copy(binContent, offset, ffs.font_head[i].count_kernings, 0, 2);
                        offset += 2;

                        ffs.font_head[i].sym_offset = new byte[4];
                        Array.Copy(binContent, offset, ffs.font_head[i].sym_offset, 0, 4);
                        offset += 4;

                        ffs.font_head[i].kern_offset = new byte[4];
                        Array.Copy(binContent, offset, ffs.font_head[i].kern_offset, 0, 4);

                        int temp_off = offset + 8;
                        byte[] checkByte = new byte[2];
                        Array.Copy(binContent, temp_off, checkByte, 0, checkByte.Length);
                        result += i.ToString() + " " + BitConverter.ToInt16(checkByte, 0).ToString() + "\r\n";
                        offset += 20;
                    }

                    //MessageBox.Show(result);

                    for (int j = 0; j < count_fonts; j++)
                    {
                        for (int k = j - 1; k >= 0; k--)
                        {
                            if (BitConverter.ToInt32(ffs.font_head[k].sym_offset, 0) > BitConverter.ToInt32(ffs.font_head[k + 1].sym_offset, 0))
                            {
                                byte[] temp_fonts_data = ffs.font_head[k].fonts_data;
                                byte[] temp_count_fonts = ffs.font_head[k].count_fonts;
                                byte[] temp_count_symbols = ffs.font_head[k].count_symbols;
                                byte[] temp_count_kernings = ffs.font_head[k].count_kernings;
                                uint temp_sym_off = ffs.font_head[k].sym_off;
                                byte[] temp_sym_offset = ffs.font_head[k].sym_offset;
                                uint temp_kern_off = ffs.font_head[k].kern_off;
                                byte[] temp_kern_offset = ffs.font_head[k].kern_offset;

                                ffs.font_head[k].fonts_data = ffs.font_head[k + 1].fonts_data;
                                ffs.font_head[k].count_fonts = ffs.font_head[k + 1].count_fonts;
                                ffs.font_head[k].count_symbols = ffs.font_head[k + 1].count_symbols;
                                ffs.font_head[k].count_kernings = ffs.font_head[k + 1].count_kernings;
                                ffs.font_head[k].sym_off = ffs.font_head[k + 1].sym_off;
                                ffs.font_head[k].sym_offset = ffs.font_head[k + 1].sym_offset;
                                ffs.font_head[k].kern_off = ffs.font_head[k + 1].kern_off;
                                ffs.font_head[k].kern_offset = ffs.font_head[k + 1].kern_offset;

                                ffs.font_head[k + 1].fonts_data = temp_fonts_data;
                                ffs.font_head[k + 1].count_fonts = temp_count_fonts;
                                ffs.font_head[k + 1].count_symbols = temp_count_symbols;
                                ffs.font_head[k + 1].count_kernings = temp_count_kernings;
                                ffs.font_head[k + 1].sym_off = temp_sym_off;
                                ffs.font_head[k + 1].sym_offset = temp_sym_offset;
                                ffs.font_head[k + 1].kern_off = temp_kern_off;
                                ffs.font_head[k + 1].kern_offset = temp_kern_offset;
                            }
                        }
                    }

                    for (int i = 0; i < count_fonts; i++)
                    {
                        int sym_count = BitConverter.ToInt16(ffs.font_head[i].count_symbols, 0);
                        int kernings_count = BitConverter.ToInt16(ffs.font_head[i].count_kernings, 0);

                        int symbols_offset = BitConverter.ToInt32(ffs.font_head[i].sym_offset, 0) + poz + 8;
                        int kernings_offset = BitConverter.ToInt32(ffs.font_head[i].kern_offset, 0) + poz + 8;



                        for (int sym = sym_steper; sym < sym_count + sym_steper; sym++)
                        {
                            ffs.coord_add(nil2, empty_byte, empty_byte, empty_byte, empty_byte, empty_byte,
                                empty_byte, empty_byte, empty_byte, empty_byte, i, false, nil2);

                            ffs.font_coord[sym].index = i + 1;

                            ffs.font_coord[sym].texture_data = new byte[4];
                            Array.Copy(binContent, symbols_offset, ffs.font_coord[sym].texture_data, 0, 4);
                            symbols_offset += 4;

                            test += "index: " + ffs.font_coord[sym].index.ToString() + "\r\n" + "tex_data: " + BitConverter.ToString(ffs.font_coord[sym].texture_data) + "\r\n";

                            ffs.font_coord[sym].symbol = new byte[2];
                            Array.Copy(binContent, symbols_offset, ffs.font_coord[sym].symbol, 0, 2);
                            symbols_offset += 2;

                            ffs.font_coord[sym].x_start = new byte[2];
                            Array.Copy(binContent, symbols_offset, ffs.font_coord[sym].x_start, 0, 2);
                            symbols_offset += 2;

                            ffs.font_coord[sym].y_start = new byte[2];
                            Array.Copy(binContent, symbols_offset, ffs.font_coord[sym].y_start, 0, 2);
                            symbols_offset += 2;

                            ffs.font_coord[sym].coord_width = new byte[2];
                            Array.Copy(binContent, symbols_offset, ffs.font_coord[sym].coord_width, 0, 2);
                            symbols_offset += 2;

                            ffs.font_coord[sym].coord_height = new byte[2];
                            Array.Copy(binContent, symbols_offset, ffs.font_coord[sym].coord_height, 0, 2);
                            symbols_offset += 2;

                            ffs.font_coord[sym].x_offset = new byte[2];
                            Array.Copy(binContent, symbols_offset, ffs.font_coord[sym].x_offset, 0, 2);
                            symbols_offset += 2;

                            ffs.font_coord[sym].x_advanced = new byte[2];
                            Array.Copy(binContent, symbols_offset, ffs.font_coord[sym].x_advanced, 0, 2);
                            symbols_offset += 2;

                            ffs.font_coord[sym].y_offset = new byte[2];
                            Array.Copy(binContent, symbols_offset, ffs.font_coord[sym].y_offset, 0, 2);
                            symbols_offset += 2;

                            ffs.font_coord[sym].last_unknown_data = new byte[4];
                            Array.Copy(binContent, symbols_offset, ffs.font_coord[sym].last_unknown_data, 0, 4);
                            symbols_offset += 4;

                            ffs.font_coord[sym].font_data = new byte[4];
                            Array.Copy(ffs.font_head[i].fonts_data, 0, ffs.font_coord[sym].font_data, 0, ffs.font_head[i].fonts_data.Length);
                        }
                        sym_steper += sym_count;

                        for (int ker = kern_steper; ker < kernings_count + kern_steper; ker++)
                        {
                            ffs.kern_add(empty_byte, empty_byte, nil2, i, false, nil2);

                            ffs.font_kern[ker].first_char = new byte[2];
                            Array.Copy(binContent, kernings_offset, ffs.font_kern[ker].first_char, 0, 2);
                            kernings_offset += 2;

                            ffs.font_kern[ker].second_char = new byte[2];
                            Array.Copy(binContent, kernings_offset, ffs.font_kern[ker].second_char, 0, 2);
                            kernings_offset += 2;

                            ffs.font_kern[ker].amount = new byte[4];
                            Array.Copy(binContent, kernings_offset, ffs.font_kern[ker].amount, 0, 4);
                            kernings_offset += 4;

                            ffs.font_kern[ker].font_data = new byte[4];
                            Array.Copy(ffs.font_head[i].fonts_data, 0, ffs.font_kern[ker].font_data, 0, ffs.font_head[i].fonts_data.Length);

                            ffs.font_kern[ker].kern_index = i + 1;
                        }
                        kern_steper += kernings_count;
                    }

                    for (int i = 0; i < links_count; i++)
                    {
                        for (int j = 0; j < ffs.font_coord.Count; j++)
                        {
                            if (BitConverter.ToString(ffs.tex_head[i].tex_data) ==
                                BitConverter.ToString(ffs.font_coord[j].texture_data))
                            {
                                ffs.tex_head[i].font_num = ffs.font_coord[j].index;
                                break;
                            }
                        }
                    }

                    #endregion

                    test = null;

                    for (int i = 0; i < ffs.tex_head.Count; i++)
                    {
                        test += "Tex num: " + BitConverter.ToInt32(ffs.tex_head[i].tex_num, 0) + ", font num: " + Convert.ToString(ffs.tex_head[i].font_num) + "\r\n";
                    }

                    label2.Text = test;
                    /*StreamWriter sw = new StreamWriter("C:\\Users\\User\\Desktop\\OpenResult.txt");
                for (int i = 0; i < ffs.font_coord.Count(); i++)
                {
                    string result = BitConverter.ToString(ffs.font_coord[i].font_data) + " " + BitConverter.ToString(ffs.font_coord[i].texture_data) + " "
                    + BitConverter.ToString(ffs.font_coord[i].symbol) + " " + BitConverter.ToString(ffs.font_coord[i].x_start) + " " + BitConverter.ToString(ffs.font_coord[i].y_start) +
                    " " + BitConverter.ToString(ffs.font_coord[i].coord_width) + " " + BitConverter.ToString(ffs.font_coord[i].coord_height) + " "
                    + BitConverter.ToString(ffs.font_coord[i].x_advanced) + "\r\n";
                    sw.Write(result);
                }
                    sw.Close();*/



                    filltextable();
                    fillcoordtable(0);
                    edited = false;
                    dataGridTextures.Enabled = true;
                    if (count_fonts > 1)
                    {
                        comboBox1.Enabled = true;
                        for (int fc = 0; fc < count_fonts; fc++) comboBox1.Items.Add(fc);
                        comboBox1.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBox1.Enabled = false;
                    }
                    if (dds_count > 1)
                    {
                        importMultyLitteraFontGeneratorXMLtypefntToolStripMenuItem.Enabled = false;
                        importMultiFontGeneratorXMLtypefntExperimentalToolStripMenuItem.Enabled = false;
                    }
                    else
                    {
                        importMultyLitteraFontGeneratorXMLtypefntToolStripMenuItem.Enabled = true;
                        importMultiFontGeneratorXMLtypefntExperimentalToolStripMenuItem.Enabled = true;
                    }
                }
            }
        }

        private void openFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            OpenFont.Filter = "dat-file (*.dat) | *.dat";

            if (OpenFont.ShowDialog() == DialogResult.OK)
            {
                
            }
        }

        public void filltextable()
        {
            dataGridTextures.RowCount = ffs.texture.Count();
            for (int i = 0; i < ffs.texture.Count(); i++)
            {
                dataGridTextures[0, i].Value = i;
                dataGridTextures[1, i].Value = BitConverter.ToInt32(ffs.texture[i].dds_width, 0);
                dataGridTextures[2, i].Value = BitConverter.ToInt32(ffs.texture[i].dds_height, 0);
                dataGridTextures[3, i].Value = BitConverter.ToInt32(ffs.texture[i].tex_length, 0);
            }
        }

        public void fillcoordtable(int font_num)
        {
            int num_font = font_num;
            dataGridCoord.RowCount = BitConverter.ToInt16(ffs.font_head[num_font].count_symbols, 0);


            
            int i = 0;
                for (int j = 0; j < ffs.font_coord.Count; j++)
                {
                    if (ffs.font_coord[j].index - 1 == num_font)
                    {
                        if (i < dataGridCoord.RowCount)
                        {
                            dataGridCoord[0, i].Value = BitConverter.ToInt16(ffs.font_coord[j].symbol, 0).ToString();
                            dataGridCoord[1, i].Value = Encoding.Unicode.GetString(ffs.font_coord[j].symbol);
                            dataGridCoord[2, i].Value = BitConverter.ToInt16(ffs.font_coord[j].x_start, 0);
                            dataGridCoord[3, i].Value = BitConverter.ToInt16(ffs.font_coord[j].x_start, 0) + BitConverter.ToInt16(ffs.font_coord[j].coord_width, 0);
                            dataGridCoord[4, i].Value = BitConverter.ToInt16(ffs.font_coord[j].y_start, 0);
                            dataGridCoord[5, i].Value = BitConverter.ToInt16(ffs.font_coord[j].y_start, 0) + BitConverter.ToInt16(ffs.font_coord[j].coord_height, 0);
                            dataGridCoord[6, i].Value = BitConverter.ToInt16(ffs.font_coord[j].coord_width, 0);
                            dataGridCoord[7, i].Value = BitConverter.ToInt16(ffs.font_coord[j].coord_height, 0);

                            for (int m = 0; m < BitConverter.ToInt32(ffs.links_count, 0); m++)
                            {
                                if (BitConverter.ToInt32(ffs.font_coord[j].texture_data, 0) == BitConverter.ToInt32(ffs.tex_head[m].tex_data, 0))
                                {
                                    //MessageBox.Show(BitConverter.ToString(ffs.font_coord[i].texture_data) + " " + BitConverter.ToString(ffs.font_data[m]));
                                    dataGridCoord[8, i].Value = BitConverter.ToInt32(ffs.tex_head[m].tex_num, 0); 

                                    break;
                                }
                            }

                            dataGridCoord[9, i].Value = BitConverter.ToInt16(ffs.font_coord[j].x_offset, 0);
                            dataGridCoord[10, i].Value = BitConverter.ToInt16(ffs.font_coord[j].x_advanced, 0);
                            dataGridCoord[11, i].Value = BitConverter.ToInt16(ffs.font_coord[j].y_offset, 0);
                            dataGridCoord[12, i].Value = BitConverter.ToInt32(ffs.font_coord[j].last_unknown_data, 0);

                            for (int n = 0; n <= 12; n++)
                            {
                                if(ffs.font_coord[j].new_coordinates == false) dataGridCoord[n, i].Style.BackColor = System.Drawing.Color.White;
                                else dataGridCoord[n, i].Style.BackColor = System.Drawing.Color.Yellow;
                            }
                            i++;
                        }
                        }
                    }

                int kernRowCount;
                if (BitConverter.ToInt16(ffs.font_head[num_font].count_kernings, 0) == 0)
                {
                    kernRowCount = 1;
                    dataGridView1.RowCount = kernRowCount;
                    dataGridView1[0, 0].Value = null;
                    dataGridView1[1, 0].Value = null;
                    dataGridView1[2, 0].Value = null;
                }
                else
                {
                    kernRowCount = BitConverter.ToInt16(ffs.font_head[num_font].count_kernings, 0);

                    dataGridView1.RowCount = kernRowCount;
                    i = 0;
                    for (int f = 0; f < ffs.font_kern.Count(); f++)
                    {
                        if (ffs.font_kern[f].kern_index - 1 == num_font)
                        {
                            if (i < dataGridView1.RowCount)
                            {
                                dataGridView1[0, i].Value = Encoding.Unicode.GetString(ffs.font_kern[f].first_char);
                                dataGridView1[1, i].Value = Encoding.Unicode.GetString(ffs.font_kern[f].second_char);
                                dataGridView1[2, i].Value = BitConverter.ToInt32(ffs.font_kern[f].amount, 0);

                                for (int a = 0; a < 3; a++)
                                {
                                    if (ffs.font_kern[f].new_kernings == false) dataGridView1[a, i].Style.BackColor = System.Drawing.Color.White;
                                    else dataGridView1[a, i].Style.BackColor = System.Drawing.Color.Aqua;
                                }
                                i++;
                            }
                        }
                    }
                }
        }


        private void dataGridTextures_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridTextures.Rows[e.RowIndex].Selected = true;
            }
            if (e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // получаем координаты
                Point pntCell = dataGridTextures.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location;
                pntCell.X += e.Location.X;
                pntCell.Y += e.Location.Y;

                // вызываем менюшку
                contextMenuStrip2.Show(dataGridTextures, pntCell);
            }
        }
        private void importTextureDDSToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog Open_tex = new OpenFileDialog();
            Open_tex.Filter = "Texture D3D (*.DDS) | *.dds";

            int tex_length;
            byte[] tex_size = new byte[4];

            if (Open_tex.ShowDialog() == DialogResult.OK)
            {
                FileStream tex_stream = new FileStream(Open_tex.FileName, FileMode.Open);
                tex_length = Convert.ToInt32(tex_stream.Length);
                tex_size = BitConverter.GetBytes(tex_length);

                byte[] new_tex = new byte[tex_length];

                using(tex_stream){                    
                 tex_stream.Read(new_tex, 0, new_tex.Length);                    
                }

                poz = Methods.FindStartOfStringSomething(binContent, 0, "DDS") - 20;

                Array.Copy(tex_size, 0, binContent, poz, 4);
                poz += 16;
                Array.Copy(tex_size, 0, binContent, poz, 4);
                edited = true;
            }
        }

        private void exportTextureddsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int dds_n = dataGridTextures.SelectedCells[0].RowIndex;
            SaveFileDialog Save_tex = new SaveFileDialog();
            Save_tex.Filter = "DDS-texture (*.DDS) | *.dds";
            Save_tex.FileName = Methods.GetFileNameOnly(OpenFont.SafeFileName.ToString(), ".dat") + "_" + Convert.ToString(dds_n + 1) + ".dds";

            //byte[] bindds_export = new byte[BitConverter.ToInt32(ffs.tex_head[dds_n - 1].tex_length, 0)];
            
            FileStream fs;

            if (Save_tex.ShowDialog() == DialogResult.OK)
            {
                if (dds_count > 1)
                {
                    if (dds_n > 0)
                    {
                        /*int dds_offset = BitConverter.ToInt32(ffs.texture[dds_n - 1].dds_offset, 0);
                        poz = Methods.FindStartOfStringSomething(binContent, dds_offset, "DDS");
                        Array.Copy(binContent, poz, bindds_export, 0, bindds_export.Length);*/
                        using (fs = new FileStream(Save_tex.FileName, FileMode.Create))
                        {
                            fs.Write(ffs.texture[dds_n - 1].dds_content, 0, ffs.texture[dds_n - 1].dds_content.Length);
                            //fs.Write(bindds_export, 0, bindds_export.Length);
                        }
                    }
                    else
                    {
                        /*poz = Methods.FindStartOfStringSomething(binContent, 0, "DDS");
                        Array.Copy(binContent, poz, bindds_export, 0, bindds_export.Length);*/
                        using (fs = new FileStream(Save_tex.FileName, FileMode.Create))
                        {
                            fs.Write(ffs.texture[0].dds_content, 0, ffs.texture[0].dds_content.Length);
                            //fs.Write(bindds_export, 0, bindds_export.Length);
                        }
                    }
                }
                else
                {
                    /*poz = Methods.FindStartOfStringSomething(binContent, 0, "DDS");
                    Array.Copy(binContent, poz, bindds_export, 0, bindds_export.Length);*/
                    using (fs = new FileStream(Save_tex.FileName, FileMode.Create))
                    {
                        fs.Write(ffs.texture[0].dds_content, 0, ffs.texture[0].dds_content.Length);
                        //fs.Write(bindds_export, 0, bindds_export.Length);
                    }
                }
            }
        }

        private void importTextureddsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenTexture = new OpenFileDialog();
            OpenTexture.Filter = "DDS-texture (*.dds) | *.dds";

            int num_dds = dataGridTextures.SelectedCells[0].RowIndex;

            if (OpenTexture.ShowDialog() == DialogResult.OK)
            {
                FileStream f_texture = new FileStream(OpenTexture.FileName, FileMode.Open);
                ffs.texture[num_dds].dds_content = Methods.ReadFull(f_texture);
                f_texture.Close();
                offset = 12;

                ffs.texture[num_dds].dds_height = new byte[4];
                Array.Copy(ffs.texture[num_dds].dds_content, offset, ffs.texture[num_dds].dds_height, 0, ffs.texture[num_dds].dds_height.Length);
                
                offset += 4;
                ffs.texture[num_dds].dds_width = new byte[4];
                Array.Copy(ffs.texture[num_dds].dds_content, offset, ffs.texture[num_dds].dds_width, 0, ffs.texture[num_dds].dds_width.Length);

                ffs.texture[num_dds].tex_length = new byte[4];
                ffs.texture[num_dds].tex_length = BitConverter.GetBytes(ffs.texture[num_dds].dds_content.Length);

                /*for (int i = 0; i < BitConverter.ToInt32(ffs.links_count, 0); i++)
                {
                    if (num_dds == BitConverter.ToInt32(ffs.tex_head[i].tex_num, 0))
                    {
                        ffs.tex_head[i].x_start = new byte[4];
                        ffs.tex_head[i].x_start = BitConverter.GetBytes(0);
                        ffs.tex_head[i].y_start = new byte[4];
                        ffs.tex_head[i].y_start = BitConverter.GetBytes(0);
                        ffs.tex_head[i].x_end = new byte[4];
                        ffs.tex_head[i].x_end = BitConverter.GetBytes(BitConverter.ToSingle(ffs.texture[num_dds].dds_width, 0) / BitConverter.ToSingle(ffs.texture[num_dds].dds_width, 0));
                        ffs.tex_head[i].y_end = new byte[4];
                        ffs.tex_head[i].y_end = BitConverter.GetBytes(BitConverter.ToSingle(ffs.texture[num_dds].dds_height, 0) / BitConverter.ToSingle(ffs.texture[num_dds].dds_height, 0));
                    }
                }*/

                    filltextable();
                edited = true;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save_fnt = new SaveFileDialog();
            save_fnt.Filter = "DAT-file (*.dat) | *.dat";

            if (save_fnt.ShowDialog() == DialogResult.OK)
            {
                font_path = save_fnt.FileName;
                SaveFont(binContent, font_path);
                edited = false;
            }
        }

        private void saveFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFont(binContent, font_path);
            edited = false;
        }

        private void exportCoordinatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sf = new SaveFileDialog();
            if (sf.ShowDialog() == DialogResult.OK)
            {
                using (FileStream temp_shit = new FileStream(sf.FileName, FileMode.Create))
                {
                    
                    int offset = 0;
                    for (int i = 0; i < dataGridView1.RowCount; i++)
                    {
                        byte[] nil = new byte[3];
                        byte[] nil3 = new byte[3];
                        byte[] nil2 = new byte[4];
                        nil = UnicodeEncoding.Unicode.GetBytes(dataGridView1[0, i].Value.ToString());
                        //MessageBox.Show(BitConverter.ToString(nil));
                        temp_shit.Write(nil, offset, nil.Length);
                        //offset += 2;
                        
                        nil3 = UnicodeEncoding.Unicode.GetBytes(dataGridView1[1, i].Value.ToString());
                        temp_shit.Write(nil3, offset, nil3.Length);
                        //offset += 2;
                        
                        nil2 = BitConverter.GetBytes(Convert.ToInt32(dataGridView1[2, i].Value));
                        //MessageBox.Show(BitConverter.ToString(nil2));
                        temp_shit.Write(nil2, offset, nil2.Length);
                        //offset += 4;
                    }
                }
            }
        }



        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            int selectedFont = comboBox1.SelectedIndex;
            fillcoordtable(selectedFont);
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }


        private void addDDStextureddsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog AddTextureDialog = new OpenFileDialog();
            AddTextureDialog.Filter = "DDS-texture (*.dds) | *.dds";

            if (AddTextureDialog.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(AddTextureDialog.FileName, FileMode.Open);
                byte[] ddscontent = Methods.ReadFull(fs);
                byte[] nil = new byte[4];
                ffs.dds_add(nil, nil, nil, nil, nil);

                ffs.texture[ffs.texture.Count - 1].dds_content = new byte[ddscontent.Length];
                Array.Copy(ddscontent, 0, ffs.texture[ffs.texture.Count - 1].dds_content, 0, ddscontent.Length);

                ffs.texture[ffs.texture.Count - 1].tex_length = new byte[4];
                ffs.texture[ffs.texture.Count - 1].tex_length = BitConverter.GetBytes(ddscontent.Length);

                int texOffset = BitConverter.ToInt32(ffs.texture[ffs.texture.Count - 2].tex_offset, 0) + BitConverter.ToInt32(ffs.texture[ffs.texture.Count - 2].tex_length, 0);
                ffs.texture[ffs.texture.Count - 1].tex_offset = new byte[4];
                ffs.texture[ffs.texture.Count - 1].tex_offset = BitConverter.GetBytes(texOffset);

                ffs.texture[ffs.texture.Count - 1].dds_width = new byte[4];
                Array.Copy(ddscontent, 16, ffs.texture[ffs.texture.Count - 1].dds_width, 0, 4);

                ffs.texture[ffs.texture.Count - 1].dds_height = new byte[4];
                Array.Copy(ddscontent, 12, ffs.texture[ffs.texture.Count - 1].dds_height, 0, 4);


                ffs.texhead_add(nil, nil, nil, nil, nil, nil, 0);

                ffs.tex_head[ffs.tex_head.Count - 1].tex_data = new byte[4];
                Random tgd = new Random();
                tgd.NextBytes(ffs.tex_head[ffs.tex_head.Count - 2].tex_data);

                ffs.tex_head[ffs.tex_head.Count - 1].tex_num = BitConverter.GetBytes(ffs.texture.Count - 1);

                ffs.tex_head[ffs.tex_head.Count - 1].x_start = BitConverter.GetBytes(0);
                ffs.tex_head[ffs.tex_head.Count - 1].y_start = BitConverter.GetBytes(0);
                float getMaxWidth = BitConverter.ToSingle(ffs.texture[ffs.texture.Count - 1].dds_width, 0) / BitConverter.ToSingle(ffs.texture[ffs.texture.Count - 1].dds_width, 0);
                float getMaxHeight = BitConverter.ToSingle(ffs.texture[ffs.texture.Count - 1].dds_height, 0) / BitConverter.ToSingle(ffs.texture[ffs.texture.Count - 1].dds_height, 0);
                ffs.tex_head[ffs.tex_head.Count - 1].x_end = BitConverter.GetBytes(getMaxWidth);
                ffs.tex_head[ffs.tex_head.Count - 1].y_end = BitConverter.GetBytes(getMaxHeight);
                

                filltextable();
            }
        }

        private void importMulticoordinateFromUBFGxmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML-type coordinates (*.xml) | *.xml";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] temp_strings = File.ReadAllLines(ofd.FileName, Encoding.UTF8);
                
                int font_num = comboBox1.SelectedIndex;
                int font_counter = 0;

                if (comboBox1.Enabled == false) font_num = 0;
                int num_dds = dataGridTextures.SelectedCells[0].RowIndex;
                List<char> first_char = new List<char>();
                List<char> second_char = new List<char>();
                List<int> amount = new List<int>();
                bool kerning_exists = false;

                for (int i = 0; i < temp_strings.Count(); i++)
                {
                    if ((temp_strings[i].IndexOf("<font name=", 0) > 0) && (font_counter == font_num))
                    {
                        font_counter++;
                        int index = i + 1;

                        while (temp_strings[index].IndexOf("</font>", 0) <= 0)
                        {
                            if (temp_strings[index].IndexOf("<char id=") > 0)
                            {
                                string[] strings = temp_strings[index].Split('\"');

                                int char_num = Convert.ToInt32(strings[1]);
                                int x = Convert.ToInt32(strings[3]);
                                int y = Convert.ToInt32(strings[5]);
                                int width = Convert.ToInt32(strings[7]);
                                int height = Convert.ToInt32(strings[9]);
                                int x_offset = Convert.ToInt32(strings[11]);
                                int y_offset = Convert.ToInt32(strings[17]) - Convert.ToInt32(strings[13]);//height;
                                int x_advanced = Convert.ToInt32(strings[15]);

                                char ch = (char)char_num;

                                for (int j = 0; j < dataGridCoord.RowCount; j++)
                                {
                                    string s = dataGridCoord[1, j].Value.ToString();

                                    if (ch == s[0])
                                    {
                                        dataGridCoord[2, j].Value = x;
                                        dataGridCoord[3, j].Value = x + width;
                                        dataGridCoord[4, j].Value = y;
                                        dataGridCoord[5, j].Value = y + height;
                                        dataGridCoord[6, j].Value = width;
                                        dataGridCoord[7, j].Value = height;
                                        dataGridCoord[8, j].Value = 0;
                                        dataGridCoord[9, j].Value = x_offset;
                                        dataGridCoord[10, j].Value = x_advanced;
                                        dataGridCoord[11, j].Value = y_offset;

                                        for (int k = 0; k < dataGridCoord.Columns.Count; k++)
                                        {
                                            dataGridCoord[k, j].Style.BackColor = System.Drawing.Color.Cyan;
                                        }
                                    }
                                }
                            }
                            else if (temp_strings[index].IndexOf("<kerning first=") > 0)
                            {
                                string[] strings = temp_strings[index].Split('\"');

                                first_char.Add((char)Convert.ToInt32(strings[1]));
                                second_char.Add((char)Convert.ToInt32(strings[3]));
                                amount.Add(Convert.ToInt32(strings[5]));

                                kerning_exists = true;
                            }
                            
                            index++;
                            if (temp_strings[index].IndexOf("</font>", 0) > 0)
                            {
                                i = index;
                                break;
                            }
                        }
                    }
                    else if ((temp_strings[i].IndexOf("<font name=", 0) > 0) && (font_counter != font_num)) font_counter++;
                }

                int ci = 0;
                for (int i = 0; i < BitConverter.ToInt32(ffs.links_count, 0); i++)
                {
                    if (num_dds == BitConverter.ToInt32(ffs.tex_head[i].tex_num, 0))
                    {
                        ffs.tex_head[i].x_start = new byte[4];
                        ffs.tex_head[i].x_start = BitConverter.GetBytes(0);
                        ffs.tex_head[i].y_start = new byte[4];
                        ffs.tex_head[i].y_start = BitConverter.GetBytes(0);
                        ffs.tex_head[i].x_end = new byte[4];
                        ffs.tex_head[i].x_end = BitConverter.GetBytes(BitConverter.ToSingle(ffs.texture[num_dds].dds_width, 0) / BitConverter.ToSingle(ffs.texture[num_dds].dds_width, 0));
                        ffs.tex_head[i].y_end = new byte[4];
                        ffs.tex_head[i].y_end = BitConverter.GetBytes(BitConverter.ToSingle(ffs.texture[num_dds].dds_height, 0) / BitConverter.ToSingle(ffs.texture[num_dds].dds_height, 0));
                    }
                }

                for (int remake = 0; remake < ffs.font_coord.Count(); remake++)
                {
                    if (font_num == ffs.font_coord[remake].index)
                    {
                        if (ffs.texture.Count > 1)
                        {
                            for (int i = 0; i < BitConverter.ToInt32(ffs.links_count, 0); i++)
                            {
                                if (BitConverter.ToInt32(ffs.tex_head[i].tex_num, 0) == Convert.ToInt32(dataGridCoord[8, ci].Value))
                                {
                                    ffs.font_coord[remake].texture_data = new byte[4];
                                    Array.Copy(ffs.tex_head[i].tex_data, 0, ffs.font_coord[remake].texture_data, 0, 4);
                                    break;
                                }
                            }
                        }

                        ffs.font_coord[remake].symbol = new byte[2];
                        ffs.font_coord[remake].symbol = UnicodeEncoding.Unicode.GetBytes(dataGridCoord[1, ci].Value.ToString());
                        ffs.font_coord[remake].x_start = new byte[2];
                        ffs.font_coord[remake].x_start = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[2, ci].Value));
                        ffs.font_coord[remake].y_start = new byte[2];
                        ffs.font_coord[remake].y_start = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[4, ci].Value));
                        ffs.font_coord[remake].coord_width = new byte[2];
                        ffs.font_coord[remake].coord_width = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[6, ci].Value));
                        ffs.font_coord[remake].coord_height = new byte[2];
                        ffs.font_coord[remake].coord_height = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[7, ci].Value));
                        ffs.font_coord[remake].x_offset = new byte[2];
                        ffs.font_coord[remake].x_offset = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[9, ci].Value));
                        ffs.font_coord[remake].x_advanced = new byte[2];
                        ffs.font_coord[remake].x_advanced = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[10, ci].Value));
                        ffs.font_coord[remake].y_offset = new byte[2];
                        ffs.font_coord[remake].y_offset = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[11, ci].Value));
                        ffs.font_coord[remake].last_unknown_data = new byte[4];
                        ffs.font_coord[remake].last_unknown_data = BitConverter.GetBytes(Convert.ToInt32(dataGridCoord[12, ci].Value));
                        ffs.font_coord[remake].new_coordinates = true;
                        ci++;
                    }
                }

                if (kerning_exists == false)
                {
                    for (int j = 0; j < ffs.font_kern.Count; j++)
                    {
                        if (ffs.font_kern[j].kern_index == font_num)
                        {
                            byte[] nil = new byte[2];
                            byte[] nil2 = new byte[4];
                            ffs.kern_add(nil, nil, nil2, font_num, true, ffs.font_head[font_num].fonts_data);


                            ffs.font_kern[ffs.font_kern.Count - 1].first_char = new byte[2];
                            ffs.font_kern[ffs.font_kern.Count - 1].first_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[0, 0].Value.ToString());
                            ffs.font_kern[ffs.font_kern.Count - 1].second_char = new byte[2];
                            ffs.font_kern[ffs.font_kern.Count - 1].second_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[1, 0].Value.ToString());
                            ffs.font_kern[ffs.font_kern.Count - 1].amount = new byte[4];
                            ffs.font_kern[ffs.font_kern.Count - 1].amount = BitConverter.GetBytes(Convert.ToInt32(dataGridView1[2, 0].Value));
                            ffs.font_head[font_num].count_kernings = new byte[2];
                            ffs.font_head[font_num].count_kernings = BitConverter.GetBytes(1);

                            break;
                        }
                    }
                }
                else
                {
                    dataGridView1.RowCount = first_char.Count;
                    ffs.font_head[font_num].count_kernings = new byte[2];
                    ffs.font_head[font_num].count_kernings = BitConverter.GetBytes(first_char.Count);

                    for (int i = 0; i < dataGridView1.RowCount; i++)
                    {
                        dataGridView1[0, i].Value = first_char[i];
                        dataGridView1[1, i].Value = second_char[i];
                        dataGridView1[2, i].Value = amount[i];

                        byte[] nil = new byte[2];
                        byte[] nil2 = new byte[4];
                        ffs.kern_add(nil, nil, nil2, font_num, true, ffs.font_head[font_num].fonts_data);

                        ffs.font_kern[ffs.font_kern.Count - 1].first_char = new byte[2];
                        ffs.font_kern[ffs.font_kern.Count - 1].first_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[0, i].Value.ToString());
                        ffs.font_kern[ffs.font_kern.Count - 1].second_char = new byte[2];
                        ffs.font_kern[ffs.font_kern.Count - 1].second_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[1, i].Value.ToString());
                        ffs.font_kern[ffs.font_kern.Count - 1].amount = new byte[4];
                        ffs.font_kern[ffs.font_kern.Count - 1].amount = BitConverter.GetBytes(Convert.ToInt32(dataGridView1[2, i].Value));

                        for (int j = 0; j < dataGridView1.ColumnCount; j++)
                        {
                            dataGridView1[j, i].Style.BackColor = System.Drawing.Color.DarkOrange;
                        }
                    }

                    kerning_exists = false;
                    first_char.Clear();
                    second_char.Clear();
                    amount.Clear();
                }
            }
        }

        private void importCoordinateFromBMPFontGeneratorXMLtypefntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //ofd.Filter = "TXT (*.txt) | *.txt";
            ofd.Filter = "FNT (*.fnt) | *.fnt";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] all_text = File.ReadAllLines(ofd.FileName);
                int sel_font = 0;
                if (comboBox1.Enabled == true) sel_font = comboBox1.SelectedIndex;
                int ci = 0;
                int kp = 0;
                int dds_num = dataGridTextures.SelectedCells[0].RowIndex;
                int max = 0;
                int line_height = 0;
                int x_right = 0;
                int x_left = 0;
                int y_up = 0;
                int y_down = 0;
                bool has_kernings = false;

                for (int i = 0; i < all_text.Length; i++)
                {
                    if (all_text[i].IndexOf(" padding=") > 0)
                    {
                        string[] get_pad = all_text[i].Split('\"');
                        string padding = get_pad[19];
                        get_pad = padding.Split(',');

                        y_up = Convert.ToInt32(get_pad[0]);
                        y_down = Convert.ToInt32(get_pad[2]);
                        x_left = Convert.ToInt32(get_pad[3]);
                        x_right = Convert.ToInt32(get_pad[1]);
                    }
                    else if (all_text[i].IndexOf("<common ") > 0)
                    {
                        string stroka = all_text[i];
                        string[] coords = stroka.Split('\"');
                        line_height = Convert.ToInt32(coords[1]);
                    }
                    else if (all_text[i].IndexOf("<char id") > 0)
                    {
                        string stroka = all_text[i];
                        string[] coords = stroka.Split('\"');

                        char sym_num = (char)Convert.ToInt32(coords[1]);
                        int x = Convert.ToInt32(coords[3]);
                        int y = Convert.ToInt32(coords[5]);
                        int width = Convert.ToInt32(coords[7]);
                        int height = Convert.ToInt32(coords[9]);
                        int x_offset = Convert.ToInt32(coords[11]) + (x_left + x_right);
                        int x_advanced = Convert.ToInt32(coords[15]);
                        int y_offset = line_height - Convert.ToInt32(coords[13]) - (y_up + y_down);//Convert.ToInt32(coords[13]);//Convert.ToInt32(coords[9]);

                        //if (Convert.ToInt32(coords[13]) < 0) y_offset += Convert.ToInt32(coords[9]);//Convert.ToInt32(coords[13]);

                        int o = -1;

                        for (int j = 0; j < dataGridCoord.RowCount; j++)
                        {
                            string str = dataGridCoord[1, j].Value.ToString();
                            if ((str[0] == sym_num) && (str[1] == '\0'))//dataGridCoord[1, j].Value.ToString() == sym_num.ToString())//ASCIIEncoding.Unicode.GetString(BitConverter.GetBytes(sym_num)))
                            {
                                o = j;
                                break;
                            }
                        }

                        if (o > -1)
                        {
                            //dataGridCoord[0, o].Value = sym_num;
                            //dataGridCoord[1, o].Value = Encoding.Unicode.GetString(BitConverter.GetBytes(sym_num));//ASCIIEncoding.Unicode.GetString(BitConverter.GetBytes(sym_num));
                            dataGridCoord[2, o].Value = x;
                            dataGridCoord[3, o].Value = x + width;
                            dataGridCoord[4, o].Value = y;
                            dataGridCoord[5, o].Value = y + height;
                            dataGridCoord[6, o].Value = width;
                            dataGridCoord[7, o].Value = height;
                            dataGridCoord[8, o].Value = dds_num;
                            dataGridCoord[9, o].Value = x_offset;
                            dataGridCoord[10, o].Value = x_advanced;
                            dataGridCoord[11, o].Value = y_offset;

                            if (max < y + height) max = y + height;
                            

                            for (int k = 0; k < dataGridCoord.ColumnCount; k++)
                            {
                                //dataGridCoord[k, j].Style.BackColor = System.Drawing.Color.MintCream;
                                dataGridCoord[k, o].Style.BackColor = System.Drawing.Color.Yellow;
                            }
                        }
                    }
                    else if (all_text[i].IndexOf("<kernings count=") > 0)
                    {
                        string[] temp = all_text[i].Split('\"');
                        int count = Convert.ToInt32(temp[1]);
                        if (count == 0) count = 1;
                        dataGridView1.RowCount = count;
                        ffs.font_head[sel_font].count_kernings = new byte[4];
                        ffs.font_head[sel_font].count_kernings = BitConverter.GetBytes(count);
                        has_kernings = true;
                    }
                    else if (all_text[i].IndexOf("<kerning first=") > 0)
                    {
                        string[] temp = all_text[i].Split('\"');
                        byte[] first_ch = new byte[2];
                        first_ch = BitConverter.GetBytes(Convert.ToInt32(temp[1]));
                        //char first_ch = (char)Convert.ToInt32(temp[1]);//BitConverter.GetBytes(Convert.ToInt32(temp[1]));
                        byte[] second_ch = new byte[2];
                        second_ch = BitConverter.GetBytes(Convert.ToInt32(temp[3]));
                        //char second_ch = (char)Convert.ToInt32(temp[3]);// BitConverter.GetBytes(Convert.ToInt32(temp[3]));
                        int amount = Convert.ToInt32(temp[5]);

                        dataGridView1[0, kp].Value = UnicodeEncoding.Unicode.GetString(first_ch);//ASCIIEncoding.GetEncoding(MainForm.settings.WINcoding).GetString(first_ch);
                        dataGridView1[1, kp].Value = UnicodeEncoding.Unicode.GetString(second_ch);//ASCIIEncoding.GetEncoding(MainForm.settings.WINcoding).GetString(second_ch);
                        dataGridView1[2, kp].Value = amount;

                        kp++;
                    }
                }

                for (int i = 0; i < BitConverter.ToInt32(ffs.links_count, 0); i++)
                {
                    
                    if (dds_num == BitConverter.ToInt32(ffs.tex_head[i].tex_num, 0))
                    {
                        float get_max_y = (float)Convert.ToDouble(max) / BitConverter.ToInt32(ffs.texture[dds_num].dds_height, 0);
                        ffs.tex_head[i].x_start = new byte[4];
                        ffs.tex_head[i].x_start = BitConverter.GetBytes(0);
                        ffs.tex_head[i].y_start = new byte[4];
                        ffs.tex_head[i].y_start = BitConverter.GetBytes(0);
                        ffs.tex_head[i].x_end = new byte[4];
                        ffs.tex_head[i].x_end = BitConverter.GetBytes(BitConverter.ToSingle(ffs.texture[dds_num].dds_width, 0) / BitConverter.ToSingle(ffs.texture[dds_num].dds_width, 0));
                        ffs.tex_head[i].y_end = new byte[4];
                        ffs.tex_head[i].y_end = BitConverter.GetBytes(get_max_y);
                    }
                }

                for (int remake = 0; remake < ffs.font_coord.Count(); remake++)
                {
                    if (sel_font == ffs.font_coord[remake].index)
                    {
                        if (ffs.texture.Count > 1)
                        {
                            for (int i = 0; i < BitConverter.ToInt32(ffs.links_count, 0); i++)
                            {
                                if (BitConverter.ToInt32(ffs.tex_head[i].tex_num, 0) == Convert.ToInt32(dataGridCoord[8, ci].Value))
                                {
                                    ffs.font_coord[remake].texture_data = new byte[4];
                                    Array.Copy(ffs.tex_head[i].tex_data, 0, ffs.font_coord[remake].texture_data, 0, 4);
                                    break;
                                }
                            }
                        }

                        ffs.font_coord[remake].symbol = new byte[2];
                        ffs.font_coord[remake].symbol = UnicodeEncoding.Unicode.GetBytes(dataGridCoord[1, ci].Value.ToString());
                        ffs.font_coord[remake].x_start = new byte[2];
                        ffs.font_coord[remake].x_start = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[2, ci].Value));
                        ffs.font_coord[remake].y_start = new byte[2];
                        ffs.font_coord[remake].y_start = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[4, ci].Value));
                        ffs.font_coord[remake].coord_width = new byte[2];
                        ffs.font_coord[remake].coord_width = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[6, ci].Value));
                        ffs.font_coord[remake].coord_height = new byte[2];
                        ffs.font_coord[remake].coord_height = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[7, ci].Value));
                        ffs.font_coord[remake].x_offset = new byte[2];
                        ffs.font_coord[remake].x_offset = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[9, ci].Value));
                        ffs.font_coord[remake].x_advanced = new byte[2];
                        ffs.font_coord[remake].x_advanced = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[10, ci].Value));
                        ffs.font_coord[remake].y_offset = new byte[2];
                        ffs.font_coord[remake].y_offset = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[11, ci].Value));
                        ffs.font_coord[remake].last_unknown_data = new byte[4];
                        ffs.font_coord[remake].last_unknown_data = BitConverter.GetBytes(Convert.ToInt32(dataGridCoord[12, ci].Value));
                        ffs.font_coord[remake].new_coordinates = true;
                        ci++;
                    }
                }

                if (has_kernings)
                {
                    for (int i = 0; i < BitConverter.ToInt32(ffs.font_head[sel_font].count_kernings, 0); i++)
                    {
                        byte[] nil = new byte[2];
                        byte[] nil2 = new byte[4];
                        ffs.kern_add(nil, nil, nil2, sel_font, true, ffs.font_head[sel_font].fonts_data);


                        ffs.font_kern[ffs.font_kern.Count - 1].first_char = new byte[2];
                        ffs.font_kern[ffs.font_kern.Count - 1].first_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[0, i].Value.ToString());
                        ffs.font_kern[ffs.font_kern.Count - 1].second_char = new byte[2];
                        ffs.font_kern[ffs.font_kern.Count - 1].second_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[1, i].Value.ToString());
                        ffs.font_kern[ffs.font_kern.Count - 1].amount = new byte[4];
                        ffs.font_kern[ffs.font_kern.Count - 1].amount = BitConverter.GetBytes(Convert.ToInt32(dataGridView1[2, i].Value));
                    }
                }
                else
                {
                    for (int j = 0; j < ffs.font_kern.Count; j++)
                    {
                        if (ffs.font_kern[j].kern_index == sel_font)
                        {
                            byte[] nil = new byte[2];
                            byte[] nil2 = new byte[4];
                            ffs.kern_add(nil, nil, nil2, sel_font, true, ffs.font_head[sel_font].fonts_data);


                            ffs.font_kern[ffs.font_kern.Count - 1].first_char = new byte[2];
                            ffs.font_kern[ffs.font_kern.Count - 1].first_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[0, 0].Value.ToString());
                            ffs.font_kern[ffs.font_kern.Count - 1].second_char = new byte[2];
                            ffs.font_kern[ffs.font_kern.Count - 1].second_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[1, 0].Value.ToString());
                            ffs.font_kern[ffs.font_kern.Count - 1].amount = new byte[4];
                            ffs.font_kern[ffs.font_kern.Count - 1].amount = BitConverter.GetBytes(Convert.ToInt32(dataGridView1[2, 0].Value));
                            ffs.font_head[sel_font].count_kernings = new byte[2];
                            ffs.font_head[sel_font].count_kernings = BitConverter.GetBytes(1);

                            break;
                        }
                    }
                }
                    edited = true;
            }
        }

        private void importCoordinateFromUBFGBMPFonttypefntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FNT-files (*.fnt) | *.fnt";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] file_strings = File.ReadAllLines(ofd.FileName);
                bool has_kernings = false;

                int sel_font = comboBox1.SelectedIndex;
                int x_right = 0;
                int x_left = 0;
                int y_up = 0;
                int y_down = 0;
                int common_height = 0;
                int dds_num = dataGridTextures.SelectedCells[0].RowIndex;
                int kpc = 0; //kerning pair counter

                for (int i = 0; i < file_strings.Length; i++)
                {
                    if (file_strings[i].IndexOf("padding=") > 0)
                    {
                        string[] splitted = file_strings[i].Split(new char[] { ' ', '=', '\"', ',' });

                        x_right = Convert.ToInt32(splitted[splitted.Length - 8]);
                        x_left = Convert.ToInt32(splitted[splitted.Length - 6]);
                        y_up = Convert.ToInt32(splitted[splitted.Length - 9]);
                        y_down = Convert.ToInt32(splitted[splitted.Length - 7]);
                    }
                    else if (file_strings[i].IndexOf("lineHeight=") > 0)
                    {
                        string[] splitted = file_strings[i].Split(new char[] { ' ', '=', ',', '\"'});

                        common_height = Convert.ToInt32(splitted[2]);
                    }
                    else if ((file_strings[i].IndexOf("char id=") >= 0))
                    {
                        string pattern = @"\s+";
                        string[] splitted = System.Text.RegularExpressions.Regex.Split(file_strings[i], pattern);//file_strings[i].Split(' ');

                        for (int j = 0; j < splitted.Length; j++)
                        {
                            if(splitted[j].IndexOf('=') > 0) splitted[j] = splitted[j].Remove(0, splitted[j].IndexOf('=') + 1);
                        }

                        int char_num = Convert.ToInt32(splitted[1]);
                        char cur_char = (char)char_num;
                        int x = Convert.ToInt32(splitted[2]);
                        int y = Convert.ToInt32(splitted[3]);
                        int width = Convert.ToInt32(splitted[4]);
                        int height = Convert.ToInt32(splitted[5]);
                        int x_offset = x_left + x_right + Convert.ToInt32(splitted[6]);
                        int y_offset = common_height - (y_down + y_up) - Convert.ToInt32(splitted[7]);
                        int x_advanced = Convert.ToInt32(splitted[8]);

                        int k = -1;


                        for (int j = 0; j < dataGridCoord.RowCount; j++)
                        {
                            string data_char = dataGridCoord[1, j].Value.ToString();

                            if ((cur_char == data_char[0]) && (data_char[1] == '\0')) k = j;
                        }

                        if (k > -1)
                        {
                            dataGridCoord[2, k].Value = x;
                            dataGridCoord[3, k].Value = x + width;
                            dataGridCoord[4, k].Value = y;
                            dataGridCoord[5, k].Value = y + height;
                            dataGridCoord[6, k].Value = width;
                            dataGridCoord[7, k].Value = height;
                            dataGridCoord[8, k].Value = dds_num;
                            dataGridCoord[9, k].Value = x_offset;
                            dataGridCoord[10, k].Value = x_advanced;
                            dataGridCoord[11, k].Value = y_offset;

                            for (int l = 0; l < dataGridCoord.ColumnCount; l++)
                            {
                                dataGridCoord[l, k].Style.BackColor = System.Drawing.Color.Cyan;
                            }
                        }
                    }
                    else if ((file_strings[i].IndexOf("kernings count=") >= 0))
                    {
                        string[] splitted = file_strings[i].Split('=');
                        dataGridView1.RowCount = Convert.ToInt32(splitted[1]);
                        ffs.font_head[sel_font].count_kernings = BitConverter.GetBytes(Convert.ToInt32(splitted[1]));

                        has_kernings = true;
                    }
                    else if (file_strings[i].IndexOf("kerning first=") >= 0)
                    {
                        string pattern = @"\s+";
                        string[] splitted = System.Text.RegularExpressions.Regex.Split(file_strings[i], pattern);

                        for (int l = 0; l < splitted.Length; l++)
                        {
                            if (splitted[l].IndexOf('=') > 0) splitted[l] = splitted[l].Remove(0, splitted[l].IndexOf('=') + 1);
                        }

                        char first_char = (char)Convert.ToInt32(splitted[1]);
                        char second_char = (char)Convert.ToInt32(splitted[2]);
                        int amount = Convert.ToInt32(splitted[3]);

                        dataGridView1[0, kpc].Value = first_char;
                        dataGridView1[1, kpc].Value = second_char;
                        dataGridView1[2, kpc].Value = amount;
                        
                        for (int k = 0; k < dataGridView1.ColumnCount; k++)
                        {
                            dataGridView1[k, kpc].Style.BackColor = System.Drawing.Color.DarkCyan;
                        }

                        kpc++;
                    }
                }

                for (int i = 0; i < BitConverter.ToInt32(ffs.links_count, 0); i++)
                {
                    if (BitConverter.ToInt32(ffs.tex_head[i].tex_num, 0) == dds_num)
                    {
                        ffs.tex_head[i].x_start = new byte[4];
                        ffs.tex_head[i].x_start = BitConverter.GetBytes(0);
                        ffs.tex_head[i].y_start = new byte[4];
                        ffs.tex_head[i].y_start = BitConverter.GetBytes(0);
                        ffs.tex_head[i].x_end = new byte[4];
                        ffs.tex_head[i].x_end = BitConverter.GetBytes(BitConverter.ToSingle(ffs.texture[dds_num].dds_width, 0) / BitConverter.ToSingle(ffs.texture[dds_num].dds_width, 0));
                        ffs.tex_head[i].y_end = new byte[4];
                        ffs.tex_head[i].y_end = BitConverter.GetBytes(BitConverter.ToSingle(ffs.texture[dds_num].dds_height, 0) / BitConverter.ToSingle(ffs.texture[dds_num].dds_height, 0));
                    }
                }

                int ci = 0;
                for (int remake = 0; remake < ffs.font_coord.Count(); remake++)
                {
                    if (sel_font == ffs.font_coord[remake].index)
                    {
                        if (ffs.texture.Count > 1)
                        {
                            for (int i = 0; i < BitConverter.ToInt32(ffs.links_count, 0); i++)
                            {
                                if (BitConverter.ToInt32(ffs.tex_head[i].tex_num, 0) == Convert.ToInt32(dataGridCoord[8, ci].Value))
                                {
                                    ffs.font_coord[remake].texture_data = new byte[4];
                                    Array.Copy(ffs.tex_head[i].tex_data, 0, ffs.font_coord[remake].texture_data, 0, 4);
                                    break;
                                }
                            }
                        }

                        ffs.font_coord[remake].symbol = new byte[2];
                        ffs.font_coord[remake].symbol = UnicodeEncoding.Unicode.GetBytes(dataGridCoord[1, ci].Value.ToString());
                        ffs.font_coord[remake].x_start = new byte[2];
                        ffs.font_coord[remake].x_start = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[2, ci].Value));
                        ffs.font_coord[remake].y_start = new byte[2];
                        ffs.font_coord[remake].y_start = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[4, ci].Value));
                        ffs.font_coord[remake].coord_width = new byte[2];
                        ffs.font_coord[remake].coord_width = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[6, ci].Value));
                        ffs.font_coord[remake].coord_height = new byte[2];
                        ffs.font_coord[remake].coord_height = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[7, ci].Value));
                        ffs.font_coord[remake].x_offset = new byte[2];
                        ffs.font_coord[remake].x_offset = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[9, ci].Value));
                        ffs.font_coord[remake].x_advanced = new byte[2];
                        ffs.font_coord[remake].x_advanced = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[10, ci].Value));
                        ffs.font_coord[remake].y_offset = new byte[2];
                        ffs.font_coord[remake].y_offset = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[11, ci].Value));
                        ffs.font_coord[remake].last_unknown_data = new byte[4];
                        ffs.font_coord[remake].last_unknown_data = BitConverter.GetBytes(Convert.ToInt32(dataGridCoord[12, ci].Value));
                        ffs.font_coord[remake].new_coordinates = true;
                        ci++;
                    }
                }

                if (has_kernings == false)
                {
                    for (int j = 0; j < ffs.font_kern.Count; j++)
                    {
                        if (ffs.font_kern[j].kern_index == sel_font)
                        {
                            byte[] nil = new byte[2];
                            byte[] nil2 = new byte[4];
                            ffs.kern_add(nil, nil, nil2, sel_font, true, ffs.font_head[sel_font].fonts_data);


                            ffs.font_kern[ffs.font_kern.Count - 1].first_char = new byte[2];
                            ffs.font_kern[ffs.font_kern.Count - 1].first_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[0, 0].Value.ToString());
                            ffs.font_kern[ffs.font_kern.Count - 1].second_char = new byte[2];
                            ffs.font_kern[ffs.font_kern.Count - 1].second_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[1, 0].Value.ToString());
                            ffs.font_kern[ffs.font_kern.Count - 1].amount = new byte[4];
                            ffs.font_kern[ffs.font_kern.Count - 1].amount = BitConverter.GetBytes(Convert.ToInt32(dataGridView1[2, 0].Value));
                            ffs.font_head[sel_font].count_kernings = new byte[2];
                            ffs.font_head[sel_font].count_kernings = BitConverter.GetBytes(1);

                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < dataGridView1.RowCount; i++)
                    {
                        byte[] nil = new byte[2];
                        byte[] nil2 = new byte[4];
                        ffs.kern_add(nil, nil, nil2, sel_font, true, ffs.font_head[sel_font].fonts_data);

                        ffs.font_kern[ffs.font_kern.Count - 1].first_char = new byte[2];
                        ffs.font_kern[ffs.font_kern.Count - 1].first_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[0, i].Value.ToString());
                        ffs.font_kern[ffs.font_kern.Count - 1].second_char = new byte[2];
                        ffs.font_kern[ffs.font_kern.Count - 1].second_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[1, i].Value.ToString());
                        ffs.font_kern[ffs.font_kern.Count - 1].amount = new byte[4];
                        ffs.font_kern[ffs.font_kern.Count - 1].amount = BitConverter.GetBytes(Convert.ToInt32(dataGridView1[2, i].Value));                        
                    }

                    has_kernings = false;
                }
            }
        }

        private void importCoordinateFromShoeBoxfntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FNT-files (*.fnt) | *.fnt";

            //ffs.tex_head[0].x_start;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] file_strings = File.ReadAllLines(ofd.FileName);
                bool has_kernings = false;

                int sel_font = comboBox1.SelectedIndex;
                if (sel_font == -1) sel_font = 0;
                int x_right = 0;
                int x_left = 0;
                int y_up = 0;
                int y_down = 0;
                int common_height = 0;
                int dds_num = dataGridTextures.SelectedCells[0].RowIndex;
                int kpc = 0; //kerning pair counter

                for (int i = 0; i < file_strings.Length; i++)
                {
                    if (file_strings[i].IndexOf("padding=") > 0)
                    {
                        string[] splitted = file_strings[i].Split(new char[] { ' ', '=', '\"', ',' });

                        x_right = Convert.ToInt32(splitted[splitted.Length - 5]);
                        x_left = Convert.ToInt32(splitted[splitted.Length - 5]);
                        y_up = Convert.ToInt32(splitted[splitted.Length - 5]);
                        y_down = Convert.ToInt32(splitted[splitted.Length - 5]);
                    }
                    else if (file_strings[i].IndexOf("lineHeight=") > 0)
                    {
                        string[] splitted = file_strings[i].Split(new char[] { ' ', '=', ',', '\"' });

                        common_height = Convert.ToInt32(splitted[3]);
                    }
                    else if ((file_strings[i].IndexOf("char id=") >= 0))
                    {
                        string pattern = @"\s+";
                        string[] splitted = System.Text.RegularExpressions.Regex.Split(file_strings[i], pattern);//file_strings[i].Split(' ');

                        for (int j = 0; j < splitted.Length; j++)
                        {
                            if (splitted[j].IndexOf('=') > 0)
                            {
                                splitted[j] = splitted[j].Remove(0, splitted[j].IndexOf('=') + 2);
                                splitted[j] = splitted[j].Remove(splitted[j].Length - 1, 1);
                            }
                        }

                        int char_num = Convert.ToInt32(splitted[2]);
                        char cur_char = (char)char_num;
                        int x = Convert.ToInt32(splitted[3]);
                        int y = Convert.ToInt32(splitted[4]);
                        int width = Convert.ToInt32(splitted[5]);
                        int height = Convert.ToInt32(splitted[6]);
                        int x_offset = x_left + x_right + Convert.ToInt32(splitted[7]);
                        int y_offset = common_height - (y_down + y_up) - Convert.ToInt32(splitted[8]);
                        int x_advanced = Convert.ToInt32(splitted[9]);
                        splitted[11] = splitted[11].Remove(0, 1);
                        splitted[11] = splitted[11].Remove(splitted[11].Length - 1, 1);
                        int visible = Convert.ToInt32(splitted[11]);

                        int k = -1;


                        for (int j = 0; j < dataGridCoord.RowCount; j++)
                        {
                            string data_char = dataGridCoord[1, j].Value.ToString();

                            if ((cur_char == data_char[0]) && (data_char[1] == '\0')) k = j;
                        }

                        if (k > -1)
                        {
                            dataGridCoord[2, k].Value = x;
                            dataGridCoord[3, k].Value = x + width;
                            dataGridCoord[4, k].Value = y;
                            dataGridCoord[5, k].Value = y + height;
                            dataGridCoord[6, k].Value = width;
                            dataGridCoord[7, k].Value = height;
                            dataGridCoord[8, k].Value = dds_num;
                            dataGridCoord[9, k].Value = x_offset;
                            dataGridCoord[10, k].Value = x_advanced;
                            dataGridCoord[11, k].Value = y_offset;
                            dataGridCoord[12, k].Value = visible;

                            for (int l = 0; l < dataGridCoord.ColumnCount; l++)
                            {
                                dataGridCoord[l, k].Style.BackColor = System.Drawing.Color.Cyan;
                            }
                        }
                    }
                    else if ((file_strings[i].IndexOf("kernings count=") >= 0))
                    {
                        string[] splitted = file_strings[i].Split('=');
                        splitted[1] = splitted[1].Remove(0, 1);
                        splitted[1] = splitted[1].Remove(splitted[1].Length - 2, 2);

                        if (Convert.ToInt32(splitted[1]) > 0)
                        {
                            dataGridView1.RowCount = Convert.ToInt32(splitted[1]);
                            ffs.font_head[sel_font].count_kernings = BitConverter.GetBytes(Convert.ToInt32(splitted[1]));

                            has_kernings = true;
                        }
                        else has_kernings = false;
                    }
                    else if (file_strings[i].IndexOf("kerning first=") >= 0)
                    {
                        string pattern = @"\s+";
                        string[] splitted = System.Text.RegularExpressions.Regex.Split(file_strings[i], pattern);

                        for (int l = 0; l < splitted.Length; l++)
                        {
                            if (splitted[l].IndexOf('=') > 0)
                            {
                                splitted[l] = splitted[l].Remove(0, splitted[l].IndexOf('=') + 1);
                                splitted[l] = splitted[l].Remove(0, 1);
                                splitted[l] = splitted[l].Remove(splitted[l].Length - 1, 1);
                            }
                        }

                        char first_char = (char)Convert.ToInt32(splitted[2]);
                        char second_char = (char)Convert.ToInt32(splitted[3]);
                        int amount = Convert.ToInt32(splitted[4]);

                        dataGridView1[0, kpc].Value = first_char;
                        dataGridView1[1, kpc].Value = second_char;
                        dataGridView1[2, kpc].Value = amount;

                        for (int k = 0; k < dataGridView1.ColumnCount; k++)
                        {
                            dataGridView1[k, kpc].Style.BackColor = System.Drawing.Color.DarkCyan;
                        }

                        kpc++;
                    }
                }

                for (int i = 0; i < BitConverter.ToInt32(ffs.links_count, 0); i++)
                {
                    if (BitConverter.ToInt32(ffs.tex_head[i].tex_num, 0) == dds_num)
                    {
                        ffs.tex_head[i].x_start = new byte[4];
                        ffs.tex_head[i].x_start = BitConverter.GetBytes(0);
                        ffs.tex_head[i].y_start = new byte[4];
                        ffs.tex_head[i].y_start = BitConverter.GetBytes(0);
                        ffs.tex_head[i].x_end = new byte[4];
                        ffs.tex_head[i].x_end = BitConverter.GetBytes(BitConverter.ToSingle(ffs.texture[dds_num].dds_width, 0) / BitConverter.ToSingle(ffs.texture[dds_num].dds_width, 0));
                        ffs.tex_head[i].y_end = new byte[4];
                        ffs.tex_head[i].y_end = BitConverter.GetBytes(BitConverter.ToSingle(ffs.texture[dds_num].dds_height, 0) / BitConverter.ToSingle(ffs.texture[dds_num].dds_height, 0));
                    }
                }

                int ci = 0;
                for (int remake = 0; remake < ffs.font_coord.Count(); remake++)
                {
                    if (sel_font == ffs.font_coord[remake].index)
                    {
                        if (ffs.texture.Count > 1)
                        {
                            for (int i = 0; i < BitConverter.ToInt32(ffs.links_count, 0); i++)
                            {
                                if (BitConverter.ToInt32(ffs.tex_head[i].tex_num, 0) == Convert.ToInt32(dataGridCoord[8, ci].Value))
                                {
                                    ffs.font_coord[remake].texture_data = new byte[4];
                                    Array.Copy(ffs.tex_head[i].tex_data, 0, ffs.font_coord[remake].texture_data, 0, 4);
                                    break;
                                }
                            }
                        }

                        ffs.font_coord[remake].symbol = new byte[2];
                        ffs.font_coord[remake].symbol = UnicodeEncoding.Unicode.GetBytes(dataGridCoord[1, ci].Value.ToString());
                        ffs.font_coord[remake].x_start = new byte[2];
                        ffs.font_coord[remake].x_start = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[2, ci].Value));
                        ffs.font_coord[remake].y_start = new byte[2];
                        ffs.font_coord[remake].y_start = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[4, ci].Value));
                        ffs.font_coord[remake].coord_width = new byte[2];
                        ffs.font_coord[remake].coord_width = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[6, ci].Value));
                        ffs.font_coord[remake].coord_height = new byte[2];
                        ffs.font_coord[remake].coord_height = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[7, ci].Value));
                        ffs.font_coord[remake].x_offset = new byte[2];
                        ffs.font_coord[remake].x_offset = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[9, ci].Value));
                        ffs.font_coord[remake].x_advanced = new byte[2];
                        ffs.font_coord[remake].x_advanced = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[10, ci].Value));
                        ffs.font_coord[remake].y_offset = new byte[2];
                        ffs.font_coord[remake].y_offset = BitConverter.GetBytes(Convert.ToInt16(dataGridCoord[11, ci].Value));
                        ffs.font_coord[remake].last_unknown_data = new byte[4];
                        ffs.font_coord[remake].last_unknown_data = BitConverter.GetBytes(Convert.ToInt32(dataGridCoord[12, ci].Value));
                        ffs.font_coord[remake].new_coordinates = true;
                        ci++;
                    }
                }

                if (has_kernings == false)
                {
                    for (int j = 0; j < ffs.font_kern.Count; j++)
                    {
                        if (ffs.font_kern[j].kern_index == sel_font)
                        {
                            byte[] nil = new byte[2];
                            byte[] nil2 = new byte[4];
                            ffs.kern_add(nil, nil, nil2, sel_font, true, ffs.font_head[sel_font].fonts_data);


                            ffs.font_kern[ffs.font_kern.Count - 1].first_char = new byte[2];
                            ffs.font_kern[ffs.font_kern.Count - 1].first_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[0, 0].Value.ToString());
                            ffs.font_kern[ffs.font_kern.Count - 1].second_char = new byte[2];
                            ffs.font_kern[ffs.font_kern.Count - 1].second_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[1, 0].Value.ToString());
                            ffs.font_kern[ffs.font_kern.Count - 1].amount = new byte[4];
                            ffs.font_kern[ffs.font_kern.Count - 1].amount = BitConverter.GetBytes(Convert.ToInt32(dataGridView1[2, 0].Value));
                            ffs.font_head[sel_font].count_kernings = new byte[2];
                            ffs.font_head[sel_font].count_kernings = BitConverter.GetBytes(1);

                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < dataGridView1.RowCount; i++)
                    {
                        byte[] nil = new byte[2];
                        byte[] nil2 = new byte[4];
                        ffs.kern_add(nil, nil, nil2, sel_font, true, ffs.font_head[sel_font].fonts_data);

                        ffs.font_kern[ffs.font_kern.Count - 1].first_char = new byte[2];
                        ffs.font_kern[ffs.font_kern.Count - 1].first_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[0, i].Value.ToString());
                        ffs.font_kern[ffs.font_kern.Count - 1].second_char = new byte[2];
                        ffs.font_kern[ffs.font_kern.Count - 1].second_char = UnicodeEncoding.Unicode.GetBytes(dataGridView1[1, i].Value.ToString());
                        ffs.font_kern[ffs.font_kern.Count - 1].amount = new byte[4];
                        ffs.font_kern[ffs.font_kern.Count - 1].amount = BitConverter.GetBytes(Convert.ToInt32(dataGridView1[2, i].Value));
                    }

                    has_kernings = false;
                }
            }
        }

        private void importMultyLitteraFontGeneratorXMLtypefntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Тут начинаются кошмары программистов идеалистов...

            //ModFont mf = new ModFont();
            //mf.Show();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FNT format (*.fnt) | *.fnt";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //Готовлю заранее нужные мне переменные (да, есть тут те, которые я тупо скопировал из других кнопок. Пришлось это тогда делать)
                //Подготовил для работы с текстурой
                int xStart = 0; 
                int yStart = 0; 
                int xEnd = 0; 
                int yEnd = 0;
                float xStartDiv = 0.0f; 
                float yStartDiv = 0.0f;
                float xEndDiv = 0.0f;
                float yEndDiv = 0.0f;
                int yCommutator = 0;
                int ddswidth = BitConverter.ToInt32(ffs.texture[0].dds_width, 0);
                int ddsheight = BitConverter.ToInt32(ffs.texture[0].dds_height, 0);

                int dds_x = 0;
                int dds_y = 0;

                //Получаем информацию о количестве шрифтов
                int font_count = BitConverter.ToInt32(ffs.font_count, 0);
                int check_font_count = 0; //Готовим счётчик для проверки с количество шрифтов в fnt файле
                int font_no = -1;
                //padding
                int x_right = 0; 
                int x_left = 0;  
                int y_up = 0;
                int y_down = 0;
                
                int common_height = 0; //Готовится переменная для правильного вычисления смещения по оси Y (тут считается не как у теллтейлов)
                int dds_num = dataGridTextures.SelectedCells[0].RowIndex; //Тестировал на шрифтах с 1 текстурой, так что потом с повожусь с несколькими шрифтами и текстурами
                
                //Заготовки для магических байтов в шрифтах и текстурах
                int texIndex = 0; 
                int dataIndex = 0;
                int fontIndex = 0;

                string[] par = File.ReadAllLines(ofd.FileName); //Считываю fnt-файл
                
                //Некоторые параметры для программы
                bool has_coord_counter = false; //Проверка на наличие информации о количестве символов в fnt файле
                                                //(да, бывало и такое, что в файле не указывалось количество символов и парных Кернингов)
                bool has_kern_counter = false; //Проверка на наличие о количестве парного Кернинга
                bool first_font = false; //Если найдётся информация о первом шрифте (возможно, от такого костыля избавлюсь, потому что у меня были другие планы по этому поводу)
                int ch_count = 0; //Информация о количестве символов (пригодится при считывании fnt-файла)
                int ker_count = 0; //Информация о количестве парного Кернинга

                uint table_size = 12 + (32 * (uint)font_count) + 4; //Заранее подготовил смещение к таблице с символами
                uint kern_off = table_size;

                Font_Structure newffs = new Font_Structure(); //Готовлю новый класс, чтобы в нём 

                for (int j = 0; j < par.Length; j++) //Прогоняю fnt-файл на наличие нужных параметров
                {
                    if (par[j].IndexOf('"') > 0 && (par[j].IndexOf("fontno=") > 0)) //Нашёл информацию о номере шрифта, плюсуем в счётчике
                    {
                        check_font_count++;
                    }

                    if (par[j].IndexOf("<chars count=") > 0) //Нашёл количество символов, меняем значение переменной на true
                    {
                        has_coord_counter = true;

                        string[] splitted = par[j].Split(new char[] { ' ', '=', '\"', ',' });
                        ch_count = Convert.ToInt32(splitted[5]); //Заодно считаем количество символов и подготавливаем к смещению для парного Кернинга

                        kern_off += (uint)ch_count * 24;
                    }
                    
                    if (par[j].IndexOf("<kernings count=") > 0) //То же самое (только зачем я считаю количество парных Кернингов тут?)
                    {
                        has_kern_counter = true;
                        string[] splitted = par[j].Split(new char[] { ' ', '=', '\"', ',' });
                        ker_count = Convert.ToInt32(splitted[5]);
                    }
                    else if (par[j].IndexOf("<kerning first=") > 0 && has_kern_counter == false) has_kern_counter = true; //И это зачем добавлял?
                }

                if (check_font_count == font_count)//Если количество шрифтов совпадает с проверенным fnt файлом, то работаем дальше
                {
                    ffs.font_kern.Clear(); //Удаляю список парного Кернинга
                    
                    //Сортировка текстур
                    uint sym_off = 0;
                    uint ker_off = 0;

                    for (int i = 0; i < par.Length; i++) //Прогоняем заново считанные строки и выбираем нужные параметры
                    {
                        if (par[i].IndexOf('"') > 0 && (par[i].IndexOf("fontno=") > 0))
                        {
                            if (par[i].IndexOf("padding=") > 0) //Нашёл значение padding, готовим переменные (для правильного отступления друг от друга символов)
                            {
                                string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });

                                x_right = Convert.ToInt32(splitted[41]);
                                x_left = Convert.ToInt32(splitted[43]);
                                y_up = Convert.ToInt32(splitted[42]);
                                y_down = Convert.ToInt32(splitted[44]);

                                font_no = Convert.ToInt32(splitted[splitted.Length - 2]); //Узнаём номер шрифта.
                                if (font_no == 1) first_font = true; //Если он первый, то ставим значение true.


                                //byte[] tex_block = new byte[4];


                                    for (int l = 0; l < ffs.tex_head.Count; l++)//Находим нужные для шрифта магические байты из старого списка
                                    {
                                        for (int k = 0; k < ffs.font_coord.Count(); k++)
                                        {
                                            if (ffs.font_coord[k].index == font_no && ffs.tex_head[l].font_num == ffs.font_coord[k].index)
                                            {
                                                //tex_block = ffs.font_coord[k].texture_data;
                                                dataIndex = k;
                                                texIndex = l;
                                                break;
                                            }
                                        }
                                    }


                                    /*for (int j = 0; j < ffs.tex_head.Count(); j++)
                                    {
                                        if (BitConverter.ToString(ffs.tex_head[j].tex_data) == BitConverter.ToString(tex_block))
                                        {
                                            texIndex = j;
                                            break;
                                        }
                                    }*/
                            }
                        }

                        if ((par[i].IndexOf("lineHeight=") > 0))
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', ',', '\"' });

                            int ind1 = 0, ind2 = 0, ind3 = 0;

                            for (int j = 0; j < splitted.Count(); j++) //Готовим некоторые параметры для текстур (ширина и высота), а также для шрифта общую высоту
                            {
                                if (splitted[j] == "lineHeight" && splitted[j+1] == "")
                                {
                                    ind1 = j + 2;
                                }
                                else if (splitted[j] == "lineHeight" && splitted[j + 1] != "")
                                {
                                    ind1 = j + 1;
                                }

                                if (splitted[j] == "scaleW" && splitted[j + 1] == "")
                                {
                                    ind2 = j + 2;
                                }
                                else if (splitted[j] == "scaleW" && splitted[j + 1] != "")
                                {
                                    ind2 = j + 1;
                                }

                                if (splitted[j] == "scaleH" && splitted[j + 1] == "")
                                {
                                    ind3 = j + 2;
                                }
                                else if (splitted[j] == "scaleH" && splitted[j + 1] != "")
                                {
                                    ind3 = j + 1;
                                }
                            }

                                common_height = Convert.ToInt32(splitted[ind1]);

                            if (first_font) //Если это первый шрифт
                            {
                                //Идёт заготовка с самого начала для указания параметров в текстурах
                                xEnd = Convert.ToInt32(splitted[ind2]);
                                yEnd = Convert.ToInt32(splitted[ind3]);
                                //yCommutator += yEnd;
                                yCommutator = Convert.ToInt32(splitted[ind3]);

                                xStartDiv = 0; //Предполагается, что первый шрифт будет вверху, поэтому поставил в начале 0
                                yStartDiv = 0;
                                xEndDiv = Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);//Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);
                                yEndDiv = Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);//Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);
                                //А тут идёт соотношение окончания ширины и высоты , взятых из fnt-файла на ширину и высоту текстуры

                                //И всё записывается в эти переменные
                                ffs.tex_head[texIndex].x_start = new byte[4];
                                ffs.tex_head[texIndex].x_start = BitConverter.GetBytes(xStartDiv);//xStartDiv);
                                ffs.tex_head[texIndex].y_start = new byte[4];
                                ffs.tex_head[texIndex].y_start = BitConverter.GetBytes(yStartDiv);//yStartDiv);
                                ffs.tex_head[texIndex].x_end = new byte[4];
                                ffs.tex_head[texIndex].x_end = BitConverter.GetBytes(xEndDiv);//xEndDiv);
                                ffs.tex_head[texIndex].y_end = new byte[4];
                                ffs.tex_head[texIndex].y_end = BitConverter.GetBytes(yEndDiv);//yEndDiv);

                                uint tablesize = (32 * (uint)font_count) + 16;
                                sym_off = tablesize; //Готовится параметры для заголовка шрифтов

                                ffs.font_head[font_no - 1].sym_offset = new byte[4];
                                ffs.font_head[font_no - 1].sym_offset = BitConverter.GetBytes(sym_off);

                                /*ffs.font_head[font_no].kern_offset = new byte[4];
                                ffs.font_head[font_no].kern_offset = BitConverter.GetBytes(ker_off);*/

                                first_font = false;
                            }
                            else //То же самое, только должны меняться начала и конец по оси Y
                            {
                                xStart = 0;
                                yStart = yCommutator;
                                xEnd = Convert.ToInt32(splitted[ind2]);
                                yEnd = Convert.ToInt32(splitted[ind3]) + yStart;
                                yCommutator = yEnd;

                                dds_x = Convert.ToInt32(splitted[ind2]);
                                dds_y = Convert.ToInt32(splitted[ind3]);

                                ffs.font_head[font_no - 1].sym_offset = new byte[4];
                                ffs.font_head[font_no - 1].sym_offset = BitConverter.GetBytes(sym_off);

                                /*ffs.font_head[font_no].kern_offset = new byte[4];
                                ffs.font_head[font_no].kern_offset = BitConverter.GetBytes(ker_off);*/

                                xStartDiv = Convert.ToSingle(xStart) / Convert.ToSingle(ddswidth);
                                yStartDiv = Convert.ToSingle(yStart) / Convert.ToSingle(ddsheight);
                                xEndDiv = Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);//Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);
                                yEndDiv = Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);//Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);

                                ffs.tex_head[texIndex].x_start = new byte[4];
                                ffs.tex_head[texIndex].x_start = BitConverter.GetBytes(xStartDiv);//xStartDiv);
                                ffs.tex_head[texIndex].y_start = new byte[4];
                                ffs.tex_head[texIndex].y_start = BitConverter.GetBytes(yStartDiv);//yStartDiv);
                                ffs.tex_head[texIndex].x_end = new byte[4];
                                ffs.tex_head[texIndex].x_end = BitConverter.GetBytes(xEndDiv);//xEndDiv);
                                ffs.tex_head[texIndex].y_end = new byte[4];
                                ffs.tex_head[texIndex].y_end = BitConverter.GetBytes(yEndDiv);//yEndDiv);
                            }
                        }

                        if (par[i].IndexOf("<chars count=") > 0) //Считываем количество символов и записываем в наш список
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                            ch_count = Convert.ToInt32(splitted[5]);

                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_symbols = new byte[2];
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_symbols = BitConverter.GetBytes(ch_count);
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_kernings = BitConverter.GetBytes(0);
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].kern_offset = BitConverter.GetBytes(kern_off);
                        }

                        if (par[i].IndexOf("<char id=") > 0) //Ну а тут просто читаем координаты и записываем в новый список координат
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });

                            int char_id = Convert.ToInt32(splitted[7]);
                            //char ch = (char)char_id;
                            //string s = ch.ToString() + "\0";
                            int x = Convert.ToInt32(splitted[11]);// +x_dds;
                            int y = Convert.ToInt32(splitted[15]);// +dds_y;
                            int width = Convert.ToInt32(splitted[19]);
                            int height = Convert.ToInt32(splitted[23]);
                            int x_offset = (x_left + x_right) + Convert.ToInt32(splitted[27]);
                            int x_advanced = Convert.ToInt32(splitted[35]);
                            int y_offset = common_height - (y_down + y_up) - Convert.ToInt32(splitted[31]);
                            int chnl = Convert.ToInt32(splitted[43]);
                            int index = font_no;
                            int visible = 0;
                            bool Edited = true;

                            if (chnl == 15) visible = 1;

                            byte[] bChar = new byte[2];
                            bChar = BitConverter.GetBytes(char_id);//UnicodeEncoding.GetEncoding(MainForm.settings.WINcoding).GetBytes(s);

                            byte[] bX = new byte[2];
                            bX = BitConverter.GetBytes(x);
                            byte[] bY = new byte[2];
                            bY = BitConverter.GetBytes(y);
                            byte[] bWidth = new byte[2];
                            bWidth = BitConverter.GetBytes(width);
                            byte[] bHeight = new byte[2];
                            bHeight = BitConverter.GetBytes(height);
                            byte[] bXoffset = new byte[2];
                            bXoffset = BitConverter.GetBytes(x_offset);
                            byte[] bXadvanced = new byte[2];
                            bXadvanced = BitConverter.GetBytes(x_advanced);
                            byte[] bYoffset = new byte[2];
                            bYoffset = BitConverter.GetBytes(y_offset);
                            byte[] bVisible = new byte[2];
                            bVisible = BitConverter.GetBytes(visible);

                            byte[] nil = new byte[2];
                            newffs.coord_add(nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, index, Edited, nil);

                            newffs.font_coord[newffs.font_coord.Count - 1].symbol = new byte[2];
                            Array.Copy(bChar, 0, newffs.font_coord[newffs.font_coord.Count - 1].symbol, 0, newffs.font_coord[newffs.font_coord.Count - 1].symbol.Length);
                            
                            newffs.font_coord[newffs.font_coord.Count - 1].x_start = new byte[2];
                            Array.Copy(bX, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_start, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_start.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].y_start = new byte[2];
                            Array.Copy(bY, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_start, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_start.Length);
                            
                            newffs.font_coord[newffs.font_coord.Count - 1].coord_width = new byte[2];
                            Array.Copy(bWidth, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_width, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_width.Length);
                            
                            newffs.font_coord[newffs.font_coord.Count - 1].coord_height = new byte[2];
                            Array.Copy(bHeight, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_height, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_height.Length);
                            
                            newffs.font_coord[newffs.font_coord.Count - 1].x_advanced = new byte[2];
                            Array.Copy(bXadvanced, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_advanced, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_advanced.Length);
                            
                            newffs.font_coord[newffs.font_coord.Count - 1].x_offset = new byte[2];
                            Array.Copy(bXoffset, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_offset, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_offset.Length);
                            
                            newffs.font_coord[newffs.font_coord.Count - 1].y_offset = new byte[2];
                            Array.Copy(bYoffset, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_offset, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_offset.Length);
                            
                            newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data = new byte[4];
                            Array.Copy(bVisible, 0, newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data.Length);
                            
                            newffs.font_coord[newffs.font_coord.Count - 1].texture_data = new byte[4];
                            Array.Copy(ffs.tex_head[texIndex].tex_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].texture_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].texture_data.Length);
                            
                            newffs.font_coord[newffs.font_coord.Count - 1].font_data = new byte[4];
                            Array.Copy(ffs.font_head[fontIndex].fonts_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].font_data, 0, ffs.font_head[ffs.tex_head[fontIndex].font_num - 1].fonts_data.Length);


                            sym_off += 24;
                        }

                        if (par[i].IndexOf("<kernings count=") > 0) //То же самое и с Кернингом
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                            ker_count = Convert.ToInt32(splitted[5]);
                            
                            ffs.font_head[font_no - 1].count_kernings = new byte[2];
                            ffs.font_head[font_no - 1].count_kernings = BitConverter.GetBytes(ker_count);
                            ffs.font_head[font_no - 1].kern_offset = new byte[2];
                            ffs.font_head[font_no - 1].kern_offset = BitConverter.GetBytes(kern_off);
                            kern_off += (uint)ker_count * 8;
                        }

                        if (par[i].IndexOf("<kerning first=") > 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });

                            int f_char_id = Convert.ToInt32(splitted[7]);
                            int s_char_id = Convert.ToInt32(splitted[11]);
                            int amount = Convert.ToInt32(splitted[15]);

                            char f_ch = (char)f_char_id;
                            char s_ch = (char)s_char_id;

                            //string f_s = f_ch.ToString() + "\0";
                            //string s_s = s_ch.ToString() + "\0";

                            int index = font_no;

                            byte[] b_f_Char = new byte[2];
                            b_f_Char = BitConverter.GetBytes(f_char_id);//UnicodeEncoding.GetEncoding(MainForm.settings.WINcoding).GetBytes(f_s);
                            byte[] b_s_Char = new byte[2];
                            b_s_Char = BitConverter.GetBytes(s_char_id);//UnicodeEncoding.GetEncoding(MainForm.settings.WINcoding).GetBytes(s_s);
                            byte[] b_amount = new byte[4];
                            b_amount = BitConverter.GetBytes(amount);


                            byte[] nil = new byte[2];
                            byte[] nil2 = new byte[4];
                            newffs.kern_add(nil, nil, nil2, index, true, nil2);

                            newffs.font_kern[newffs.font_kern.Count - 1].first_char = new byte[2];
                            Array.Copy(b_f_Char, 0, newffs.font_kern[newffs.font_kern.Count - 1].first_char, 0, newffs.font_kern[newffs.font_kern.Count - 1].first_char.Length);
                            
                            newffs.font_kern[newffs.font_kern.Count - 1].second_char = new byte[2];
                            Array.Copy(b_s_Char, 0, newffs.font_kern[newffs.font_kern.Count - 1].second_char, 0, newffs.font_kern[newffs.font_kern.Count - 1].second_char.Length);
                            
                            newffs.font_kern[newffs.font_kern.Count - 1].amount = new byte[4];
                            Array.Copy(b_amount, 0, newffs.font_kern[newffs.font_kern.Count - 1].amount, 0, newffs.font_kern[newffs.font_kern.Count - 1].amount.Length);

                            newffs.font_kern[newffs.font_kern.Count - 1].font_data = new byte[4];
                            Array.Copy(ffs.font_head[ffs.font_coord[fontIndex].index - 1].fonts_data, 0, newffs.font_kern[newffs.font_kern.Count - 1].font_data, 0, ffs.font_head[ffs.font_coord[fontIndex].index - 1].fonts_data.Length);


                            ker_off += 8;
                        }
                    }

                    /*for (int j = 0; j < ffs.font_head.Count; j++)
                    {
                        uint kern_off = BitConverter.ToUInt32(ffs.font_head[j].kern_offset, 0);
                        kern_off += sym_off;

                        ffs.font_head[j].kern_offset = new byte[4];
                        ffs.font_head[j].kern_offset = BitConverter.GetBytes(kern_off);
                    }*/


                        for (int k = 0; k < newffs.font_coord.Count; k++) //Вот тут весь смак. После записи нужных данных, идёт сортировка
                        {
                            for (int m = k - 1; m >= 0; m--)
                            {
                                //Прогоняем и проверяем на наименьший код символа, смещая его в начало, и чтобы он был из нужного шрифта
                                if(BitConverter.ToInt16(newffs.font_coord[m].symbol, 0) >
                                    BitConverter.ToInt16(newffs.font_coord[m + 1].symbol, 0)
                                    && newffs.font_coord[m].index - 1 == newffs.font_coord[m + 1].index - 1) //.index и есть номер шрифта
                                {
                                    byte[] temp_texture_data = newffs.font_coord[m].texture_data;
                                    byte[] temp_symbol = newffs.font_coord[m].symbol;
                                    byte[] temp_x_start = newffs.font_coord[m].x_start;
                                    byte[] temp_y_start = newffs.font_coord[m].y_start;
                                    byte[] temp_coord_width = newffs.font_coord[m].coord_width;
                                    byte[] temp_coord_height = newffs.font_coord[m].coord_height;
                                    byte[] temp_x_advanced = newffs.font_coord[m].x_advanced;
                                    byte[] temp_x_offset = newffs.font_coord[m].x_offset;
                                    byte[] temp_y_offset = newffs.font_coord[m].y_offset;
                                    byte[] temp_last_unknown_data = newffs.font_coord[m].last_unknown_data;
                                    int temp_index = newffs.font_coord[m].index;
                                    byte[] temp_font_data = newffs.font_coord[m].font_data;
                                    bool temp_new_coordinates = newffs.font_coord[m].new_coordinates;

                                    newffs.font_coord[m].texture_data = newffs.font_coord[m + 1].texture_data;
                                    newffs.font_coord[m].symbol = newffs.font_coord[m + 1].symbol;
                                    newffs.font_coord[m].x_start = newffs.font_coord[m + 1].x_start;
                                    newffs.font_coord[m].y_start = newffs.font_coord[m + 1].y_start;
                                    newffs.font_coord[m].coord_width = newffs.font_coord[m + 1].coord_width;
                                    newffs.font_coord[m].coord_height = newffs.font_coord[m + 1].coord_height;
                                    newffs.font_coord[m].x_advanced = newffs.font_coord[m + 1].x_advanced;
                                    newffs.font_coord[m].x_offset = newffs.font_coord[m + 1].x_offset;
                                    newffs.font_coord[m].y_offset = newffs.font_coord[m + 1].y_offset;
                                    newffs.font_coord[m].last_unknown_data = newffs.font_coord[m + 1].last_unknown_data;
                                    newffs.font_coord[m].index = newffs.font_coord[m + 1].index;
                                    newffs.font_coord[m].font_data = newffs.font_coord[m + 1].font_data;
                                    newffs.font_coord[m].new_coordinates = newffs.font_coord[m + 1].new_coordinates;

                                    newffs.font_coord[m + 1].texture_data = temp_texture_data;
                                    newffs.font_coord[m + 1].symbol = temp_symbol;
                                    newffs.font_coord[m + 1].x_start = temp_x_start;
                                    newffs.font_coord[m + 1].y_start = temp_y_start;
                                    newffs.font_coord[m + 1].coord_width = temp_coord_width;
                                    newffs.font_coord[m + 1].coord_height = temp_coord_height;
                                    newffs.font_coord[m + 1].x_advanced = temp_x_advanced;
                                    newffs.font_coord[m + 1].x_offset = temp_x_offset;
                                    newffs.font_coord[m + 1].y_offset = temp_y_offset;
                                    newffs.font_coord[m + 1].last_unknown_data = temp_last_unknown_data;
                                    newffs.font_coord[m + 1].index = temp_index;
                                    newffs.font_coord[m + 1].font_data = temp_font_data;
                                    newffs.font_coord[m + 1].new_coordinates = temp_new_coordinates;
                                }
                        }
                    }

                        //Далее из нового класса копируем в нужные места параметры
                        for (int i = 0; i < newffs.tex_head.Count; i++)
                        {
                            for (int j = newffs.tex_head.Count - i - 1; j >= 0; j--)
                            {
                                if (BitConverter.ToString(ffs.tex_head[j].tex_data) == BitConverter.ToString(newffs.tex_head[i].tex_data))
                                {
                                    ffs.tex_head[j].x_start = newffs.tex_head[i].x_start;
                                    ffs.tex_head[j].y_start = newffs.tex_head[i].x_start;
                                    ffs.tex_head[j].x_end = newffs.tex_head[i].x_end;
                                    ffs.tex_head[j].y_end = newffs.tex_head[i].y_end;
                                    ffs.tex_head[j].tex_num = newffs.tex_head[i].tex_num;
                                    ffs.tex_head[j].tex_data = newffs.tex_head[i].tex_data;
                                }
                            }
                        }

                    ffs.font_coord = newffs.font_coord; //Заменяем старые координаты на новые
                    ffs.font_kern = newffs.font_kern; //То же самое с Кернингом
                    fillcoordtable(0); //Обновляем таблицу координат
                    edited = true; 
                    //В следующих кнопках мелкие отличия: пытался облегчить муки, чтобы не лазить каждый раз в фотошоп, склеивая тупо DDS-ки,
                                                        //пытался из-за одной проги сделать общий формат, чтобы в случае чего удалялись XML-структуры
                                                        //и всё, вроде. Но суть оставалась почти одна и та же.
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DAT-file (*.dat) | *.dat";

            if(ofd.ShowDialog() == DialogResult.OK)
            {
                font_path = ofd.FileName;
                rel_path = font_path.Remove(font_path.Length - 3, 3);
                rel_path += "rel";
                if (File.Exists(rel_path))
                {
                    FileStream fs = new FileStream(rel_path, FileMode.Open);
                    relContent = Methods.ReadFull(fs);
                    fs.Close();

                    rel_exists = true;
                }
                else rel_exists = false;

                openandreadfont(font_path);
                saveAsToolStripMenuItem.Enabled = true;
                saveToolStripMenuItem.Enabled = true;
                tex_count_changed = false;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (edited)
            {
                SaveFont(binContent, font_path);
                rel_exists = false;
                edited = false;
            }
        }

        private void saveAsToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "DAT-file (*.dat) | *.dat";

            if(sfd.ShowDialog() == DialogResult.OK)
            {
                if (!edited)
                {
                    if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                    FileStream fs = new FileStream(sfd.FileName, FileMode.CreateNew);
                    fs.Write(binContent, 0, binContent.Length);
                    fs.Close();

                    rel_exists = false;
                }
                else
                {
                    if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                    SaveFont(binContent, sfd.FileName);
                    rel_exists = false;
                    edited = false;
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void importOldWorkedMethodfntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FNT format (*.fnt) | *.fnt";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] par = File.ReadAllLines(ofd.FileName);

                int xStart = 0;
                int yStart = 0;
                int xEnd = 0;
                int yEnd = 0;
                float xStartDiv = 0.0f;
                float yStartDiv = 0.0f;
                float xEndDiv = 0.0f;
                float yEndDiv = 0.0f;
                int yCommutator = 0;
                int ddswidth = BitConverter.ToInt32(ffs.texture[0].dds_width, 0);
                int ddsheight = BitConverter.ToInt32(ffs.texture[0].dds_height, 0);

                int dds_x = 0;
                int dds_y = 0;

                int sel_font = comboBox1.SelectedIndex;
                if (sel_font == -1) sel_font = 0;

                int font_count = BitConverter.ToInt32(ffs.font_count, 0);
                int check_font_count = 0;
                int font_no = -1;
                int x_right = 0;
                int x_left = 0;
                int y_up = 0;
                int y_down = 0;
                int x_spacing = 0;
                int y_spacing = 0;
                int common_height = 0;
                int dds_num = dataGridTextures.SelectedCells[0].RowIndex;
                int kpc = 0; //kerning pair counter
                int texIndex = 0;
                int dataIndex = 0;
                int fontIndex = 0;

                bool has_coord_counter = false;
                bool has_kern_counter = false;
                bool first_font = false;
                int ch_count = 0;
                int ker_count = 0;

                byte[] TextureFile = null;
                int TextureSize = 0;

                uint table_size = 12 + (32 * (uint)font_count) + 4;
                uint kern_off = table_size;

                int counts = 0;
                int counts1 = 0;

                Font_Structure newffs = new Font_Structure();


                for (int n = 0; n < par.Length; n++) //Убираем дебильные кавычки и скобки от xml файла
                {
                    if ((par[n].IndexOf('<') >= 0) || (par[n].IndexOf('<') >= 0 && par[n].IndexOf('/') > 0))
                    {
                        par[n] = par[n].Remove(par[n].IndexOf('<'), 1);
                        if (par[n].IndexOf('/') >= 0) par[n] = par[n].Remove(par[n].IndexOf('/'), 1);
                    }
                    if (par[n].IndexOf('>') >= 0 || (par[n].IndexOf('/') >= 0 && par[n + 1].IndexOf('>') > 0))
                    {
                        par[n] = par[n].Remove(par[n].IndexOf('>'), 1);
                        if (par[n].IndexOf('/') >= 0) par[n] = par[n].Remove(par[n].IndexOf('/'), 1);
                    }
                    if (par[n].IndexOf('"') >= 0)
                    {
                        while (par[n].IndexOf('"') >= 0) par[n] = par[n].Remove(par[n].IndexOf('"'), 1);
                    }
                }

                for (int j = 0; j < par.Length; j++)
                {
                    if (par[j].IndexOf("fontno=") >= 0)
                    {
                        check_font_count++;
                    }

                    if (par[j].IndexOf("chars count=") >= 0)
                    {
                        has_coord_counter = true;

                        string[] splitted = par[j].Split(new char[] { ' ', '=', ',' });
                        ch_count = Convert.ToInt32(splitted[splitted.Length - 1]);

                        kern_off += (uint)ch_count * 24;
                    }

                    if (par[j].IndexOf("kernings count=") >= 0)
                    {
                        has_kern_counter = true;
                        string[] splitted = par[j].Split(new char[] { ' ', '=', ',' });
                        ker_count = Convert.ToInt32(splitted[splitted.Length - 1]);
                    }
                    else if (par[j].IndexOf("kerning first=") >= 0 && has_kern_counter == false) has_kern_counter = true;
                }

                if (check_font_count == font_count)
                {
                    if ((ffs.texture.Count > 1))
                    {
                        int counter = 1;
                        while (counter <= font_count)
                        {
                            for (int m = 0; m < ffs.tex_head.Count; m++)
                            {
                                if (counter == ffs.tex_head[m].font_num)
                                {
                                    ffs.tex_head[m].tex_num = new byte[4];
                                    ffs.tex_head[m].tex_num = BitConverter.GetBytes(0);

                                    newffs.texhead_add(ffs.tex_head[m].tex_data, ffs.tex_head[m].x_start, ffs.tex_head[m].y_start,
                                        ffs.tex_head[m].x_end, ffs.tex_head[m].y_end, ffs.tex_head[m].tex_num, ffs.tex_head[m].font_num);

                                    counter++;
                                }
                            }
                        }

                        ffs.tex_head = newffs.tex_head;

                        if (BitConverter.ToInt32(ffs.tex_count, 0) != 1) tex_count_changed = true;

                        ffs.tex_count = new byte[4];
                        ffs.tex_count = BitConverter.GetBytes(1);
                        ffs.links_count = new byte[4];
                        ffs.links_count = BitConverter.GetBytes(ffs.tex_head.Count);
                        ffs.texture.Clear();
                        byte[] empty = new byte[4];
                        ffs.dds_add(empty, empty, empty, empty, empty);
                    }

                    ffs.font_kern.Clear();
                    //ffs.font_coord.Clear();

                    FileInfo fi = new FileInfo(ofd.FileName);
                    string FilesPath = fi.DirectoryName;

                    for (int j = 0; j < par.Length; j++)
                    {
                        if (par[j].IndexOf("page id") >= 0 && par[j].IndexOf("file") > 0)
                        {
                            string[] spiltted = par[j].Split(new char[] { ' ', '=' });
                            int ind = 0;

                            for (int t = 0; t < spiltted.Length; t++)
                            {
                                if (spiltted[t] == "file") ind = t + 1;
                            }

                            string GetFilename = spiltted[ind];
                            spiltted = GetFilename.Split('.');
                            GetFilename = FilesPath + "\\" + spiltted[0] + ".dds";

                            if (TextureFile == null && TextureSize == 0)
                            {
                                TextureFile = File.ReadAllBytes(GetFilename);
                                TextureSize = TextureFile.Length;

                                byte[] bWidth = new byte[4];
                                byte[] bHeight = new byte[4];

                                Array.Copy(TextureFile, 12, bHeight, 0, bHeight.Length);
                                Array.Copy(TextureFile, 16, bWidth, 0, bWidth.Length);

                                ddswidth = BitConverter.ToInt32(bWidth, 0);
                                ddsheight = BitConverter.ToInt32(bHeight, 0);
                            }
                            else
                            {
                                byte[] TempFile = File.ReadAllBytes(GetFilename);
                                byte[] temp = TextureFile;
                                TextureFile = new byte[temp.Length + TempFile.Length - 128];
                                Array.Copy(temp, 0, TextureFile, 0, temp.Length);
                                Array.Copy(TempFile, 128, TextureFile, temp.Length, TempFile.Length - 128);
                                TextureSize = TextureFile.Length - 128;

                                byte[] bWidth = new byte[4];
                                byte[] bHeight = new byte[4];

                                Array.Copy(TempFile, 12, bHeight, 0, bHeight.Length);
                                Array.Copy(TempFile, 16, bWidth, 0, bWidth.Length);

                                int tempWidth = BitConverter.ToInt32(bWidth, 0);
                                int tempHeight = BitConverter.ToInt32(bHeight, 0);

                                if (tempWidth != ddswidth && tempWidth > ddswidth) ddswidth = tempWidth;
                                ddsheight += tempHeight;

                                temp = null;
                                TempFile = null;
                                bWidth = null;
                                bHeight = null;
                            }
                        }
                    }

                    ffs.texture[0].dds_height = new byte[4];
                    ffs.texture[0].dds_width = new byte[4];
                    ffs.texture[0].dds_height = BitConverter.GetBytes(ddsheight);
                    ffs.texture[0].dds_width = BitConverter.GetBytes(ddswidth);
                    ffs.texture[0].tex_length = new byte[4];
                    ffs.texture[0].tex_length = BitConverter.GetBytes(TextureSize);
                    ffs.texture[0].dds_content = new byte[TextureFile.Length];
                    Array.Copy(TextureFile, 0, ffs.texture[0].dds_content, 0, TextureFile.Length);
                    Array.Copy(ffs.texture[0].dds_height, 0, ffs.texture[0].dds_content, 12, ffs.texture[0].dds_height.Length);
                    Array.Copy(ffs.texture[0].dds_width, 0, ffs.texture[0].dds_content, 16, ffs.texture[0].dds_width.Length);
                    Array.Copy(ffs.texture[0].tex_length, 0, ffs.texture[0].dds_content, 20, ffs.texture[0].tex_length.Length);

                    ddswidth = BitConverter.ToInt32(ffs.texture[0].dds_width, 0);
                    ddsheight = BitConverter.ToInt32(ffs.texture[0].dds_height, 0);

                    uint sym_off = 0;
                    uint ker_off = 0;
                    for (int i = 0; i < par.Length; i++)
                    {
                        if (par[i].IndexOf("fontno=") >= 0)
                        {
                            if (par[i].IndexOf("padding=") >= 0)
                            {
                                string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                                int ind = 0;
                                int ind2 = 0;

                                for (int t = 0; t < splitted.Length; t++)
                                {
                                    if (splitted[t] == "padding")
                                    {
                                        ind = t + 1;
                                        //break;
                                    }
                                    if (splitted[t] == "spacing")
                                    {
                                        ind2 = t + 1;
                                    }
                                }

                                x_right = Convert.ToInt32(splitted[ind]);
                                x_left = Convert.ToInt32(splitted[ind + 2]);
                                y_up = Convert.ToInt32(splitted[ind + 1]);
                                y_down = Convert.ToInt32(splitted[ind + 3]);

                                x_spacing = Convert.ToInt32(splitted[ind2]);
                                y_spacing = Convert.ToInt32(splitted[ind2 + 1]);


                                font_no = Convert.ToInt32(splitted[splitted.Length - 1]);
                                if (font_no == 1) first_font = true;


                                //byte[] tex_block = new byte[4];


                                for (int l = 0; l < ffs.tex_head.Count; l++)
                                {
                                    for (int k = 0; k < ffs.font_coord.Count(); k++)
                                    {
                                        if (ffs.font_coord[k].index == font_no && ffs.tex_head[l].font_num == ffs.font_coord[k].index)
                                        {
                                            //tex_block = ffs.font_coord[k].texture_data;
                                            dataIndex = k;
                                            texIndex = l;
                                            break;
                                        }
                                    }
                                }


                                /*for (int j = 0; j < ffs.tex_head.Count(); j++)
                                {
                                    if (BitConverter.ToString(ffs.tex_head[j].tex_data) == BitConverter.ToString(tex_block))
                                    {
                                        texIndex = j;
                                        break;
                                    }
                                }*/
                            }
                        }

                        if ((par[i].IndexOf("lineHeight=") >= 0))
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', ',', '\"' });

                            int ind1 = 0, ind2 = 0, ind3 = 0;

                            for (int j = 0; j < splitted.Count(); j++)
                            {
                                if (splitted[j] == "lineHeight")// && splitted[j + 1] != "")
                                {
                                    ind1 = j + 1;
                                }

                                else if (splitted[j] == "scaleW")// && splitted[j + 1] != "")
                                {
                                    ind2 = j + 1;
                                }

                                else if (splitted[j] == "scaleH")// && splitted[j + 1] != "")
                                {
                                    ind3 = j + 1;
                                }
                            }

                            common_height = Convert.ToInt32(splitted[ind1]);

                            if (first_font)
                            {
                                xEnd = Convert.ToInt32(splitted[ind2]);
                                yEnd = Convert.ToInt32(splitted[ind3]);
                                //yCommutator += yEnd;
                                yCommutator = Convert.ToInt32(splitted[ind3]);

                                xStartDiv = 0;
                                yStartDiv = 0;
                                xEndDiv = Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);//Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);
                                yEndDiv = Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);//Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);

                                ffs.tex_head[texIndex].x_start = new byte[4];
                                ffs.tex_head[texIndex].x_start = BitConverter.GetBytes(xStartDiv);//xStartDiv);
                                ffs.tex_head[texIndex].y_start = new byte[4];
                                ffs.tex_head[texIndex].y_start = BitConverter.GetBytes(yStartDiv);//yStartDiv);
                                ffs.tex_head[texIndex].x_end = new byte[4];
                                ffs.tex_head[texIndex].x_end = BitConverter.GetBytes(xEndDiv);//xEndDiv);
                                ffs.tex_head[texIndex].y_end = new byte[4];
                                ffs.tex_head[texIndex].y_end = BitConverter.GetBytes(yEndDiv);//yEndDiv);

                                uint tablesize = (32 * (uint)font_count) + 16;
                                sym_off = tablesize;

                                ffs.font_head[font_no - 1].sym_offset = new byte[4];
                                ffs.font_head[font_no - 1].sym_offset = BitConverter.GetBytes(sym_off);

                                /*ffs.font_head[font_no].kern_offset = new byte[4];
                                ffs.font_head[font_no].kern_offset = BitConverter.GetBytes(ker_off);*/

                                first_font = false;
                            }
                            else
                            {
                                xStart = 0;
                                yStart = yCommutator;
                                xEnd = Convert.ToInt32(splitted[ind2]);
                                yEnd = Convert.ToInt32(splitted[ind3]) + yStart;
                                yCommutator = yEnd;

                                if (yStart == 0) counts++;
                                if (yCommutator == 0) counts1++;

                                dds_x = Convert.ToInt32(splitted[ind2]);
                                dds_y = Convert.ToInt32(splitted[ind3]);

                                ffs.font_head[font_no - 1].sym_offset = new byte[4];
                                ffs.font_head[font_no - 1].sym_offset = BitConverter.GetBytes(sym_off);

                                /*ffs.font_head[font_no].kern_offset = new byte[4];
                                ffs.font_head[font_no].kern_offset = BitConverter.GetBytes(ker_off);*/

                                xStartDiv = Convert.ToSingle(xStart) / Convert.ToSingle(ddswidth);
                                yStartDiv = Convert.ToSingle(yStart) / Convert.ToSingle(ddsheight);
                                xEndDiv = Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);//Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);
                                yEndDiv = Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);//Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);

                                ffs.tex_head[texIndex].x_start = new byte[4];
                                ffs.tex_head[texIndex].x_start = BitConverter.GetBytes(xStartDiv);//xStartDiv);
                                ffs.tex_head[texIndex].y_start = new byte[4];
                                ffs.tex_head[texIndex].y_start = BitConverter.GetBytes(yStartDiv);//yStartDiv);
                                ffs.tex_head[texIndex].x_end = new byte[4];
                                ffs.tex_head[texIndex].x_end = BitConverter.GetBytes(xEndDiv);//xEndDiv);
                                ffs.tex_head[texIndex].y_end = new byte[4];
                                ffs.tex_head[texIndex].y_end = BitConverter.GetBytes(yEndDiv);//yEndDiv);
                            }
                        }

                        if (par[i].IndexOf("chars count=") >= 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                            ch_count = Convert.ToInt32(splitted[splitted.Length - 1]);

                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_symbols = new byte[2];
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_symbols = BitConverter.GetBytes(ch_count);
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_kernings = BitConverter.GetBytes(0);
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].kern_offset = BitConverter.GetBytes(kern_off);
                        }

                        if (par[i].IndexOf("char id=") >= 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                            int ind = 0, ind2 = 0, ind3 = 0, ind4 = 0, ind5 = 0, ind6 = 0, ind7 = 0, ind8 = 0, ind9 = 0;

                            for (int t = 0; t < splitted.Length; t++)
                            {
                                switch (splitted[t])
                                {
                                    case "id":
                                        ind = t + 1;
                                        break;
                                    case "x":
                                        ind2 = t + 1;
                                        break;
                                    case "y":
                                        ind3 = t + 1;
                                        break;
                                    case "width":
                                        ind4 = t + 1;
                                        break;
                                    case "height":
                                        ind5 = t + 1;
                                        break;
                                    case "xoffset":
                                        ind6 = t + 1;
                                        break;
                                    case "yoffset":
                                        ind7 = t + 1;
                                        break;
                                    case "xadvance":
                                        ind8 = t + 1;
                                        break;
                                    case "chnl":
                                        ind9 = t + 1;
                                        break;
                                }
                            }

                            short char_id = Convert.ToInt16(splitted[ind]);
                            //char ch = (char)char_id;
                            //string s = ch.ToString() + "\0";
                            int x = Convert.ToInt32(splitted[ind2]);// +x_dds;
                            int y = Convert.ToInt32(splitted[ind3]);// +dds_y;
                            int width = Convert.ToInt32(splitted[ind4]);
                            int height = Convert.ToInt32(splitted[ind5]);
                            int x_offset = (x_left + x_right + x_spacing) + Convert.ToInt32(splitted[ind6]);// +x_spacing;
                            int x_advanced = Convert.ToInt32(splitted[ind8]);
                            int y_offset = common_height - (y_down + y_up + y_spacing) - Convert.ToInt32(splitted[ind7]);// +y_spacing;
                            int chnl = Convert.ToInt32(splitted[ind9]);
                            int index = font_no;
                            int visible = 0;
                            bool Edited = true;

                            if (chnl == 15) visible = 1;

                            byte[] bChar = new byte[2];
                            bChar = BitConverter.GetBytes(char_id);

                            byte[] bX = new byte[2];
                            bX = BitConverter.GetBytes(x);
                            byte[] bY = new byte[2];
                            bY = BitConverter.GetBytes(y);
                            byte[] bWidth = new byte[2];
                            bWidth = BitConverter.GetBytes(width);
                            byte[] bHeight = new byte[2];
                            bHeight = BitConverter.GetBytes(height);
                            byte[] bXoffset = new byte[2];
                            bXoffset = BitConverter.GetBytes(x_offset);
                            byte[] bXadvanced = new byte[2];
                            bXadvanced = BitConverter.GetBytes(x_advanced);
                            byte[] bYoffset = new byte[2];
                            bYoffset = BitConverter.GetBytes(y_offset);
                            byte[] bVisible = new byte[2];
                            bVisible = BitConverter.GetBytes(visible);

                            byte[] nil = new byte[2];
                            newffs.coord_add(nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, index, Edited, nil);

                            newffs.font_coord[newffs.font_coord.Count - 1].symbol = new byte[2];
                            Array.Copy(bChar, 0, newffs.font_coord[newffs.font_coord.Count - 1].symbol, 0, newffs.font_coord[newffs.font_coord.Count - 1].symbol.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].x_start = new byte[2];
                            Array.Copy(bX, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_start, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_start.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].y_start = new byte[2];
                            Array.Copy(bY, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_start, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_start.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].coord_width = new byte[2];
                            Array.Copy(bWidth, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_width, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_width.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].coord_height = new byte[2];
                            Array.Copy(bHeight, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_height, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_height.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].x_advanced = new byte[2];
                            Array.Copy(bXadvanced, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_advanced, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_advanced.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].x_offset = new byte[2];
                            Array.Copy(bXoffset, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_offset, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_offset.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].y_offset = new byte[2];
                            Array.Copy(bYoffset, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_offset, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_offset.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data = new byte[4];
                            Array.Copy(bVisible, 0, newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].texture_data = new byte[4];
                            Array.Copy(ffs.tex_head[texIndex].tex_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].texture_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].texture_data.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].font_data = new byte[4];
                            Array.Copy(ffs.font_head[fontIndex].fonts_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].font_data, 0, ffs.font_head[ffs.tex_head[fontIndex].font_num - 1].fonts_data.Length);


                            sym_off += 24;
                        }

                        if (par[i].IndexOf("kernings count=") >= 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                            ker_count = Convert.ToInt32(splitted[splitted.Length - 1]);


                            ffs.font_head[font_no - 1].count_kernings = new byte[2];
                            ffs.font_head[font_no - 1].count_kernings = BitConverter.GetBytes(ker_count);
                            ffs.font_head[font_no - 1].kern_offset = new byte[2];
                            ffs.font_head[font_no - 1].kern_offset = BitConverter.GetBytes(kern_off);
                            kern_off += (uint)ker_count * 8;
                        }

                        if (par[i].IndexOf("kerning first=") >= 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });

                            int ind = 0, ind2 = 0, ind3 = 0;

                            for (int t = 0; t < splitted.Length; t++)
                            {
                                switch (splitted[t])
                                {
                                    case "first":
                                        ind = t + 1;
                                        break;
                                    case "second":
                                        ind2 = t + 1;
                                        break;
                                    case "amount":
                                        ind3 = t + 1;
                                        break;
                                }
                            }

                            ushort f_char_id = Convert.ToUInt16(splitted[ind]);
                            ushort s_char_id = Convert.ToUInt16(splitted[ind2]);
                            int amount = Convert.ToInt32(splitted[ind3]);

                            /*char f_ch = (char)f_char_id;
                            char s_ch = (char)s_char_id;

                            string f_s = f_ch.ToString() + "\0";
                            string s_s = s_ch.ToString() + "\0";*/

                            int index = font_no;

                            byte[] b_f_Char = new byte[2];
                            b_f_Char = BitConverter.GetBytes(f_char_id);
                            byte[] b_s_Char = new byte[2];
                            b_s_Char = BitConverter.GetBytes(s_char_id);
                            byte[] b_amount = new byte[4];
                            b_amount = BitConverter.GetBytes(amount);


                            byte[] nil = new byte[2];
                            byte[] nil2 = new byte[4];
                            newffs.kern_add(nil, nil, nil2, index, true, nil2);

                            newffs.font_kern[newffs.font_kern.Count - 1].first_char = new byte[2];
                            Array.Copy(b_f_Char, 0, newffs.font_kern[newffs.font_kern.Count - 1].first_char, 0, newffs.font_kern[newffs.font_kern.Count - 1].first_char.Length);

                            newffs.font_kern[newffs.font_kern.Count - 1].second_char = new byte[2];
                            Array.Copy(b_s_Char, 0, newffs.font_kern[newffs.font_kern.Count - 1].second_char, 0, newffs.font_kern[newffs.font_kern.Count - 1].second_char.Length);

                            newffs.font_kern[newffs.font_kern.Count - 1].amount = new byte[4];
                            Array.Copy(b_amount, 0, newffs.font_kern[newffs.font_kern.Count - 1].amount, 0, newffs.font_kern[newffs.font_kern.Count - 1].amount.Length);

                            newffs.font_kern[newffs.font_kern.Count - 1].font_data = new byte[4];
                            Array.Copy(ffs.font_head[ffs.font_coord[fontIndex].index - 1].fonts_data, 0, newffs.font_kern[newffs.font_kern.Count - 1].font_data, 0, ffs.font_head[ffs.font_coord[fontIndex].index - 1].fonts_data.Length);


                            ker_off += 8;
                        }
                    }

                    /*for (int j = 0; j < ffs.font_head.Count; j++)
                    {
                        uint kern_off = BitConverter.ToUInt32(ffs.font_head[j].kern_offset, 0);
                        kern_off += sym_off;

                        ffs.font_head[j].kern_offset = new byte[4];
                        ffs.font_head[j].kern_offset = BitConverter.GetBytes(kern_off);
                    }*/


                    ResortTable(ref newffs);

                    for (int i = 0; i < newffs.tex_head.Count; i++)
                    {
                        for (int j = newffs.tex_head.Count - i - 1; j >= 0; j--)
                        {
                            if (BitConverter.ToString(ffs.tex_head[j].tex_data) == BitConverter.ToString(newffs.tex_head[i].tex_data))
                            {
                                ffs.tex_head[j].x_start = newffs.tex_head[i].x_start;
                                ffs.tex_head[j].y_start = newffs.tex_head[i].y_start;
                                ffs.tex_head[j].x_end = newffs.tex_head[i].x_end;
                                ffs.tex_head[j].y_end = newffs.tex_head[i].y_end;
                                ffs.tex_head[j].tex_num = newffs.tex_head[i].tex_num;
                                ffs.tex_head[j].tex_data = newffs.tex_head[i].tex_data;
                            }
                        }
                    }



                    ffs.font_coord = newffs.font_coord;
                    ffs.font_kern = newffs.font_kern;
                    filltextable();
                    fillcoordtable(sel_font);
                    edited = true;
                }
            }
        }

        private void importMultiFontGeneratorXMLtypefntExperimentalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Тут начинаются кошмары программистов идеалистов...

            //ModFont mf = new ModFont();
            //mf.Show();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FNT format (*.fnt) | *.fnt";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                int xStart = 0;
                int yStart = 0;
                int xEnd = 0;
                int yEnd = 0;
                float xStartDiv = 0.0f;
                float yStartDiv = 0.0f;
                float xEndDiv = 0.0f;
                float yEndDiv = 0.0f;
                int yCommutator = 0;
                int ddswidth = BitConverter.ToInt32(ffs.texture[0].dds_width, 0);
                int ddsheight = BitConverter.ToInt32(ffs.texture[0].dds_height, 0);

                int dds_x = 0;
                int dds_y = 0;

                int sel_font = comboBox1.SelectedIndex;
                if (sel_font == -1) sel_font = 0;

                int font_count = BitConverter.ToInt32(ffs.font_count, 0);
                int check_font_count = 0;
                int font_no = -1;
                int x_right = 0;
                int x_left = 0;
                int y_up = 0;
                int y_down = 0;
                int common_height = 0;
                int dds_num = dataGridTextures.SelectedCells[0].RowIndex;
                int kpc = 0; //kerning pair counter
                int texIndex = 0;
                int dataIndex = 0;
                int fontIndex = 0;

                ffs.font_kern.Clear();

                string[] par = File.ReadAllLines(ofd.FileName);
                bool has_coord_counter = false;
                bool has_kern_counter = false;
                bool first_font = false;
                int ch_count = 0;
                int ker_count = 0;

                byte[] TextureFile = null;
                int TextureSize = 0;

                uint table_size = 12 + (32 * (uint)font_count) + 4;
                uint kern_off = table_size;

                Font_Structure newffs = new Font_Structure();

                for (int j = 0; j < par.Length; j++)
                {
                    if (par[j].IndexOf('"') > 0 && (par[j].IndexOf("fontno=") > 0))
                    {
                        check_font_count++;
                    }

                    if (par[j].IndexOf("<chars count=") > 0)
                    {
                        has_coord_counter = true;

                        string[] splitted = par[j].Split(new char[] { ' ', '=', '\"', ',' });
                        ch_count = Convert.ToInt32(splitted[5]);

                        kern_off += (uint)ch_count * 24;
                    }

                    if (par[j].IndexOf("<kernings count=") > 0)
                    {
                        has_kern_counter = true;
                        string[] splitted = par[j].Split(new char[] { ' ', '=', '\"', ',' });
                        ker_count = Convert.ToInt32(splitted[5]);
                    }
                    else if (par[j].IndexOf("<kerning first=") > 0 && has_kern_counter == false) has_kern_counter = true;
                }

                if (check_font_count == font_count)
                {
                    //Сортировка текстур

                    FileInfo fi = new FileInfo(ofd.FileName);
                    string FilesPath = fi.DirectoryName;

                    for (int j = 0; j < par.Length; j++)
                    {
                        if (par[j].IndexOf("page id=") > 0 && par[j].IndexOf("file=") > 0)
                        {
                            string[] spiltted = par[j].Split(new char[] { ' ', '=', '\"', ',' });

                            string GetFilename = spiltted[11];
                            spiltted = GetFilename.Split('.');
                            GetFilename = FilesPath + "\\" + spiltted[0] + ".dds";

                            if (TextureFile == null && TextureSize == 0)
                            {
                                TextureFile = File.ReadAllBytes(GetFilename);
                                TextureSize = TextureFile.Length;

                                byte[] bWidth = new byte[4];
                                byte[] bHeight = new byte[4];

                                Array.Copy(TextureFile, 12, bHeight, 0, bHeight.Length);
                                Array.Copy(TextureFile, 16, bWidth, 0, bWidth.Length);

                                ddswidth = BitConverter.ToInt32(bWidth, 0);
                                ddsheight = BitConverter.ToInt32(bHeight, 0);
                            }
                            else
                            {
                                byte[] TempFile = File.ReadAllBytes(GetFilename);
                                byte[] temp = TextureFile;
                                TextureFile = new byte[temp.Length + TempFile.Length - 128];
                                Array.Copy(temp, 0, TextureFile, 0, temp.Length);
                                Array.Copy(TempFile, 128, TextureFile, temp.Length, TempFile.Length - 128);
                                TextureSize = TextureFile.Length - 128;

                                byte[] bWidth = new byte[4];
                                byte[] bHeight = new byte[4];

                                Array.Copy(TempFile, 12, bHeight, 0, bHeight.Length);
                                Array.Copy(TempFile, 16, bWidth, 0, bWidth.Length);

                                int tempWidth = BitConverter.ToInt32(bWidth, 0);
                                int tempHeight = BitConverter.ToInt32(bHeight, 0);

                                if (tempWidth != ddswidth) ddswidth = tempWidth;
                                ddsheight += tempHeight;

                                temp = null;
                                TempFile = null;
                                bWidth = null;
                                bHeight = null;
                            }
                        }
                    }

                    ffs.texture[0].dds_height = new byte[4];
                    ffs.texture[0].dds_width = new byte[4];
                    ffs.texture[0].dds_height = BitConverter.GetBytes(ddsheight);
                    ffs.texture[0].dds_width = BitConverter.GetBytes(ddswidth);
                    ffs.texture[0].tex_length = new byte[4];
                    ffs.texture[0].tex_length = BitConverter.GetBytes(TextureSize);
                    ffs.texture[0].dds_content = new byte[TextureFile.Length];
                    Array.Copy(TextureFile, 0, ffs.texture[0].dds_content, 0, TextureFile.Length);
                    Array.Copy(ffs.texture[0].dds_height, 0, ffs.texture[0].dds_content, 12, ffs.texture[0].dds_height.Length);
                    Array.Copy(ffs.texture[0].dds_width, 0, ffs.texture[0].dds_content, 16, ffs.texture[0].dds_width.Length);
                    Array.Copy(ffs.texture[0].tex_length, 0, ffs.texture[0].dds_content, 20, ffs.texture[0].tex_length.Length);

                    ddswidth = BitConverter.ToInt32(ffs.texture[0].dds_width, 0);
                    ddsheight = BitConverter.ToInt32(ffs.texture[0].dds_height, 0);

                    uint sym_off = 0;
                    uint ker_off = 0;
                    for (int i = 0; i < par.Length; i++)
                    {
                        if (par[i].IndexOf('"') > 0 && (par[i].IndexOf("fontno=") > 0))
                        {
                            if (par[i].IndexOf("padding=") > 0)
                            {
                                string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });

                                x_right = Convert.ToInt32(splitted[41]);
                                x_left = Convert.ToInt32(splitted[43]);
                                y_up = Convert.ToInt32(splitted[42]);
                                y_down = Convert.ToInt32(splitted[44]);

                                font_no = Convert.ToInt32(splitted[splitted.Length - 2]);
                                if (font_no == 1) first_font = true;


                                //byte[] tex_block = new byte[4];


                                for (int l = 0; l < ffs.tex_head.Count; l++)
                                {
                                    for (int k = 0; k < ffs.font_coord.Count(); k++)
                                    {
                                        if (ffs.font_coord[k].index == font_no && ffs.tex_head[l].font_num == ffs.font_coord[k].index)
                                        {
                                            //tex_block = ffs.font_coord[k].texture_data;
                                            dataIndex = k;
                                            texIndex = l;
                                            break;
                                        }
                                    }
                                }


                                /*for (int j = 0; j < ffs.tex_head.Count(); j++)
                                {
                                    if (BitConverter.ToString(ffs.tex_head[j].tex_data) == BitConverter.ToString(tex_block))
                                    {
                                        texIndex = j;
                                        break;
                                    }
                                }*/
                            }
                        }

                        if ((par[i].IndexOf("lineHeight=") > 0))
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', ',', '\"' });

                            int ind1 = 0, ind2 = 0, ind3 = 0;

                            for (int j = 0; j < splitted.Count(); j++)
                            {
                                if (splitted[j] == "lineHeight" && splitted[j + 1] == "")
                                {
                                    ind1 = j + 2;
                                }
                                else if (splitted[j] == "lineHeight" && splitted[j + 1] != "")
                                {
                                    ind1 = j + 1;
                                }

                                if (splitted[j] == "scaleW" && splitted[j + 1] == "")
                                {
                                    ind2 = j + 2;
                                }
                                else if (splitted[j] == "scaleW" && splitted[j + 1] != "")
                                {
                                    ind2 = j + 1;
                                }

                                if (splitted[j] == "scaleH" && splitted[j + 1] == "")
                                {
                                    ind3 = j + 2;
                                }
                                else if (splitted[j] == "scaleH" && splitted[j + 1] != "")
                                {
                                    ind3 = j + 1;
                                }
                            }

                            common_height = Convert.ToInt32(splitted[ind1]);

                            if (first_font)
                            {
                                xEnd = Convert.ToInt32(splitted[ind2]);
                                yEnd = Convert.ToInt32(splitted[ind3]);
                                //yCommutator += yEnd;
                                yCommutator = Convert.ToInt32(splitted[ind3]);

                                xStartDiv = 0;
                                yStartDiv = 0;
                                xEndDiv = Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);//Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);
                                yEndDiv = Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);//Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);

                                ffs.tex_head[texIndex].x_start = new byte[4];
                                ffs.tex_head[texIndex].x_start = BitConverter.GetBytes(xStartDiv);//xStartDiv);
                                ffs.tex_head[texIndex].y_start = new byte[4];
                                ffs.tex_head[texIndex].y_start = BitConverter.GetBytes(yStartDiv);//yStartDiv);
                                ffs.tex_head[texIndex].x_end = new byte[4];
                                ffs.tex_head[texIndex].x_end = BitConverter.GetBytes(xEndDiv);//xEndDiv);
                                ffs.tex_head[texIndex].y_end = new byte[4];
                                ffs.tex_head[texIndex].y_end = BitConverter.GetBytes(yEndDiv);//yEndDiv);

                                uint tablesize = (32 * (uint)font_count) + 16;
                                sym_off = tablesize;

                                ffs.font_head[font_no - 1].sym_offset = new byte[4];
                                ffs.font_head[font_no - 1].sym_offset = BitConverter.GetBytes(sym_off);

                                /*ffs.font_head[font_no].kern_offset = new byte[4];
                                ffs.font_head[font_no].kern_offset = BitConverter.GetBytes(ker_off);*/

                                first_font = false;
                            }
                            else
                            {
                                xStart = 0;
                                yStart = yCommutator;
                                xEnd = Convert.ToInt32(splitted[ind2]);
                                yEnd = Convert.ToInt32(splitted[ind3]) + yStart;
                                yCommutator = yEnd;

                                dds_x = Convert.ToInt32(splitted[ind2]);
                                dds_y = Convert.ToInt32(splitted[ind3]);

                                ffs.font_head[font_no - 1].sym_offset = new byte[4];
                                ffs.font_head[font_no - 1].sym_offset = BitConverter.GetBytes(sym_off);

                                /*ffs.font_head[font_no].kern_offset = new byte[4];
                                ffs.font_head[font_no].kern_offset = BitConverter.GetBytes(ker_off);*/

                                xStartDiv = Convert.ToSingle(xStart) / Convert.ToSingle(ddswidth);
                                yStartDiv = Convert.ToSingle(yStart) / Convert.ToSingle(ddsheight);
                                xEndDiv = Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);//Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);
                                yEndDiv = Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);//Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);

                                ffs.tex_head[texIndex].x_start = new byte[4];
                                ffs.tex_head[texIndex].x_start = BitConverter.GetBytes(xStartDiv);//xStartDiv);
                                ffs.tex_head[texIndex].y_start = new byte[4];
                                ffs.tex_head[texIndex].y_start = BitConverter.GetBytes(yStartDiv);//yStartDiv);
                                ffs.tex_head[texIndex].x_end = new byte[4];
                                ffs.tex_head[texIndex].x_end = BitConverter.GetBytes(xEndDiv);//xEndDiv);
                                ffs.tex_head[texIndex].y_end = new byte[4];
                                ffs.tex_head[texIndex].y_end = BitConverter.GetBytes(yEndDiv);//yEndDiv);
                            }
                        }

                        if (par[i].IndexOf("<chars count=") > 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                            ch_count = Convert.ToInt32(splitted[5]);

                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_symbols = new byte[2];
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_symbols = BitConverter.GetBytes(ch_count);
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_kernings = BitConverter.GetBytes(0);
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].kern_offset = BitConverter.GetBytes(kern_off);
                        }

                        if (par[i].IndexOf("<char id=") > 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });

                            int char_id = Convert.ToInt32(splitted[7]);
                            char ch = (char)char_id;
                            string s = ch.ToString() + "\0";
                            int x = Convert.ToInt32(splitted[11]);// +x_dds;
                            int y = Convert.ToInt32(splitted[15]);// +dds_y;
                            int width = Convert.ToInt32(splitted[19]);
                            int height = Convert.ToInt32(splitted[23]);
                            int x_offset = (x_left + x_right) + Convert.ToInt32(splitted[27]);
                            int x_advanced = Convert.ToInt32(splitted[35]);
                            int y_offset = common_height - (y_down + y_up) - Convert.ToInt32(splitted[31]);
                            int chnl = Convert.ToInt32(splitted[43]);
                            int index = font_no;
                            int visible = 0;
                            bool Edited = true;

                            if (chnl == 15) visible = 1;

                            byte[] bChar = new byte[2];
                            bChar = UnicodeEncoding.Unicode.GetBytes(s);

                            byte[] bX = new byte[2];
                            bX = BitConverter.GetBytes(x);
                            byte[] bY = new byte[2];
                            bY = BitConverter.GetBytes(y);
                            byte[] bWidth = new byte[2];
                            bWidth = BitConverter.GetBytes(width);
                            byte[] bHeight = new byte[2];
                            bHeight = BitConverter.GetBytes(height);
                            byte[] bXoffset = new byte[2];
                            bXoffset = BitConverter.GetBytes(x_offset);
                            byte[] bXadvanced = new byte[2];
                            bXadvanced = BitConverter.GetBytes(x_advanced);
                            byte[] bYoffset = new byte[2];
                            bYoffset = BitConverter.GetBytes(y_offset);
                            byte[] bVisible = new byte[2];
                            bVisible = BitConverter.GetBytes(visible);

                            byte[] nil = new byte[2];
                            newffs.coord_add(nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, index, Edited, nil);

                            newffs.font_coord[newffs.font_coord.Count - 1].symbol = new byte[2];
                            Array.Copy(bChar, 0, newffs.font_coord[newffs.font_coord.Count - 1].symbol, 0, newffs.font_coord[newffs.font_coord.Count - 1].symbol.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].x_start = new byte[2];
                            Array.Copy(bX, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_start, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_start.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].y_start = new byte[2];
                            Array.Copy(bY, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_start, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_start.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].coord_width = new byte[2];
                            Array.Copy(bWidth, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_width, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_width.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].coord_height = new byte[2];
                            Array.Copy(bHeight, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_height, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_height.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].x_advanced = new byte[2];
                            Array.Copy(bXadvanced, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_advanced, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_advanced.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].x_offset = new byte[2];
                            Array.Copy(bXoffset, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_offset, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_offset.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].y_offset = new byte[2];
                            Array.Copy(bYoffset, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_offset, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_offset.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data = new byte[4];
                            Array.Copy(bVisible, 0, newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].texture_data = new byte[4];
                            Array.Copy(ffs.tex_head[texIndex].tex_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].texture_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].texture_data.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].font_data = new byte[4];
                            Array.Copy(ffs.font_head[fontIndex].fonts_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].font_data, 0, ffs.font_head[ffs.tex_head[fontIndex].font_num - 1].fonts_data.Length);


                            sym_off += 24;
                        }

                        if (par[i].IndexOf("<kernings count=") > 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                            ker_count = Convert.ToInt32(splitted[5]);

                            ffs.font_head[font_no - 1].count_kernings = new byte[2];
                            ffs.font_head[font_no - 1].count_kernings = BitConverter.GetBytes(ker_count);
                            ffs.font_head[font_no - 1].kern_offset = new byte[2];
                            ffs.font_head[font_no - 1].kern_offset = BitConverter.GetBytes(kern_off);
                            kern_off += (uint)ker_count * 8;
                        }

                        if (par[i].IndexOf("<kerning first=") > 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });

                            int f_char_id = Convert.ToInt32(splitted[7]);
                            int s_char_id = Convert.ToInt32(splitted[11]);
                            int amount = Convert.ToInt32(splitted[15]);

                            char f_ch = (char)f_char_id;
                            char s_ch = (char)s_char_id;

                            string f_s = f_ch.ToString() + "\0";
                            string s_s = s_ch.ToString() + "\0";

                            int index = font_no;

                            byte[] b_f_Char = new byte[2];
                            b_f_Char = UnicodeEncoding.Unicode.GetBytes(f_s);
                            byte[] b_s_Char = new byte[2];
                            b_s_Char = UnicodeEncoding.Unicode.GetBytes(s_s);
                            byte[] b_amount = new byte[4];
                            b_amount = BitConverter.GetBytes(amount);


                            byte[] nil = new byte[2];
                            byte[] nil2 = new byte[4];
                            newffs.kern_add(nil, nil, nil2, index, true, nil2);

                            newffs.font_kern[newffs.font_kern.Count - 1].first_char = new byte[2];
                            Array.Copy(b_f_Char, 0, newffs.font_kern[newffs.font_kern.Count - 1].first_char, 0, newffs.font_kern[newffs.font_kern.Count - 1].first_char.Length);

                            newffs.font_kern[newffs.font_kern.Count - 1].second_char = new byte[2];
                            Array.Copy(b_s_Char, 0, newffs.font_kern[newffs.font_kern.Count - 1].second_char, 0, newffs.font_kern[newffs.font_kern.Count - 1].second_char.Length);

                            newffs.font_kern[newffs.font_kern.Count - 1].amount = new byte[4];
                            Array.Copy(b_amount, 0, newffs.font_kern[newffs.font_kern.Count - 1].amount, 0, newffs.font_kern[newffs.font_kern.Count - 1].amount.Length);

                            newffs.font_kern[newffs.font_kern.Count - 1].font_data = new byte[4];
                            Array.Copy(ffs.font_head[ffs.font_coord[fontIndex].index - 1].fonts_data, 0, newffs.font_kern[newffs.font_kern.Count - 1].font_data, 0, ffs.font_head[ffs.font_coord[fontIndex].index - 1].fonts_data.Length);


                            ker_off += 8;
                        }
                    }

                    /*for (int j = 0; j < ffs.font_head.Count; j++)
                    {
                        uint kern_off = BitConverter.ToUInt32(ffs.font_head[j].kern_offset, 0);
                        kern_off += sym_off;

                        ffs.font_head[j].kern_offset = new byte[4];
                        ffs.font_head[j].kern_offset = BitConverter.GetBytes(kern_off);
                    }*/


                    for (int k = 0; k < newffs.font_coord.Count; k++)
                    {
                        for (int m = k - 1; m >= 0; m--)
                        {
                            if (BitConverter.ToInt16(newffs.font_coord[m].symbol, 0) >
                                BitConverter.ToInt16(newffs.font_coord[m + 1].symbol, 0)
                                && newffs.font_coord[m].index - 1 == newffs.font_coord[m + 1].index - 1)
                            {
                                byte[] temp_texture_data = newffs.font_coord[m].texture_data;
                                byte[] temp_symbol = newffs.font_coord[m].symbol;
                                byte[] temp_x_start = newffs.font_coord[m].x_start;
                                byte[] temp_y_start = newffs.font_coord[m].y_start;
                                byte[] temp_coord_width = newffs.font_coord[m].coord_width;
                                byte[] temp_coord_height = newffs.font_coord[m].coord_height;
                                byte[] temp_x_advanced = newffs.font_coord[m].x_advanced;
                                byte[] temp_x_offset = newffs.font_coord[m].x_offset;
                                byte[] temp_y_offset = newffs.font_coord[m].y_offset;
                                byte[] temp_last_unknown_data = newffs.font_coord[m].last_unknown_data;
                                int temp_index = newffs.font_coord[m].index;
                                byte[] temp_font_data = newffs.font_coord[m].font_data;
                                bool temp_new_coordinates = newffs.font_coord[m].new_coordinates;

                                newffs.font_coord[m].texture_data = newffs.font_coord[m + 1].texture_data;
                                newffs.font_coord[m].symbol = newffs.font_coord[m + 1].symbol;
                                newffs.font_coord[m].x_start = newffs.font_coord[m + 1].x_start;
                                newffs.font_coord[m].y_start = newffs.font_coord[m + 1].y_start;
                                newffs.font_coord[m].coord_width = newffs.font_coord[m + 1].coord_width;
                                newffs.font_coord[m].coord_height = newffs.font_coord[m + 1].coord_height;
                                newffs.font_coord[m].x_advanced = newffs.font_coord[m + 1].x_advanced;
                                newffs.font_coord[m].x_offset = newffs.font_coord[m + 1].x_offset;
                                newffs.font_coord[m].y_offset = newffs.font_coord[m + 1].y_offset;
                                newffs.font_coord[m].last_unknown_data = newffs.font_coord[m + 1].last_unknown_data;
                                newffs.font_coord[m].index = newffs.font_coord[m + 1].index;
                                newffs.font_coord[m].font_data = newffs.font_coord[m + 1].font_data;
                                newffs.font_coord[m].new_coordinates = newffs.font_coord[m + 1].new_coordinates;

                                newffs.font_coord[m + 1].texture_data = temp_texture_data;
                                newffs.font_coord[m + 1].symbol = temp_symbol;
                                newffs.font_coord[m + 1].x_start = temp_x_start;
                                newffs.font_coord[m + 1].y_start = temp_y_start;
                                newffs.font_coord[m + 1].coord_width = temp_coord_width;
                                newffs.font_coord[m + 1].coord_height = temp_coord_height;
                                newffs.font_coord[m + 1].x_advanced = temp_x_advanced;
                                newffs.font_coord[m + 1].x_offset = temp_x_offset;
                                newffs.font_coord[m + 1].y_offset = temp_y_offset;
                                newffs.font_coord[m + 1].last_unknown_data = temp_last_unknown_data;
                                newffs.font_coord[m + 1].index = temp_index;
                                newffs.font_coord[m + 1].font_data = temp_font_data;
                                newffs.font_coord[m + 1].new_coordinates = temp_new_coordinates;
                            }
                        }
                    }


                    for (int i = 0; i < newffs.tex_head.Count; i++)
                    {
                        for (int j = newffs.tex_head.Count - i - 1; j >= 0; j--)
                        {
                            if (BitConverter.ToString(ffs.tex_head[j].tex_data) == BitConverter.ToString(newffs.tex_head[i].tex_data))
                            {
                                ffs.tex_head[j].x_start = newffs.tex_head[i].x_start;
                                ffs.tex_head[j].y_start = newffs.tex_head[i].x_start;
                                ffs.tex_head[j].x_end = newffs.tex_head[i].x_end;
                                ffs.tex_head[j].y_end = newffs.tex_head[i].y_end;
                                ffs.tex_head[j].tex_num = newffs.tex_head[i].tex_num;
                                ffs.tex_head[j].tex_data = newffs.tex_head[i].tex_data;
                            }
                        }
                    }

                    

                    ffs.font_coord = newffs.font_coord;
                    ffs.font_kern = newffs.font_kern;
                    filltextable();
                    fillcoordtable(sel_font);
                    edited = true;

                }
            }
        }

        private void importTexturesSomehowfntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FNT format (*.fnt) | *.fnt";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                int xStart = 0;
                int yStart = 0;
                int xEnd = 0;
                int yEnd = 0;
                float xStartDiv = 0.0f;
                float yStartDiv = 0.0f;
                float xEndDiv = 0.0f;
                float yEndDiv = 0.0f;
                int yCommutator = 0;
                int ddswidth = BitConverter.ToInt32(ffs.texture[0].dds_width, 0);
                int ddsheight = BitConverter.ToInt32(ffs.texture[0].dds_height, 0);

                int dds_x = 0;
                int dds_y = 0;

                int sel_font = comboBox1.SelectedIndex;
                if (sel_font == -1) sel_font = 0;

                int font_count = BitConverter.ToInt32(ffs.font_count, 0);
                int check_font_count = 0;
                int font_no = -1;
                int x_right = 0;
                int x_left = 0;
                int y_up = 0;
                int y_down = 0;
                int x_spacing = 0;
                int y_spacing = 0;
                int common_height = 0;
                int dds_num = dataGridTextures.SelectedCells[0].RowIndex;
                int kpc = 0; //kerning pair counter
                int texIndex = 0;
                int dataIndex = 0;
                int fontIndex = 0;

                string[] par = File.ReadAllLines(ofd.FileName);
                bool has_coord_counter = false;
                bool has_kern_counter = false;
                bool first_font = false;
                int ch_count = 0;
                int ker_count = 0;

                byte[] TextureFile = null;
                int TextureSize = 0;

                uint table_size = 12 + (32 * (uint)font_count) + 4;
                uint kern_off = table_size;

                Font_Structure newffs = new Font_Structure();

                for (int n = 0; n < par.Length; n++) //Убираем дебильные кавычки и скобки от xml файла
                {
                    if ((par[n].IndexOf('<') >= 0) || (par[n].IndexOf('<') >= 0 && par[n].IndexOf('/') > 0))
                    {
                        par[n] = par[n].Remove(par[n].IndexOf('<'), 1);
                        if (par[n].IndexOf('/') >= 0) par[n] = par[n].Remove(par[n].IndexOf('/'), 1);
                    }
                    if (par[n].IndexOf('>') >= 0 || (par[n].IndexOf('/') >= 0 && par[n + 1].IndexOf('>') > 0))
                    {
                        par[n] = par[n].Remove(par[n].IndexOf('>'), 1);
                        if (par[n].IndexOf('/') >= 0) par[n] = par[n].Remove(par[n].IndexOf('/'), 1);
                    }
                    if (par[n].IndexOf('"') >= 0)
                    {
                        while (par[n].IndexOf('"') >= 0) par[n] = par[n].Remove(par[n].IndexOf('"'), 1);
                    }
                }

                    for (int j = 0; j < par.Length; j++)
                    {
                        if (par[j].IndexOf("fontno=") >= 0)
                        {
                            check_font_count++;
                        }

                        if (par[j].IndexOf("chars count=") >= 0)
                        {
                            has_coord_counter = true;

                            string[] splitted = par[j].Split(new char[] { ' ', '=', ',' });
                            ch_count = Convert.ToInt32(splitted[splitted.Length - 1]);

                            kern_off += (uint)ch_count * 24;
                        }

                        if (par[j].IndexOf("kernings count=") >= 0)
                        {
                            has_kern_counter = true;
                            string[] splitted = par[j].Split(new char[] { ' ', '=', ',' });
                            ker_count = Convert.ToInt32(splitted[splitted.Length - 1]);
                        }
                        else if (par[j].IndexOf("kerning first=") >= 0 && has_kern_counter == false) has_kern_counter = true;
                    }

                if (check_font_count == font_count)
                {
                    //Сортировка текстур

                    FileInfo fi = new FileInfo(ofd.FileName);
                    string FilesPath = fi.DirectoryName;

                    ffs.font_kern.Clear();

                    for (int j = 0; j < par.Length; j++)
                    {
                        if (par[j].IndexOf("page id") >= 0 && par[j].IndexOf("file") > 0)
                        {
                            string[] spiltted = par[j].Split(new char[] { ' ', '=' });
                            int ind = 0;

                            for (int t = 0; t < spiltted.Length; t++)
                            {
                                if (spiltted[t] == "file") ind = t + 1;
                            }

                            string GetFilename = spiltted[ind];
                            spiltted = GetFilename.Split('.');
                            GetFilename = FilesPath + "\\" + spiltted[0] + ".dds";

                            if (TextureFile == null && TextureSize == 0)
                            {
                                TextureFile = File.ReadAllBytes(GetFilename);
                                TextureSize = TextureFile.Length;

                                byte[] bWidth = new byte[4];
                                byte[] bHeight = new byte[4];

                                Array.Copy(TextureFile, 12, bHeight, 0, bHeight.Length);
                                Array.Copy(TextureFile, 16, bWidth, 0, bWidth.Length);

                                ddswidth = BitConverter.ToInt32(bWidth, 0);
                                ddsheight = BitConverter.ToInt32(bHeight, 0);
                            }
                            else
                            {
                                byte[] TempFile = File.ReadAllBytes(GetFilename);
                                byte[] temp = TextureFile;
                                TextureFile = new byte[temp.Length + TempFile.Length - 128];
                                Array.Copy(temp, 0, TextureFile, 0, temp.Length);
                                Array.Copy(TempFile, 128, TextureFile, temp.Length, TempFile.Length - 128);
                                TextureSize = TextureFile.Length - 128;

                                byte[] bWidth = new byte[4];
                                byte[] bHeight = new byte[4];

                                Array.Copy(TempFile, 12, bHeight, 0, bHeight.Length);
                                Array.Copy(TempFile, 16, bWidth, 0, bWidth.Length);

                                int tempWidth = BitConverter.ToInt32(bWidth, 0);
                                int tempHeight = BitConverter.ToInt32(bHeight, 0);

                                if (tempWidth != ddswidth && tempWidth > ddswidth) ddswidth = tempWidth;
                                ddsheight += tempHeight;

                                temp = null;
                                TempFile = null;
                                bWidth = null;
                                bHeight = null;
                            }
                        }
                    }

                    ffs.texture[0].dds_height = new byte[4];
                    ffs.texture[0].dds_width = new byte[4];
                    ffs.texture[0].dds_height = BitConverter.GetBytes(ddsheight);
                    ffs.texture[0].dds_width = BitConverter.GetBytes(ddswidth);
                    ffs.texture[0].tex_length = new byte[4];
                    ffs.texture[0].tex_length = BitConverter.GetBytes(TextureSize);
                    ffs.texture[0].dds_content = new byte[TextureFile.Length];
                    Array.Copy(TextureFile, 0, ffs.texture[0].dds_content, 0, TextureFile.Length);
                    Array.Copy(ffs.texture[0].dds_height, 0, ffs.texture[0].dds_content, 12, ffs.texture[0].dds_height.Length);
                    Array.Copy(ffs.texture[0].dds_width, 0, ffs.texture[0].dds_content, 16, ffs.texture[0].dds_width.Length);
                    Array.Copy(ffs.texture[0].tex_length, 0, ffs.texture[0].dds_content, 20, ffs.texture[0].tex_length.Length);

                    ddswidth = BitConverter.ToInt32(ffs.texture[0].dds_width, 0);
                    ddsheight = BitConverter.ToInt32(ffs.texture[0].dds_height, 0);

                    uint sym_off = 0;
                    uint ker_off = 0;
                    for (int i = 0; i < par.Length; i++)
                    {
                        if (par[i].IndexOf("fontno=") >= 0)
                        {
                            if (par[i].IndexOf("padding=") >= 0)
                            {
                                string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                                int ind = 0;
                                int ind2 = 0;

                                for (int t = 0; t < splitted.Length; t++)
                                {
                                    if (splitted[t] == "padding")
                                    {
                                        ind = t + 1;
                                        //break;
                                    }
                                    if (splitted[t] == "spacing")
                                    {
                                        ind2 = t + 1;
                                    }
                                }

                                x_right = Convert.ToInt32(splitted[ind]);
                                x_left = Convert.ToInt32(splitted[ind + 2]);
                                y_up = Convert.ToInt32(splitted[ind + 1]);
                                y_down = Convert.ToInt32(splitted[ind + 3]);

                                x_spacing = Convert.ToInt32(splitted[ind2]);
                                y_spacing = Convert.ToInt32(splitted[ind2 + 1]);


                                font_no = Convert.ToInt32(splitted[splitted.Length - 1]);
                                if (font_no == 1) first_font = true;


                                //byte[] tex_block = new byte[4];


                                for (int l = 0; l < ffs.tex_head.Count; l++)
                                {
                                    for (int k = 0; k < ffs.font_coord.Count(); k++)
                                    {
                                        if (ffs.font_coord[k].index == font_no && ffs.tex_head[l].font_num == ffs.font_coord[k].index)
                                        {
                                            //tex_block = ffs.font_coord[k].texture_data;
                                            dataIndex = k;
                                            texIndex = l;
                                            break;
                                        }
                                    }
                                }


                                /*for (int j = 0; j < ffs.tex_head.Count(); j++)
                                {
                                    if (BitConverter.ToString(ffs.tex_head[j].tex_data) == BitConverter.ToString(tex_block))
                                    {
                                        texIndex = j;
                                        break;
                                    }
                                }*/
                            }
                        }

                        if ((par[i].IndexOf("lineHeight=") >= 0))
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', ',', '\"' });

                            int ind1 = 0, ind2 = 0, ind3 = 0;

                            for (int j = 0; j < splitted.Count(); j++)
                            {
                                if (splitted[j] == "lineHeight")// && splitted[j + 1] != "")
                                {
                                    ind1 = j + 1;
                                }

                                else if (splitted[j] == "scaleW")// && splitted[j + 1] != "")
                                {
                                    ind2 = j + 1;
                                }

                                else if (splitted[j] == "scaleH")// && splitted[j + 1] != "")
                                {
                                    ind3 = j + 1;
                                }
                            }

                            common_height = Convert.ToInt32(splitted[ind1]);

                            if (first_font)
                            {
                                xEnd = Convert.ToInt32(splitted[ind2]);
                                yEnd = Convert.ToInt32(splitted[ind3]);
                                //yCommutator += yEnd;
                                yCommutator = Convert.ToInt32(splitted[ind3]);

                                xStartDiv = 0;
                                yStartDiv = 0;
                                xEndDiv = Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);//Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);
                                yEndDiv = Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);//Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);

                                ffs.tex_head[texIndex].x_start = new byte[4];
                                ffs.tex_head[texIndex].x_start = BitConverter.GetBytes(xStartDiv);//xStartDiv);
                                ffs.tex_head[texIndex].y_start = new byte[4];
                                ffs.tex_head[texIndex].y_start = BitConverter.GetBytes(yStartDiv);//yStartDiv);
                                ffs.tex_head[texIndex].x_end = new byte[4];
                                ffs.tex_head[texIndex].x_end = BitConverter.GetBytes(xEndDiv);//xEndDiv);
                                ffs.tex_head[texIndex].y_end = new byte[4];
                                ffs.tex_head[texIndex].y_end = BitConverter.GetBytes(yEndDiv);//yEndDiv);

                                uint tablesize = (32 * (uint)font_count) + 16;
                                sym_off = tablesize;

                                ffs.font_head[font_no - 1].sym_offset = new byte[4];
                                ffs.font_head[font_no - 1].sym_offset = BitConverter.GetBytes(sym_off);

                                /*ffs.font_head[font_no].kern_offset = new byte[4];
                                ffs.font_head[font_no].kern_offset = BitConverter.GetBytes(ker_off);*/

                                first_font = false;
                            }
                            else
                            {
                                xStart = 0;
                                yStart = yCommutator;
                                xEnd = Convert.ToInt32(splitted[ind2]);
                                yEnd = Convert.ToInt32(splitted[ind3]) + yStart;
                                yCommutator = yEnd;

                                dds_x = Convert.ToInt32(splitted[ind2]);
                                dds_y = Convert.ToInt32(splitted[ind3]);

                                ffs.font_head[font_no - 1].sym_offset = new byte[4];
                                ffs.font_head[font_no - 1].sym_offset = BitConverter.GetBytes(sym_off);

                                /*ffs.font_head[font_no].kern_offset = new byte[4];
                                ffs.font_head[font_no].kern_offset = BitConverter.GetBytes(ker_off);*/

                                xStartDiv = Convert.ToSingle(xStart) / Convert.ToSingle(ddswidth);
                                yStartDiv = Convert.ToSingle(yStart) / Convert.ToSingle(ddsheight);
                                xEndDiv = Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);//Convert.ToSingle(xEnd) / Convert.ToSingle(ddswidth);
                                yEndDiv = Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);//Convert.ToSingle(yEnd) / Convert.ToSingle(ddsheight);

                                ffs.tex_head[texIndex].x_start = new byte[4];
                                ffs.tex_head[texIndex].x_start = BitConverter.GetBytes(xStartDiv);//xStartDiv);
                                ffs.tex_head[texIndex].y_start = new byte[4];
                                ffs.tex_head[texIndex].y_start = BitConverter.GetBytes(yStartDiv);//yStartDiv);
                                ffs.tex_head[texIndex].x_end = new byte[4];
                                ffs.tex_head[texIndex].x_end = BitConverter.GetBytes(xEndDiv);//xEndDiv);
                                ffs.tex_head[texIndex].y_end = new byte[4];
                                ffs.tex_head[texIndex].y_end = BitConverter.GetBytes(yEndDiv);//yEndDiv);
                            }
                        }

                        if (par[i].IndexOf("chars count=") >= 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                            ch_count = Convert.ToInt32(splitted[splitted.Length - 1]);

                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_symbols = new byte[2];
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_symbols = BitConverter.GetBytes(ch_count);
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].count_kernings = BitConverter.GetBytes(0);
                            ffs.font_head[ffs.font_coord[dataIndex].index - 1].kern_offset = BitConverter.GetBytes(kern_off);
                        }

                        if (par[i].IndexOf("char id=") >= 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                            int ind = 0, ind2 = 0, ind3 = 0, ind4 = 0, ind5 = 0, ind6 = 0, ind7 = 0, ind8 = 0, ind9 = 0;

                            for (int t = 0; t < splitted.Length; t++)
                            {
                                switch (splitted[t])
                                {
                                    case "id":
                                        ind = t + 1;
                                        break;
                                    case "x":
                                        ind2 = t + 1;
                                        break;
                                    case "y":
                                        ind3 = t + 1;
                                        break;
                                    case "width":
                                        ind4 = t + 1;
                                        break;
                                    case "height":
                                        ind5 = t + 1;
                                        break;
                                    case "xoffset":
                                        ind6 = t + 1;
                                        break;
                                    case "yoffset":
                                        ind7 = t + 1;
                                        break;
                                    case "xadvance":
                                        ind8 = t + 1;
                                        break;
                                    case "chnl":
                                        ind9 = t + 1;
                                        break;
                                }
                            }

                            short char_id = Convert.ToInt16(splitted[ind]);
                            //char ch = (char)char_id;
                            //string s = ch.ToString() + "\0";
                            int x = Convert.ToInt32(splitted[ind2]);// +x_dds;
                            int y = Convert.ToInt32(splitted[ind3]);// +dds_y;
                            int width = Convert.ToInt32(splitted[ind4]);
                            int height = Convert.ToInt32(splitted[ind5]);
                            int x_offset = (x_left + x_right + x_spacing) + Convert.ToInt32(splitted[ind6]);// +x_spacing;
                            int x_advanced = Convert.ToInt32(splitted[ind8]);
                            int y_offset = common_height - (y_down + y_up + y_spacing) - Convert.ToInt32(splitted[ind7]);// +y_spacing;
                            int chnl = Convert.ToInt32(splitted[ind9]);
                            int index = font_no;
                            int visible = 0;
                            bool Edited = true;

                            if (chnl == 15) visible = 1;

                            byte[] bChar = new byte[2];
                            bChar = BitConverter.GetBytes(char_id);

                            byte[] bX = new byte[2];
                            bX = BitConverter.GetBytes(x);
                            byte[] bY = new byte[2];
                            bY = BitConverter.GetBytes(y);
                            byte[] bWidth = new byte[2];
                            bWidth = BitConverter.GetBytes(width);
                            byte[] bHeight = new byte[2];
                            bHeight = BitConverter.GetBytes(height);
                            byte[] bXoffset = new byte[2];
                            bXoffset = BitConverter.GetBytes(x_offset);
                            byte[] bXadvanced = new byte[2];
                            bXadvanced = BitConverter.GetBytes(x_advanced);
                            byte[] bYoffset = new byte[2];
                            bYoffset = BitConverter.GetBytes(y_offset);
                            byte[] bVisible = new byte[2];
                            bVisible = BitConverter.GetBytes(visible);

                            byte[] nil = new byte[2];
                            newffs.coord_add(nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, index, Edited, nil);

                            newffs.font_coord[newffs.font_coord.Count - 1].symbol = new byte[2];
                            Array.Copy(bChar, 0, newffs.font_coord[newffs.font_coord.Count - 1].symbol, 0, newffs.font_coord[newffs.font_coord.Count - 1].symbol.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].x_start = new byte[2];
                            Array.Copy(bX, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_start, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_start.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].y_start = new byte[2];
                            Array.Copy(bY, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_start, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_start.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].coord_width = new byte[2];
                            Array.Copy(bWidth, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_width, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_width.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].coord_height = new byte[2];
                            Array.Copy(bHeight, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_height, 0, newffs.font_coord[newffs.font_coord.Count - 1].coord_height.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].x_advanced = new byte[2];
                            Array.Copy(bXadvanced, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_advanced, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_advanced.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].x_offset = new byte[2];
                            Array.Copy(bXoffset, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_offset, 0, newffs.font_coord[newffs.font_coord.Count - 1].x_offset.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].y_offset = new byte[2];
                            Array.Copy(bYoffset, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_offset, 0, newffs.font_coord[newffs.font_coord.Count - 1].y_offset.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data = new byte[4];
                            Array.Copy(bVisible, 0, newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].last_unknown_data.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].texture_data = new byte[4];
                            Array.Copy(ffs.tex_head[texIndex].tex_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].texture_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].texture_data.Length);

                            newffs.font_coord[newffs.font_coord.Count - 1].font_data = new byte[4];
                            Array.Copy(ffs.font_head[fontIndex].fonts_data, 0, newffs.font_coord[newffs.font_coord.Count - 1].font_data, 0, ffs.font_head[ffs.tex_head[fontIndex].font_num - 1].fonts_data.Length);


                            sym_off += 24;
                        }

                        if (par[i].IndexOf("kernings count=") >= 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });
                            ker_count = Convert.ToInt32(splitted[splitted.Length - 1]);


                            ffs.font_head[font_no - 1].count_kernings = new byte[2];
                            ffs.font_head[font_no - 1].count_kernings = BitConverter.GetBytes(ker_count);
                            ffs.font_head[font_no - 1].kern_offset = new byte[2];
                            ffs.font_head[font_no - 1].kern_offset = BitConverter.GetBytes(kern_off);
                            kern_off += (uint)ker_count * 8;
                        }

                        if (par[i].IndexOf("kerning first=") >= 0)
                        {
                            string[] splitted = par[i].Split(new char[] { ' ', '=', '\"', ',' });

                            int ind = 0, ind2 = 0, ind3 = 0;

                            for (int t = 0; t < splitted.Length; t++)
                            {
                                switch (splitted[t])
                                {
                                    case "first":
                                        ind = t + 1;
                                        break;
                                    case "second":
                                        ind2 = t + 1;
                                        break;
                                    case "amount":
                                        ind3 = t + 1;
                                        break;
                                }
                            }

                            short f_char_id = Convert.ToInt16(splitted[ind]);
                            short s_char_id = Convert.ToInt16(splitted[ind2]);
                            int amount = Convert.ToInt32(splitted[ind3]);

                            /*char f_ch = (char)f_char_id;
                            char s_ch = (char)s_char_id;

                            string f_s = f_ch.ToString() + "\0";
                            string s_s = s_ch.ToString() + "\0";*/

                            int index = font_no;

                            byte[] b_f_Char = new byte[2];
                            b_f_Char = BitConverter.GetBytes(f_char_id);
                            byte[] b_s_Char = new byte[2];
                            b_s_Char = BitConverter.GetBytes(s_char_id);
                            byte[] b_amount = new byte[4];
                            b_amount = BitConverter.GetBytes(amount);


                            byte[] nil = new byte[2];
                            byte[] nil2 = new byte[4];
                            newffs.kern_add(nil, nil, nil2, index, true, nil2);

                            newffs.font_kern[newffs.font_kern.Count - 1].first_char = new byte[2];
                            Array.Copy(b_f_Char, 0, newffs.font_kern[newffs.font_kern.Count - 1].first_char, 0, newffs.font_kern[newffs.font_kern.Count - 1].first_char.Length);

                            newffs.font_kern[newffs.font_kern.Count - 1].second_char = new byte[2];
                            Array.Copy(b_s_Char, 0, newffs.font_kern[newffs.font_kern.Count - 1].second_char, 0, newffs.font_kern[newffs.font_kern.Count - 1].second_char.Length);

                            newffs.font_kern[newffs.font_kern.Count - 1].amount = new byte[4];
                            Array.Copy(b_amount, 0, newffs.font_kern[newffs.font_kern.Count - 1].amount, 0, newffs.font_kern[newffs.font_kern.Count - 1].amount.Length);

                            newffs.font_kern[newffs.font_kern.Count - 1].font_data = new byte[4];
                            Array.Copy(ffs.font_head[ffs.font_coord[fontIndex].index - 1].fonts_data, 0, newffs.font_kern[newffs.font_kern.Count - 1].font_data, 0, ffs.font_head[ffs.font_coord[fontIndex].index - 1].fonts_data.Length);


                            ker_off += 8;
                        }
                    }

                    /*for (int j = 0; j < ffs.font_head.Count; j++)
                    {
                        uint kern_off = BitConverter.ToUInt32(ffs.font_head[j].kern_offset, 0);
                        kern_off += sym_off;

                        ffs.font_head[j].kern_offset = new byte[4];
                        ffs.font_head[j].kern_offset = BitConverter.GetBytes(kern_off);
                    }*/


                    for (int k = 0; k < newffs.font_coord.Count; k++)
                    {
                        for (int m = k - 1; m >= 0; m--)
                        {
                            if (BitConverter.ToInt16(newffs.font_coord[m].symbol, 0) >
                                BitConverter.ToInt16(newffs.font_coord[m + 1].symbol, 0)
                                && newffs.font_coord[m].index - 1 == newffs.font_coord[m + 1].index - 1)
                            {
                                byte[] temp_texture_data = newffs.font_coord[m].texture_data;
                                byte[] temp_symbol = newffs.font_coord[m].symbol;
                                byte[] temp_x_start = newffs.font_coord[m].x_start;
                                byte[] temp_y_start = newffs.font_coord[m].y_start;
                                byte[] temp_coord_width = newffs.font_coord[m].coord_width;
                                byte[] temp_coord_height = newffs.font_coord[m].coord_height;
                                byte[] temp_x_advanced = newffs.font_coord[m].x_advanced;
                                byte[] temp_x_offset = newffs.font_coord[m].x_offset;
                                byte[] temp_y_offset = newffs.font_coord[m].y_offset;
                                byte[] temp_last_unknown_data = newffs.font_coord[m].last_unknown_data;
                                int temp_index = newffs.font_coord[m].index;
                                byte[] temp_font_data = newffs.font_coord[m].font_data;
                                bool temp_new_coordinates = newffs.font_coord[m].new_coordinates;

                                newffs.font_coord[m].texture_data = newffs.font_coord[m + 1].texture_data;
                                newffs.font_coord[m].symbol = newffs.font_coord[m + 1].symbol;
                                newffs.font_coord[m].x_start = newffs.font_coord[m + 1].x_start;
                                newffs.font_coord[m].y_start = newffs.font_coord[m + 1].y_start;
                                newffs.font_coord[m].coord_width = newffs.font_coord[m + 1].coord_width;
                                newffs.font_coord[m].coord_height = newffs.font_coord[m + 1].coord_height;
                                newffs.font_coord[m].x_advanced = newffs.font_coord[m + 1].x_advanced;
                                newffs.font_coord[m].x_offset = newffs.font_coord[m + 1].x_offset;
                                newffs.font_coord[m].y_offset = newffs.font_coord[m + 1].y_offset;
                                newffs.font_coord[m].last_unknown_data = newffs.font_coord[m + 1].last_unknown_data;
                                newffs.font_coord[m].index = newffs.font_coord[m + 1].index;
                                newffs.font_coord[m].font_data = newffs.font_coord[m + 1].font_data;
                                newffs.font_coord[m].new_coordinates = newffs.font_coord[m + 1].new_coordinates;

                                newffs.font_coord[m + 1].texture_data = temp_texture_data;
                                newffs.font_coord[m + 1].symbol = temp_symbol;
                                newffs.font_coord[m + 1].x_start = temp_x_start;
                                newffs.font_coord[m + 1].y_start = temp_y_start;
                                newffs.font_coord[m + 1].coord_width = temp_coord_width;
                                newffs.font_coord[m + 1].coord_height = temp_coord_height;
                                newffs.font_coord[m + 1].x_advanced = temp_x_advanced;
                                newffs.font_coord[m + 1].x_offset = temp_x_offset;
                                newffs.font_coord[m + 1].y_offset = temp_y_offset;
                                newffs.font_coord[m + 1].last_unknown_data = temp_last_unknown_data;
                                newffs.font_coord[m + 1].index = temp_index;
                                newffs.font_coord[m + 1].font_data = temp_font_data;
                                newffs.font_coord[m + 1].new_coordinates = temp_new_coordinates;
                            }
                        }
                    }


                    for (int i = 0; i < newffs.tex_head.Count; i++)
                    {
                        for (int j = newffs.tex_head.Count - i - 1; j >= 0; j--)
                        {
                            if (BitConverter.ToString(ffs.tex_head[j].tex_data) == BitConverter.ToString(newffs.tex_head[i].tex_data))
                            {
                                ffs.tex_head[j].x_start = newffs.tex_head[i].x_start;
                                ffs.tex_head[j].y_start = newffs.tex_head[i].x_start;
                                ffs.tex_head[j].x_end = newffs.tex_head[i].x_end;
                                ffs.tex_head[j].y_end = newffs.tex_head[i].y_end;
                                ffs.tex_head[j].tex_num = newffs.tex_head[i].tex_num;
                                ffs.tex_head[j].tex_data = newffs.tex_head[i].tex_data;
                            }
                        }
                    }



                    ffs.font_coord = newffs.font_coord;
                    ffs.font_kern = newffs.font_kern;
                    filltextable();
                    fillcoordtable(sel_font);
                    edited = true;

                }
            }
        }

        private void importMultitexturesfntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FNT file (*.fnt) | *.fnt";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string[] par = File.ReadAllLines(ofd.FileName);
                //Старый исходник хранится в файле Backup_FNTImport_Button.txt
                List<fnt_struct.coords> temp_coord = new List<fnt_struct.coords>();
                List<fnt_struct.kern> temp_kern = new List<fnt_struct.kern>();
                List<fnt_struct.texture_names> temp_tex = new List<fnt_struct.texture_names>();

                int fnt_count = BitConverter.ToInt32(ffs.font_count, 0);
                int[] coords_cnt = { 0 }, kerns_cnt = { 0 };
                FileInfo fi = new FileInfo(ofd.FileName);

                string GetPath = fi.DirectoryName;


                fnt_struct.GetData(par, fnt_count, ref temp_coord, ref temp_kern, ref coords_cnt, ref kerns_cnt, ref temp_tex);

                #region Проверка работы координат
                /*string temp_str = null;
                int fnt_c = 1;


                for (int c = 0; c < fnt_count; c++)
                {
                    temp_str += "Counts of chars: " + coords_cnt[c] + "\r\n";
                    for(int d = 0; d < temp_coord.Count; d++)
                    {
                        if(temp_coord[d].font_no == fnt_c)
                        {
                            temp_str += "Char " + temp_coord[d].char_id + "\tPage " + temp_coord[d].page + "\t" + temp_coord[d].x_start;
                            if (d < temp_coord.Count - 1) temp_str += "\r\n";
                        }
                        
                    }
                    fnt_c++;
                }

                temp_str += "\r\n";

                for (int c = 0; c < temp_tex.Count; c++)
                {
                    temp_str += temp_tex[c].font_num + "\t" + temp_tex[c].tex_num + "\t" + temp_tex[c].page_id + "\t" + temp_tex[c].FileName;
                    if (c < temp_tex.Count - 1) temp_str += "\r\n";
                }

                if (File.Exists(@"CheckCoords.txt")) File.Delete(@"CheckCoords.txt");
                FileStream fs = new FileStream(@"CheckCoords.txt", FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs, Encoding.Unicode);
                sw.Write(temp_str);
                sw.Close();
                fs.Close();

                MessageBox.Show("Done");*/
                #endregion


                if (temp_coord.Count != 0 && coords_cnt.Length != 0 && kerns_cnt.Length != 0 && temp_tex.Count != 0)
                {
                    /*Font_Structure newffs = new Font_Structure();

                    newffs = ffs;

                    newffs.texture.Clear();
                    newffs.font_coord.Clear();
                    newffs.font_kern.Clear();
                    */
                    //MessageBox.Show(temp_coord.Count + "\r\n" + coords_cnt.Length + "\r\n" + kerns_cnt.Length + "\r\n" + temp_tex.Count + "\r\n");
                    //try
                    //{

                    Font_Structure newffs = ffs.Copy();

                    int links_count = BitConverter.ToInt32(ffs.links_count, 0);
                    int tex_num = 1;
                    //newffs.texture.Clear();
                    //newffs.font_coord.Clear();
                    //newffs.font_kern.Clear();


                    if (temp_tex.Count <= links_count)
                    {
                        for (int tex = 0; tex < temp_tex.Count; tex++)
                        {
                            if ((temp_tex[tex].tex_num) > tex_num) tex_num = temp_tex[tex].tex_num;
                        }

                        uint offset = (uint)(40 * temp_tex.Count) + 12 + (uint)(8 * tex_num);

                        if (fnt_struct.TextureLoaded(ref newffs, temp_tex, GetPath, offset))
                        {
                            if (BitConverter.ToInt32(ffs.tex_count, 0) != tex_num + 1) tex_count_changed = true;

                            newffs.tex_count = new byte[4];
                            newffs.tex_count = BitConverter.GetBytes(tex_num + 1);
                            newffs.links_count = new byte[4];
                            newffs.links_count = BitConverter.GetBytes(temp_tex.Count);

                            if (newffs.texture.Count <= BitConverter.ToInt32(newffs.links_count, 0))
                            {
                                offset = (uint)(40 * temp_tex.Count) + 4;
                                int index = 0;

                                for (int i = 0; i < temp_tex.Count; i++)
                                {
                                    newffs.tex_head[i].x_start = new byte[4];
                                    newffs.tex_head[i].x_start = BitConverter.GetBytes(0);
                                    newffs.tex_head[i].y_start = new byte[4];
                                    newffs.tex_head[i].y_start = BitConverter.GetBytes(0);

                                    float x_end = 0.0f, y_end = 0.0f;
                                    int get_ind = 0;

                                    for (int j = 0; j < newffs.texture.Count; j++)
                                    {
                                        if (j == temp_tex[i].tex_num)
                                        {
                                            get_ind = j;
                                            break;
                                        }
                                    }
                                    x_end = BitConverter.ToSingle(newffs.texture[get_ind].dds_width, 0);

                                    y_end = BitConverter.ToSingle(newffs.texture[get_ind].dds_height, 0);

                                    newffs.tex_head[i].x_end = new byte[4];
                                    newffs.tex_head[i].x_end = BitConverter.GetBytes(x_end / x_end);//newffs.texture[i].dds_width;
                                    newffs.tex_head[i].y_end = new byte[4];
                                    newffs.tex_head[i].y_end = BitConverter.GetBytes(y_end / y_end);//newffs.texture[i].dds_height;
                                    newffs.tex_head[i].tex_num = new byte[4];
                                    newffs.tex_head[i].tex_num = BitConverter.GetBytes(temp_tex[i].tex_num);
                                    newffs.tex_head[i].font_num = temp_tex[i].font_num;
                                    index++;
                                }

                                if (index < newffs.tex_head.Count)
                                {
                                    newffs.tex_head.RemoveRange(index, newffs.tex_head.Count - index);
                                }

                                index = 0;
                                int kern_in = 0;

                                for (int i = 0; i < fnt_count; i++)
                                {
                                    for (int j = 0; j < coords_cnt[i]; j++)
                                    {
                                        byte[] nil = new byte[2];

                                        newffs.coord_add(nil, nil, nil, nil, nil, nil, nil, nil, nil, nil, 0, true, nil);

                                        for (int r = 0; r < newffs.tex_head.Count; r++)
                                        {
                                            for (int s = 0; s < temp_coord.Count; s++)
                                            {
                                                if (((newffs.tex_head[r].font_num - 1) == i) && (temp_coord[s].page == BitConverter.ToInt32(newffs.tex_head[r].tex_num, 0)))
                                                {
                                                    newffs.font_coord[index].index = newffs.tex_head[r].font_num;
                                                    newffs.font_coord[index].texture_data = newffs.tex_head[r].tex_data;
                                                    break;
                                                }
                                            }
                                        }

                                        for (int r = 0; r < ffs.font_coord.Count; r++)
                                        {
                                            if (newffs.font_coord[index].index == ffs.font_coord[r].index)
                                            {
                                                newffs.font_coord[index].font_data = new byte[4];
                                                Array.Copy(ffs.font_coord[r].font_data, 0, newffs.font_coord[index].font_data, 0, newffs.font_coord[index].font_data.Length);
                                                break;
                                            }
                                        }

                                        newffs.font_coord[index].symbol = new byte[2];
                                        newffs.font_coord[index].symbol = BitConverter.GetBytes(temp_coord[index].char_id);
                                        newffs.font_coord[index].x_start = new byte[4];
                                        newffs.font_coord[index].x_start = BitConverter.GetBytes(temp_coord[index].x_start);
                                        newffs.font_coord[index].y_start = new byte[4];
                                        newffs.font_coord[index].y_start = BitConverter.GetBytes(temp_coord[index].y_start);
                                        newffs.font_coord[index].coord_width = new byte[4];
                                        newffs.font_coord[index].coord_width = BitConverter.GetBytes(temp_coord[index].width);
                                        newffs.font_coord[index].coord_height = new byte[4];
                                        newffs.font_coord[index].coord_height = BitConverter.GetBytes(temp_coord[index].height);
                                        newffs.font_coord[index].x_offset = new byte[4];
                                        newffs.font_coord[index].x_offset = BitConverter.GetBytes(temp_coord[index].x_offset);
                                        newffs.font_coord[index].y_offset = new byte[4];
                                        newffs.font_coord[index].y_offset = BitConverter.GetBytes(temp_coord[index].y_offset);
                                        newffs.font_coord[index].x_advanced = new byte[4];
                                        newffs.font_coord[index].x_advanced = BitConverter.GetBytes(temp_coord[index].x_advance);
                                        newffs.font_coord[index].last_unknown_data = new byte[4];
                                        if (temp_coord[index].visable) newffs.font_coord[index].last_unknown_data = BitConverter.GetBytes(1);
                                        else newffs.font_coord[index].last_unknown_data = BitConverter.GetBytes(0);
                                        index++;
                                    }

                                    for (int k = 0; k < kerns_cnt[i]; k++)
                                    {
                                        byte[] nil = new byte[2];
                                        newffs.kern_add(nil, nil, nil, 0, true, nil);
                                        newffs.font_kern[kern_in].first_char = new byte[2];
                                        newffs.font_kern[kern_in].first_char = BitConverter.GetBytes(temp_kern[kern_in].first_ch);
                                        newffs.font_kern[kern_in].second_char = new byte[2];
                                        newffs.font_kern[kern_in].second_char = BitConverter.GetBytes(temp_kern[kern_in].second_ch);
                                        newffs.font_kern[kern_in].amount = new byte[4];
                                        newffs.font_kern[kern_in].amount = BitConverter.GetBytes(temp_kern[kern_in].amount);
                                        newffs.font_kern[kern_in].kern_index = temp_kern[kern_in].font_no;

                                        for (int r = 0; r < newffs.font_coord.Count; r++)
                                        {
                                            if (newffs.font_kern[kern_in].kern_index == newffs.font_coord[r].index)
                                            {
                                                newffs.font_kern[kern_in].font_data = newffs.font_coord[r].font_data;
                                                break;
                                            }
                                        }

                                        kern_in++;
                                    }
                                }

                                uint sym_offset = ((uint)fnt_count * 32) + 16; //Количество шрифтов * длину данных о шрифтах + 12 байт до таблиц + 4 байта для выравнивания
                                uint kern_offset = sym_offset + (24 * (uint)newffs.font_coord.Count);

                                for (int t = 0; t < fnt_count; t++)
                                {
                                    for (int l = 0; l < ffs.font_head.Count; l++)
                                    {
                                        for (int m = 0; m < newffs.font_coord.Count; m++)
                                        {
                                            if ((BitConverter.ToString(newffs.font_coord[m].font_data) == BitConverter.ToString(ffs.font_head[l].fonts_data))
                                                && (newffs.font_coord[m].index - 1 == t))
                                            {
                                                ffs.font_head[l].count_kernings = new byte[2];
                                                ffs.font_head[l].count_kernings = BitConverter.GetBytes(Convert.ToInt16(kerns_cnt[t]));
                                                ffs.font_head[l].count_symbols = new byte[2];
                                                ffs.font_head[l].count_symbols = BitConverter.GetBytes(Convert.ToInt16(coords_cnt[t]));
                                                ffs.font_head[l].sym_off = sym_offset;
                                                ffs.font_head[l].kern_off = kern_offset;
                                                ffs.font_head[l].sym_offset = new byte[4];
                                                ffs.font_head[l].sym_offset = BitConverter.GetBytes(sym_offset);
                                                ffs.font_head[l].kern_offset = new byte[4];
                                                ffs.font_head[l].kern_offset = BitConverter.GetBytes(kern_offset);

                                                sym_offset += 24 * (uint)coords_cnt[t];
                                                kern_offset += 8 * (uint)kerns_cnt[t];

                                                break;
                                            }
                                        }
                                    }
                                }

                                ResortTable(ref newffs);

                                ffs.texture = newffs.texture;
                                ffs.links_count = newffs.links_count;
                                ffs.tex_head = newffs.tex_head;
                                ffs.tex_count = newffs.tex_count;
                                ffs.font_coord = newffs.font_coord;
                                ffs.font_kern = newffs.font_kern;
                                filltextable();
                                fillcoordtable(0);
                                edited = true;


                                string test = null;

                                for (int i = 0; i < ffs.tex_head.Count; i++)
                                {
                                    test += "Tex num: " + BitConverter.ToInt32(ffs.tex_head[i].tex_num, 0) + ", font num: " + Convert.ToString(ffs.tex_head[i].font_num) + "\r\n";
                                }

                                label2.Text = test;
                            }
                            //newffs.tex_head.RemoveAt

                        }
                        else
                        {
                            MessageBox.Show("Пока не работает. Я думаю над этим!");
                            //newffs = ffs;
                        }
                    }
                    //}
                    //catch
                   // {
                        
//}
                }
                else MessageBox.Show("Что-то не соответствует для модификации шрифтов!");
                

            }
        }

        

    }
}
