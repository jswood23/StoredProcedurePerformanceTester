using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using NodaTime;

namespace StoredProcedurePerformanceTester;

public class TestUsageSummaryPerformance
{
    private LocalDate _dateFrom = new(2023, 1, 1);
    private LocalDate _dateTo = new(2024, 1, 1);
    private readonly IConfigurationRoot _configuration;
    private readonly int _iterationsPerChunkSize = 10;
    
    public TestUsageSummaryPerformance(IConfigurationRoot config)
    {
        _configuration = config;
    }

    public async Task TestAll(StreamWriter writer)
    {
        List<int> sequence = new List<int>();
        for (int i = 365; i > 1; i -= 20)
        {
            sequence.Add(i);
        }

        // Randomize the sequence
        Random random = new Random();
        sequence = sequence.OrderBy(x => random.Next()).ToList();
        
        writer.WriteLine("Chunk size, Requests, Runtime");
        foreach (var i in sequence)
        {
            for (int j = 0; j < _iterationsPerChunkSize; j++)
            {
                (TimeSpan timeSpan, int requests) = (default, 0);
                (timeSpan, requests) = await GetChunkSizePerformance(i);
                writer.WriteLine($"{i}, {requests}, {timeSpan}");
            }
        }
    }
    
    private async Task<(TimeSpan, int)> GetChunkSizePerformance(int dayChunkSize)
    {
        List<Task> tasks = new();
        
        LocalDate dateFrom = _dateFrom;

        Stopwatch stopwatch = Stopwatch.StartNew();
        int requests = 0;
        
        while (dateFrom < _dateTo)
        {
            LocalDate dateTo = dateFrom.PlusDays(dayChunkSize);
            if (dateTo > _dateTo)
            {
                dateTo = _dateTo;
            }
            
            UsageSummaryCreditsReport report = new(_configuration, dateFrom, dateTo);
            tasks.Add(report.Run());

            dateFrom = dateTo;
            requests ++;
        }

        await Task.WhenAll(tasks);
        
        stopwatch.Stop();
        return (stopwatch.Elapsed, requests);
    }
}