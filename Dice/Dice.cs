using System;
using Godot;

public partial class Dice : PanelContainer
{
    private Label _numberLabel;
    public int OriginalValue { get; private set; }
    private bool _isSelected = false;

    private ModifierCard _appliedModifier = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // % 符號代表搜尋場景內的唯一名稱
        _numberLabel = GetNode<Label>("%NumberLabel");

        // // 1.4: Juice: add a little bounce animation when the dice show up
        // GetTree().ProcessFrame += OnWaitFrameFinished;

        PivotOffset = Size / 2;
        GuiInput += OnGuiInput;
    }

    // 記得宣告信號，讓 Main 知道是哪顆骰子被選了
    [Signal]
    public delegate void DiceSelectedEventHandler(bool isSelected);

    private void OnGuiInput(InputEvent @event)
    {
        if (
            @event is InputEventMouseButton mouseEvent
            && mouseEvent.Pressed
            && mouseEvent.ButtonIndex == MouseButton.Left
        )
        {
            SetSelected(!_isSelected);
            // 發出訊號給 Main (我們待會處理)
            EmitSignal(SignalName.DiceSelected, _isSelected);
        }
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;

        // 建立動畫
        var tween = GetTree().CreateTween().SetParallel(true);

        if (_isSelected)
        {
            // 放大 1.2 倍，並變色（淡淡的藍色高亮）
            tween.TweenProperty(this, "scale", new Vector2(1.2f, 1.2f), 0.1f);
            tween.TweenProperty(this, "modulate", new Color(0.8f, 0.9f, 1.0f), 0.1f);
        }
        else
        {
            // 恢復原狀
            tween.TweenProperty(this, "scale", new Vector2(1.0f, 1.0f), 0.1f);
            tween.TweenProperty(this, "modulate", new Color(1, 1, 1), 0.1f);
            // Unselect the modifier card if the dice is unselected
            if (_appliedModifier != null)
            {
                _appliedModifier.SetSelected(false);
                _appliedModifier = null;
                SetUI();
            }
        }
    }

    public void SetValue(int value)
    {
        OriginalValue = value;
        SetUI();

        // 1.4: Juice it up with random colors
        // 隨機顏色：給骰子一點色彩
        // 使用 HSV 模式可以確保顏色飽和度適中，不會太暗
        float randomHue = (float)GD.RandRange(0.0, 1.0);
        SelfModulate = Color.FromHsv(randomHue, 0.6f, 0.9f);
    }

    // private void OnWaitFrameFinished()
    // {
    //     GetTree().ProcessFrame -= OnWaitFrameFinished;
    //     Control wrapper = GetNode<Control>("%AnimationWrapper");

    //     // 現在 wrapper 的 Size 是由 Dice (PanelContainer) 撐開的
    //     wrapper.PivotOffset = wrapper.Size / 2;
    //     wrapper.Scale = Vector2.Zero;

    //     Tween tween = GetTree().CreateTween().SetParallel(true);

    //     // 縮放動畫
    //     tween
    //         .TweenProperty(wrapper, "scale", Vector2.One, 0.4f)
    //         .SetTrans(Tween.TransitionType.Back)
    //         .SetEase(Tween.EaseType.Out);

    //     // 淡入動畫
    //     tween.TweenProperty(wrapper, "modulate:a", 1.0f, 0.2f);
    // }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    private void SetUI()
    {
        if (_appliedModifier == null)
        {
            _numberLabel.Text = OriginalValue.ToString();
        }
        else
        {
            // format: "Modified (Original)"
            _numberLabel.Text = $"{GetModifiedValue()} ({OriginalValue})";
        }
    }

    public void ApplyModifier(ModifierCard modifierCard)
    {
        _appliedModifier?.SetSelected(false);
        _appliedModifier = modifierCard;
        _appliedModifier.SetSelected(true);
        SetUI();
    }

    public void UnbindModifier(ModifierCard modifierCard)
    {
        modifierCard.SetSelected(false);
        if (_appliedModifier == modifierCard)
        {
            _appliedModifier = null;
        }
        SetUI();
    }

    public int GetModifiedValue()
    {
        if (_appliedModifier == null)
        {
            return OriginalValue;
        }
        return _appliedModifier.SourceResource.Apply(OriginalValue);
    }

    //
    // UI helper
    //
    public void SetToConditionType()
    {
        Modulate = new Color(0.6f, 0.6f, 0.6f); // 條件骰：灰色
    }

    public void SetToScoreType()
    {
        Modulate = new Color(1.0f, 0.9f, 0.2f); // 得分骰：醒目色
    }
}
