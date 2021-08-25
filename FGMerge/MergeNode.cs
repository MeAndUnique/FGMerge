using System.Xml;

namespace FGMerge
{
    public record MergeNode(string Id, XmlElement BaseNode, XmlElement LocalNode, XmlElement RemoteNode, bool Merged, XmlElement MergedNode = null);
}