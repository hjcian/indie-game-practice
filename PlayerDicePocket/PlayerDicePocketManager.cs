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
    private List<DicePocketCard> _selectedCards = [];

    // 通知 Main 目前選擇是否達到可投擲數量
    [Signal]
    public delegate void SelectionReadyChangedEventHandler(bool isReady);

    public void Initialize()
    {
        for (int i = 0; i < _maxInitialDice; i++)
        {
            AddDice(DiceRegistry.Basic);
        }
    }

    public void AddDice(DiceData diceData)
    {
        DicePocketCard card = DiceScene.Instantiate<DicePocketCard>();
        PocketContainer.AddChild(card);
        card.SetDiceData(diceData);
        card.CardSelected += isSelected => OnCardSelected(card, isSelected);
    }

    private void OnCardSelected(DicePocketCard clickedCard, bool isSelected)
    {
        if (isSelected)
        {
            if (_selectedCards.Count >= _maxSelectable)
            {
                // FIFO：超過上限時自動移除最早選的
                var removedCard = _selectedCards[0];
                removedCard.SetSelected(false);
                _selectedCards.RemoveAt(0);
            }
            _selectedCards.Add(clickedCard);
        }
        else
        {
            _selectedCards.Remove(clickedCard);
        }

        EmitSignal(SignalName.SelectionReadyChanged, _selectedCards.Count >= _maxSelectable);
    }

    public List<DiceData> GetSelectedDices()
    {
        return _selectedCards.ConvertAll(card => card.GetDiceData());
    }
}
