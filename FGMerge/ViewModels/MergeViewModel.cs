using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Xml;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace FGMerge.ViewModels
{
    public class MergeViewModel : ReactiveValidationObject
    {
        private XmlDocument _baseDocument;
        private NodeViewModel _selectedNode;

        private List<CategoryViewModel> _categories = new();
        public IReadOnlyList<CategoryViewModel> Categories => _categories;

        public NodeViewModel SelectedNode
        {
            get => _selectedNode;
            set => this.RaiseAndSetIfChanged(ref _selectedNode, value);
        }

        public void Initialize(IReadOnlyCollection<MergeCategory> mergeCategories)
        {
            _baseDocument = mergeCategories.SelectMany(category => category.Nodes).First(node => node.BaseNode != null)
                .BaseNode.OwnerDocument;
            _categories.AddRange(mergeCategories.Select(category => new CategoryViewModel(category)));
            // TODO be better
            SelectedNode = _categories.SelectMany(category => category.Nodes)
                .FirstOrDefault(node => !string.IsNullOrWhiteSpace(node.RemoteText));
        }

        public XmlDocument GenerateResult(IMergeResolver resolver)
        {
            resolver.Initialize(_baseDocument);
            foreach (CategoryViewModel category in Categories.OrderBy(category=>category.Name))
            {
                resolver.AddCategory(category.Name, category.IsPublic);
                foreach (NodeViewModel node in category.Nodes.OrderBy(node=>node.Id))
                {
                    resolver.AddNode(category.Name, node.Id, node.MergedText);
                }
            }

            return resolver.Resolve();
        }
    }
}