using JobBoardAPI.Data;
using JobBoardAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobBoardAPI.Services
{
    public class JobsService : IJobsService
    {
        private readonly DataContext _dbContext;

        public JobsService(DataContext dbContext)
        {
            dbContext = _dbContext;
        }


        public async Task<List<Job>> GetJobs(string title = "")
        {
            try
            {
                var jobs = await _dbContext.Jobs
                    .Where(j => j.Title.Contains(title))
                    .ToListAsync();
                return jobs;
            }
            catch (Exception)
            {
                throw new Exception("Database error, please try again later.");
            }
        }
    }
}
