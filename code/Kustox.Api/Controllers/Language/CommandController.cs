using Microsoft.AspNetCore.Mvc;

namespace Kustox.Api.Controllers.Language
{
    [ApiController]
    [Route("[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly ILogger<CommandController> _logger;

        public CommandController(ILogger<CommandController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public CommandOutput Post(CommandInput input)
        {
            return new CommandOutput
            {
                Number = 42
            };
        }
    }
}