using MMAWikiProvider.Models;
using System.Collections.Generic;

namespace MMAWikiProvider.Logic
{
    public interface IRunsState
    {
        public void SetRuns(List<Runs> runs);

        public List<Runs> GetRuns();
    }

    public class RunsState : IRunsState
    {
        private List<Runs> _runs;

        public List<Runs> GetRuns()
        {
            return _runs;
        }

        public void SetRuns(List<Runs> runs)
        {
            _runs = runs;
        }
    }
}
