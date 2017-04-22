using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace DoctorWhoToolsWorking
{
    public partial class MainForm : Form
    {
        public static Settings settings = new Settings("", "");
        public MainForm()
        {
            InitializeComponent();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.Show();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string xmlpath = Application.StartupPath + "\\config.xml";

            if (File.Exists(xmlpath)) //Check configuration file
            {
                XmlReader configfile = new XmlTextReader(xmlpath);
                XmlSerializer settingsDessializer = new XmlSerializer(typeof(Settings));
                settings = (Settings)settingsDessializer.Deserialize(configfile);
                configfile.Close();
            }
            else
            {
                MessageBox.Show("Config.xml not found. Please, go to settings and check all parametrs.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SettingForm config = new SettingForm();
            config.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TextTool textForm = new TextTool();
            textForm.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FontEditorForm fontForm = new FontEditorForm();
            fontForm.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            EpcViewer epcform = new EpcViewer();
            epcform.Show();
        }
    }
}
