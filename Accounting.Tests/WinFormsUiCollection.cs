namespace Accounting.Tests;

/// <summary>
/// Tests that spin up DevExpress/WinForms controls on STA threads share this collection so xUnit runs them one
/// at a time. Run in parallel, the first-use DevExpress initialization competes for CPU and a cold run could
/// exceed the per-test thread timeout.
/// </summary>
[CollectionDefinition(Name)]
public sealed class WinFormsUiCollection
{
    public const string Name = "WinForms UI";
}
