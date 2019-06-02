using System;
using System.Xml.Serialization;

namespace DoctorWhoToolsWorking
{
    [Serializable()]
    public class Settings
    {
        private string _inputpath;
        private string _outputpath;

        [XmlAttribute("inputpath")]
        public string inputpath
        {
            get
            {
                return _inputpath;
            }
            set
            {
                _inputpath = value;
            }
        }
        [XmlAttribute("outputpath")]
        public string outputpath
        {
            get
            {
                return _outputpath;
            }
            set
            {
                _outputpath = value;
            }
        }
        public Settings(
            string _inputpath,
            string _outputpath)
        {
            this.inputpath = _inputpath;
            this.outputpath = _outputpath;
        }
        public Settings()
        {

        }
    }
}
