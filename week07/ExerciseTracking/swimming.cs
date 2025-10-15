using System;

public class Swimming : Activity
{
    private int _laps;

    public Swimming(DateTime date, int minutes, int laps)
        : base(date, minutes)
    {
        _laps = laps;
    }

    protected override string GetActivityName()
    {
        return "Swimming";
    }

    public override double GetDistance()
    {
        // Pista de 50m
        // km = laps * 50 / 1000
        // millas = km * 0.62
        double km = (_laps * 50.0) / 1000.0;
        return km * 0.62;
    }

    public override double GetSpeed()
    {
        double distance = GetDistance();
        if (distance <= 0) return 0;
        return (distance / GetMinutes()) * 60.0;
    }

    public override double GetPace()
    {
        double distance = GetDistance();
        if (distance <= 0) return 0;
        return GetMinutes() / distance;
    }
}
