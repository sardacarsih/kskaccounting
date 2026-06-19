using System;
using System.Collections.Generic;
using Accounting.CoaImport.Domain;

namespace Accounting.CoaImport.Application;

public sealed class CoaImportValidationException : Exception
{
    public CoaImportValidationException(IReadOnlyList<CoaImportValidationIssue> issues)
        : base(issues.Count == 0 ? "Format file Excel daftar perkiraan salah." : issues[0].Message)
    {
        Issues = issues;
    }

    public IReadOnlyList<CoaImportValidationIssue> Issues { get; }
}
