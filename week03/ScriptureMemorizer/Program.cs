using System;

class Program
{
    static void Main(string[] args)
    {
        var reference = new Reference("John", "3", "16");
        string text = "For God so Loved the world that he gave his one and only Son, " +
                      "that whoever believes in him shall not perish but have eternal life.";
        var scripture = new Scripture(reference, text);

        Console.Clear();
        Console.WriteLine("Welcome to the Scripture Memorizer!");
        Console.WriteLine("Press Enter to hide words, or type 'quit' to exit.\n");

        while (true)
        {
            Console.WriteLine(scripture.GetDisplayText());
            Console.Write("\n> ");
            string input = Console.ReadLine()?.Trim().ToLower();

            if (input == "quit") break;

            if (scripture.AllWordsHidden())
            {
                Console.WriteLine("\nAll words are hidden! Great job!");
                break;
            }

            scripture.HideRandomWords(3); // hides 3 words each time
            Console.Clear();
        }
    }
}
