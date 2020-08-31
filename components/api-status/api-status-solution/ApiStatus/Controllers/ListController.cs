using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApiStatus.Controllers
{
    [ApiController]
    [Route("/list")]
    public class ListController : ControllerBase
    {
        private readonly ILogger<RootController> _logger;

        public ListController(ILogger<RootController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<int> Get()
        {
            return new ObjectResult(new[] { 12, 45, 123 });
        }
    }
}
