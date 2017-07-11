using System;
using System.Collections.Generic;
using System.Text;

namespace RemindMe.Models
{
    public class SpellcheckModel
    {
        public string _Type { get; set; }
        public List<FlaggedToken> FlaggedTokens { get; set; }
    }
    
    public class FlaggedToken
    {
        public int Offset { get; set; }
        public string Token { get; set; }
        public string Type { get; set; }
        public List<SuggestedWord> Suggestions { get; set; }
    }

    public class SuggestedWord
    {
        public string Suggestion { get; set; }
        public double Score { get; set; }
    }
}
