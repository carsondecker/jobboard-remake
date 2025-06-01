using JobBoardAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace JobBoardAPI.Controllers
{
    public class JobParams
    {
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Description { get; set; } = null!;
        [Required] 
        public string City { get; set; } = null!;
    }

    [ApiController]
    [Route("/api/[controller]")]
    public class JobsController : Controller
    {
        private readonly IJobsService _jobsService;
        public JobsController(IJobsService jobsService)
        {
            _jobsService = jobsService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetJobs([FromQuery] string title = "")
        {
            try
            {
                var jobs = await _jobsService.GetJobs(title);
                var trimmedJobs = jobs.Select(job => new
                {
                    jobId = job.JobId,
                    title = job.Title,
                    description = job.Description,
                    city = job.City,
                    userId = job.UserId
                });

                return Ok(trimmedJobs);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "A database error occured, please try again later." });
            }
        }

        [AllowAnonymous]
        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetJob([FromRoute] Guid jobId)
        {
            try
            {
                var job = await _jobsService.GetJob(jobId);
                if (job == null)
                {
                    return NotFound();
                }

                return Ok(job);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "A database error occured, please try again later." });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] JobParams jobParams)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var job = await _jobsService.CreateJob(jobParams, userId);
                if (job == null)
                {
                    return BadRequest("The user you are creating a job for does not exist.");
                }

                return Ok(new
                {
                    jobId = job.JobId,
                    title = job.Title,
                    description = job.Description,
                    city = job.City,
                    userId = job.UserId
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "A database error occured, please try again later." });
            }
        }


        [Authorize]
        [HttpDelete("{jobId}")]
        public async Task<IActionResult> DeleteJob([FromRoute] Guid jobId)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null)
            {
                return Unauthorized();
            }

            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            try
            {
                await _jobsService.DeleteJob(jobId, userId);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("You do not own this job.");
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "A database error occured, please try again later." });
            }
        }
    }
}
