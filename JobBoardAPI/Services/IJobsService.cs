using JobBoardAPI.Controllers;
using JobBoardAPI.Models;

namespace JobBoardAPI.Services
{
    public interface IJobsService
    {
        public Task<List<Job>> GetJobs(string title);
        public Task<Job?> GetJob(Guid id);
        public Task<Job?> CreateJob(JobParams jobParams, Guid userId);
    }
}
