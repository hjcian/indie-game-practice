using System;
using System.Collections.Generic;
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
    /// 信號：當玩家確認獎勵後觸發，傳遞選中的骰子值
    /// </summary>
    [Signal]
    public delegate void RewardConfirmedEventHandler(int selectedDiceValue);

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

        // 隨機生成三個骰子值 (1-6)
        var rewardDiceValues = new int[3];
        for (int i = 0; i < 3; i++)
        {
            rewardDiceValues[i] = _random.Next(1, 7);
        }

        // 為每個骰子值建立奖励卡片並顯示
        for (int i = 0; i < 3; i++)
        {
            var card = DiceRewardCardScene.Instantiate<DiceRewardCard>();
            RandomDiceOptions.AddChild(card);
            // 先加入場景樹，讓 _Ready() 執行，然後才設置數值
            card.SetDiceValue(rewardDiceValues[i]);

            // 連結卡片選擇訊號
            card.CardSelected += isSelected => OnRewardCardSelected(card, isSelected);
        }

        // 顯示奖励區域
        RandomDiceOptions.Visible = true;
        ConfirmButton.Visible = true;
        ConfirmButton.Disabled = true; // 預設禁用，直到玩家選擇一個卡片
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

        int selectedDiceValue = _selectedRewardCard.GetDiceValue();
        GD.Print($"[Reward] 玩家選擇了骰子值: {selectedDiceValue}");

        // 隱藏奖励區域
        HideRewardOptions();

        // 觸發信號通知 Main
        EmitSignal(SignalName.RewardConfirmed, selectedDiceValue);
    }
}
