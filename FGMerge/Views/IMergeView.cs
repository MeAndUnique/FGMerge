using System.Collections.Generic;

namespace FGMerge.Views
{
    public interface IMergeView
    {
        public void Show(IReadOnlyCollection<MergeCategory> mergeCategories);
    }
}