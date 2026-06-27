using AgentCfo.Core.Common;
using AgentCfo.Core.Enums;

namespace AgentCfo.Core.Entities;

public class Forecast : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public ForecastSource GeneratedBy { get; private set; }
    public Money CurrentCashBalance { get; private set; } = Money.Zero();
    public Money MonthlyBurnRate { get; private set; } = Money.Zero();
    public Money MonthlyRevenue { get; private set; } = Money.Zero();
    public int RunwayDays { get; private set; }
    public DateOnly RunwayEndDate { get; private set; }
    public string? Scenario { get; private set; }
    public ForecastConfidence Confidence { get; private set; } = ForecastConfidence.Medium;
    
    private readonly List<ProjectionPoint> _projectionPoints = new();
    public IReadOnlyCollection<ProjectionPoint> ProjectionPoints => _projectionPoints.AsReadOnly();
    
    public Organization Organization { get; private set; } = null!;
    
    private Forecast() { }
    
    public static Forecast Create(
        Guid organizationId,
        Money currentCashBalance,
        Money monthlyBurnRate,
        Money monthlyRevenue,
        ForecastSource source = ForecastSource.Agent,
        string? scenario = null)
    {
        var netBurn = monthlyBurnRate.Subtract(monthlyRevenue);
        var runwayDays = netBurn.IsZero || netBurn.IsNegative 
            ? 9999  // Profitable or breaking even
            : (int)(currentCashBalance.Amount / netBurn.Amount * 30);
        
        return new Forecast
        {
            OrganizationId = organizationId,
            GeneratedBy = source,
            CurrentCashBalance = currentCashBalance,
            MonthlyBurnRate = monthlyBurnRate,
            MonthlyRevenue = monthlyRevenue,
            RunwayDays = runwayDays,
            RunwayEndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(runwayDays)),
            Scenario = scenario ?? "base",
            Confidence = runwayDays > 365 ? ForecastConfidence.Low : 
                         runwayDays > 180 ? ForecastConfidence.Medium : ForecastConfidence.High
        };
    }
    
    public void AddProjectionPoint(DateOnly date, Money projectedBalance, Money projectedRevenue, Money projectedExpenses)
    {
        _projectionPoints.Add(new ProjectionPoint(date, projectedBalance, projectedRevenue, projectedExpenses));
    }
}

public class ProjectionPoint
{
    public DateOnly Date { get; private set; }
    public Money ProjectedBalance { get; private set; } = Money.Zero();
    public Money ProjectedRevenue { get; private set; } = Money.Zero();
    public Money ProjectedExpenses { get; private set; } = Money.Zero();

    private ProjectionPoint() { }

    public ProjectionPoint(DateOnly date, Money projectedBalance, Money projectedRevenue, Money projectedExpenses)
    {
        Date = date;
        ProjectedBalance = projectedBalance;
        ProjectedRevenue = projectedRevenue;
        ProjectedExpenses = projectedExpenses;
    }
}
