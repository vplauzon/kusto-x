using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebPortal.Pages
{
    public class ClustersModel : PageModel
    {
        private readonly ILogger<ClustersModel> _logger;

        public ClustersModel(ILogger<ClustersModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}
