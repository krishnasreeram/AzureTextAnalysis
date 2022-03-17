using System;
using System.IO;
using Azure;
using Azure.AI.TextAnalytics;
using Azure.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace TextAnalysisSQLBrains
{
    public class Function1
    {
        [FunctionName("Function1")]
        public void Run([BlobTrigger("document/{name}", Connection = "DocumentStorageAppSetting")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
            string endpoint = "https://textanalysissqlbrains.cognitiveservices.azure.com/";
            var credential = new ChainedTokenCredential(new ManagedIdentityCredential(), new AzureCliCredential());
            TextAnalyticsClient client = new TextAnalyticsClient(new Uri(endpoint), credential);
                

            try
            {
                using (var reader = new StreamReader(myBlob))
                {
                    var document = reader.ReadToEnd();
                    Response<DocumentSentiment> response = client.AnalyzeSentiment(document);
                    DocumentSentiment docSentiment = response.Value;

                    log.LogInformation($"Sentiment was {docSentiment.Sentiment}, with confidence scores: ");
                    log.LogInformation($"  Positive confidence score: {docSentiment.ConfidenceScores.Positive}.");
                    log.LogInformation($"  Neutral confidence score: {docSentiment.ConfidenceScores.Neutral}.");
                    log.LogInformation($"  Negative confidence score: {docSentiment.ConfidenceScores.Negative}.");
                }
            }
            catch (RequestFailedException exception)
            {
                log.LogInformation($"Error Code: {exception.ErrorCode}");
                log.LogInformation($"Message: {exception.Message}");
            }
        }
    }
}
