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

        public IReadOnlyCollection<MergeCategory> Calculate(FileInfo baseFile, FileInfo localFile, FileInfo remoteFile)
        {
            XmlDocument baseDoc = _loader.Load(baseFile.OpenRead());
            XmlDocument localDoc = _loader.Load(localFile.OpenRead());
            XmlDocument remoteDoc = _loader.Load(remoteFile.OpenRead());

            Dictionary<string, MergeCategoryBuilder> nodesByCategoryAndId = new();
            AddCategories(nodesByCategoryAndId, baseDoc.SelectSingleNode("root"), (node, builder) => builder.BaseNode = node);
            AddCategories(nodesByCategoryAndId, localDoc.SelectSingleNode("root"), (node, builder) => builder.LocalNode = node);
            AddCategories(nodesByCategoryAndId, remoteDoc.SelectSingleNode("root"), (node, builder) => builder.RemoteNode = node);

            foreach (MergeCategoryBuilder categoryBuilder in nodesByCategoryAndId.Values)
            {
                List<MergeNodeBuilder> renamedNodes = new();
                HashSet<string> knownIds = new(categoryBuilder.NodesById.Keys);
                foreach (MergeNodeBuilder builder in categoryBuilder.NodesById.Values)
                {
                    if (!CheckBuilder(builder))
                    {
                        int newId = 0;
                        while (knownIds.Contains($"id-{++newId:D5}")) {}

                        string id = $"id-{newId:D5}";
                        knownIds.Add(id);

                        XmlElement renamedElement = remoteDoc.CreateElement($"id-{newId:D5}");
                        renamedElement.InnerXml = builder.RemoteNode.InnerXml;

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
                    categoryBuilder.NodesById[renamedNode.Id] = renamedNode;
                }
            }

            return nodesByCategoryAndId.Values.Select(categoryBuilder=>categoryBuilder.Build()).ToList();
        }

        private void AddCategories(Dictionary<string, MergeCategoryBuilder> nodesByCategoryAndId,
            XmlNode rootNode,
            Action<XmlElement, MergeNodeBuilder> setter)
        {
            foreach (XmlElement categoryNode in rootNode.ChildNodes.OfType<XmlElement>())
            {
                IEnumerable<XmlElement> nodes = categoryNode.ChildNodes.OfType<XmlElement>();
                if (_settings.ComplexCategories.TryGetValue(categoryNode.Name, out ISet<string> listFields))
                {
                    foreach (XmlElement listNode in nodes.Where(node=>listFields.Contains(node.Name)))
                    {
                        string listCategoryName = $"{categoryNode.Name}/{listNode.Name}";
                        if (!nodesByCategoryAndId.TryGetValue(listCategoryName, out MergeCategoryBuilder listCategoryBuilder))
                        {
                            listCategoryBuilder = new MergeCategoryBuilder { Name = listCategoryName };
                            nodesByCategoryAndId[listCategoryName] = listCategoryBuilder;
                        }
                        AddNodes(listCategoryBuilder, listNode.ChildNodes.OfType<XmlElement>(), setter);
                    }

                    nodes = categoryNode.ChildNodes.OfType<XmlElement>().Where(node => !listFields.Contains(node.Name));
                }

                string categoryName = categoryNode.Name;
                if (!nodesByCategoryAndId.TryGetValue(categoryName, out MergeCategoryBuilder categoryBuilder))
                {
                    categoryBuilder = new MergeCategoryBuilder { Name = categoryName };
                    nodesByCategoryAndId[categoryName] = categoryBuilder;
                }
                AddNodes(categoryBuilder, nodes, setter);
            }
        }

        private void AddNodes(MergeCategoryBuilder categoryBuilder, IEnumerable<XmlElement> nodes, Action<XmlElement, MergeNodeBuilder> setter)
        {
            foreach (XmlElement node in nodes)
            {
                if (node.Name == "public")
                {
                    categoryBuilder.IsPublic = true;
                    continue;
                }

                if (!categoryBuilder.NodesById.TryGetValue(node.Name, out MergeNodeBuilder builder))
                {
                    builder = new MergeNodeBuilder { Id = node.Name };
                    categoryBuilder.NodesById[node.Name] = builder;
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

        private class MergeCategoryBuilder
        {
            public string Name { get; set; }

            public bool IsPublic { get; set; }

            public Dictionary<string, MergeNodeBuilder> NodesById { get; } = new();

            public MergeCategory Build() => new(Name, IsPublic, NodesById.Values.Select(nodeBuilder => nodeBuilder.Build()).ToList());
        }

        private class MergeNodeBuilder
        {
            public string Id { get; set; }

            public XmlElement BaseNode { get; set; }

            public XmlElement LocalNode { get; set; }

            public XmlElement RemoteNode { get; set; }

            public bool Merged { get; set; }

            public XmlElement ResultNode { get; set; }

            public MergeNode Build() => new(Id, BaseNode, LocalNode, RemoteNode, Merged, ResultNode);
        }
    }
}