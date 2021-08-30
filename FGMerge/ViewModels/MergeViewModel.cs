using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace FGMerge.ViewModels
{
    public class MergeViewModel : ReactiveValidationObject
    {
        private XmlDocument? _baseDocument;
        private NodeViewModel? _selectedNode;

        private readonly List<GroupViewModel> _groups = new();
        public IReadOnlyList<GroupViewModel> Groups => _groups;

        public NodeViewModel? SelectedNode
        {
            get => _selectedNode;
            set => this.RaiseAndSetIfChanged(ref _selectedNode, value);
        }

        public void Initialize(IReadOnlyCollection<MergeGroup> mergeGroups)
        {
            _baseDocument = mergeGroups.SelectMany(group => @group.Nodes).First(node => node.BaseNode != null)
                .BaseNode!.OwnerDocument;
            _groups.AddRange(mergeGroups.Select(group => new GroupViewModel(group)));
            // TODO be better
            SelectedNode = _groups.SelectMany(group => group.Nodes)
                .FirstOrDefault(node => !string.IsNullOrWhiteSpace(node.RemoteText));
        }

        public XmlDocument GenerateResult(IMergeResolver resolver)
        {
            resolver.Initialize(_baseDocument!);
            foreach (GroupViewModel group in Groups.OrderBy(group => group.Name))
            {
                resolver.AddGroup(group.Name, group.IsPublic);
                foreach (NodeViewModel node in group.Nodes.OrderBy(node=>node.Category).ThenBy(node=>node.Id))
                {
                    resolver.AddNode(group.Name, node.Id, node.MergedText!, node.Category);
                }
            }

            return resolver.Resolve();
        }
    }
}