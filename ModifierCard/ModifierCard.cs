using System;
using Godot;

public partial class ModifierCard : PanelContainer
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // 1. 隱藏內容，防止閃現
        Modulate = new Color(1, 1, 1, 0);

        // 2. 雙重保險：等待一幀 (Frame)，確保容器已經計算完所有 Size
        // 這是解決 UI 尺寸為 0 最專業的做法
        GetTree().ProcessFrame += OnWaitFrameFinished;
    }

    private void OnWaitFrameFinished()
    {
        // 斷開連接，防止每幀都跑
        GetTree().ProcessFrame -= OnWaitFrameFinished;

        Control mover = GetNode<Control>("PivotMover");

        // 關鍵：如果這裡列印出來是 (0,0)，那動畫絕對不會動
        // GD.Print($"Mover Size: {mover.Size}");

        // 設定中心點與初始縮放
        mover.PivotOffset = mover.Size / 2;
        mover.Scale = Vector2.Zero;

        // 執行動畫
        Tween tween = GetTree().CreateTween();

        // 淡入
        tween.Parallel().TweenProperty(this, "modulate:a", 1.0f, 0.2f);

        // 縮放 (調大一點 0.5s 方便觀察)
        tween
            .Parallel()
            .TweenProperty(mover, "scale", Vector2.One, 0.5f)
            .SetTrans(Tween.TransitionType.Back)
            .SetEase(Tween.EaseType.Out);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public new void SetName(string name)
    {
        GetNode<Label>("%ModifierNameLabel").Text = name;
    }
}
