using IntegrationControlFlow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace KustoxApiFrontEnd.Controllers
{
    [ApiController]
    [Route("/v1")]
    public class RequestController : ControllerBase
    {
        private readonly ILogger<RootController> _logger;

        public RequestController(ILogger<RootController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Consumes("text/plain")]
        public async Task<string> PostAsync()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var text = await reader.ReadToEndAsync();
                var planner = new IntegratedPlanner();
                var output = await planner.PushRequestAsync(text);

                return "42";
            }
        }
    }
}