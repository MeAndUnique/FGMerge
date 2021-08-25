using System.Xml;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;

namespace FGMerge.ViewModels
{
    public class NodeViewModel : ReactiveValidationObject
    {
        public string Id { get; }

        public string BaseText { get; }

        public string LocalText { get; }

        private string _mergedText;
        public string MergedText
        {
            get => _mergedText;
            set => this.RaiseAndSetIfChanged(ref _mergedText, value);
        }

        public string RemoteText { get; }

        public bool HasChange { get; }

        private readonly ObservableAsPropertyHelper<bool> _hasConflict;
        public bool HasConflict => _hasConflict.Value;

        public NodeViewModel(MergeNode node)
        {
            Id = node.Id;
            BaseText = node.BaseNode?.InnerXml;
            LocalText = node.LocalNode?.InnerXml;
            MergedText = node.MergedNode?.InnerXml;
            RemoteText = node.RemoteNode?.InnerXml;

            HasChange = node.Merged || node.RemoteNode == null;

            _hasConflict = this.WhenAnyValue(x => x.MergedText, x => x.ValidationContext.IsValid, 
                    (text, isValid) => !isValid || text == null)
                .ToProperty(this, x => x.HasConflict);

            // Creates the validation for the MergedText property.
            this.ValidationRule( viewModel => viewModel.MergedText, ValidateXml, "The XML cannot be parsed.");
        }

        private bool ValidateXml(string text)
        {
            try
            {
                new XmlDocument().CreateElement("validate").InnerXml = text;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}