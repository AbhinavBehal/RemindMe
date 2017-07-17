using Newtonsoft.Json;
using RemindMe.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RemindMe
{
    public class Spellchecker
    {
        public static async Task<string> Check(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var textToSend = text.Replace(' ', '+');
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "dd84e5f31c714990bb61399e24f4c14f");
            string endpoint = "https://api.cognitive.microsoft.com/bing/v5.0/spellcheck";
            string url = endpoint + "?text=" + textToSend;

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return text;

            var responseString = await response.Content.ReadAsStringAsync();
            SpellcheckModel model = JsonConvert.DeserializeObject<SpellcheckModel>(responseString);

            //No spelling mistakes
            if (model.FlaggedTokens.Count == 0)
                return text;

            //Sort the tokens by their offset
            var flaggedTokens = model.FlaggedTokens.OrderBy(t => t.Offset);

            StringBuilder builder = new StringBuilder();
            int index = 0;
            foreach(FlaggedToken token in flaggedTokens)
            {
                //Get the suggestion with the highest score
                var suggestion = token.Suggestions.OrderByDescending(s => s.Score).FirstOrDefault().Suggestion;

                //Append everything before the spelling mistake
                builder.Append(text.Substring(index, token.Offset - index));

                //Fix the mistake
                builder.Append(suggestion);
                index = token.Offset + token.Token.Length;
            }

            //Append the remaining text
            builder.Append(text.Substring(index, text.Length - index));

            return builder.ToString();

            /*
            //Simulate the api call
            await Task.Delay(1000);
            return text;*/
        }
    }
}
