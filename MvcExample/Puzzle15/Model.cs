using System.Collections.Immutable;

namespace MvcExample.Puzzle15;

public record Model(
    ImmutableArray<int?> Board,
    int EmptyRow = 0,
    int EmptyCol = 0
)
{
    public const int BoardSize = 4;

    public Model MoveRight()
    {
        return EmptyCol < BoardSize - 1
            ? this with {
                Board = Swap(Board, EmptyRow, EmptyCol, EmptyRow, EmptyCol + 1),
                EmptyCol = EmptyCol + 1
            }
            : this;
    }

    public Model MoveLeft()
    {
        return EmptyCol > 0
            ? this with {
                Board = Swap(Board, EmptyRow, EmptyCol, EmptyRow, EmptyCol - 1),
                EmptyCol = EmptyCol - 1
            }
            : this;
    }

    public Model MoveUp()
    {
        return EmptyRow > 0
            ? this with {
                Board = Swap(Board, EmptyRow, EmptyCol, EmptyRow - 1, EmptyCol),
                EmptyRow = EmptyRow - 1
            }
            : this;
    }

    public Model MoveDown()
    {
        return EmptyRow < BoardSize - 1
            ? this with {
                Board = Swap(Board, EmptyRow, EmptyCol, EmptyRow + 1, EmptyCol),
                EmptyRow = EmptyRow + 1
            }
            : this;
    }

    private static ImmutableArray<int?> Swap(ImmutableArray<int?> source, int row1, int col1, int row2, int col2)
    {
        return source
            .SetItem(row1 * BoardSize + col1, source[row2 * BoardSize + col2])
            .SetItem(row2 * BoardSize + col2, source[row1 * BoardSize + col1]);
    } 

    public static Model CreateRandom()
    {
        var tiles = GetRandomTiles();
        var emptyIndex = tiles.IndexOf(null);
        var emptyRow = emptyIndex / BoardSize;
        var emptyCol = emptyIndex % BoardSize;

        return new Model(
            Board: tiles, 
            EmptyRow: emptyRow, 
            EmptyCol: emptyCol);

        static ImmutableArray<int?> GetRandomTiles() 
        {
            var random = new Random();
            return Enumerable
                .Range(0, BoardSize * BoardSize)
                .Select(GetTileValue)
                .OrderBy(_ => random.Next())
                .ToImmutableArray();
        }

        static int? GetTileValue(int index) => index switch {
            0 => (int?)null,
            > 0 => index,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
