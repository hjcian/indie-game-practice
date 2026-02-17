using Godot;

[GlobalClass]
public partial class MinusOneModifier : ModifierResource
{
    public override int Apply(int input) => input - 1;

    public override string ToString() => "-1";
}
