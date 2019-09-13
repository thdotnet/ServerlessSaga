using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FunctionApp10
{
    public static class OrderSaga
    {
        [FunctionName("OrderSaga")]
        public static async Task<bool> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var orderId = context.GetInput<Guid>();

            Task<bool> orderResponse = context.CallActivityAsync<bool>("OrderActivity", orderId);
            Task<bool> paymentResponse = context.CallActivityAsync<bool>("PaymentActivity", orderId);

            await Task.WhenAll(orderResponse, paymentResponse);

            if (orderResponse.Result == false || paymentResponse.Result == false)
            {
                await context.CallActivityAsync("RollbackOrderActivity", orderId);
                await context.CallActivityAsync("RollbackPaymentActivity", orderId);

                return false;
            }

            return true;
        }

        [FunctionName("OrderActivity")]
        public static bool OrderActivity([ActivityTrigger] Guid orderId, ILogger log)
        {
            log.LogInformation($"Started activity OrderActivity for orderId= '{orderId}'.");

            //should contain real logic in here
            return true;
        }

        [FunctionName("RollbackOrderActivity")]
        public static bool RollbackOrderActivity([ActivityTrigger] Guid orderId, ILogger log)
        {
            log.LogInformation($"Started activity RollbackOrderActivity for orderId= '{orderId}'.");

            //should contain real logic in here
            return true;
        }

        [FunctionName("RollbackPaymentActivity")]
        public static bool RollbackPaymentActivity([ActivityTrigger] Guid orderId, ILogger log)
        {
            log.LogInformation($"Started activity RollbackPaymentActivity for orderId= '{orderId}'.");

            //should contain real logic in here
            return true;
        }

        [FunctionName("PaymentActivity")]
        public static bool PaymentActivity([ActivityTrigger] Guid orderId, ILogger log)
        {
            log.LogInformation($"Started activity PaymentActivity for orderId= '{orderId}'.");

            //should contain real logic in here
            return false;
        }


        [FunctionName("OrderSaga_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            ILogger log)
        {
            var orderId = Guid.NewGuid();

            string instanceId = await starter.StartNewAsync("OrderSaga", orderId);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }

}