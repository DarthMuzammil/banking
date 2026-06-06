using Banking.Domain.Enums;

namespace Banking.Application.Common;

public static class TransactionCategoryInferrer
{
    public static TransactionCategory Infer(string? description, TransactionType type)
    {
        if (type == TransactionType.Credit)
        {
            return TransactionCategory.Income;
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return TransactionCategory.Other;
        }

        var lower = description.ToLowerInvariant();

        if (lower.Contains("transfer")) return TransactionCategory.Transfer;
        if (lower.Contains("bill") || lower.Contains("rent") || lower.Contains("electric") || lower.Contains("insurance"))
            return TransactionCategory.Bills;
        if (lower.Contains("shop")) return TransactionCategory.Shopping;
        if (lower.Contains("food")) return TransactionCategory.Food;
        if (lower.Contains("transport") || lower.Contains("uber") || lower.Contains("gas"))
            return TransactionCategory.Transport;
        if (lower.Contains("deposit") || lower.Contains("paycheck") || lower.Contains("income"))
            return TransactionCategory.Income;

        return TransactionCategory.Other;
    }
}
