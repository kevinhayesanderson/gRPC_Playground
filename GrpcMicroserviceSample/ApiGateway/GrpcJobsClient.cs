using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Worker;

namespace ApiGateway
{
    public interface IGrpcJobsClient
    {
        Task SendJobs(IEnumerable<JobModel> jobs);

        Task TriggerJobs(int jobCount);
    }

    public class GrpcJobsClient : IGrpcJobsClient, IDisposable
    {
        private readonly GrpcChannel _channel;

        private readonly JobManager.JobManagerClient _client;

        public GrpcJobsClient(string serverUrl)
        {
            _channel = GrpcChannel.ForAddress(serverUrl);
            _client = new JobManager.JobManagerClient(_channel);
        }

        public void Dispose()
        {
            _channel.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task SendJobs(IEnumerable<JobModel> jobs)
        {
            using var call = _client.SendJobs();

            foreach (var job in jobs)
            {
                await call.RequestStream.WriteAsync(new SendJobsRequest
                {
                    JobId = job.JobId,
                    JobDescription = job.JobDescription
                });
            }

            await call.RequestStream.CompleteAsync();

            await call;
        }

        public async Task TriggerJobs(int jobCount)
        {
            using var call = _client.TriggerJobs(new TriggerJobsRequest { JobsCount = jobCount });

            while (await call.ResponseStream.MoveNext())
            {
                var response = call.ResponseStream.Current;

                await Console.Out.WriteLineAsync($"Job sequence: {response.JobSequence}");
                await Console.Out.WriteLineAsync($"Job message: {response.JobMessage}");

                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }
}