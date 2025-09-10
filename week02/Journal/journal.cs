using System;
using System.Collections.Generic;
using System.IO;

public class Journal
{
    
    public List<Entry> _entries = new List<Entry>();

    public void AddEntry(Entry newEntry)
    {
        _entries.Add(newEntry);
    }

    public void DisplayAll()
    {
        if (_entries.Count == 0)
        {
            Console.WriteLine("There are no entries in the diary yet.");
        }
        else
        {
            foreach (Entry entry in _entries)
            {
                entry.Display();
            }
        }
    }

    public void SaveToFile(string file)
    {
        using (StreamWriter writer = new StreamWriter(file))
        {
            foreach (Entry entry in _entries)
            {
                writer.WriteLine(entry._date);
                writer.WriteLine(entry._promptText);
                writer.WriteLine(entry._entry);
                writer.WriteLine("----"); // separador
            }
        }
    }

    public void LoadFromFile(string file)
    {
        _entries.Clear();

        using (StreamReader reader = new StreamReader(file))
        {
            while (!reader.EndOfStream)
            {
                string date = reader.ReadLine();
                string prompt = reader.ReadLine();
                string entryText = reader.ReadLine();
                string separator = reader.ReadLine(); // "----"

                Entry entry = new Entry(prompt, entryText);
                entry._date = date;
                _entries.Add(entry);
            }
        }
    }
}
