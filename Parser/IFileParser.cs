using System;
using System.Collections.Generic;
using Awam.Tracker.Model;

namespace Awam.Tracker.Parser
{
    public interface IFileParser
    {
        IList<Hand> Parse(string filePath, DateTime lastImport);
    }
}
