using Godot;

public partial class DiceRewardCard : PanelContainer
{
    private Label _diceValueLabel;
    private Label _diceInfoLabel;
    private int _diceValue;
    private bool _isSelected = false;

    // 信號：當卡片被點選時發出
    [Signal]
    public delegate void CardSelectedEventHandler(bool isSelected);

    public override void _Ready()
    {
        _diceValueLabel = GetNode<Label>("%DiceValueLabel");
        _diceInfoLabel = GetNode<Label>("%DiceInfoLabel");

        GuiInput += OnGuiInput;
        PivotOffset = Size / 2;
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (
            @event is InputEventMouseButton mouseEvent
            && mouseEvent.Pressed
            && mouseEvent.ButtonIndex == MouseButton.Left
        )
        {
            SetSelected(!_isSelected);
            EmitSignal(SignalName.CardSelected, _isSelected);
        }
    }

    public void SetDiceValue(int value)
    {
        _diceValue = value;
        // 確保標籤已初始化（_Ready 可能還沒執行）
        if (_diceValueLabel == null)
        {
            _diceValueLabel = GetNode<Label>("%DiceValueLabel");
        }
        if (_diceInfoLabel == null)
        {
            _diceInfoLabel = GetNode<Label>("%DiceInfoLabel");
        }
        _diceValueLabel.Text = value.ToString();
        UpdateInfo();
    }

    public int GetDiceValue()
    {
        return _diceValue;
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;

        var tween = GetTree().CreateTween().SetParallel(true);

        if (_isSelected)
        {
            tween.TweenProperty(this, "scale", new Vector2(1.1f, 1.1f), 0.1f);
            tween.TweenProperty(this, "modulate", new Color(0.8f, 0.9f, 1.0f), 0.1f);
        }
        else
        {
            tween.TweenProperty(this, "scale", new Vector2(1.0f, 1.0f), 0.1f);
            tween.TweenProperty(this, "modulate", new Color(1, 1, 1), 0.1f);
        }
    }

    public bool IsSelected()
    {
        return _isSelected;
    }

    // 更新骰子的額外資訊（基本能力等）
    // TODO: 後續根據骰子類型補充詳細資訊
    private void UpdateInfo()
    {
        _diceInfoLabel.Text = $"Value: {_diceValue}\n\n[骰子能力資訊待補充]";
    }
}
