using System;
using System.IO;
using System.Xml;

namespace FGMerge
{
    public class FileWriter : IFileWriter
    {
        public bool WriteFile(string fileName, XmlDocument document)
        {
            try
            {
                document.PreserveWhitespace = false;
                using StringWriter stringWriter = new();
                document.Save(stringWriter);
                document.LoadXml(stringWriter.ToString());

                XmlWriterSettings settings = new()
                {
                    Indent = true,
                    IndentChars = "\t",
                    NewLineHandling = NewLineHandling.Entitize,
                    NewLineChars = Environment.NewLine
                };
                using FileStream outStream = new(fileName, FileMode.Create);
                using XmlWriter writer = XmlWriter.Create(outStream, settings);
                document.Save(writer);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}