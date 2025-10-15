using System;

public abstract class Activity
{
    private DateTime _date;
    private int _minutes;

    protected Activity(DateTime date, int minutes)
    {
        _date = date;
        _minutes = minutes;
    }

    // Getters simples (encapsulación)
    public DateTime GetDate()
    {
        return _date;
    }

    public int GetMinutes()
    {
        return _minutes;
    }

    // Métodos de cálculo (polimórficos)
    public abstract double GetDistance(); // en millas
    public abstract double GetSpeed();    // mph
    public abstract double GetPace();     // min/mi

    // Nombre del tipo de actividad (cada derivada lo devuelve)
    protected abstract string GetActivityName();

    // Un solo resumen en la base que usa los métodos polimórficos
    public string GetSummary()
    {
        string dateStr = _date.ToString("dd MMM yyyy"); // ej: 03 Nov 2022

        double distance = GetDistance();
        double speed = GetSpeed();
        double pace = GetPace();

        return dateStr + " " + GetActivityName() + " (" + _minutes + " min) - " +
               "Distance " + distance.ToString("0.0") + " miles, " +
               "Speed " + speed.ToString("0.0") + " mph, " +
               "Pace: " + pace.ToString("0.00") + " min per mile";
    }
}
