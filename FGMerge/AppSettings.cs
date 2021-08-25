using System.Collections.Generic;

namespace FGMerge
{
    public class AppSettings
    {
        public string ExecutionMode { get; set; }

        public string BaseFile { get; set; }

        public string LocalFile { get; set; }

        public string MergedFile { get; set; }

        public string RemoteFile { get; set; }

        public string DefaultDiffCommand { get; set; }

        public string DefaultMergeCommand { get; set; }

        public bool AutoResolveMerge { get; set; }

        public IDictionary<string, ISet<string>> ComplexCategories { get; set; } = new Dictionary<string, ISet<string>>
        {
            { "combattracker", new HashSet<string>{"list"} },
        };
    }
}