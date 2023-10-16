using MvcExample.Puzzle15;

public class Controller
{
    public Model HandleInput(Model source, ConsoleKey key) => key switch {
        ConsoleKey.RightArrow => source.MoveRight(),
        ConsoleKey.LeftArrow => source.MoveLeft(),
        ConsoleKey.UpArrow => source.MoveUp(),
        ConsoleKey.DownArrow => source.MoveDown(),
        _ => source
    };
}
