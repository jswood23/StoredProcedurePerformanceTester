using System.Globalization;
using Microsoft.Extensions.Configuration;
using NodaTime;

namespace StoredProcedurePerformanceTester;

public class UsageSummaryCreditsReport
{
    private const string SprocName = "FIREBIRD_GetUsageSummaryCredits";
    private readonly IConfigurationRoot _configuration;
    private readonly Dictionary<string, SprocParam> _parameters;
    
    public UsageSummaryCreditsReport(IConfigurationRoot config, LocalDate dateFrom, LocalDate dateTo)
    {
        _parameters = new Dictionary<string, SprocParam>();
        _parameters.Add("@AccountId", new SprocParam.IntParam(1088103));
        _parameters.Add("@DefaultUserId", new SprocParam.IntParam(1485135));
        _parameters.Add("@DateFrom", new SprocParam.StringParam(dateFrom.ToString("yyyy-MM-dd", new DateTimeFormatInfo())));
        _parameters.Add("@DateTo", new SprocParam.StringParam(dateTo.ToString("yyyy-MM-dd", new DateTimeFormatInfo())));
        
        _configuration = config;
    }
    
    public async Task Run()
    {
        DbAccess dbAccess = new DbAccess(_configuration);
        try
        {
            await dbAccess.RunSproc(SprocName, _parameters);
        }
        finally
        {
            dbAccess.Dispose();
        }
    }
}