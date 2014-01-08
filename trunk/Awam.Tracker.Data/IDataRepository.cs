
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awam.Tracker.Data
{
    public interface IDataRepository
    {
        IHands GetHandsRepository();
        IManagement GetManagementRepository();
        IImports GetImportsRepository();
    }
}
