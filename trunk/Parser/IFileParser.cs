using System;
using System.Collections.Generic;
using Awam.Tracker.Model;

namespace Awam.Tracker.Parser
{
    public interface IFileParser
    {
        string FilePath { get; }
        IList<Hand> Parse(string filePath, DateTime lastImport);
    }
}
