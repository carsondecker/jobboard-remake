using JobBoardAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobBoardAPI.Controllers
{
    [Route("/api/[controller]")]
    public class JobsController : Controller
    {
        private readonly IJobsService _jobsService;
        public JobsController(IJobsService jobsService)
        {
            _jobsService = jobsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetJobs([FromQuery] string title = "")
        {
            try
            {
                return Ok(await _jobsService.GetJobs(title));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "A database error occured, please try again later." });
            }
        }
    }
}
