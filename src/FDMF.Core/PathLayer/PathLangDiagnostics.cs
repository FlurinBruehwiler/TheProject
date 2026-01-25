namespace FDMF.Core.PathLayer;

public enum PathLangDiagnosticSeverity : byte
{
    Warning,
    Error,
}

public readonly record struct PathLangDiagnostic(
    PathLangDiagnosticSeverity Severity,
    string Message,
    int Line,
    int Column,
    TextView Span
);

public sealed record PathLangParseResult(
    List<AstPredicate> Predicates,
    List<PathLangDiagnostic> Diagnostics
);
