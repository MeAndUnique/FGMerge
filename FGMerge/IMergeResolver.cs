using System.Xml;

namespace FGMerge
{
    public interface IMergeResolver
    {
        public void Initialize(XmlDocument template);

        public void AddCategory(string name, bool isPublic);

        public void AddNode(string category, string id, string innerXml);

        public XmlDocument Resolve();
    }
}