using System;
using Godot;

[GlobalClass]
public partial class ModifierResource : Resource
{
    [Export]
    public string Name;

    // 定義一個抽象方法，讓子類別去實作邏輯
    public virtual int Apply(int input) => input;
}
