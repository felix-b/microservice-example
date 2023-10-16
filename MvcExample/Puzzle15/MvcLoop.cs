using MvcExample.Puzzle15;

public static class MvcLoop
{
    public static void Run(Model model, View view, Controller controller)
    {
        while (true)
        {
            view.Render(model);
            var input = Console.ReadKey();
            model = controller.HandleInput(model, input.Key);
        }
        // ok?
    }
}