using System.Collections.Generic;

namespace FGMerge
{
    public record MergeCategory(string Name, bool IsPublic, IReadOnlyCollection<MergeNode> Nodes);
}