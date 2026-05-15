namespace CsvJoin.Models;

/// <summary>
/// Captures diagnostic metrics produced while joining source datasets.
/// </summary>
/// <param name="LeftSourceRows">The number of rows read from the left source.</param>
/// <param name="RightSourceRows">The number of rows read from the right source.</param>
/// <param name="LeftRowsAfterFilters">The number of left rows after source filters.</param>
/// <param name="RightRowsAfterFilters">The number of right rows after source filters.</param>
/// <param name="MatchedRowPairs">The number of matched left/right row pairs.</param>
/// <param name="UnmatchedLeftRows">The number of filtered left rows without a matching right row.</param>
/// <param name="UnmatchedRightRows">The number of filtered right rows without a matching left row.</param>
/// <param name="ProjectedRowsBeforeResultOptions">The number of projected rows before DISTINCT, ORDER BY, and LIMIT.</param>
/// <param name="DuplicateLeftJoinKeys">The number of duplicated join key groups in the filtered left source.</param>
/// <param name="DuplicateRightJoinKeys">The number of duplicated join key groups in the filtered right source.</param>
internal sealed record JoinDiagnostics(
    int LeftSourceRows,
    int RightSourceRows,
    int LeftRowsAfterFilters,
    int RightRowsAfterFilters,
    int MatchedRowPairs,
    int UnmatchedLeftRows,
    int UnmatchedRightRows,
    int ProjectedRowsBeforeResultOptions,
    int DuplicateLeftJoinKeys,
    int DuplicateRightJoinKeys);
