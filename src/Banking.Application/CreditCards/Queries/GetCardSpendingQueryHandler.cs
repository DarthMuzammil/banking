using Banking.Application.Abstractions;
using Banking.Application.Common;
using Banking.Application.CreditCards.DTOs;

namespace Banking.Application.CreditCards.Queries;

public sealed class GetCardSpendingQueryHandler
{
    private readonly ICreditCardRepository _creditCardRepository;

    public GetCardSpendingQueryHandler(ICreditCardRepository creditCardRepository)
    {
        _creditCardRepository = creditCardRepository;
    }

    public async Task<Result<IReadOnlyList<CreditCardSpendingDto>>> HandleAsync(
        GetCardSpendingQuery query,
        CancellationToken cancellationToken = default)
    {
        var card = await _creditCardRepository.GetByIdAsync(query.CardId, cancellationToken);
        if (card is null || card.CustomerId != query.CustomerId)
        {
            return Result<IReadOnlyList<CreditCardSpendingDto>>.Failure(
                "Credit card not found.",
                "CARD_NOT_FOUND");
        }

        var rows = await _creditCardRepository.GetSpendingByCardIdAsync(query.CardId, cancellationToken);
        var total = rows.Sum(r => r.Amount);

        var items = rows
            .Select(r => new CreditCardSpendingDto(
                r.Category,
                r.Amount,
                total > 0 ? Math.Round(r.Amount / total * 100, 0) : 0))
            .ToList();

        return Result<IReadOnlyList<CreditCardSpendingDto>>.Success(items);
    }
}
