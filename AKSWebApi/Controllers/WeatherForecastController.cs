using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AKSWebApi.Controllers
{
    [AKSWebApi.Attributes.ControllerName("Weather")]
    public class WeatherForecastController : BaseApiController
    {
        int result;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
            result = 0;
        }

        /// <summary>
        /// Gets 5 days weather forecast
        /// </summary>
        /// <response code="200">Success response with the forecast list</response>
        /// <response code="400">Invalid Request</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation("Weather")]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("Inside Weather Get - Begin");
            var rng = new Random();
            var res =  Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
            _logger.LogInformation("Inside Weather Get - End");
            return res;
        }

        /// <summary>
        /// Logs an exception
        /// </summary>
        /// <response code="200">Success response with the return value</response>
        /// <response code="400">Invalid Request</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation("WeatherException")]
        public int GetException()
        {
            try
            {
                int num1 = 25;
                int num2 = 0;
                result = num1 / num2;
            }
            catch (DivideByZeroException ex)
            {
                //_logger.LogError("Exception caught: {0}", ex);
                throw ex;
            }
            //finally
            //{
            //    _logger.LogError("Result: {0}", result);
            //}
            return result;
        }
    }
}
