using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

// Project W06: "Eternal Quest"
// --------------------------------------------------------------
// This program implements a goal system with gamification.
// Requirements covered:
//  - Simple goals (completed once and grant points)
//  - Eternal goals (never end, grant points each time)
//  - Checklist goals (n times + bonus on completion)
//  - Show score
//  - Create new goals of any type
//  - Record events to grant points
//  - List goals with state (completed, counts, etc.)
//  - Save / Load goals and score from file
//  - Inheritance, polymorphism, encapsulation
//
// Creativity (beyond requirements):
//  1) Level System: the user levels up every 1000 points.
//     Shows progress toward the next level.
//  2) Badges: awarded for completing simple and checklist goals.
//  3) Negative Goal (optional): logs bad habits and subtracts points.
//  4) Progress Goal (optional): add progress units toward a big target;
//     grants points per unit and a bonus upon reaching the total.
//  5) Persistence with a simple, human-readable file format.
//
// Reviewer note: creativity description is here in Program.cs
// --------------------------------------------------------------

namespace EternalQuest
{
    abstract class Goal
    {
        private string _name;
        private string _description;
        private int _basePoints;

        protected Goal(string name, string description, int basePoints)
        {
            _name = name;
            _description = description;
            _basePoints = basePoints;
        }

        public string Name => _name;
        public string Description => _description;
        public int BasePoints => _basePoints;

        public abstract string Type { get; }
        public abstract bool IsComplete { get; }
        public abstract int RecordEvent();
        public abstract string GetStatus();

        // For save/load (line-based format)
        public abstract string Serialize();

        public virtual void PostLoadFixup() { }
    }

    class SimpleGoal : Goal
    {
        private bool _done = false;
        public SimpleGoal(string name, string description, int basePoints, bool done = false)
            : base(name, description, basePoints)
        {
            _done = done;
        }

        public override string Type => "Simple";
        public override bool IsComplete => _done;

        public override int RecordEvent()
        {
            if (_done) return 0; // already completed
            _done = true;
            return BasePoints;
        }

        public override string GetStatus()
        {
            return $"[{(_done ? 'X' : ' ')}] {Name} — {Description} ({BasePoints} pts)";
        }

        public override string Serialize()
        {
            // Simple|Name|Description|BasePoints|Done
            return string.Join("|", new[]
            {
                Type,
                Escape(Name),
                Escape(Description),
                BasePoints.ToString(CultureInfo.InvariantCulture),
                _done ? "1" : "0"
            });
        }

        public static SimpleGoal Deserialize(string[] parts)
        {
            // parts: 0=Simple 1=Name 2=Desc 3=Base 4=Done
            var name = Unescape(parts[1]);
            var desc = Unescape(parts[2]);
            var basePts = int.Parse(parts[3], CultureInfo.InvariantCulture);
            var done = parts.Length > 4 && parts[4] == "1";
            return new SimpleGoal(name, desc, basePts, done);
        }

        protected static string Escape(string s) => s.Replace("|", "\\|");
        protected static string Unescape(string s) => s.Replace("\\|", "|");
    }

    class EternalGoal : Goal
    {
        private int _timesLogged = 0;
        public EternalGoal(string name, string description, int basePoints, int timesLogged = 0)
            : base(name, description, basePoints)
        {
            _timesLogged = timesLogged;
        }

        public override string Type => "Eternal";
        public override bool IsComplete => false;

        public override int RecordEvent()
        {
            _timesLogged++;
            return BasePoints;
        }

        public override string GetStatus()
        {
            return $"[∞] {Name} — {Description} (+{BasePoints} per log, total {_timesLogged})";
        }

        public override string Serialize()
        {
            // Eternal|Name|Description|BasePoints|Times
            return string.Join("|", new[]
            {
                Type,
                Escape(Name),
                Escape(Description),
                BasePoints.ToString(CultureInfo.InvariantCulture),
                _timesLogged.ToString(CultureInfo.InvariantCulture)
            });
        }

        public static EternalGoal Deserialize(string[] parts)
        {
            var name = Unescape(parts[1]);
            var desc = Unescape(parts[2]);
            var basePts = int.Parse(parts[3], CultureInfo.InvariantCulture);
            var times = parts.Length > 4 ? int.Parse(parts[4], CultureInfo.InvariantCulture) : 0;
            return new EternalGoal(name, desc, basePts, times);
        }

        protected static string Escape(string s) => s.Replace("|", "\\|");
        protected static string Unescape(string s) => s.Replace("\\|", "|");
    }

    class ChecklistGoal : Goal
    {
        private int _targetCount;
        private int _currentCount;
        private int _bonus;
        private bool _completed;

        public ChecklistGoal(string name, string description, int basePoints, int targetCount, int bonus, int currentCount = 0, bool completed = false)
            : base(name, description, basePoints)
        {
            _targetCount = Math.Max(1, targetCount);
            _bonus = Math.Max(0, bonus);
            _currentCount = currentCount;
            _completed = completed;
        }

        public override string Type => "Checklist";
        public override bool IsComplete => _completed;

        public override int RecordEvent()
        {
            if (_completed) return 0;
            _currentCount++;
            int gained = BasePoints;
            if (_currentCount >= _targetCount)
            {
                _completed = true;
                gained += _bonus;
            }
            return gained;
        }

        public override string GetStatus()
        {
            string box = _completed ? "[X]" : "[ ]";
            return $"{box} {Name} — {Description} ({(_currentCount)}/{_targetCount}, +{BasePoints} each, bonus {_bonus} on completion)";
        }

        public override string Serialize()
        {
            // Checklist|Name|Description|BasePoints|Target|Current|Bonus|Completed
            return string.Join("|", new[]
            {
                Type,
                Escape(Name),
                Escape(Description),
                BasePoints.ToString(CultureInfo.InvariantCulture),
                _targetCount.ToString(CultureInfo.InvariantCulture),
                _currentCount.ToString(CultureInfo.InvariantCulture),
                _bonus.ToString(CultureInfo.InvariantCulture),
                _completed ? "1" : "0"
            });
        }

        public static ChecklistGoal Deserialize(string[] parts)
        {
            var name = Unescape(parts[1]);
            var desc = Unescape(parts[2]);
            var basePts = int.Parse(parts[3], CultureInfo.InvariantCulture);
            var target = int.Parse(parts[4], CultureInfo.InvariantCulture);
            var current = int.Parse(parts[5], CultureInfo.InvariantCulture);
            var bonus = int.Parse(parts[6], CultureInfo.InvariantCulture);
            var completed = parts.Length > 7 && parts[7] == "1";
            return new ChecklistGoal(name, desc, basePts, target, bonus, current, completed);
        }

        protected static string Escape(string s) => s.Replace("|", "\\|");
        protected static string Unescape(string s) => s.Replace("\\|", "|");
    }

    // Creativity: Negative Goal (subtracts points when logged). Never completes.
    class NegativeGoal : Goal
    {
        private int _times;
        public NegativeGoal(string name, string description, int penaltyPoints, int times = 0)
            : base(name, description, -Math.Abs(penaltyPoints))
        {
            _times = times;
        }

        public override string Type => "Negative";
        public override bool IsComplete => false;

        public override int RecordEvent()
        {
            _times++;
            return BasePoints; // it's negative
        }

        public override string GetStatus()
        {
            return $"[!] {Name} — {Description} ({_times} times, {BasePoints} per event)";
        }

        public override string Serialize()
        {
            // Negative|Name|Description|Penalty|Times
            return string.Join("|", new[]
            {
                Type,
                Escape(Name),
                Escape(Description),
                BasePoints.ToString(CultureInfo.InvariantCulture),
                _times.ToString(CultureInfo.InvariantCulture)
            });
        }

        public static NegativeGoal Deserialize(string[] parts)
        {
            var name = Unescape(parts[1]);
            var desc = Unescape(parts[2]);
            var penalty = int.Parse(parts[3], CultureInfo.InvariantCulture);
            var times = parts.Length > 4 ? int.Parse(parts[4], CultureInfo.InvariantCulture) : 0;
            return new NegativeGoal(name, desc, Math.Abs(penalty), times);
        }

        protected static string Escape(string s) => s.Replace("|", "\\|");
        protected static string Unescape(string s) => s.Replace("\\|", "|");
    }

    // Creativity: Progress Goal by units.
    class ProgressGoal : Goal
    {
        private int _unitsTarget;
        private int _unitsDone;
        private int _perUnitPoints;
        private int _bonusOnFinish;
        private bool _completed;

        public ProgressGoal(string name, string description, int perUnitPoints, int unitsTarget, int bonus, int unitsDone = 0, bool completed = false)
            : base(name, description, perUnitPoints)
        {
            _perUnitPoints = Math.Max(0, perUnitPoints);
            _unitsTarget = Math.Max(1, unitsTarget);
            _bonusOnFinish = Math.Max(0, bonus);
            _unitsDone = Math.Max(0, unitsDone);
            _completed = completed;
        }

        public override string Type => "Progress";
        public override bool IsComplete => _completed;

        public override int RecordEvent()
        {
            Console.Write("How many units did you advance? ");
            if (!int.TryParse(Console.ReadLine(), out int units) || units <= 0)
            {
                Console.WriteLine("Invalid input. No progress recorded.");
                return 0;
            }
            int before = _unitsDone;
            _unitsDone = Math.Min(_unitsTarget, _unitsDone + units);
            int delta = _unitsDone - before;
            int gained = delta * _perUnitPoints;
            if (!_completed && _unitsDone >= _unitsTarget)
            {
                _completed = true;
                gained += _bonusOnFinish;
            }
            return gained;
        }

        public override string GetStatus()
        {
            string box = _completed ? "[X]" : "[ ]";
            return $"{box} {Name} — {Description} ({_unitsDone}/{_unitsTarget} units, +{_perUnitPoints}/unit, bonus {_bonusOnFinish})";
        }

        public override string Serialize()
        {
            // Progress|Name|Description|PerUnit|Target|Done|Bonus|Completed
            return string.Join("|", new[]
            {
                Type,
                Escape(Name),
                Escape(Description),
                _perUnitPoints.ToString(CultureInfo.InvariantCulture),
                _unitsTarget.ToString(CultureInfo.InvariantCulture),
                _unitsDone.ToString(CultureInfo.InvariantCulture),
                _bonusOnFinish.ToString(CultureInfo.InvariantCulture),
                _completed ? "1" : "0"
            });
        }

        public static ProgressGoal Deserialize(string[] parts)
        {
            var name = Unescape(parts[1]);
            var desc = Unescape(parts[2]);
            var perUnit = int.Parse(parts[3], CultureInfo.InvariantCulture);
            var target = int.Parse(parts[4], CultureInfo.InvariantCulture);
            var done = int.Parse(parts[5], CultureInfo.InvariantCulture);
            var bonus = int.Parse(parts[6], CultureInfo.InvariantCulture);
            var completed = parts.Length > 7 && parts[7] == "1";
            return new ProgressGoal(name, desc, perUnit, target, bonus, done, completed);
        }

        protected static string Escape(string s) => s.Replace("|", "\\|");
        protected static string Unescape(string s) => s.Replace("\\|", "|");
    }

    class PlayerProfile
    {
        private int _score;
        private readonly List<string> _badges = new();

        public int Score => _score;
        public IReadOnlyList<string> Badges => _badges.AsReadOnly();

        public void AddPoints(int pts)
        {
            _score += pts;
            if (_score < 0) _score = 0; // no negative global score
        }

        public int Level => 1 + (_score / 1000);
        public int PointsIntoLevel => _score % 1000;
        public int PointsToNextLevel => 1000 - PointsIntoLevel;

        public void AwardBadge(string badge)
        {
            if (!_badges.Contains(badge))
                _badges.Add(badge);
        }

        public string Serialize()
        {
            // SCORE:<int>
            // BADGES:<comma-separated>
            var badges = string.Join(",", _badges);
            return $"SCORE:{_score}\nBADGES:{badges}";
        }

        public static PlayerProfile Deserialize(TextReader reader)
        {
            var profile = new PlayerProfile();
            string? s = reader.ReadLine();
            if (s != null && s.StartsWith("SCORE:"))
            {
                profile._score = int.Parse(s.Substring(6));
            }
            string? b = reader.ReadLine();
            if (b != null && b.StartsWith("BADGES:"))
            {
                string csv = b.Substring(7);
                if (!string.IsNullOrWhiteSpace(csv))
                {
                    foreach (var badge in csv.Split(",", StringSplitOptions.RemoveEmptyEntries))
                        profile.AwardBadge(badge.Trim());
                }
            }
            return profile;
        }
    }

    class GoalManager
    {
        private readonly List<Goal> _goals = new();
        private readonly PlayerProfile _profile = new();

        public void Run()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("=== ETERNAL QUEST ===");
                Console.WriteLine($"Level: {_profile.Level}  Points: {_profile.Score}  (missing {_profile.PointsToNextLevel} for the next level)");
                if (_profile.Badges.Count > 0)
                    Console.WriteLine("Badges: " + string.Join(" | ", _profile.Badges));
                Console.WriteLine("------------------------------");
                Console.WriteLine("1) Create a new goal");
                Console.WriteLine("2) List goals");
                Console.WriteLine("3) Record event");
                Console.WriteLine("4) Show score");
                Console.WriteLine("5) Save to file");
                Console.WriteLine("6) Load from file");
                Console.WriteLine("7) Exit");
                Console.Write("Choose an option: ");

                string? choice = Console.ReadLine();
                Console.WriteLine();
                switch (choice)
                {
                    case "1": CreateGoal(); break;
                    case "2": ListGoals(); break;
                    case "3": RecordEvent(); break;
                    case "4": ShowScore(); break;
                    case "5": Save(); break;
                    case "6": Load(); break;
                    case "7": return;
                    default: Console.WriteLine("Invalid option."); break;
                }
            }
        }

        private void CreateGoal()
        {
            Console.WriteLine("Types: 1) Simple  2) Eternal  3) Checklist  4) Negative  5) Progress");
            Console.Write("Choose a type: ");
            string? t = Console.ReadLine();

            Console.Write("Name: ");
            string name = Console.ReadLine() ?? "(no name)";
            Console.Write("Description: ");
            string desc = Console.ReadLine() ?? "";

            if (t == "1")
            {
                int pts = PromptInt("Points on completion: ", 1);
                _goals.Add(new SimpleGoal(name, desc, pts));
                Console.WriteLine("Simple goal created.");
            }
            else if (t == "2")
            {
                int pts = PromptInt("Points per log: ", 1);
                _goals.Add(new EternalGoal(name, desc, pts));
                Console.WriteLine("Eternal goal created.");
            }
            else if (t == "3")
            {
                int pts = PromptInt("Points per log: ", 1);
                int target = PromptInt("How many times to complete?: ", 1);
                int bonus = PromptInt("Completion bonus: ", 0);
                _goals.Add(new ChecklistGoal(name, desc, pts, target, bonus));
                Console.WriteLine("Checklist goal created.");
            }
            else if (t == "4")
            {
                int penalty = PromptInt("Points to subtract per event (positive number): ", 1);
                _goals.Add(new NegativeGoal(name, desc, penalty));
                Console.WriteLine("Negative goal created.");
            }
            else if (t == "5")
            {
                int perUnit = PromptInt("Points per progress unit: ", 1);
                int target = PromptInt("Total target units: ", 1);
                int bonus = PromptInt("Bonus on completion: ", 0);
                _goals.Add(new ProgressGoal(name, desc, perUnit, target, bonus));
                Console.WriteLine("Progress goal created.");
            }
            else
            {
                Console.WriteLine("Invalid type. Goal not created.");
            }
        }

        private void ListGoals()
        {
            if (_goals.Count == 0)
            {
                Console.WriteLine("There are no goals yet. Create one with option 1.");
                return;
            }
            Console.WriteLine("--- Goals ---");
            for (int i = 0; i < _goals.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_goals[i].GetStatus()}  (type: {_goals[i].Type})");
            }
        }

        private void RecordEvent()
        {
            if (_goals.Count == 0)
            {
                Console.WriteLine("There are no goals to record.");
                return;
            }
            ListGoals();
            Console.Write("Select the goal number to record: ");
            if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > _goals.Count)
            {
                Console.WriteLine("Invalid selection.");
                return;
            }
            var goal = _goals[idx - 1];
            int pts = goal.RecordEvent();
            _profile.AddPoints(pts);

            // Creative: award badges
            if (goal is SimpleGoal sg && sg.IsComplete)
            {
                _profile.AwardBadge($"Completed: {goal.Name}");
            }
            if (goal is ChecklistGoal cg && cg.IsComplete)
            {
                _profile.AwardBadge($"Checklist completed: {goal.Name}");
            }
            if (pts != 0)
                Console.WriteLine($"Points gained: {pts}. Total score: {_profile.Score}");
            else
                Console.WriteLine("No change in points.");
        }

        private void ShowScore()
        {
            Console.WriteLine($"Score: {_profile.Score}");
            Console.WriteLine($"Level: {_profile.Level} — Level progress: {_profile.PointsIntoLevel}/1000 (missing {_profile.PointsToNextLevel})");
            if (_profile.Badges.Count > 0)
            {
                Console.WriteLine("Badges: " + string.Join(" | ", _profile.Badges));
            }
        }

        private void Save()
        {
            Console.Write("File name (e.g., goals.txt): ");
            string file = Console.ReadLine() ?? "goals.txt";
            using var writer = new StreamWriter(file);

            // Profile first
            writer.WriteLine("#PROFILE");
            writer.WriteLine(_profile.Serialize());

            writer.WriteLine("#GOALS");
            // One serialized line per goal
            foreach (var g in _goals)
            {
                writer.WriteLine(g.Serialize());
            }
            Console.WriteLine($"Saved to '{file}'.");
        }

        private void Load()
        {
            Console.Write("File to load: ");
            string file = Console.ReadLine() ?? "goals.txt";
            if (!File.Exists(file))
            {
                Console.WriteLine("File does not exist.");
                return;
            }
            using var reader = new StreamReader(file);
            string? line;

            // clear
            _goals.Clear();
            // profile
            line = reader.ReadLine();
            if (line == null || line != "#PROFILE")
            {
                Console.WriteLine("Invalid format (missing #PROFILE).");
                return;
            }
            // SCORE and BADGES
            var profile = PlayerProfile.Deserialize(reader);
            // copy into our profile
            CopyProfile(profile);

            // goals header
            line = reader.ReadLine();
            if (line == null || line != "#GOALS")
            {
                Console.WriteLine("Invalid format (missing #GOALS).");
                return;
            }
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = SplitKeepingEscapes(line);
                if (parts.Length == 0) continue;
                Goal? g = parts[0] switch
                {
                    "Simple"   => SimpleGoal.Deserialize(parts),
                    "Eternal"  => EternalGoal.Deserialize(parts),
                    "Checklist"=> ChecklistGoal.Deserialize(parts),
                    "Negative" => NegativeGoal.Deserialize(parts),
                    "Progress" => ProgressGoal.Deserialize(parts),
                    _ => null
                };
                if (g != null) _goals.Add(g);
            }
            Console.WriteLine($"Loaded {_goals.Count} goals from '{file}'.");
        }

        // Copy values from another profile to ours (without exposing public setters)
        private void CopyProfile(PlayerProfile other)
        {
            // Simple approach: re-add points and badges
            int current = _profile.Score;
            int delta = Math.Max(0, other.Score - current);
            if (delta > 0) _profile.AddPoints(delta);
            foreach (var b in other.Badges)
                _profile.AwardBadge(b);
        }

        private static int PromptInt(string prompt, int min)
        {
            Console.Write(prompt);
            if (!int.TryParse(Console.ReadLine(), out int v) || v < min)
            {
                Console.WriteLine($"Using default value: {min}");
                return min;
            }
            return v;
        }

        // Split keeping escaped '|' sequences
        private static string[] SplitKeepingEscapes(string s)
        {
            var list = new List<string>();
            var cur = new System.Text.StringBuilder();
            bool escape = false;
            foreach (char c in s)
            {
                if (escape)
                {
                    cur.Append(c);
                    escape = false;
                }
                else if (c == '\\')
                {
                    escape = true;
                }
                else if (c == '|')
                {
                    list.Add(cur.ToString());
                    cur.Clear();
                }
                else
                {
                    cur.Append(c);
                }
            }
            list.Add(cur.ToString());
            return list.ToArray();
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var manager = new GoalManager();
            manager.Run();
        }
    }
}
