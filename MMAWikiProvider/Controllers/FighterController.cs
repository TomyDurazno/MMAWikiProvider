using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UFCWikiProvider.Logic;
using UFCWikiProvider.Models;

namespace UFCWikiProvider.Controllers
{
    [ApiController]
    public class FightersController : ControllerBase
    {
        private readonly ILogger<FightersController> _logger;
        private readonly IFighterListHandler handler;

        public FightersController(ILogger<FightersController> logger, IFighterListHandler handler)
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
