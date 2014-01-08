using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Awam.Tracker.Data
{
    public class DataRepository : IDataRepository
    {
        public IHands GetHandsRepository()
        {
            return new Hands(); 
        }

        public IManagement GetManagementRepository()
        {
         return new Management(); 
        }

        public IImports GetImportsRepository()
        {
             return new Imports(); 
        }

        public IStatistics GetStatisticsRepository()
        {
            return new Statistics();
        }
    }
}
