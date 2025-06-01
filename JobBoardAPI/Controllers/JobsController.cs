using JobBoardAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobBoardAPI.Controllers
{
    public class JobParams
    {
        public string Title = null!;
        public string Description = null!;
        public string City = null!;
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
                return Ok(await _jobsService.GetJobs(title));
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "A database error occured, please try again later." });
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob([FromRoute] Guid id)
        {
            try
            {
                var job = await _jobsService.GetJob(id);
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
    }
}
