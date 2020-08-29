using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiFrontEnd.Controllers
{
    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        private readonly ILogger<RootController> _logger;

        public RootController(ILogger<RootController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult Get()
        {
            return new ObjectResult(new
            {
                Name = "front-end",
                Version = ApiVersion.FullVersion,
                Status = "Ready"
            });
        }
    }
}
