using GitLogViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GitLogViewer.Models;

namespace GitLogViewer.Services
{
    public static class XmlParser
    {
        public static List<PersonModel> ParseModelConfig(string folderPath)
        {
            string filePath = Path.Combine(folderPath, "ModelConfig.xml");
            var list = new List<PersonModel>();

            if (!File.Exists(filePath))
                return list;

            var doc = XDocument.Load(filePath);
            foreach (var data in doc.Descendants("Data"))
            {
                var name = data.Element("name")?.Attribute("Value")?.Value ?? "";
                var lastname = data.Element("lastname")?.Attribute("Value")?.Value ?? "";

                list.Add(new PersonModel
                {
                    Name = name,
                    Lastname = lastname
                });
            }

            return list;
        }
    }
}
