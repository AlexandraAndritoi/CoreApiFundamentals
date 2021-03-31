using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationsController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<OperationsController> logger;

        public OperationsController(IConfiguration configuration, ILogger<OperationsController> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpOptions("reloadconfig")]
        public IActionResult ReloadConfig()
        {
            try
            {
                var root = configuration as IConfigurationRoot;
                root.Reload();
                return Ok();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
    }
}
