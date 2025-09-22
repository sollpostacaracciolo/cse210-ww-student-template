using System;
using System.Collections.Generic;
using YoutubeVideos;

class Program
{
    static void Main()
    {
        var v1 = new Video("Landing on the Moon", "Neil Armstrong", 5400);
        v1.AddComment(new Comment("Maria", "Amazing footage!"));
        v1.AddComment(new Comment("Jhon", "Incredible history."));
        v1.AddComment(new Comment("Sofia", "Truly inspiring."));

        var v2 = new Video("The Beauty of Nature", "David Attenborough", 3600);
        v2.AddComment(new Comment("Liam", "Breathtaking visuals!"));
        v2.AddComment(new Comment("Emma", "Nature at its finest"));
        v2.AddComment(new Comment("Olivia", "So peaceful and calming"));

        var v3 = new Video("The future of AI", "Elon Musk", 2700);
        v3.AddComment(new Comment("Ava", "Fascinating insights!"));
        v3.AddComment(new Comment("Noah", "The future is here."));
        v3.AddComment(new Comment("Isabella", "Can+'t wait to see what comes next."));
        var videos = new List<Video> { v1, v2, v3 };

        foreach (var v in videos)
        {
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Title: {v.Title}");
            Console.WriteLine($"Time: {v.DurationAsHms()}");
            Console.WriteLine($"Author: {v.Author}");
            Console.WriteLine($"Comments({v.GetCommentCount()})");
            Console.WriteLine("Comments:");
            int i = 1;
            foreach (var comment in v.getAllComments())
            {
                Console.WriteLine($"  {i++}. {comment}");
            }

            Console.WriteLine();

        }

        Console.WriteLine(new string('=', 60));
        Console.WriteLine("End of program. Press Enter to exit.");
        Console.ReadLine();
        
    }
}