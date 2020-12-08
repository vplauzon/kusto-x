using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebPortal.Pages
{
    public class NewDataFlowModel : PageModel
    {
        private readonly ILogger<NewDataFlowModel> _logger;

        public NewDataFlowModel(ILogger<NewDataFlowModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
