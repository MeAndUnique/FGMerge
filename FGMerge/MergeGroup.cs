using System.Collections.Generic;

namespace FGMerge
{
    public record MergeGroup(string Name, bool IsPublic, IReadOnlyCollection<MergeNode> Nodes);
}