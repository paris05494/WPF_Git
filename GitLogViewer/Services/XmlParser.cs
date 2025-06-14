using GitLogViewer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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

        public static void RevertBackup(string folderPath)
        {
            string filePath = Path.Combine(folderPath, "ModelConfig.xml");
            string backupPath = filePath + ".bak";

            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, filePath, overwrite: true);
            }
        }

        public static void MergeModelConfig(string folderPath, List<PersonModel> copiedPeople)
        {
            string filePath = Path.Combine(folderPath, "ModelConfig.xml");

            var existingPeople = ParseModelConfig(folderPath);

            // รวมข้อมูลแบบ merge
            foreach (var copy in copiedPeople)
            {
                var existing = existingPeople.FirstOrDefault(p => p.Name == copy.Name);
                if (existing != null)
                {
                    existing.Lastname = copy.Lastname; // ✅ อัปเดต lastname ถ้าชื่อซ้ำ
                }
                else
                {
                    existingPeople.Add(copy); // ✅ เพิ่มใหม่ถ้าไม่ซ้ำ
                }
            }

            // สร้าง XML
            var doc = new XDocument(
                new XElement("ModelConfig",
                    new XElement("processors",
                        existingPeople.Select(p =>
                            new XElement("Data",
                                new XElement("name", new XAttribute("Value", p.Name)),
                                new XElement("lastname", new XAttribute("Value", p.Lastname))
                            )
                        )
                    )
                )
            );

            Directory.CreateDirectory(folderPath);
            doc.Save(filePath);
        }

    }
}
