using System;

namespace YoutubeVideos

{
    public class Comment
    {
        public string Author { get; }
        public string Text { get; }

        public Comment(string Author, string Text)
        {
            this.Author = Author;
            this.Text = Text;
        }

        public override string ToString() => $"{Author}: {Text}";

    }
}