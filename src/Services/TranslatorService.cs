using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using csandra.translate.Models;

namespace csandra.translate.Services{

    public class TranslatorService{
        private readonly string subscriptionKey = "03a527c0e997496493e3de6a7294669a";//"@@@SUBSCRIPTION_KEY@@@";
        private readonly string endpoint = "https://csandra-translate.cognitiveservices.azure.com/sts/v1.0/issuetoken";//"@@@ENDPOINT@@@";
        protected static List<TranslationItem> Items {get;private set;}
        public async Task TranslateTextRequest(string inputText, string targetLang)
        {
            //TODO: check Request Repos
            string route = $"/translate?api-version=3.0&to={targetLang}";
            if(Items != null && Items.Any(i=> i.LanguageItems.Any(k=> k.Text.ToLower().Trim() == inputText.ToLower().Trim()))){
                var langItem = Items.FirstOrDefault(i=> i.LanguageItems.Any(k=> k.Text.ToLower().Trim() == inputText.ToLower().Trim()));
                if(langItem.LanguageItems.Any(c=> c.To == targetLang)){
                    var txt = langItem.LanguageItems.FirstOrDefault(c=> c.To == targetLang);
                    Console.WriteLine(txt.Text);
                    return;
                }
            }
            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);
            using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    // Build the request.
                    // Set the method to Post.
                    request.Method = HttpMethod.Post;
                    // Construct the URI and add headers.
                    request.RequestUri = new Uri(endpoint + route);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                    // Send the request and get response.
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    // Read response as a string.
                    string result = await response.Content.ReadAsStringAsync();
                    // Deserialize the response using the classes created earlier.
                    TranslationResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(result);
                    var langItem = new TranslationItem(){
                        LanguageItems = new List<Translation>(){
                            new Translation(){
                                Text = inputText,
                                To = targetLang
                            }
                        }
                    };
                    // Iterate over the deserialized results.
                    foreach (TranslationResult o in deserializedOutput)
                    {
                        // Print the detected input language and confidence score.
                        Console.WriteLine("Detected input language: {0}\nConfidence score: {1}\n", o.DetectedLanguage.Language, o.DetectedLanguage.Score);
                        // Iterate over the results and print each translation.
                        foreach (Translation t in o.Translations)
                        {
                            langItem.LanguageItems.Add(t);
                            Console.WriteLine("Translated to {0}: {1}", t.To, t.Text);
                        }
                    }
                    Items.Add(langItem);
                }
        }
    }

    public class TranslationItem{
        public List<Translation> LanguageItems { get; set; }
    }
}