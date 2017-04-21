using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace DoctorWhoToolsWorking
{
    public partial class SettingForm : Form
    {
        public SettingForm()
        {
            InitializeComponent();
        }

        public string SetFolder(string str){
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            ofd.ShowNewFolderButton = true;
            if (Directory.Exists(str))
            {
                ofd.SelectedPath = str;
            }
            else ofd.SelectedPath = Application.StartupPath;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                return ofd.SelectedPath;
            }
            else return str;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = MainForm.settings.inputpath;
            textBox2.Text = MainForm.settings.outputpath;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = SetFolder(textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Text = SetFolder(textBox2.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MainForm.settings.inputpath = textBox1.Text;
            MainForm.settings.outputpath = textBox2.Text;

            string confpath = Application.StartupPath + "\\config.xml";
            XmlSerializer xmlser = new XmlSerializer(typeof(Settings));
            TextWriter xmlconf = new StreamWriter(confpath);
            xmlser.Serialize(xmlconf, MainForm.settings);
            xmlconf.Flush();
            xmlconf.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button3_Click(sender, e);
            Close();
        }
    }
}
