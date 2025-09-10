using System;

public class Job
{
    public string _jobTitle;
    public string _company;
    public int _startyear;

    public int _endyear;


    public void Display()
    {
        Console.WriteLine($"{_jobTitle} ({_company}) {_startyear}-{_endyear}");

    }




    }