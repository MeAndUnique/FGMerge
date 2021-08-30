using System.Xml;

namespace FGMerge
{
    public record MergeNode(string Id,
        string? Category,
        XmlElement? BaseNode,
        XmlElement? LocalNode,
        XmlElement? RemoteNode,
        bool Merged,
        XmlElement? MergedNode);
}