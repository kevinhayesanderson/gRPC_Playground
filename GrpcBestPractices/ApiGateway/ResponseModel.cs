﻿namespace ApiGateway
{
    public class ResponseModel
    {
        public List<PerformanceStatusModel> PerformanceStatuses { get; } = [];

        public double RequestProcessingTime { get; set; }

        public class PerformanceStatusModel
        {
            public double CpuPercentageUsage { get; set; }

            public double MemoryUsage { get; set; }

            public int ProcessesRunning { get; set; }

            public int ActiveConnections { get; set; }

            public byte[] DataLoad1 { get; set; }

            public byte[] DataLoad2 { get; set; }
        }
    }
}