using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NLPHomework3
{
    class HMMTagger
    {
        private static Dictionary<string, List<string>> wordTagPairs = new Dictionary<string, List<string>>();
        private static Dictionary<string, List<string>> NextTagDict = new Dictionary<string, List<string>>();

        private static Dictionary<string, Dictionary<string, int>> nextTagCount = new Dictionary<string, Dictionary<string, int>>();
        private static Dictionary<string, Dictionary<string, int>> TagWordCount = new Dictionary<string, Dictionary<string, int>>();

        private static Dictionary<string, Dictionary<string, double>> nextTagProbability = new Dictionary<string, Dictionary<string, double>>();
        private static Dictionary<string, Dictionary<string, double>> TagWordProbability = new Dictionary<string, Dictionary<string, double>>();

        private static List<string> Wordlist = new List<string>();
        private static List<string> TagCounterList = new List<string>();
        private static int VocabularySizeforWords;
        private static int VocabularySizeforTags;
        private static Dictionary<string, int> TagCounter = new Dictionary<string, int>();

        static List<List<Word>> Sentences = new List<List<Word>>();

        static Dictionary<string, double> PosTagSequence = new Dictionary<string, double>();
        static string tagSequence = "";
        static string TrainFilelocation;
        const Int32 BufferSize = 128;

        //Loads the training corpus and calculates the probabilities and count for the words and tags
        public void LoadData(string FilePath)
        {
            TrainFilelocation = FilePath;
            using (var fileStream = File.OpenRead(TrainFilelocation))
            {
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {

                    List<Word> data = new List<Word>();
                    String line;
                    int position = -1;
                    string currentword = "";
                    string currenttag = "";
                    string previousword = "";
                    string previoustag = "";
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        Wordlist.Add(line.Split('/')[0].ToString());
                        TagCounterList.Add(line.Split('/')[1].ToString());

                        if (line.Contains("###"))
                        {
                            //Starting of a sentence, so create a list to store the items of that sentence
                            position = -1;
                            data = new List<Word>();
                            Word w = new Word("###", "xxx", "###", previoustag, position);
                            data.Add(w);
                            currentword = line.Split('/')[0].ToString();
                            previoustag = currenttag;
                            currenttag = "<end>";
                            //previousword = "<end>";
                        }
                        else
                        {
                            //Just keep on appending the word to the current sentence.
                            position = position + 1;
                            currentword = line.Split('/')[0].ToString();
                            currenttag = line.Split('/')[1].ToString();
                            previousword = data[data.Count - 1].Item;
                            previoustag = data[data.Count - 1].Tag;
                            Word w = new Word(currentword, previousword, currenttag, previoustag, position);
                            data.Add(w);

                        }
                        if (previoustag != "")
                        {
                            InsertDataIntoDict(nextTagCount, previoustag, currenttag);
                            InsertDataIntoDict(TagWordCount, currenttag, currentword);
                        }


                    }
                    VocabularySizeforWords = Wordlist.Distinct().Count();
                    VocabularySizeforTags = TagCounterList.Distinct().Count();

                    InsertDataintoProbabilityDict(nextTagProbability, nextTagCount, 'T');
                    InsertDataintoProbabilityDict(TagWordProbability, TagWordCount, 'C');
                }

            }
        }

        
        public void InsertDataIntoDict(Dictionary<string, Dictionary<string, int>> CountDict, string key, string previoustag)
        {
            if (!CountDict.Keys.Contains(key))
            {
                //key not present. So add it.
                Dictionary<string, int> dict = new Dictionary<string, int>();
                dict.Add(previoustag, 1);
                CountDict.Add(key, dict);

            }
            else
            {
                //key is present
                var item = CountDict[key];
                //check if previoustag if present, if present just increment the counter. else add with counter as 1.
                if (item.Keys.Contains(previoustag))
                {
                    item[previoustag] = item[previoustag] + 1;
                }
                else
                {
                    item.Add(previoustag, 1);

                }
            }
        }


        public void InsertDataintoProbabilityDict(Dictionary<string, Dictionary<string, double>> ProbDict, Dictionary<string, Dictionary<string, int>> CountDict, char c)
        {
            int vocabularysize = 0;
            if (c == 'T')
            {
                vocabularysize = VocabularySizeforTags;
            }
            else
                vocabularysize = VocabularySizeforWords;

            foreach (string key in CountDict.Keys)
            {
                //calculate total no of occurence for the k.
                int count = 0;
                double prob = 0;
                foreach (string k in CountDict[key].Keys)
                {
                    count = count + CountDict[key][k];
                }
                //For Laplace smoothing
                count = count + vocabularysize;

                foreach (string k in CountDict[key].Keys)
                {
                    Dictionary<string, double> d;
                    prob = (double)(CountDict[key][k] + 1) / (double)count;

                    if (ProbDict.Keys.Contains(key))
                    {
                        d = ProbDict[key];
                        if (d.Keys.Contains(k))
                        {
                            d[k] = prob;
                        }
                        else
                        {
                            d.Add(k, prob);
                        }

                    }
                    else
                    {
                        d = new Dictionary<string, double>();
                        d.Add(k, prob);
                        ProbDict.Add(key, d);

                    }


                }
                //Add unknown probability
                //For Laplace smoothing
                ProbDict[key].Add("<unknown>", (double)1 / count);
            }
        }


        //Viterbi algorithm implementation
        public List<string> ViterbiAlgorithm(List<string> ListWord)
        {
            List<string> tags = new List<string>();
            bool start = true;
            string previosutagfinal = "";
            double maxprior = 0;
            double prevprob = 0;
            double obsprob = 0;
            string tag = "";
            try
            {

                for (int i = 0; i < ListWord.Count; i++)
                {
                    string word = ListWord[i];
                    maxprior = 0;
                    obsprob = 0;
                    //For sentence starting
                    if (start)
                    {
                        previosutagfinal = "###";
                        prevprob = 1.0;
                        start = false;
                    }
                    //If the word is present in out training vocabulary, we check there for the word and maximize the probability.
                    if (Wordlist.Contains(word))
                    {
                        foreach (string item in TagWordProbability.Keys)
                        {
                            //Get all possible tags for the word
                            if (TagWordProbability[item].Keys.Contains(word))
                            {
                                double prob = 0.0;
                                //If the tag containing the word is available in next state of previous state.
                                if (nextTagProbability[previosutagfinal].Keys.Contains(item))
                                {
                                    prob = nextTagProbability[previosutagfinal][item];
                                }
                                    //If not available then we consider unknown as tag
                                else
                                {
                                    prob = nextTagProbability[previosutagfinal]["<unknown>"];
                                }
                                    obsprob = TagWordProbability[item][word];
                                    double multiplied = prob * obsprob;
                                    if (multiplied >= maxprior)
                                    {
                                        tag = item;
                                        maxprior = multiplied;
                                    }
                              


                            }
                        }
                        tags.Add(tag);
                        previosutagfinal = tag;
                        prevprob = prevprob * maxprior;

                    }

                    else
                    {
                        //word not present in vocab, check all tags and maximize the probability
                        foreach (string item in nextTagProbability[previosutagfinal].Keys)
                        {
                            if (!item.Equals("<unknown>") && !item.Equals("<end>") )
                            {
                                double prob = nextTagProbability[previosutagfinal][item];
                                obsprob = TagWordProbability[item]["<unknown>"];
                                double multiplied = prob * obsprob;
                                if (multiplied >= maxprior)
                                {
                                    tag = item;
                                    maxprior = multiplied;
                                }
                            }
                        }
                        tags.Add(tag);
                        previosutagfinal = tag;
                        prevprob = prevprob * maxprior;
                    }

                }
                return tags;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                return tags;
            }
        }


    }

}
