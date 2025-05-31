using JobBoardAPI.Models;

namespace JobBoardAPI.Services
{
    public interface IJobsService
    {
        public Task<List<Job>> GetJobs(string title);
    }
}
