using System.Xml;

namespace FGMerge
{
    public interface IFileWriter
    {
        bool WriteFile(string fileName, XmlDocument document);
    }
}