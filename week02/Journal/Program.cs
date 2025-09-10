using System;
using System.IO; // para FileNotFoundException
using System.Collections.Generic;





class Program
{
    static void Main(string[] args)
    {
        promptGenerator promptobjeto = new promptGenerator();

        promptobjeto._prompts.Add("What is God teaching me today?");
        promptobjeto._prompts.Add("How can I show kindness like Jesus?");
        promptobjeto._prompts.Add("What blessing did I see today?");
        promptobjeto._prompts.Add("How can I love others more");
        promptobjeto._prompts.Add("What verse speaks to me this morning?");
        promptobjeto._prompts.Add("if I had one thing I could do over today, what would it be? ");

        Journal diary = new Journal();
        string userInput = "";

        while (userInput != "5")
        {
            Console.WriteLine("Welcome to our Journal Part!");
            Console.WriteLine("Please select one of the following choices:");
            Console.WriteLine("1- Write");
            Console.WriteLine("2-Display");
            Console.WriteLine("3-Load");
            Console.WriteLine("4-Save");
            Console.WriteLine("5-Exit");
            userInput = Console.ReadLine();

            if (userInput == "1")
            {
                string prompt = promptobjeto.GetRandomPrompt();
                Console.WriteLine($":{prompt}");

                Console.Write("> ");
                string text = Console.ReadLine();

                Entry newEntry = new Entry(prompt, text);
                diary.AddEntry(newEntry);

            }
            else if (userInput == "2")
            {
                diary.DisplayAll();
            }
            else if (userInput == "3")
            {
                try
                {
                    Console.WriteLine("What is the Filename?");
                    string Filename = Console.ReadLine();
                    diary.LoadFromFile(Filename);
                    Console.WriteLine("The filename was loaded successfully!");
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("The file was not found. Please check the filename and try again.");
                }
            }
            else if (userInput == "4")
            {
                Console.WriteLine("What is the filename?");
                string filename = Console.ReadLine();
                diary.SaveToFile(filename);
                Console.WriteLine("The filename was saved successfully!");
            }
            else if (userInput == "5")
            {
                break;
            }
            else
            {
                Console.WriteLine("Invalid option. Please try again.");
            }
        }
    }
}
