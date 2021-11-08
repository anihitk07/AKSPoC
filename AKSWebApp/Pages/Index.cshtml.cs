using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AKSWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async void OnGet()
        {
            _logger.LogInformation("Inside Index View - Init");
            var apiBaseUrl = "http://10.30.128.52";
            var valuesUrl = System.IO.Path.Combine(apiBaseUrl, "/aks-api/v1/get");
            var client = new HttpClient();
            var result = await client.GetAsync(valuesUrl);
            dynamic values = JsonConvert.DeserializeObject(await result.Content.ReadAsStringAsync());
            _logger.LogInformation("Inside Index View - End");
        }
    }
}
