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
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public async void OnGet()
        {
            _logger.LogInformation("Inside Privacy View - Init");

            try
            {
                var apiBaseUrl = "http://10.30.128.52";
                var valuesUrl = System.IO.Path.Combine(apiBaseUrl, "/aks-api/v1/getexception");
                var client = new HttpClient();
                var result = await client.GetAsync(valuesUrl);
                dynamic values = JsonConvert.DeserializeObject(await result.Content.ReadAsStringAsync());
            }
            catch(Exception ex)
            {
                _logger.LogError("Exception caught: {0}", ex);
            }
            _logger.LogInformation("Inside Privacy View - End");
        }
    }
}
