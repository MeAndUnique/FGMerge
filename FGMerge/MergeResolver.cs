using System.Xml;

namespace FGMerge
{
    public class MergeResolver : IMergeResolver
    {
        private XmlDocument _template;
        private XmlElement _rootNode;

        public void Initialize(XmlDocument template)
        {
            _template = (XmlDocument)template.CloneNode(true);
            _rootNode = (XmlElement)_template.SelectSingleNode("root");
            _rootNode.InnerXml = "";
        }

        public void AddCategory(string name, bool isPublic)
        {
            XmlElement categoryNode = _template.CreateElement(name);
            _rootNode.AppendChild(categoryNode);

            if (isPublic)
            {
                XmlElement publicNode = _template.CreateElement("public");
                categoryNode.AppendChild(publicNode);
            }
        }

        public void AddNode(string category, string id, string innerXml)
        {
            XmlNode categoryNode = _rootNode.SelectSingleNode(category);
            XmlElement node = _template.CreateElement(id);
            categoryNode.AppendChild(node);
            node.InnerXml = innerXml;
        }

        public XmlDocument Resolve()
        {
            return _template;
        }
    }
}