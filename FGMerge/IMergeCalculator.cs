using System.Collections.Generic;
using System.IO;

namespace FGMerge
{
    public interface IMergeCalculator
    {
        IReadOnlyCollection<MergeCategory> Calculate(FileInfo baseFile, FileInfo localFile, FileInfo remoteFile);
    }
}