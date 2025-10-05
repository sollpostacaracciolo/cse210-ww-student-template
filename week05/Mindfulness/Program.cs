
using System;
using System.Collections.Generic;
using System.Threading;

public abstract class Activity
{
    // ===== Encapsulated data =====
    private string _name;
    private string _description;
    private int _durationSeconds;

    protected Random _random = new Random();

    // ===== Public read-only access =====
    public string Name { get { return _name; } }
    public string Description { get { return _description; } }
    public int DurationSeconds { get { return _durationSeconds; } }

    protected Activity(string name, string description)
    {
        _name = name;
        _description = description;
    }

    // Common start message: ask duration and pause
    public void Start()
    {
        Console.Clear();
        Console.WriteLine($"*** {Name} ***\n");
        Console.WriteLine(Description + "\n");
        Console.Write("How many seconds would you like to practice? ");
        _durationSeconds = ReadPositiveInt();
        Console.WriteLine("Get ready...");
        ShowCountDown(5);
        Console.Clear();
    }

    // Common ending message
    public void End()
    {
        Console.WriteLine();
        Console.WriteLine("Well done!");
        Console.WriteLine($"You have completed {Name} for {DurationSeconds} seconds.");
        Console.Write("Returning to menu in ");
        ShowCountDown(3);
    }

    // To be implemented by derived classes
    public abstract void RunActivity();

    // ===== Protected utilities =====
    protected void ShowSpinner(int seconds)
    {
        List<string> frames = new List<string> { "|", "/", "-", "\\", "|", "/", "-", "\\" };
        DateTime endTime = DateTime.Now.AddSeconds(seconds);
        int i = 0;
        while (DateTime.Now < endTime)
        {
            string s = frames[i];
            Console.Write(s);
            Thread.Sleep(250);
            Console.Write("\b \b");
            i++;
            if (i >= frames.Count) i = 0;
        }
    }

    protected void ShowCountDown(int seconds)
    {
        for (int j = seconds; j > 0; j--)
        {
            string s = j.ToString();
            Console.Write(s);
            Thread.Sleep(1000);
            for (int k = 0; k < s.Length; k++) Console.Write("\b");
        }
        Console.WriteLine();
    }

    protected int ReadPositiveInt()
    {
        while (true)
        {
            string input = Console.ReadLine();
            int n;
            if (int.TryParse(input, out n) && n > 0) return n;
            Console.Write("Please enter a positive integer: ");
        }
    }

    // Non-repeating random pick
    protected string PickRandom(List<string> list, ref List<string> bag)
    {
        if (bag == null || bag.Count == 0)
        {
            bag = new List<string>(list);
        }
        int idx = _random.Next(bag.Count);
        string item = bag[idx];
        bag.RemoveAt(idx);
        return item;
    }
}

public class BreathingActivity : Activity
{
    public BreathingActivity() : base(
        "Breathing Activity",
        "This activity will help you relax by walking you through slow breathing. Clear your mind and focus on your breathing.")
    { }

    public override void RunActivity()
    {
        DateTime endTime = DateTime.Now.AddSeconds(DurationSeconds);
        while (DateTime.Now < endTime)
        {
            Console.Write("Breathe in... ");
            ShowCountDown(4);
            Console.Write("Breathe out... ");
            ShowCountDown(4);
            Console.WriteLine();
        }
    }
}

public class ReflectionActivity : Activity
{
    private List<string> _prompts = new List<string>
    {
        "Think of a time when you stood up for someone else.",
        "Think of a time when you did something really difficult.",
        "Think of a time when you helped someone in need.",
        "Think of a time when you did something truly selfless."
    };

    private List<string> _questions = new List<string>
    {
        "Why was this experience meaningful to you?",
        "Have you ever done anything like this before?",
        "How did you get started?",
        "How did you feel when it was complete?",
        "What made this time different than other times when you were not as successful?",
        "What is your favorite thing about this experience?",
        "What could you learn from this experience that applies to other situations?",
        "What did you learn about yourself?",
        "How can you keep this experience in mind in the future?"
    };

    private List<string> _promptBag;
    private List<string> _questionBag;

    public ReflectionActivity() : base(
        "Reflection Activity",
        "This activity will help you reflect on times when you have shown strength and resilience.")
    { }

    public override void RunActivity()
    {
        Console.WriteLine("Prompt:");
        Console.WriteLine("- " + PickRandom(_prompts, ref _promptBag));
        Console.WriteLine();

        DateTime endTime = DateTime.Now.AddSeconds(DurationSeconds);
        while (DateTime.Now < endTime)
        {
            Console.Write("> " + PickRandom(_questions, ref _questionBag) + " ");
            ShowSpinner(4);
            Console.WriteLine();
        }
    }
}

public class ListingActivity : Activity
{
    private List<string> _prompts = new List<string>
    {
        "Who are people that you appreciate?",
        "What are your personal strengths?",
        "Who have you helped this week?",
        "When have you felt peace this month?",
        "Who are some of your personal heroes?"
    };

    private List<string> _promptBag;

    public ListingActivity() : base(
        "Listing Activity",
        "This activity will help you reflect on the good things in your life by having you list as many as you can in a certain area.")
    { }

    public override void RunActivity()
    {
        Console.WriteLine("Prompt:");
        Console.WriteLine("- " + PickRandom(_prompts, ref _promptBag));
        Console.WriteLine("Get ready...");
        ShowCountDown(5);

        List<string> items = new List<string>();
        DateTime endTime = DateTime.Now.AddSeconds(DurationSeconds);

        while (DateTime.Now < endTime)
        {
            Console.Write("→ ");
            string line = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                items.Add(line.Trim());
            }
        }

        Console.WriteLine();
        Console.WriteLine($"You listed {items.Count} items. Great job!");
    }
}

public class BodyScanActivity : Activity
{
    private string[] _areas = { "head", "neck", "shoulders", "arms", "hands", "chest", "abdomen", "legs", "feet" };

    public BodyScanActivity() : base(
        "Body Scan Activity",
        "Guide your attention through different parts of your body, noticing sensations without judgment.")
    { }

    public override void RunActivity()
    {
        DateTime endTime = DateTime.Now.AddSeconds(DurationSeconds);
        int i = 0;
        while (DateTime.Now < endTime)
        {
            Console.Write($"Focus on your { _areas[i] }... ");
            ShowSpinner(3);
            Console.WriteLine();
            i++;
            if (i >= _areas.Length) i = 0;
        }
    }
}

/*
CREATIVITY / BEYOND REQUIREMENTS (#12):
- Added an extra activity: BodyScanActivity.
- Prevents repeats of prompts/questions until all are used (bag system).
- Clear comments document how rubric criteria are met.
*/

class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("===== Mindfulness Program (W05) =====");
            Console.WriteLine("1) Breathing Activity");
            Console.WriteLine("2) Reflection Activity");
            Console.WriteLine("3) Listing Activity");
            Console.WriteLine("4) Body Scan Activity (extra)");
            Console.WriteLine("5) Exit");
            Console.Write("Choose an option: ");

            string option = Console.ReadLine();

            if (option == "5") break;

            Activity activity = null;
            if (option == "1") activity = new BreathingActivity();
            else if (option == "2") activity = new ReflectionActivity();
            else if (option == "3") activity = new ListingActivity();
            else if (option == "4") activity = new BodyScanActivity();
            else
            {
                Console.WriteLine("Invalid option. Press Enter to continue...");
                Console.ReadLine();
                continue;
            }

            activity.Start();
            activity.RunActivity();
            activity.End();
        }

        Console.Clear();
        Console.WriteLine("Thank you for practicing. See you soon!");
    }
}
