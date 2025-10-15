using System;

public class Running : Activity
{
    private double _distanceMiles;

    public Running(DateTime date, int minutes, double distanceMiles)
        : base(date, minutes)
    {
        _distanceMiles = distanceMiles;
    }

    protected override string GetActivityName()
    {
        return "Running";
    }

    public override double GetDistance()
    {
        return _distanceMiles;
    }

    public override double GetSpeed()
    {
        // mph = (distance / minutes) * 60
        return (_distanceMiles / GetMinutes()) * 60.0;
    }

    public override double GetPace()
    {
        // min/mi = minutes / distance
        if (_distanceMiles <= 0) return 0;
        return GetMinutes() / _distanceMiles;
    }
}
