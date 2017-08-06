using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OrganizeMe
{
    public class Folder
    {
        public String Name { get; set; }
        public String Extensions { get; set; }
        public String LibColor { get; set; }
        public String Path { get; set; }

        public ConsoleColor Color {
            get
            {
                if (Enum.TryParse(LibColor, out ConsoleColor color))
                    return color;
                else
                    return ConsoleColor.White;
            }
        }
    }
}
