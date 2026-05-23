using System;
using Accounting.FixedAssets.Domain.Entities;

namespace Accounting.FixedAssets.Domain;

public static class DepreciationCalculator
{
    public static decimal CalculateMonthlyDepreciationAmount(FixedAsset asset)
    {
        if (asset.DepreciationMethod == DepreciationMethod.NoDepreciation)
        {
            return 0m;
        }

        if (asset.UsefulLifeMonths <= 0)
        {
            throw new InvalidOperationException($"Useful life bulan tidak valid untuk asset {asset.AssetCode}.");
        }

        decimal openingNbv = asset.OpeningNetBookValue;
        if (openingNbv <= asset.ResidualValue)
        {
            return 0m;
        }

        decimal rawAmount = asset.DepreciationMethod switch
        {
            DepreciationMethod.StraightLine => (asset.CostBasis - asset.ResidualValue) / asset.UsefulLifeMonths,
            DepreciationMethod.DecliningBalance => openingNbv * (2m / asset.UsefulLifeMonths),
            _ => 0m
        };

        decimal maxAllowed = openingNbv - asset.ResidualValue;
        decimal bounded = Math.Min(rawAmount, maxAllowed);
        return Math.Round(Math.Max(0m, bounded), 2, MidpointRounding.AwayFromZero);
    }
}
