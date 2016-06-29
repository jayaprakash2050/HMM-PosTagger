/*NLP Homework 3
 * HMM POS tagger using Viterbi algorithm
 * Jayaprakash Jayakumar
 * 1209340128
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NLPHomework3
{
    class Program
    {

        public static string TestFilePath = @"C:\Users\hp pc\Documents\JP\ASU- Courses\Natural Language Processing\NLP Homework\3\entest.txt";
        public static string TrainFilePath = @"C:\Users\hp pc\Documents\JP\ASU- Courses\Natural Language Processing\NLP Homework\3\entrain.txt";
        static void Main(string[] args)
        {
            try
            {
                
                HMMTagger tagger = new HMMTagger();
                Console.WriteLine("Enter the training file path: ");

                TrainFilePath = Console.ReadLine();
                Console.WriteLine("Enter the test file path: ");
                TestFilePath = Console.ReadLine();


                tagger.LoadData(TrainFilePath);
                //List<string> value = new List<string> {"It","was","the","spartans", ",", "who", "won","the","match","." };
                //List<string> tags = tagger.ViterbiAlgorithm(value);
                //foreach (string s in tags)
                //    Console.WriteLine(s);
                Console.WriteLine("Training Completed...");
                Console.WriteLine("Evaluation of test file started...");
                double errorrate = ReadTest();
                Console.WriteLine("Error rate is : " + errorrate);
                Console.WriteLine("Error Percentage is : " + errorrate * 100);
                Console.WriteLine("Success Percentage is : " + (1 - errorrate) * 100);
                Console.WriteLine();
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
        }
        //Method to read the test file and evaluate it on the trained system.
        static double ReadTest()
        {
            try
            {
                string line = "";
                List<List<string>> wordlists = new List<List<string>>();
                List<List<string>> taglists = new List<List<string>>();
                List<string> lw = new List<string>();
                List<string> tw = new List<string>();
                HMMTagger tagger = new HMMTagger();
                List<List<string>> outputtagsequences = new List<List<string>>();
                int flag = 0;
                int errorcount = 0;
                int totalcount = 0;
                using (var fileStream = File.OpenRead(TestFilePath))
                {
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, 128))
                    {

                        while ((line = streamReader.ReadLine()) != null)
                        {

                            string word = line.Split('/')[0];

                            string tag = line.Split('/')[1];
                            if (word.Equals("###"))
                            {
                                if (lw.Count > 0)
                                {
                                    wordlists.Add(lw);
                                    taglists.Add(tw);
                                    lw = new List<string>();
                                    tw = new List<string>();
                                }
                                else
                                {

                                    lw = new List<string>();
                                    tw = new List<string>();
                                }

                            }
                            else
                            {
                                lw.Add(word);
                                tw.Add(tag);
                            }
                        }


                    }

                }
                foreach (List<string> element in wordlists)
                {
                    var result = tagger.ViterbiAlgorithm(element);
                    outputtagsequences.Add(result);
                }

                for (int i = 0; i < outputtagsequences.Count; i++)
                {
                    List<string> gotsequence = outputtagsequences[i];
                    List<string> actualsequence = taglists[i];

                    for (int j = 0; j < gotsequence.Count; j++)
                    {
                        if (gotsequence[j] != actualsequence[j])
                        {
                            errorcount++;
                        }
                        totalcount++;
                    }
                }
                double errorrate = (double)errorcount / totalcount;
                //Console.WriteLine(errorrate);
                Console.WriteLine("Total No of Words: " + totalcount);
                Console.WriteLine("Total No of wrongly tagged Words: " + errorcount);
                Console.WriteLine("Total No of correctly tagged Words: " + (totalcount-errorcount));
                return errorrate;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                return 0.0;
            }
        }

    }
}
