using Banking.Application.Abstractions;
using Banking.Application.Investments.DTOs;

namespace Banking.Application.Investments.Queries;

public sealed class GetPortfolioQueryHandler
{
    private readonly IInvestmentRepository _investmentRepository;

    public GetPortfolioQueryHandler(IInvestmentRepository investmentRepository)
    {
        _investmentRepository = investmentRepository;
    }

    public async Task<PortfolioDto> HandleAsync(
        GetPortfolioQuery query,
        CancellationToken cancellationToken = default)
    {
        var holdings = await _investmentRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);

        var holdingDtos = holdings
            .Select(h => new HoldingDto(
                h.Id,
                h.Symbol,
                h.Name,
                h.Shares,
                h.CurrentValue,
                h.DayChangePercent))
            .ToList();

        var totalValue = holdingDtos.Sum(h => h.Value);
        var dayChange = holdingDtos.Sum(h => h.Value * (h.ChangePercent / 100m));
        var dayChangePercent = totalValue > 0
            ? Math.Round(dayChange / totalValue * 100, 2)
            : 0;

        return new PortfolioDto(totalValue, Math.Round(dayChange, 2), dayChangePercent, holdingDtos);
    }
}
