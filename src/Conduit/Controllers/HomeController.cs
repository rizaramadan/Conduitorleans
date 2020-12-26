using GrainInterfaces;
using GrainInterfaces.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Conduit.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IClusterClient _client;

        public HomeController(ILogger<HomeController> logger, IClusterClient c)
        {
            _logger = logger;
            _client = c;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var greeting = "Good morning, HelloGrain!";
            var friend = _client.GetGrain<IHello>(0);
            _logger.LogInformation(greeting);
            var response = await friend.SayHello(greeting);
            return new JsonResult(new { response = response });
        }
    }
}
