using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knn
{
    class Document
    {
        public int docId;
        public Dictionary<string, int> wordCounts;
        public string classLabel;
        public List<string> Keys;
        public double SumSquared;
        public Document ()
	    {
            classLabel = "";
            wordCounts = new Dictionary<string, int>();
            Keys = new List<string>();
            SumSquared = 0;
	    }
        public Document (string cl, int dId)
        {
            classLabel = cl;
            docId = dId;
            wordCounts = new Dictionary<string, int>();
            Keys = new List<string>();
            SumSquared = 0;
        }
    }
}
