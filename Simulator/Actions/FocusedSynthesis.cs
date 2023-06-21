namespace Craftimizer.Simulator.Actions;

internal sealed class FocusedSynthesis : BaseAction
{
    public override ActionCategory Category => ActionCategory.Synthesis;
    public override int Level => 67;
    public override uint ActionId => 100235;

    public override int CPCost => 5;
    public override float Efficiency => 2.00f;
    public override bool IncreasesProgress => true;
    public override float SuccessRate => Simulation.ActionStates.Observed ? 1.00f : 0.50f;
}
