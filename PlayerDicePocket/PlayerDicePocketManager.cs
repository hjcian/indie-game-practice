using System.Collections.Generic;
using Godot;

public partial class PlayerDicePocketManager : Node
{
    [Export]
    public GridContainer PocketContainer;

    [Export]
    public PackedScene DiceScene;

    private int _maxInitialDice = 4;
    private int _maxSelectable = 4;
    private List<Dice> _selectedDices = [];

    // 通知 Main 目前選擇是否達到可投擲數量
    [Signal]
    public delegate void SelectionReadyChangedEventHandler(bool isReady);

    public void Initialize()
    {
        for (int i = 0; i < _maxInitialDice; i++)
        {
            Dice diceUI = DiceScene.Instantiate<Dice>();
            PocketContainer.AddChild(diceUI);
            diceUI.SetValue(1);
            diceUI.DiceSelected += isSelected => OnDiceSelected(diceUI, isSelected);
        }
    }

    private void OnDiceSelected(Dice clickedDice, bool isSelected)
    {
        if (isSelected)
        {
            if (_selectedDices.Count >= _maxSelectable)
            {
                // FIFO：超過上限時自動移除最早選的
                var removedDice = _selectedDices[0];
                removedDice.SetSelected(false);
                _selectedDices.RemoveAt(0);
            }
            _selectedDices.Add(clickedDice);
        }
        else
        {
            _selectedDices.Remove(clickedDice);
        }

        EmitSignal(SignalName.SelectionReadyChanged, _selectedDices.Count >= _maxSelectable);
    }

    public List<Dice> GetSelectedDices()
    {
        return _selectedDices;
    }
}
