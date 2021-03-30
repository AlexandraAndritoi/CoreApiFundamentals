using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : Controller
    {
        private readonly ICampRepository campRepository;
        private readonly IMapper mapper;
        private readonly LinkGenerator linkGenerator;
        private readonly ILogger<TalksController> logger;

        public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator, ILogger<TalksController> logger)
        {
            this.campRepository = campRepository;
            this.mapper = mapper;
            this.linkGenerator = linkGenerator;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await campRepository.GetTalksByMonikerAsync(moniker, true);
                return mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if(talk == null)
                {
                    return NotFound($"Talk with {id} could not be found");
                }
                return mapper.Map<TalkModel>(talk);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel talkModel)
        {
            try
            {
                var camp = await campRepository.GetCampAsync(moniker);
                if (camp == null)
                {
                    return BadRequest($"Could not find camp with moniker of {moniker}");
                }

                var talk = mapper.Map<Talk>(talkModel);
                talk.Camp = camp;

                if (talkModel.Speaker == null)
                {
                    return BadRequest("Speaker Id is required");
                }

                var speaker = await campRepository.GetSpeakerAsync(talkModel.Speaker.SpeakerId);
                if (speaker == null)
                {
                    return BadRequest("Speaker could not be found");
                }

                talk.Speaker = speaker;
                campRepository.Add(talk);

                if (await campRepository.SaveChangesAsync())
                {
                    var location = linkGenerator.GetPathByAction(HttpContext, "Get", values: new { moniker, id = talk.TalkId });
                    return Created(location, mapper.Map<TalkModel>(talk));
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

            return BadRequest();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel talkModel)
        {
            try
            {
                var talk = await campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null)
                {
                    return NotFound($"Talk with {id} could not be found");
                }

                mapper.Map(talkModel, talk);

                if (talkModel.Speaker != null)
                {
                    var speaker = await campRepository.GetSpeakerAsync(talkModel.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                if (await campRepository.SaveChangesAsync())
                {
                    return mapper.Map<TalkModel>(talk);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }

            return BadRequest();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var talk = await campRepository.GetTalkByMonikerAsync(moniker, id);
                if(talk == null)
                {
                    return NotFound($"Talk with {id} could not be found");
                }
                campRepository.Delete(talk);
                if(await campRepository.SaveChangesAsync())
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
