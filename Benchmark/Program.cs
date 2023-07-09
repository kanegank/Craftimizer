using Craftimizer.Simulator;
using Craftimizer.Solver.Crafty;
using System.Diagnostics;

namespace Craftimizer.Benchmark;

internal static class Program
{
    private static void Main()
    {
        //var summary = BenchmarkRunner.Run<Bench>();
        //return;

        //TypeLayout.PrintLayout<ArenaNode<SimulationNode>>(true);
        //return;

        var input = new SimulationInput(
            new CharacterStats
            {
                Craftsmanship = 4078,
                Control = 3897,
                CP = 704,
                Level = 90,
                CanUseManipulation = true,
                HasSplendorousBuff = false,
                IsSpecialist = false,
                CLvl = 560,
            },
            new RecipeInfo()
            {
                IsExpert = false,
                ClassJobLevel = 90,
                RLvl = 640,
                ConditionsFlag = 15,
                MaxDurability = 70,
                MaxQuality = 14040,
                MaxProgress = 6600,
                QualityModifier = 70,
                QualityDivider = 115,
                ProgressModifier = 80,
                ProgressDivider = 130,
            }
        );

        var config = new SolverConfig()
        {
            Iterations = 100_000,
            ForkCount = 8,
        };

        for (var i = 0; i < 7; ++i)
        {
            Console.WriteLine($"{i + 1}");
            var c = config with { FurcatedActionCount = i + 1 };
            Benchmark(() => Solver.Crafty.Solver.SearchStepwiseFurcated(c, input).State);
        }
    }

    private static void Benchmark(Func<SimulationState> search)
    {
        var s = Stopwatch.StartNew();
        List<int> q = new();
        for (var i = 0; i < 60; ++i)
        {
            var state = search();
            //Console.WriteLine($"Qual: {state.Quality}/{state.Input.Recipe.MaxQuality}");

            q.Add(state.Quality);
        }

        s.Stop();
        Console.WriteLine($"{s.Elapsed.TotalMilliseconds/60:0.00}ms/cycle");
        Console.WriteLine(string.Join(',', q));
        q.Sort();
        Console.WriteLine($"Min: {Quartile(q, 0)}, Max: {Quartile(q, 4)}, Avg: {Quartile(q, 2)}, Q1: {Quartile(q, 1)}, Q3: {Quartile(q, 3)}");
    }

    // https://stackoverflow.com/a/31536435
    private static float Quartile(List<int> input, int quartile)
    {
        float dblPercentage = quartile switch
        {
            0 => 0,     // Smallest value in the data set
            1 => 25,    // First quartile (25th percentile)
            2 => 50,    // Second quartile (50th percentile)
            3 => 75,    // Third quartile (75th percentile)
            4 => 100,   // Largest value in the data set
            _ => 0,
        };
        if (dblPercentage >= 100) return input[^1];

        var position = (input.Count + 1) * dblPercentage / 100f;
        var n = (dblPercentage / 100f * (input.Count - 1)) + 1;

        float leftNumber, rightNumber;
        if (position >= 1)
        {
            leftNumber = input[(int)MathF.Floor(n) - 1];
            rightNumber = input[(int)MathF.Floor(n)];
        }
        else
        {
            leftNumber = input[0]; // first data
            rightNumber = input[1]; // first data
        }

        if (leftNumber == rightNumber)
            return leftNumber;
        else
        {
            var part = n - MathF.Floor(n);
            return leftNumber + (part * (rightNumber - leftNumber));
        }
    }
}
