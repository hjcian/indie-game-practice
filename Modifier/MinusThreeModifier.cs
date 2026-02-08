using Godot;

[GlobalClass]
public partial class MinusThreeModifier : ModifierResource
{
    public override int Apply(int input) => input - 3;
}
