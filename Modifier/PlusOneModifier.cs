using Godot;

[GlobalClass]
public partial class PlusOneModifier : ModifierResource
{
    public override int Apply(int input) => input + 1;
}
