using System.IO;
using System.Xml;

namespace FGMerge
{
    public interface IFileLoader
    {
        XmlDocument Load(Stream fileStream);
    }
}