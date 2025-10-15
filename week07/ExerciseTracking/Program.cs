using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        // Crear una de cada tipo
        List<Activity> activities = new List<Activity>();
        activities.Add(new Running(new DateTime(2022, 11, 3), 30, 3.0));   // 3.0 mi
        activities.Add(new Cycling(new DateTime(2022, 11, 3), 30, 15.0)); // 15 mph
        activities.Add(new Swimming(new DateTime(2022, 11, 3), 40, 64));  // 64 largos

        // Mostrar resúmenes (polimorfismo)
        foreach (Activity a in activities)
        {
            Console.WriteLine(a.GetSummary());
        }
    }
}
