using Godot;

[GlobalClass]
public partial class MinusOneModifier : ModifierResource
{
    public override int Apply(int input) => input - 1;
}
