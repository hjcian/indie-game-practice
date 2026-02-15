using System;
using Godot;

public partial class Dice : PanelContainer
{
    // [Export]
    // public Label NumberLabel;
    private Label _numberLabel;
    public int CurrentValue { get; private set; }

    public void SetValue(int value)
    {
        CurrentValue = value;
        _numberLabel.Text = value.ToString();

        // 1.4: Juice it up with random colors
        // 隨機顏色：給骰子一點色彩
        // 使用 HSV 模式可以確保顏色飽和度適中，不會太暗
        float randomHue = (float)GD.RandRange(0.0, 1.0);
        SelfModulate = Color.FromHsv(randomHue, 0.6f, 0.9f);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // % 符號代表搜尋場景內的唯一名稱
        _numberLabel = GetNode<Label>("%NumberLabel");

        // 1.4: Juice: add a little bounce animation when the dice show up
        GetTree().ProcessFrame += OnWaitFrameFinished;
    }

    private void OnWaitFrameFinished()
    {
        GetTree().ProcessFrame -= OnWaitFrameFinished;
        Control wrapper = GetNode<Control>("%AnimationWrapper");

        // 現在 wrapper 的 Size 是由 Dice (PanelContainer) 撐開的
        wrapper.PivotOffset = wrapper.Size / 2;
        wrapper.Scale = Vector2.Zero;

        Tween tween = GetTree().CreateTween().SetParallel(true);

        // 縮放動畫
        tween
            .TweenProperty(wrapper, "scale", Vector2.One, 0.4f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);

        // 淡入動畫
        tween.TweenProperty(wrapper, "modulate:a", 1.0f, 0.2f);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}
