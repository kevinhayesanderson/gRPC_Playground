using Grpc.Core;
using System;
using System.Threading.Tasks;
using Worker;

namespace StatusMicroservice.Services
{
    public class JobManagerService : JobManager.JobManagerBase
    {
        public override async Task TriggerJobs(TriggerJobsRequest request, IServerStreamWriter<TriggerJobsResponse> responseStream, ServerCallContext context)
        {
            for (int i = 0; i < request.JobsCount; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));

                await responseStream.WriteAsync(new TriggerJobsResponse
                {
                    JobSequence = i + 1,
                    JobMessage = "Job executed successfully"
                });
            }
        }

        public override async Task<SendJobsResponse> SendJobs(IAsyncStreamReader<SendJobsRequest> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var job = requestStream.Current;
                Console.WriteLine($"Job Id: {job.JobId}");
                Console.WriteLine($"Job description: {job.JobDescription}");
                await Task.Delay(TimeSpan.FromSeconds(3));
            }

            return new SendJobsResponse
            {
                Completed = true
            };
        }
    }
}