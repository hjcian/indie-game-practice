using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RewardWindowManager : Control
{
    [Export]
    public HBoxContainer RandomDiceOptions;

    [Export]
    public Button ConfirmButton;

    [Export]
    public Label InfoLabel;

    [Export]
    public PackedScene DiceRewardCardScene;

    private DiceRewardCard _selectedRewardCard;
    private Random _random = new();

    /// <summary>
    /// 當玩家確認獎勵後觸發，傳遞選中的 DiceData
    /// </summary>
    public event Action<DiceData> RewardConfirmed;

    public override void _Ready()
    {
        // 初始時隱藏獎勵區域
        RandomDiceOptions.Visible = false;
        ConfirmButton.Visible = false;

        // 連結按鈕信號
        ConfirmButton.Pressed += OnConfirmButtonPressed;
    }

    /// <summary>
    /// 顯示隨機三個獎勵骰子選項
    /// </summary>
    public void ShowRewardOptions()
    {
        // 清空舊的奖励卡片
        foreach (var child in RandomDiceOptions.GetChildren())
            child.QueueFree();

        _selectedRewardCard = null;

        // 從 DiceRegistry 隨機挑選三個（不重複）
        var pool = DiceRegistry.All.ToList();
        pool.Sort((_, _) => _random.Next(-1, 2)); // 洗牌
        var selected = pool.Take(3).ToList();

        // 為每個 DiceData 建立奖励卡片
        foreach (var diceData in selected)
        {
            var card = DiceRewardCardScene.Instantiate<DiceRewardCard>();
            RandomDiceOptions.AddChild(card);
            card.SetDiceData(diceData);

            // 連結卡片選擇訊號
            card.CardSelected += isSelected => OnRewardCardSelected(card, isSelected);
        }

        // 顯示奖励區域
        RandomDiceOptions.Visible = true;
        ConfirmButton.Visible = true;
        ConfirmButton.Disabled = true;
        InfoLabel.Text = "選擇一顆奖励骰子";
    }

    /// <summary>
    /// 隱藏獎勵區域
    /// </summary>
    public void HideRewardOptions()
    {
        RandomDiceOptions.Visible = false;
        ConfirmButton.Visible = false;
        InfoLabel.Visible = false;
    }

    private void OnRewardCardSelected(DiceRewardCard selectedCard, bool isSelected)
    {
        if (isSelected)
        {
            // 取消之前選中的卡片
            if (_selectedRewardCard != null && _selectedRewardCard != selectedCard)
            {
                _selectedRewardCard.SetSelected(false);
            }
            _selectedRewardCard = selectedCard;
        }
        else
        {
            if (_selectedRewardCard == selectedCard)
            {
                _selectedRewardCard = null;
            }
        }

        // Update UI
        if (_selectedRewardCard != null)
        {
            ConfirmButton.Disabled = false;
        }
        else
        {
            ConfirmButton.Disabled = true;
        }
    }

    private void OnConfirmButtonPressed()
    {
        if (_selectedRewardCard == null)
        {
            InfoLabel.Text = "請先選擇一顆骰子";
            return;
        }

        DiceData selectedDice = _selectedRewardCard.GetDiceData();
        GD.Print($"[Reward] 玩家選擇了: {selectedDice.Name}");

        // 隱藏奖励區域
        HideRewardOptions();

        // 通知 Main
        RewardConfirmed?.Invoke(selectedDice);
    }
}
