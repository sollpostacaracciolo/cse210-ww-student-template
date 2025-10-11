using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

// Proyecto W06: Programa de "Búsqueda Eterna"
// --------------------------------------------------------------
// Este programa implementa un sistema de metas con gamificación.
// Requisitos cubiertos:
//  - Metas simples (se completan una sola vez y otorgan puntos)
//  - Metas eternas (nunca terminan, otorgan puntos cada vez)
//  - Metas tipo checklist (n veces + bonificación al completar)
//  - Mostrar puntuación
//  - Crear nuevas metas de cualquier tipo
//  - Registrar eventos para otorgar puntos
//  - Listar metas con estado (completado, conteos, etc.)
//  - Guardar / Cargar metas y puntuación desde archivo
//  - Herencia, polimorfismo, encapsulación
//
// Creatividad (superando requisitos) — ver más abajo:
//  1) Sistema de Niveles: El usuario sube de nivel cada 1000 puntos.
//     Muestra progreso hacia el siguiente nivel.
//  2) Insignias: Se otorgan insignias por completar metas simples y checklists.
//  3) Meta Negativa (opcional): Registra malos hábitos y resta puntos.
//  4) Meta de Progreso (opcional): Permite sumar unidades de avance a un objetivo grande; otorga puntos por unidad y bonificación al alcanzar el total.
//  5) Persistencia con formato de archivo simple y legible por humanos.
//
// Nota para el revisor: La descripción de creatividad está aquí en Program.cs
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

        // Para guardar/cargar (formato por líneas)
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
            if (_done) return 0; // ya completado
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
            return $"[∞] {Name} — {Description} (+{BasePoints} por registro, total { _timesLogged })";
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
            return $"{box} {Name} — {Description} ({_currentCount}/{_targetCount}, +{BasePoints} c/u, bonus { _bonus } al completar)";
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

    // Creatividad: Meta Negativa (resta puntos al registrarse). Nunca se completa.
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
            return BasePoints; // es negativo
        }
        public override string GetStatus()
        {
            return $"[!] {Name} — {Description} ({_times} veces, {BasePoints} por evento)";
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

    // Creatividad: Meta de Progreso por unidades.
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
            Console.Write("¿Cuántas unidades avanzaste? ");
            if (!int.TryParse(Console.ReadLine(), out int units) || units <= 0)
            {
                Console.WriteLine("Entrada inválida. No se registró avance.");
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
            return $"{box} {Name} — {Description} ({_unitsDone}/{_unitsTarget} unidades, +{_perUnitPoints}/u, bonus {_bonusOnFinish})";
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
            if (_score < 0) _score = 0; // sin negativos globales
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
                Console.WriteLine("=== BÚSQUEDA ETERNA ===");
                Console.WriteLine($"Nivel: {_profile.Level}  Puntos: {_profile.Score}  (faltan {_profile.PointsToNextLevel} para el próximo nivel)");
                if (_profile.Badges.Count > 0)
                    Console.WriteLine("Insignias: " + string.Join(" | ", _profile.Badges));
                Console.WriteLine("------------------------------");
                Console.WriteLine("1) Crear nueva meta");
                Console.WriteLine("2) Listar metas");
                Console.WriteLine("3) Registrar evento (log)");
                Console.WriteLine("4) Mostrar puntuación");
                Console.WriteLine("5) Guardar a archivo");
                Console.WriteLine("6) Cargar desde archivo");
                Console.WriteLine("7) Salir");
                Console.Write("Seleccione una opción: ");

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
                    default: Console.WriteLine("Opción inválida."); break;
                }
            }
        }

        private void CreateGoal()
        {
            Console.WriteLine("Tipos: 1) Simple  2) Eterna  3) Checklist  4) Negativa  5) Progreso");
            Console.Write("Seleccione tipo: ");
            string? t = Console.ReadLine();

            Console.Write("Nombre: ");
            string name = Console.ReadLine() ?? "(sin nombre)";
            Console.Write("Descripción: ");
            string desc = Console.ReadLine() ?? "";

            if (t == "1")
            {
                int pts = PromptInt("Puntos al completar: ", 1);
                _goals.Add(new SimpleGoal(name, desc, pts));
                Console.WriteLine("Meta simple creada.");
            }
            else if (t == "2")
            {
                int pts = PromptInt("Puntos por registro: ", 1);
                _goals.Add(new EternalGoal(name, desc, pts));
                Console.WriteLine("Meta eterna creada.");
            }
            else if (t == "3")
            {
                int pts = PromptInt("Puntos por registro: ", 1);
                int target = PromptInt("¿Cuántas veces para completar?: ", 1);
                int bonus = PromptInt("Bonificación al completar: ", 0);
                _goals.Add(new ChecklistGoal(name, desc, pts, target, bonus));
                Console.WriteLine("Meta checklist creada.");
            }
            else if (t == "4")
            {
                int penalty = PromptInt("Puntos a restar por evento (número positivo): ", 1);
                _goals.Add(new NegativeGoal(name, desc, penalty));
                Console.WriteLine("Meta negativa creada.");
            }
            else if (t == "5")
            {
                int perUnit = PromptInt("Puntos por unidad de progreso: ", 1);
                int target = PromptInt("Unidades totales objetivo: ", 1);
                int bonus = PromptInt("Bonificación al completar: ", 0);
                _goals.Add(new ProgressGoal(name, desc, perUnit, target, bonus));
                Console.WriteLine("Meta de progreso creada.");
            }
            else
            {
                Console.WriteLine("Tipo inválido. No se creó la meta.");
            }
        }

        private void ListGoals()
        {
            if (_goals.Count == 0)
            {
                Console.WriteLine("No hay metas aún. Cree una con la opción 1.");
                return;
            }
            Console.WriteLine("--- Metas ---");
            for (int i = 0; i < _goals.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_goals[i].GetStatus()}  (tipo: {_goals[i].Type})");
            }
        }

        private void RecordEvent()
        {
            if (_goals.Count == 0)
            {
                Console.WriteLine("No hay metas para registrar.");
                return;
            }
            ListGoals();
            Console.Write("Seleccione el número de la meta a registrar: ");
            if (!int.TryParse(Console.ReadLine(), out int idx) || idx < 1 || idx > _goals.Count)
            {
                Console.WriteLine("Selección inválida.");
                return;
            }
            var goal = _goals[idx - 1];
            int pts = goal.RecordEvent();
            _profile.AddPoints(pts);

            // Insignias por creatividad
            if (goal is SimpleGoal sg && sg.IsComplete)
            {
                _profile.AwardBadge($"¡Completaste: {goal.Name}!");
            }
            if (goal is ChecklistGoal cg && cg.IsComplete)
            {
                _profile.AwardBadge($"¡Checklist logrado: {goal.Name}!");
            }
            if (pts != 0)
                Console.WriteLine($"Puntos ganados: {pts}. Puntuación total: {_profile.Score}");
            else
                Console.WriteLine("Sin cambio de puntos.");
        }

        private void ShowScore()
        {
            Console.WriteLine($"Puntuación: {_profile.Score}");
            Console.WriteLine($"Nivel: {_profile.Level} — Progreso en nivel: {_profile.PointsIntoLevel}/1000 (faltan {_profile.PointsToNextLevel})");
            if (_profile.Badges.Count > 0)
            {
                Console.WriteLine("Insignias: " + string.Join(" | ", _profile.Badges));
            }
        }

        private void Save()
        {
            Console.Write("Nombre de archivo (ej. metas.txt): ");
            string file = Console.ReadLine() ?? "metas.txt";
            using var writer = new StreamWriter(file);

            // Primero, perfil
            writer.WriteLine("#PROFILE");
            writer.WriteLine(_profile.Serialize());

            writer.WriteLine("#GOALS");
            // Cada meta una línea serializada
            foreach (var g in _goals)
            {
                writer.WriteLine(g.Serialize());
            }
            Console.WriteLine($"Guardado en '{file}'.");
        }

        private void Load()
        {
            Console.Write("Archivo a cargar: ");
            string file = Console.ReadLine() ?? "metas.txt";
            if (!File.Exists(file))
            {
                Console.WriteLine("No existe el archivo.");
                return;
            }
            using var reader = new StreamReader(file);
            string? line;

            // limpiar
            _goals.Clear();
            // perfil
            line = reader.ReadLine();
            if (line == null || line != "#PROFILE")
            {
                Console.WriteLine("Formato inválido (falta #PROFILE).");
                return;
            }
            // SCORE y BADGES
            var profile = PlayerProfile.Deserialize(reader);
            // Reemplazar estado interno
            // (no hay setters, así que reconstruimos GoalManager? simplificamos así:)
            // hack simple: reflejar mediante campos privados no es ideal; en cambio, reasignamos con copia
            CopyProfile(profile);

            // Leer separador de metas
            line = reader.ReadLine();
            if (line == null || line != "#GOALS")
            {
                Console.WriteLine("Formato inválido (falta #GOALS).");
                return;
            }
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = SplitKeepingEscapes(line);
                if (parts.Length == 0) continue;
                Goal? g = parts[0] switch
                {
                    "Simple" => SimpleGoal.Deserialize(parts),
                    "Eternal" => EternalGoal.Deserialize(parts),
                    "Checklist" => ChecklistGoal.Deserialize(parts),
                    "Negative" => NegativeGoal.Deserialize(parts),
                    "Progress" => ProgressGoal.Deserialize(parts),
                    _ => null
                };
                if (g != null) _goals.Add(g);
            }
            Console.WriteLine($"Cargadas {_goals.Count} metas desde '{file}'.");
        }

        // Copia valores de otro perfil al nuestro (sin exponer setters públicos)
        private void CopyProfile(PlayerProfile other)
        {
            // Truco: volver a sumar puntos e insignias
            // (es simple y respeta encapsulación)
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
                Console.WriteLine($"Usando valor por defecto: {min}");
                return min;
            }
            return v;
        }

        // Divide conservando escapes de '|'
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
