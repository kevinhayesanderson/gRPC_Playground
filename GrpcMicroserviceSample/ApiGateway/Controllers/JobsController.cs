using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ApiGateway.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class JobsController(IGrpcJobsClient client) : ControllerBase
    {
        [HttpPost("")]
        public void SendJobs([FromBody] IEnumerable<JobModel> jobs)
        {
            _ = client.SendJobs(jobs);
        }

        [HttpPost("{jobsCount}")]
        public void TriggerJobs(int jobsCount)
        {
            _ = client.TriggerJobs(jobsCount);
        }
    }
}
