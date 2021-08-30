using System.Collections.Generic;
using System.IO;

namespace FGMerge
{
    public interface IMergeCalculator
    {
        IReadOnlyCollection<MergeGroup> Calculate(FileInfo baseFile, FileInfo localFile, FileInfo remoteFile);
    }
}