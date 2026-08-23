namespace Plocica.Helpers;

/// <summary>
/// Picks a column count for a grid of `count` items (capped at `maxColumns`) that never
/// leaves a single orphan item alone in the last row, e.g. 5 items at max 4 columns
/// yields 3 (3+2 rows) instead of 4 (4+1 rows).
/// </summary>
public static class GridLayout
{
    public static int BestColumns(int count, int maxColumns)
    {
        if (maxColumns < 1) maxColumns = 1;
        if (count <= 0) return maxColumns;
        if (count <= maxColumns) return count;

        for (var cols = maxColumns; cols >= 1; cols--)
        {
            if (count % cols != 1) return cols;
        }

        return 1;
    }
}
