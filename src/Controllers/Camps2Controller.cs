using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [Route("api/camps")]
    [ApiVersion("2.0")]
    [ApiController]
    public class Camps2Controller : ControllerBase
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;
        private readonly ILogger<CampsController> logger;
        private readonly LinkGenerator linkGenerator;

        public Camps2Controller(ICampRepository campRepository, IMapper mapper, ILogger<CampsController> logger, LinkGenerator linkGenerator)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
            this.logger = logger;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> Get(bool includeTalks = false)
        {
            try
            {
                var results = await campRepository.GetAllCampsAsync(includeTalks);
                var result = new
                {
                    Count = results.Count(),
                    Results = mapper.Map<CampModel[]>(results),
                };
                return Ok(result);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var result = await campRepository.GetCampAsync(moniker);
                if (result == null) return NotFound();
                return mapper.Map<CampModel>(result);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> Search(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                var results = await campRepository.GetAllCampsByEventDate(theDate, includeTalks);
                if (!results.Any()) return NotFound();
                return mapper.Map<CampModel[]>(results);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost()]
        public async Task<ActionResult<CampModel>> Post(CampModel campModel)
        {
            try
            {
                var existing = await campRepository.GetCampAsync(campModel.Moniker);
                if (existing != null)
                {
                    return BadRequest("Moniker in use");
                }
                var location = linkGenerator.GetPathByAction("Get", "Camps", new { moniker = campModel.Moniker });
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }
                var camp = mapper.Map<Camp>(campModel);
                campRepository.Add(camp);
                if (await campRepository.SaveChangesAsync())
                {
                    return Created(location, mapper.Map<CampModel>(camp));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel campModel)
        {
            try
            {
                var oldCamp = await campRepository.GetCampAsync(moniker);
                if (oldCamp == null)
                {
                    return NotFound($"Could not find camp with moniker of {moniker}");
                }

                mapper.Map(campModel, oldCamp);

                if (await campRepository.SaveChangesAsync())
                {
                    return mapper.Map<CampModel>(oldCamp);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

            return BadRequest();
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var camp = await campRepository.GetCampAsync(moniker);
                if (camp == null)
                {
                    return NotFound($"Could not find camp with moniker of {moniker}");
                }
                campRepository.Delete(camp);
                if (await campRepository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

            return BadRequest();
        }
    }
}
