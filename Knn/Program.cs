using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knn
{
    class Program
    {
        static void Main (string[] args)
        {
            string trainingPath = args[0];
            string testPath = args[1];
            int k_val = Convert.ToInt32(args[2]);
            int simFunc = Convert.ToInt32(args[3]);
            List<Document> TrainDocs = new List<Document>();
            string sysOutput = (args[4]);
            int docid = 0;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<String> Classes = ReadFile(trainingPath, ref docid, ref TrainDocs);
            
            string line;
            List<TestDoc> TestingDoc = new List<TestDoc>();
            Dictionary<String, int> ConfusionDict = new Dictionary<string, int>();
            string key;
            int value, index;
            using (StreamReader Sr = new StreamReader(testPath))
            {
                while ((line = Sr.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;
                    string[] words = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    TestDoc test = new TestDoc(words[0]);
                    for (int i = 1; i < words.Length; i++)
                    {
                        index = words[i].IndexOf(":");
                        key = words[i].Substring(0, index);
                        value = Convert.ToInt32(words[i].Substring(index + 1));
                        test.SumSquared += System.Math.Pow(value, 2);
                        if (test.wordCounts.ContainsKey(key))
                            test.wordCounts[key] += value;
                        else
                            test.wordCounts.Add(key, value);
                    }
                    if (simFunc == 1)
                        ProcessTrainEuclidean(TrainDocs, k_val, ref test, ref  ConfusionDict);
                    else if (simFunc == 2)
                        ProcessTrainCosine(TrainDocs, k_val, ref test, ref  ConfusionDict);
                    else
                        throw new Exception("invalid simfunc code");
                    TestingDoc.Add(test);
                }
            }

            
            WriteSysOutput (sysOutput,TestingDoc, Classes,k_val);
            WriteConfusionMatrix(Classes, ConfusionDict, "test", TestingDoc.Count);
            stopwatch.Stop();
            Console.WriteLine("Time elapsed: {0:hh\\:mm\\:ss}", stopwatch.Elapsed);
            //Console.ReadLine();
        }


        public static void WriteSysOutput (string sysOutput, List<TestDoc> TestingDoc, List<String> Classes, int k_val)
        {
            using (StreamWriter Sw1 = new StreamWriter(sysOutput))
            {
                int totalDoc = 0;
                foreach (var doc in TestingDoc)
                {
                    Sw1.Write("array:" + totalDoc + "\t");
                    Dictionary<String, bool> ClassWritten = new Dictionary<string, bool>();
                    foreach (var item in Classes)
                    {
                        ClassWritten.Add(item, false);
                    }
                    totalDoc++;
                    foreach (var item in doc.ClassCount)
                    {
                        Sw1.Write(item.Key + "\t" + (item.Value / ( double )k_val) + "\t");
                        if (ClassWritten.ContainsKey(item.Key))
                            ClassWritten[item.Key] = true;
                    }
                    foreach (var pair in ClassWritten)
                    {
                        if (!pair.Value)
                            Sw1.Write(pair.Key + "\t" + "0" + "\t");
                    }
                    Sw1.WriteLine();
                }
            }

        }
        public static void WriteConfusionMatrix (List<String> ClassBreakDown, Dictionary<String, int> ConfusionDict, string testOrTrain,int totalInstances)
        {
            int correctPred = 0;
            Console.WriteLine("Confusion matrix for the " + testOrTrain + " data:\n row is the truth, column is the system output");
            Console.Write("\t\t\t");
            foreach (var actClass in ClassBreakDown)
            {
                Console.Write(actClass + "\t");
            }
            Console.WriteLine();
            foreach (var actClass in ClassBreakDown)
            {
                
                Console.Write(actClass + "\t");
                foreach (var predClass in ClassBreakDown)
                {
                    
                    if (ConfusionDict.ContainsKey(actClass + "_" + predClass))
                    {
                        Console.Write(ConfusionDict[actClass + "_" + predClass] + "\t");
                        if (actClass == predClass)
                            correctPred += ConfusionDict[actClass + "_" + predClass];
                    }
                    else
                        Console.Write("0" + "\t");

                }
                Console.WriteLine();
            }
            Console.WriteLine(testOrTrain + " accuracy=" + Convert.ToString(correctPred / ( double )totalInstances));
            Console.WriteLine();


        }
        public static void ProcessTrainCosine (List<Document> TrainDocs, int k_val, ref TestDoc test, ref Dictionary<String, int> ConfusionDict)
        {
            List<Score> ScoreList = new List<Score>();
            double score;
            double V1=0, V2=0;
            V2 = test.SumSquared;
            V2 = System.Math.Sqrt(V2);

            foreach (var item in TrainDocs)
            {
                V1 = item.SumSquared;
                V1 = System.Math.Sqrt(V1);
                score = 0;

                var keyList = item.Keys.Intersect(test.wordCounts.Keys).Distinct();
                foreach (var word in keyList)
                {
                    score += (item.wordCounts[word] * test.wordCounts[word]);
                }
                score = (score) / (V1 * V2);
                ScoreList.Add(new Score(item.classLabel, score));
            }
            ScoreList = ScoreList.OrderByDescending(s => s.ScoreValue).ToList();
            string key;
            for (int i = 0; i < k_val; i++)
            {
                key = ScoreList[i].classLabel;
                if (test.ClassCount.ContainsKey(key))
                    test.ClassCount[key]++;
                else
                    test.ClassCount.Add(key, 1);
            }
            test.ClassCount = test.ClassCount.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            test.PredClass = test.ClassCount.Keys.First();
            key = test.ActualClass + "_" + test.PredClass;
            if (ConfusionDict.ContainsKey(key))
                ConfusionDict[key]++;
            else
                ConfusionDict.Add(key, 1);
        }
        public static void ProcessTrainEuclidean (List<Document> TrainDocs, int k_val, ref TestDoc test, ref Dictionary<String, int> ConfusionDict)
        {
            List<Score> ScoreList = new List<Score>();
            double score;
            foreach (var item in TrainDocs)
            {
                score = 0;
                var keyList = item.Keys.Intersect(test.wordCounts.Keys).Distinct();
                foreach (var word in keyList)
                    score += (item.wordCounts[word]*test.wordCounts[word]);
                //(a-b)^2 = a^2 +b^2 -2 *a * b
                score = test.SumSquared+item.SumSquared - (2 * score);
                ScoreList.Add(new Score(item.classLabel, score));
            }
            ScoreList = ScoreList.OrderBy(s => s.ScoreValue).ToList();
            string key;
            for (int i = 0; i < k_val; i++)
            {
                key = ScoreList[i].classLabel;
                if (test.ClassCount.ContainsKey(key))
                    test.ClassCount[key]++;
                else
                    test.ClassCount.Add(key, 1);
            }
            test.ClassCount = test.ClassCount.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            test.PredClass = test.ClassCount.Keys.First();
            key = test.ActualClass + "_" + test.PredClass;
            if (ConfusionDict.ContainsKey(key))
                ConfusionDict[key]++;
            else
                ConfusionDict.Add(key, 1);
        }
        public static List<String> ReadFile (string trainingPath, ref int docid, ref List<Document> TrainDocs)
        {
            string line;
            string key;
            int value, index;
            List<String> Classes = new List<string>();
            using (StreamReader Sr = new StreamReader(trainingPath))
            {
                while ((line = Sr.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;
                    string[] words = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    Classes.Add(words[0]);
                    Document train = new Document(words[0], docid++);
                    for (int i = 1; i < words.Length; i++)
                    {
                        index = words[i].IndexOf(":");
                        key = words[i].Substring(0, index);
                        value = Convert.ToInt32(words[i].Substring(index + 1));
                        train.SumSquared += System.Math.Pow(value, 2);
                        if (train.wordCounts.ContainsKey(key))
                            train.wordCounts[key] += value;
                        else
                            train.wordCounts.Add(key, value);
                    }
                    train.Keys = train.wordCounts.Keys.Distinct().ToList();
                    TrainDocs.Add(train);
                }
            }
            return Classes = Classes.Distinct().ToList();
        }
    }
}
