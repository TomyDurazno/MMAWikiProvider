using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            var dic = await stats.TopByDic(number, f => f.Record.Count(r => r.Event.IsUFCEvent), 
                                                        f => f.Record.Where(r => r.Event.IsUFCEvent)
                                                                     .Select(r => r.Event.Description));

            return Ok(dic);
        }

        [Route("stats/top/{number}/ufcfighters/byoctagontime")]
        [HttpGet]
        public async Task<IActionResult> TopByElapsedTime(int number)
        {
            var dic = await stats.TopByDic(number, f => f.Record.Where(r => r.Event.IsUFCEvent).ElapsedTime(), 
                                                   f => f.Record.Where(r => r.Event.IsUFCEvent).ElapsedTime().ToString("hh':'mm':'ss"));

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

        [Route("stats/top/{number}/ufcfighters/byknockouts")]
        [HttpGet]
        public async Task<IActionResult> TopByKnockouts(int number)
        {
            var dic = await stats.TopByDic(number, f => f.Record.Sum(r => r.Event.IsUFCEvent && r.Method.IsKO_TKO() && r.Result == FightResult.Win ? 1 : 0));

            return Ok(dic);
        }

        [Route("stats/top/{number}/ufcfighters/bysubmissions")]
        [HttpGet]
        public async Task<IActionResult> TopBySubmissions(int number)
        {
            var dic = await stats.TopByDic(number, f => f.Record.Sum(r => r.Event.IsUFCEvent && r.Method.IsSubmission() && r.Result == FightResult.Win ? 1 : 0));

            return Ok(dic);
        }

        [Route("stats/top/{number}/ufcfighters/byeyepokes")]
        [HttpGet]
        public async Task<IActionResult> TopByEyepokes(int number)
        {
            var dic = await stats.TopByDic(number, f => f.Record.Sum(r => r.Event.IsUFCEvent && r.Method.Description.Contains("eye poke") ? 1 : 0));

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
