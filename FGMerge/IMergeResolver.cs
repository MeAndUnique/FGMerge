using System.Xml;

namespace FGMerge
{
    public interface IMergeResolver
    {
        public void Initialize(XmlDocument template);

        public void AddGroup(string name, bool isPublic);

        public void AddNode(string group, string id, string innerXml, string? category = null);

        public XmlDocument Resolve();
    }
}