using System.Collections.Generic;

namespace FGMerge
{
    public class AppSettings
    {
        public string ExecutionMode { get; set; } = string.Empty;

        public string BaseFile { get; set; } = string.Empty;

        public string LocalFile { get; set; } = string.Empty;

        public string MergedFile { get; set; } = string.Empty;

        public string RemoteFile { get; set; } = string.Empty;

        public string DefaultDiffCommand { get; set; } = string.Empty;

        public string DefaultMergeCommand { get; set; } = string.Empty;

        public bool AutoResolveMerge { get; set; }

        public IDictionary<string, ISet<string>> ComplexGroups { get; set; } = new Dictionary<string, ISet<string>>
        {
            { "combattracker", new HashSet<string>{"list"} },
        };
    }
}