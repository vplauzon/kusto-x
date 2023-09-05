using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;

namespace Kustox.Workbench.Controllers.Language
{
    [ApiController]
    [Route("api/[controller]")]
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
                Input = input.Csl,
                Table = new TableOutput
                {
                    Columns = ImmutableArray<ColumnOutput>.Empty
                    .Add(new ColumnOutput { Name = "Id", Type = "System.Int32" })
                }
            };
        }
    }
}