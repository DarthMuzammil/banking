using Banking.Application.Abstractions;
using Banking.Application.CreditCards.DTOs;

namespace Banking.Application.CreditCards.Queries;

public sealed class GetCreditCardsQueryHandler
{
    private readonly ICreditCardRepository _creditCardRepository;

    public GetCreditCardsQueryHandler(ICreditCardRepository creditCardRepository)
    {
        _creditCardRepository = creditCardRepository;
    }

    public async Task<IReadOnlyList<CreditCardDto>> HandleAsync(
        GetCreditCardsQuery query,
        CancellationToken cancellationToken = default)
    {
        var cards = await _creditCardRepository.GetByCustomerIdAsync(query.CustomerId, cancellationToken);

        return cards
            .Select(c => new CreditCardDto(
                c.Id,
                c.Name,
                c.LastFour,
                c.Balance,
                c.CreditLimit,
                c.DueDate,
                c.MinPayment,
                c.Apr))
            .ToList();
    }
}
