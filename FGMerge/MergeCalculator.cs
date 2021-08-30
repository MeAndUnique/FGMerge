using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Extensions.Options;

namespace FGMerge
{
    public class MergeCalculator : IMergeCalculator
    {
        private readonly IFileLoader _loader;
        private readonly AppSettings _settings;

        public MergeCalculator(IFileLoader loader, IOptions<AppSettings> settings)
        {
            _loader = loader;
            _settings = settings.Value;
        }

        public IReadOnlyCollection<MergeGroup> Calculate(FileInfo baseFile, FileInfo localFile, FileInfo remoteFile)
        {
            XmlDocument baseDoc = _loader.Load(baseFile.OpenRead());
            XmlDocument localDoc = _loader.Load(localFile.OpenRead());
            XmlDocument remoteDoc = _loader.Load(remoteFile.OpenRead());

            Dictionary<string, MergeGroupBuilder> nodesByGroupAndId = new();
            AddGroups(nodesByGroupAndId, baseDoc.SelectSingleNode("root")!, (node, builder) => builder.BaseNode = node);
            AddGroups(nodesByGroupAndId, remoteDoc.SelectSingleNode("root")!, (node, builder) => builder.RemoteNode = node);
            AddGroups(nodesByGroupAndId, localDoc.SelectSingleNode("root")!, (node, builder) => builder.LocalNode = node); // Local last for prioritization

            foreach (MergeGroupBuilder groupBuilder in nodesByGroupAndId.Values)
            {
                List<MergeNodeBuilder> renamedNodes = new();
                HashSet<string> knownIds = new(groupBuilder.NodesById.Keys);
                foreach (MergeNodeBuilder builder in groupBuilder.NodesById.Values)
                {
                    if (!CheckBuilder(builder))
                    {
                        int newId = 0;
                        while (knownIds.Contains($"id-{++newId:D5}")) {}

                        string id = $"id-{newId:D5}";
                        knownIds.Add(id);

                        XmlElement renamedElement = remoteDoc.CreateElement($"id-{newId:D5}");
                        renamedElement.InnerXml = builder.RemoteNode!.InnerXml;

                        MergeNodeBuilder renamedBuilder = new()
                        {
                            Id = id,
                            RemoteNode = renamedElement,
                            Merged = true,
                            ResultNode = renamedElement
                        };
                        renamedNodes.Add(renamedBuilder);

                        builder.RemoteNode = null;
                    }
                }

                foreach (MergeNodeBuilder renamedNode in renamedNodes)
                {
                    groupBuilder.NodesById[renamedNode.Id] = renamedNode;
                }
            }

            return nodesByGroupAndId.Values.Select(groupBuilder => groupBuilder.Build()).ToList();
        }

        private void AddGroups(Dictionary<string, MergeGroupBuilder> nodesByGroupAndId,
            XmlNode rootNode,
            Action<XmlElement, MergeNodeBuilder> setter)
        {
            foreach (XmlElement groupNode in rootNode.ChildNodes.OfType<XmlElement>())
            {
                IEnumerable<XmlElement> nodes = groupNode.ChildNodes.OfType<XmlElement>();
                if (_settings.ComplexGroups.TryGetValue(groupNode.Name, out ISet<string>? listFields))
                {
                    foreach (XmlElement listNode in nodes.Where(node=>listFields.Contains(node.Name)))
                    {
                        string listGroupName = $"{groupNode.Name}/{listNode.Name}";
                        if (!nodesByGroupAndId.TryGetValue(listGroupName, out MergeGroupBuilder? listGroupBuilder))
                        {
                            listGroupBuilder = new MergeGroupBuilder { Name = listGroupName };
                            nodesByGroupAndId[listGroupName] = listGroupBuilder;
                        }
                        AddNodes(listGroupBuilder, listNode.ChildNodes.OfType<XmlElement>(), setter);
                    }

                    nodes = groupNode.ChildNodes.OfType<XmlElement>().Where(node => !listFields.Contains(node.Name));
                }

                string groupName = groupNode.Name;
                if (!nodesByGroupAndId.TryGetValue(groupName, out MergeGroupBuilder? groupBuilder))
                {
                    groupBuilder = new MergeGroupBuilder { Name = groupName };
                    nodesByGroupAndId[groupName] = groupBuilder;
                }
                AddNodes(groupBuilder, nodes, setter);
            }
        }

        private void AddNodes(MergeGroupBuilder groupBuilder, IEnumerable<XmlElement> nodes, Action<XmlElement, MergeNodeBuilder> setter, string? category = null)
        {
            foreach (XmlElement node in nodes)
            {
                if (node.Name == "public")
                {
                    groupBuilder.IsPublic = true;
                    continue;
                }

                // TODO category stuff here
                if (node.Name == "category")
                {
                    string categoryName = node.GetAttribute("name");
                    AddNodes(groupBuilder, node.ChildNodes.OfType<XmlElement>(), setter, categoryName);
                }

                if (!groupBuilder.NodesById.TryGetValue(node.Name, out MergeNodeBuilder? builder))
                {
                    builder = new MergeNodeBuilder { Id = node.Name, Category = category };
                    groupBuilder.NodesById[node.Name] = builder;
                }

                setter(node, builder);
            }
        }

        private bool CheckBuilder(MergeNodeBuilder builder)
        {
            if (builder.BaseNode != null)
            {
                if (builder.BaseNode.InnerText == builder.LocalNode?.InnerText)
                {
                    builder.Merged = builder.BaseNode.InnerText != builder.RemoteNode?.InnerText;
                    builder.ResultNode = builder.RemoteNode;
                }
                else if(builder.BaseNode.InnerText == builder.RemoteNode?.InnerText)
                {
                    builder.Merged = builder.BaseNode.InnerText != builder.LocalNode?.InnerText;
                    builder.ResultNode = builder.LocalNode;
                }
                else if (builder.LocalNode?.InnerText == builder.RemoteNode?.InnerText)
                {
                    builder.Merged = true;
                    builder.ResultNode = builder.LocalNode;
                }
                // Otherwise there was a conflict of edits, leave the result empty.
            }
            else
            {
                builder.Merged = true;
                if (builder.LocalNode == null)
                {
                    builder.ResultNode = builder.RemoteNode;
                }
                else if (builder.RemoteNode == null || builder.LocalNode?.InnerText == builder.RemoteNode?.InnerText)
                {
                    builder.ResultNode = builder.LocalNode;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private class MergeGroupBuilder
        {
            public string Name { get; set; }

            public bool IsPublic { get; set; }

            public Dictionary<string, MergeNodeBuilder> NodesById { get; } = new();

            public MergeGroup Build() => new(Name, IsPublic, NodesById.Values.Select(nodeBuilder => nodeBuilder.Build()).ToList());
        }

        private class MergeNodeBuilder
        {
            public string Id { get; set; }

            public string? Category { get; set; }

            public XmlElement? BaseNode { get; set; }

            public XmlElement? LocalNode { get; set; }

            public XmlElement? RemoteNode { get; set; }

            public bool Merged { get; set; }

            public XmlElement? ResultNode { get; set; }

            public MergeNode Build() => new(Id, Category, BaseNode, LocalNode, RemoteNode, Merged, ResultNode);
        }
    }
}