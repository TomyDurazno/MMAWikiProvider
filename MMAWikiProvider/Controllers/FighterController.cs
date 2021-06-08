using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UFCWikiProvider.Logic;

namespace UFCWikiProvider.Controllers
{
    [ApiController]
    public class FightersController : ControllerBase
    {
        private readonly ILogger<FightersController> _logger;
        private readonly IFighterStoreHandler handler;

        public FightersController(ILogger<FightersController> logger, IFighterStoreHandler handler)
        {
            _logger = logger;
            this.handler = handler;
        }

        [Route("fighters/{name}")]
        [HttpGet]
        public IActionResult Get(string name)
        {
            var fighter = handler.GetFighter(name);

            if (fighter == null)
                return NotFound();

            return Ok(fighter);
        }

        [Route("fighters/byname/{name}")]
        [HttpGet]
        public IActionResult GetByName(string name)
        {
            var fighters = handler.FightersByName(name);

            if (fighters == null)
                return NotFound();

            return Ok(fighters);
        }
    }
}
