using System;
using System.Collections.Generic;
using System.Linq;

public class Scripture
{
    private Reference _reference;
    private List<Word> _words;
    private Random _rng = new Random();

    public Scripture(Reference reference, string text)
    {
        _reference = reference;
        _words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                     .Select(w => new Word(w))
                     .ToList();
    }

    public string GetDisplayText()
    {
        string refText = _reference.GetDisplayText();
        string body = string.Join(" ", _words.Select(w => w.GetDisplayText()));
        return $"{refText}\n{body}";
    }

    public void HideRandomWords(int count)
    {
        var visibles = _words.Where(w => !w.IsHidden()).ToList();
        int toHide = Math.Min(count, visibles.Count);

        for (int i = 0; i < toHide; i++)
        {
            int idx = _rng.Next(visibles.Count);
            visibles[idx].Hide();
            visibles.RemoveAt(idx);
        }
    }

        public bool AllWordsHidden() => _words.All(w => w.IsHidden());
    }

    