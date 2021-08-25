using System.IO;
using System.Xml;

namespace FGMerge
{
    public class FileLoader :  IFileLoader
    {
        public XmlDocument Load(Stream fileStream)
        {
            XmlDocument document = new() { PreserveWhitespace = true };
            document.Load(fileStream);
            fileStream.Close();
            fileStream.Dispose();
            return document;
        }
    }
}