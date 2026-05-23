namespace Accounting.FixedAssets.Domain;

public enum FixedAssetStatus
{
    Draft = 0,
    Active = 1,
    UnderConstruction = 2,
    Disposed = 3,
    Sold = 4,
    Transferred = 5,
    WrittenOff = 6,
    Retired = 7
}

public enum DepreciationMethod
{
    StraightLine = 0,
    DecliningBalance = 1,
    NoDepreciation = 2
}

public enum FixedAssetTransactionType
{
    Acquisition = 0,
    Activation = 1,
    Transfer = 2,
    Revaluation = 3,
    Improvement = 4,
    Impairment = 5,
    PartialDisposal = 6,
    FullDisposal = 7,
    Sale = 8,
    WriteOff = 9,
    Retirement = 10,
    Reclassification = 11,
    CipCapitalization = 12,
    Depreciation = 13
}

public enum WorkflowStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,
    Posted = 4,
    Reversed = 5
}
