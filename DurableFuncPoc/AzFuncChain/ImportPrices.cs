using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace AzFuncChain
{
    public class ImportPrices
    {
        [FunctionName("ImportPrices")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log,
            [DurableClient] IDurableClient client
            )
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            //var gameMonitor = await Global.FindJob(
            //client,
            //DateTime.UtcNow,
            //nameof(MonitorFunctions.GameMonitorWorkflow),
            //name,
            //true,
            //false);
            //if (gameMonitor != null)

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<SomeObj>(requestBody);

            log.LogInformation("Added delay for 5 minutes and now waiting to finish 5 minutes");
            //await Task.Delay(300 * 1000);

            await client.RaiseEventAsync(data.ReqBy, "PriceLevelImported");

            log.LogInformation("done waiting for 5 minutes");
            return new OkObjectResult($"Import done from {data.ReqBy}");
        }


    }

    public class SomeObj
    {
        public string ReqBy { get; set; }

    }
}
