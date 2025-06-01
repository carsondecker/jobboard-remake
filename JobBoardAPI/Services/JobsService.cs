using JobBoardAPI.Controllers;
using JobBoardAPI.Data;
using JobBoardAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobBoardAPI.Services
{
    public class JobsService : IJobsService
    {
        private readonly DataContext _dbContext;

        public JobsService(DataContext dbContext)
        {
            _dbContext = dbContext;
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

        public async Task<Job?> GetJob(Guid id)
        {
            try
            {
                var job = await _dbContext.Jobs
                    .Where(j => j.JobId == id)
                    .FirstOrDefaultAsync();
                return job;
            }
            catch (Exception)
            {
                throw new Exception("Database error, please try again later.");
            }
        }

        public async Task<Job?> CreateJob(JobParams jobParams, Guid userId)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user == null)
                {
                    return null;
                }

                var job = new Job
                {
                    JobId = Guid.NewGuid(),
                    Title = jobParams.Title,
                    Description = jobParams.Description,
                    City = jobParams.City,
                    UserId = userId,
                    User = user
                };

                await _dbContext.Jobs.AddAsync(job);
                await _dbContext.SaveChangesAsync();

                return job;
            }
            catch (Exception)
            {
                throw new Exception("Database error, please try again later.");
            }
        }

        public async Task DeleteJob(Guid jobId, Guid userId)
        {
            var job = await _dbContext.Jobs.FindAsync(jobId);
            if (job == null)
            {
                throw new KeyNotFoundException("Job not found.");
            }

            if (job.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not own this job.");
            }

            _dbContext.Jobs.Remove(job);
            await _dbContext.SaveChangesAsync();
        }
    }
}
