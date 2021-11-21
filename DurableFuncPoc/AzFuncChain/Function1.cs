using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzFuncChain
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<List<string>> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("Function1_Hello", "London"));

            //var response = await context.CallHttpAsync(HttpMethod.Post, new System.Uri("http://localhost:7071/api/ImportPrices") , "{'ReqBy':'Hasibul'}");
            //outputs.Add(response.Content);
            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]

            //var res = await context.CallActivityAsync<string>("Function1_LongRunningActivity", "HEHE");
            //context.WaitForExternalEvent<>


            log.LogInformation("Waiting  for PriceLevelImported...");
            var approvalEvent = await context.WaitForExternalEvent<bool>("PriceLevelImported");


            return outputs;
        }

        [FunctionName("Function1_Hello")]
        public static async Task<string> SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            await Task.Delay(2000);
            return $"Hello {name}!";
        }

        [FunctionName("Function1_LongRunningActivity")]
        public static async Task<string> LongRunningActivity([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying long running to {name}. and waiting for 5 minutes");
            await Task.Delay(10 * 1000);
            log.LogInformation($"Done Saying long running to {name}. and waiting for 5 minutes");
            return $"Long runnin done for 5 minutes {name}!";
        }



        [FunctionName("Function1_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function1", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}