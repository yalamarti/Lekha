using Lekha.Scheduler.BusinessLogic;
using Lekha.Scheduler.BusinessLogic.Consumers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Lekha.Scheduler.FunctionApp
{
    public class AccountSchedulerFunction
    {
        private readonly AccountScheduler accountScheduler;
        private readonly AccountMessageConsumer accountMessageConsumer;

        public AccountSchedulerFunction(AccountScheduler accountScheduler, AccountMessageConsumer accountMessageConsumer)
        {
            this.accountScheduler = accountScheduler;
            this.accountMessageConsumer = accountMessageConsumer;
        }
        [Function("ScheduleAccount")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Function1");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            CancellationTokenSource cts = new CancellationTokenSource();
            await accountScheduler.Start(cts.Token);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
