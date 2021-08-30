using System.Xml;

namespace FGMerge
{
    public class MergeResolver : IMergeResolver
    {
        private XmlDocument _template = null!;
        private XmlElement _rootNode = null!;

        public void Initialize(XmlDocument template)
        {
            _template = (XmlDocument)template.CloneNode(true);
            _rootNode = (XmlElement)_template.SelectSingleNode("root")!;
            _rootNode.InnerXml = "";
        }

        public void AddGroup(string name, bool isPublic)
        {
            XmlElement groupNode = _template.CreateElement(name);
            _rootNode.AppendChild(groupNode);

            if (isPublic)
            {
                XmlElement publicNode = _template.CreateElement("public");
                groupNode.AppendChild(publicNode);
            }
        }

        public void AddNode(string group, string id, string innerXml, string? category = null)
        {
            XmlElement groupNode = (XmlElement)_rootNode.SelectSingleNode(group)!;
            XmlElement containerNode = groupNode;
            if (!string.IsNullOrWhiteSpace(category))
            {
                foreach (XmlElement categoryNode in groupNode.SelectNodes("category")!)
                {
                    string categoryName = categoryNode.GetAttribute("name");
                    if (categoryName == category)
                    {
                        containerNode = categoryNode;
                        break;
                    }
                }

                if(containerNode == groupNode)
                {
                    containerNode = _template.CreateElement(category);
                    containerNode.SetAttribute("name", category);
                    groupNode.AppendChild(containerNode);
                }
            }

            XmlElement node = _template.CreateElement(id);
            containerNode.AppendChild(node);
            node.InnerXml = innerXml;
        }

        public XmlDocument Resolve()
        {
            return _template;
        }
    }
}