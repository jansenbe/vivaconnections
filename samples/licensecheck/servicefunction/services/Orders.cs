using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace services
{
    public static class Orders
    {
        [FunctionName("Orders")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Orders service called");

            string jsonData = "{\"items\":[{\"id\":1,\"orderDate\":\"2016-01-06T00:00:00\",\"region\":\"east\",\"rep\":\"Jones\",\"item\":\"Pencil\",\"units\":95,\"unitCost\":1.99,\"total\":189.05},{\"id\":2,\"orderDate\":\"2016-01-23T00:00:00\",\"region\":\"central\",\"rep\":\"Kivell\",\"item\":\"Binder\",\"units\":50,\"unitCost\":19.99,\"total\":999.5},{\"id\":3,\"orderDate\":\"2016-02-09T00:00:00\",\"region\":\"central\",\"rep\":\"Jardine\",\"item\":\"Pencil\",\"units\":36,\"unitCost\":4.99,\"total\":179.64},{\"id\":4,\"orderDate\":\"2016-02-26T00:00:00\",\"region\":\"central\",\"rep\":\"Gill\",\"item\":\"Pen\",\"units\":27,\"unitCost\":19.99,\"total\":539.73},{\"id\":5,\"orderDate\":\"2016-03-15T00:00:00\",\"region\":\"west\",\"rep\":\"Sorvino\",\"item\":\"Pencil\",\"units\":56,\"unitCost\":2.99,\"total\":167.44}]}";
            
            return new OkObjectResult(jsonData);
        }
    }
}
