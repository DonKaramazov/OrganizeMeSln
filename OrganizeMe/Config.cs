using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OrganizeMe
{
    public class Config
    {
        public bool IsSurveillanceMode { get; set; }

        private long? _timeLapse;
        public long TimeLapse { get { return _timeLapse ?? 100000; } set { _timeLapse = value; } }

        [XmlArrayItem("Path")]
        public List<String> DirectoriesUnderSurveillance { get; set; }

        public List<Folder> Folders { get; set; }


        public static Config LoadSettings()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Config));
            Config conf = null;
            try
            {
                using (XmlReader reader = XmlReader.Create("settings.xml"))
                {
                    conf = (Config)ser.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                //En cas de probleme un settings.xml vide mais valide est créer 
                CreateEmptyFile();
                Console.WriteLine("Un erreur est survenue dans la lecture de fichier de configuration : {0} \t Un nouveau fichier vide mais valide à été recrée ", ex.Message);
            }

            return conf;
        }

        private static void CreateEmptyFile()
        {
            Config conf = new Config()
            {
                IsSurveillanceMode = false,
                DirectoriesUnderSurveillance = new List<String>() { @"C:\Users\Audouze\Downloads" },
                Folders = new List<Folder>()
                {
                    new Folder() { Name = "Images", Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),Extensions = ".png,.jpg", LibColor = "Blue"  },
                    new Folder() { Name = "MesDocuments", Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),Extensions = ".txt,.doc,.pdf" ,LibColor = "Red" }
                }
            };


            XmlSerializer xs = new XmlSerializer(typeof(Config));
            using (StreamWriter wr = new StreamWriter("settings.xml"))
            {
                xs.Serialize(wr, conf);
            }
        }
    }
}
