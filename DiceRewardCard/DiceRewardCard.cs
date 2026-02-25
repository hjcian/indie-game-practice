using Godot;

public partial class DiceRewardCard : PanelContainer
{
    private Label _diceNameLabel;
    private Label _diceDescLabel;
    private DiceData _diceData;
    private bool _isSelected = false;

    // 信號：當卡片被點選時發出
    [Signal]
    public delegate void CardSelectedEventHandler(bool isSelected);

    public override void _Ready()
    {
        _diceNameLabel = GetNode<Label>("%DiceNameLabel");
        _diceDescLabel = GetNode<Label>("%DiceDescLabel");

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

    public void SetDiceData(DiceData data)
    {
        _diceData = data;
        // 確保標籤已初始化（_Ready 可能還沒執行）
        if (_diceNameLabel == null)
        {
            _diceNameLabel = GetNode<Label>("%DiceNameLabel");
        }
        if (_diceDescLabel == null)
        {
            _diceDescLabel = GetNode<Label>("%DiceInfoLabel");
        }
        _diceNameLabel.Text = data.Name;
        UpdateInfo();
    }

    public DiceData GetDiceData()
    {
        return _diceData;
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

    // 更新骰子的額外資訊（使用 zh-TW 描述）
    private void UpdateInfo()
    {
        _diceDescLabel.Text = _diceData.DescriptionZhTw;
    }
}
