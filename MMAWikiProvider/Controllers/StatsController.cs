using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MMAWikiProvider.Logic;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using MMAWikiProvider.Logic;
using MMAWikiProvider.Models;

namespace MMAWikiProvider.Controllers
{
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly ILogger<StatsController> _logger;
        private readonly IFighterStats stats;

        public StatsController(ILogger<StatsController> logger, IFighterStats stats)
        {
            _logger = logger;
            this.stats = stats;
        }

        [Route("stats/top/{number}/ufcfighters/bybuchholzratio")]
        [HttpGet]
        public async Task<IActionResult> TopByBuchholzRatio(int number)
        {
            var dic = await stats.TopByDic(number, f => f.Record.Sum(a => a.Opponent.Record.WinLoseRatio()));

            return Ok(dic);
        }

        [Route("stats/top/{number}/ufcfighters/bytitledefenses")]
        [HttpGet]
        public async Task<IActionResult> TopByUFCTitleDefenses(int number)
        {
            var dic = await stats.TopByDic(number, f => f.Record.Count(r => r.Notes.DefendedUFCChampionship == true));

            return Ok(dic);
        }

        [Route("stats/top/{number}/ufcfighters/bytitlefights")]
        [HttpGet]
        public async Task<IActionResult> TopByUFCTitleFights(int number)
        {
            var dic = await stats.TopByDic(number, f => f.Record.Count(r => r.Notes.IsUFCChampionshipBout == true));

            return Ok(dic);
        }

        [Route("stats/top/{number}/ufcfighters/byufcppvevents")]
        [HttpGet]
        public async Task<IActionResult> TopByUFCPPVEvents(int number)
        {
            var dic = await stats.TopByDic(number, f => f.Record.Count(r => r.Event.IsUFCPPV));

            return Ok(dic);
        }

        [Route("stats/top/{number}/ufcfighters/byufcevents")]
        [HttpGet]
        public async Task<IActionResult> TopByUFCEvents(int number)
        {
            var dic = await stats.TopByDic(number, f => f.Record.Count(r => r.Event.IsUFCEvent));

            return Ok(dic);
        }

        [Route("stats/top/{number}/ufcfighters/bywins")]
        [HttpGet]
        public async Task<IActionResult> TopByUFCWins(int number)
        {
            var dic = await stats.TopByDic(number, f => f.Record.Count(r => r.Event.IsUFCEvent && r.Result == FightResult.Win));

            return Ok(dic);
        }

        [Route("stats/top/{number}/ufcfighters/bybonus")]
        [HttpGet]
        public async Task<IActionResult> TopByBonus(int number)
        {
            var dic = await stats.TopByDic(number, f => f.Record.Sum(r => r.Notes.Bonuses.Count()), f => f.BonusGroup());

            return Ok(dic);
        }

        [Route("stats/buchholzratio/{name}")]
        [HttpGet]
        public IActionResult BuchholzRatio(string name)
        {
            var ratio = stats.BuchholzRatio(name);

            /*if (fighters == null)
                return NotFound();*/

            return Ok(ratio);
        }
    }
}
