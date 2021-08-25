using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace FGMerge.ViewModels
{
    public class CategoryViewModel : ReactiveObject
    {
        public string Name { get; }

        public bool IsPublic { get; }

        private readonly ObservableCollection<NodeViewModel> _nodes = new();
        public IReadOnlyList<NodeViewModel> Nodes => _nodes;

        private readonly ObservableAsPropertyHelper<bool> _hasConflicts;
        public bool HasConflicts => _hasConflicts.Value;

        public bool HasChanges { get; }

        public CategoryViewModel(MergeCategory category)
        {
            Name = category.Name;
            IsPublic = category.IsPublic;
            _nodes.AddRange(category.Nodes.Select(node => new NodeViewModel(node)));
            HasChanges = _nodes.Any(node => node.HasChange);

            // Here, T inherits from the ReactiveObject class.
            // 'hasConflicts' is IObservable<bool>
            var hasConflicts = _nodes
                .ToObservableChangeSet()
                .AutoRefresh(node => node.HasConflict) // Subscribe only to HasConflict property changes
                .ToCollection() // Get the new collection of items
                .Select(x => x.Any(y => y.HasConflict)); // Verify all elements satisfy a condition etc.

            // Then you can convert IObservable<bool> to a view model property.
            // '_hasConflicts' is of type ObservableAsPropertyHelper<bool> here.
            _hasConflicts = hasConflicts.ToProperty(this, x => x.HasConflicts);
        }

    }
}