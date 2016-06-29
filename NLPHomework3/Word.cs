using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLPHomework3
{
    class Word
    {
        public string Item;
        public string PreviousWord;
        public string Tag;
        public string PreviousTag;
        public int Position;
        public double Probability;

        public Word(string i, string p, string t, string pt, int position)
        {
            Item = i;
            PreviousWord = p;
            Tag = t;
            PreviousTag = pt;
            Position = position;
            Probability = 0.0;
        }
        public Word(string i, string p, string t, string pt, int position, double probability)
        {
            Item = i;
            PreviousWord = p;
            Tag = t;
            PreviousTag = pt;
            Position = position;
            Probability = probability;
        }
    }
}
