using System;

public class Cycling : Activity
{
    private double _speedMph;

    public Cycling(DateTime date, int minutes, double speedMph)
        : base(date, minutes)
    {
        _speedMph = speedMph;
    }

    protected override string GetActivityName()
    {
        return "Cycling";
    }

    public override double GetDistance()
    {
        // distance = speed * hours
        double hours = GetMinutes() / 60.0;
        return _speedMph * hours;
    }

    public override double GetSpeed()
    {
        return _speedMph;
    }

    public override double GetPace()
    {
        // pace = 60 / speed
        if (_speedMph <= 0) return 0;
        return 60.0 / _speedMph;
    }
}
