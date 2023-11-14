using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.dotTrace;
using BenchmarkDotNet.Jobs;
using Craftimizer.Simulator;
using Craftimizer.Solver;
using System.Security.Cryptography;
using System.Text;

namespace Craftimizer.Benchmark;

[SimpleJob(RuntimeMoniker.Net70, baseline: true)]
[SimpleJob(RuntimeMoniker.Net80)]
[MinColumn, Q1Column, Q3Column, MaxColumn]
[DotTraceDiagnoser]
public class Bench
{
    public record struct SHAWrapper<T>(T Data) where T : notnull
    {
        public static implicit operator T(SHAWrapper<T> wrapper) => wrapper.Data;

        public override readonly string ToString() =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Data.ToString()!)));
    }

    private static SimulationInput[] Inputs { get; } = new SimulationInput[] {
        // https://craftingway.app/rotation/loud-namazu-jVe9Y
        // Chondrite Saw
        new(new()
        {
            Craftsmanship = 3304,
            Control = 3374,
            CP = 575,
            Level = 90,
            CanUseManipulation = true,
            HasSplendorousBuff = false,
            IsSpecialist = false,
            CLvl = 560,
        },
        new()
        {
            IsExpert = false,
            ClassJobLevel = 90,
            RLvl = 560,
            ConditionsFlag = 0b1111,
            MaxDurability = 80,
            MaxQuality = 7200,
            MaxProgress = 3500,
            QualityModifier = 80,
            QualityDivider = 115,
            ProgressModifier = 90,
            ProgressDivider = 130
        }),

        // https://craftingway.app/rotation/sandy-fafnir-doVCs
        // Classical Longsword
        new(new()
        {
            Craftsmanship = 3290,
            Control = 3541,
            CP = 649,
            Level = 90,
            CanUseManipulation = true,
            HasSplendorousBuff = false,
            IsSpecialist = false,
            CLvl = 560,
        },
        new()
        {
            IsExpert = false,
            ClassJobLevel = 90,
            RLvl = 580,
            ConditionsFlag = 0b1111,
            MaxDurability = 70,
            MaxQuality = 10920,
            MaxProgress = 3900,
            QualityModifier = 70,
            QualityDivider = 115,
            ProgressModifier = 80,
            ProgressDivider = 130
        })
    };

    public static IEnumerable<SHAWrapper<SimulationState>> States => Inputs.Select(i => new SHAWrapper<SimulationState>(new(i)));

    public static IEnumerable<SHAWrapper<SolverConfig>> Configs => new SHAWrapper<SolverConfig>[]
    {
        new(new()
        {
            Algorithm = SolverAlgorithm.Stepwise,
            Iterations = 30_000,
        })
    };

    [ParamsSource(nameof(States))]
    public SHAWrapper<SimulationState> State { get; set; }

    [ParamsSource(nameof(Configs))]
    public SHAWrapper<SolverConfig> Config { get; set; }

    [Benchmark]
    public async Task<float> Solve()
    {
        var solver = new Solver.Solver(Config, State);
        solver.Start();
        var (_, s) = await solver.GetTask().ConfigureAwait(false);
        return (float)s.Quality / s.Input.Recipe.MaxQuality;
    }
}