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
    public class FighterController : ControllerBase
    {
        private readonly ILogger<FighterController> _logger;
        private readonly IFighterListHandler handler;

        public FighterController(ILogger<FighterController> logger, IFighterListHandler handler)
        {
            _logger = logger;
            this.handler = handler;
        }

        [Route("Fighter/{name}")]
        public async Task<Fighter> Get(string name)
        {
            try
            {
                var fighter = handler.GetFighter(name);

                return fighter;
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return null;
            }
        }
    }
}
