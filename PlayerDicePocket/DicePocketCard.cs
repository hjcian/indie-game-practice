using Godot;

public partial class DicePocketCard : PanelContainer
{
    private Label _diceNameLabel;
    private Label _diceDescLabel;
    private DiceData _diceData;
    private bool _isSelected = false;

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
        if (_diceNameLabel == null)
            _diceNameLabel = GetNode<Label>("%DiceNameLabel");
        if (_diceDescLabel == null)
            _diceDescLabel = GetNode<Label>("%DiceDescLabel");

        _diceNameLabel.Text = data.Name;
        _diceDescLabel.Text = data.DescriptionZhTw;
    }

    public DiceData GetDiceData() => _diceData;

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

    public bool IsSelected() => _isSelected;
}
