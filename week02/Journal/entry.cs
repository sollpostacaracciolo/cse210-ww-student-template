public class Entry
{
    public string _date;
    public string _promptText;

    public string _entry;




    public Entry(string prompt, string entryText)
    {
        _date = DateTime.Now.ToShortDateString();
        _promptText = prompt;      // asigna al campo
        _entry = entryText;  
    }
    public void Display()
    {
        Console.WriteLine($"{_date} - {_promptText}");
        Console.WriteLine(_entry);
        Console.WriteLine();


    }

    

}