using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using YoutubeVideos;

namespace YoutubeVideos
{
    public class Video
    {
        public string Title { get; }
        public string Author { get; }

        public int DurationSeconds { get; }
        private readonly List<Comment> _comments = new();

        public Video(string title, string author, int durationSeconds)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Author = author ?? throw new ArgumentNullException(nameof(author));
            if (durationSeconds < 0) throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be non-negative.");
            DurationSeconds = durationSeconds;
        }



        public void AddComment(Comment comment)
        {
            if (comment == null) throw new ArgumentNullException(nameof(comment));
            _comments.Add(comment);

        }


        public int GetCommentCount() => _comments.Count;
        public IEnumerable<Comment> getAllComments() => _comments;

        public string DurationAsHms()
        {
            var ts = TimeSpan.FromSeconds(DurationSeconds);
            return ts.Hours > 0 ? $"{(int)ts.TotalHours}: {ts.Minutes: 00}: {ts.Seconds:00}"
            : $"{ts.Minutes} : {ts.Seconds: 00}";  
                  }
}

    }

