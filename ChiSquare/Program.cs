using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChiSquare
{
    class Program
    {
        static void Main (string[] args)
        {
            string inputFile = args[0];
            Dictionary<string,Dictionary<String,int>> ClassWC = new Dictionary<string,Dictionary<string,int>>();
            Dictionary<string,int> ClassDocCount = new Dictionary<string,int>();
            Dictionary<string, int> Vocab = new Dictionary<string,int>();
            string line,classLabel,key ;
            int index;
            int totalDoc=0;
            using(StreamReader Sr = new StreamReader(inputFile))
            {
                while((line=Sr.ReadLine())!=null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;
                    string[] words = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    classLabel = words[0];
                    totalDoc++;
                    //Count of docs in a class
                    if (ClassDocCount.ContainsKey(classLabel))
                        ClassDocCount[classLabel]++;
                    else
                        ClassDocCount.Add(classLabel, 1);

                    for (int i = 1; i < words.Length; i++)
                    {
                        index = words[i].IndexOf(":");
                        if (index < 0)
                            continue;
                        key = words[i].Substring(0, index);
                        //wordCount
                        if (ClassWC.ContainsKey(classLabel) && ClassWC[classLabel].ContainsKey(key))
                            (ClassWC[classLabel])[key]++;
                        else if (ClassWC.ContainsKey(classLabel))
                            ClassWC[classLabel].Add(key, 1);
                        else
                        {
                            Dictionary<string, int> temp = new Dictionary<string, int>();
                            temp.Add(key, 1);
                            ClassWC.Add(classLabel, temp);
                        }

                        //building vocab
                        if (Vocab.ContainsKey(key))
                            Vocab[key]++;
                        else
                            Vocab.Add(key, 1);
                    }
                }
                
            }
            List<String> UniqueClass = ClassDocCount.Keys.Distinct().ToList();
            double score;
            double expectedValue,tempValue;
            int observedValue1, observedValue2,totalDocsInClass;
            Dictionary<string, double> FeatureScore = new Dictionary<string, double>();
            foreach (var featureitem in Vocab)
            {
                score = 0;

                foreach (var classString in UniqueClass)
                {
                    //featureitem.Value: gives row Total
                    //ClassDocCount[classString] : gives column total i.e number of docs per class
                    if(ClassDocCount.ContainsKey(classString))
                        totalDocsInClass = ClassDocCount[classString];
                    else
                        totalDocsInClass = 0;
                    expectedValue = (featureitem.Value * totalDocsInClass) / totalDoc;

                    if (ClassWC.ContainsKey(classString) && (ClassWC[classString]).ContainsKey(featureitem.Key))
                        observedValue1 = (ClassWC[classString])[featureitem.Key];
                    else
                        observedValue1 = 0;
                    observedValue2 = totalDocsInClass - observedValue1;

                    tempValue = System.Math.Pow((observedValue1 - expectedValue) ,2) ;
                    score += (tempValue / expectedValue);
                    tempValue = System.Math.Pow((observedValue1 - expectedValue), 2);
                    score += (tempValue / expectedValue);
                }
                FeatureScore.Add(featureitem.Key, score);
            }
            FeatureScore = FeatureScore.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            int count = 0;
            foreach (var item in FeatureScore)
            {
                Console.WriteLine(item.Key+" "+item.Value+" " + Vocab[item.Key]);
                count++;
                if (count > 15)
                    break;
            }
            Console.ReadLine();
        }
    }
}
