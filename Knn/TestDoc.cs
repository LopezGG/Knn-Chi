using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knn
{
    class TestDoc
    {
        public string ActualClass;
        public string PredClass;
        public Dictionary<String, int> wordCounts;
        public Dictionary <string, int> ClassCount;

        public TestDoc (string actClass)
        {
            wordCounts = new Dictionary<string, int>();
            ClassCount = new Dictionary<string, int>();
            ActualClass = actClass;
        }
    }
}
