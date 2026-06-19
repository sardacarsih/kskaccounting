using System;
using System.Collections.Generic;
using System.Linq;
using Accounting.JurnalImport.Domain;

namespace Accounting.JurnalImport.Application;

public sealed class JurnalImportValidationException : Exception
{
    public JurnalImportValidationException(IReadOnlyList<JurnalImportValidationIssue> issues)
        : base(issues.Count == 0 ? "Validasi import jurnal gagal." : issues[0].Message)
    {
        Issues = issues;
    }

    public IReadOnlyList<JurnalImportValidationIssue> Issues { get; }

    public static void ThrowIfAny(IEnumerable<JurnalImportValidationIssue> issues)
    {
        List<JurnalImportValidationIssue> issueList = issues.ToList();
        if (issueList.Count > 0)
        {
            throw new JurnalImportValidationException(issueList);
        }
    }
}
