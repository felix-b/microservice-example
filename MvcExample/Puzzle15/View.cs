using MvcExample.Puzzle15;

public class View
{
    public void Render(Model model)
    {
        Console.Clear();

        for (int i = 0; i < model.Board.Length ; i++)
        {
            if (i > 0 && i % Model.BoardSize == 0)
            {
                Console.WriteLine();
            }

            Console.Write((model.Board[i]?.ToString() ?? string.Empty).PadRight(5));
        }
    }
}