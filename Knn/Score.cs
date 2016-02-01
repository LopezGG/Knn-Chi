using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knn
{
    class Score
    {
        public string classLabel;
        public double ScoreValue;
        public Score (string cl, double sc)
        {
            classLabel = cl;
            ScoreValue = sc;
        }
    }
}
