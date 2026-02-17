using Godot;

[GlobalClass]
public partial class DoubleModifier : ModifierResource
{
    public override int Apply(int input) => input * 2;

    public override string ToString() => "x2";
}
