using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;

public enum Phase
{
    PlayerTurn, // Waiting for player input (Roll)
    PlayerThinking, // Waiting for player to choose modifiers/operators and commit action
    Processing,

    EnemyTurn,
    Victory,
    Defeat,
}

public partial class Main : Control
{
    [Export]
    public Label LevelInfo;

    [Export]
    public Label RoundInfo;

    private int _currentLevel = 1;
    private int _goal = 1;
    private int _maxAttempts = 3;
    private int _currentAttempt = 0;
    private int _accumulatedScore = 0;

    public void UpdateLevelInfoUI()
    {
        LevelInfo.Text = $"Level {_currentLevel} | Goal: {_goal}";
        int remaining = _goal - _accumulatedScore;
        if (remaining < 0)
            remaining = 0;
        RoundInfo.Text =
            $"Current/Left: {_accumulatedScore}/{remaining} | Attempts: {_currentAttempt} / {_maxAttempts}";
    }

    [Export]
    public GridContainer PlayerDiceContainer;
    private int _MaxInitialDice = 4;
    private int _MaxSelectable = 4;
    private List<Dice> _selectedDices = [];

    public void InitializePlayerDice()
    {
        for (int i = 0; i < _MaxInitialDice; i++)
        {
            Dice diceUI = DiceScene.Instantiate<Dice>();
            PlayerDiceContainer.AddChild(diceUI);
            diceUI.SetValue(1);
            diceUI.DiceSelected += isSelected => OnDiceSelected(diceUI, isSelected);
        }
    }

    private void OnDiceSelected(Dice clickedDice, bool isSelected)
    {
        // Add or remove the clicked dice from the selected list based on its new state
        if (isSelected)
        {
            if (_selectedDices.Count >= _MaxSelectable)
            {
                // FIFO
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
        UpdateRollButtonUI();
    }

    private void _DisableRollButton()
    {
        RollButton.Disabled = true;
    }

    private void _EnableRollButton()
    {
        RollButton.Disabled = false;
    }

    [Export]
    public Button RollButton;

    private void UpdateRollButtonUI()
    {
        if (_selectedDices.Count < _MaxSelectable)
        {
            _DisableRollButton();
        }
        else
        {
            _EnableRollButton();
        }
    }

    [Export]
    public GridContainer DiceResultContainer;
    private List<Dice> _resultedDices = [];

    [Export]
    public Label ResultLabel;

    [Export]
    public Label LevelResultLabel;

    private void OnRollButtonPressed()
    {
        // update the UI to show the rolled dice results
        // A. 清除舊的骰子
        foreach (var d in _resultedDices)
            d.QueueFree();
        _resultedDices.Clear();

        int total = 0;
        for (int i = 0; i < _selectedDices.Count; i++)
        {
            // 擲骰
            int val = _random.Next(1, 7);
            // var d = _selectedDices[i].Duplicate();
            Dice d = DiceScene.Instantiate<Dice>();
            DiceResultContainer.AddChild(d);
            _resultedDices.Add(d);
            d.SetValue(val);
            total += val;
        }

        // C. 判斷是否為有效組合
        if (!IsValidCombination(_resultedDices))
        {
            ResultLabel.Text = "無有效組合，請重新選擇骰子再投擲。";
            _EnableRollButton(); // 允許再擲
            return;
        }

        int score = HighlightConditionAndScoreDices(_resultedDices);
        ResultLabel.Text = $"得分骰總和：{score}";

        CheckLevelFinish(score);
        _EnableRollButton();
        // _DisableRollButton();
    }

    private void CheckLevelFinish(int score)
    {
        _currentAttempt++;
        _accumulatedScore += score;

        if (_accumulatedScore >= _goal)
        {
            LevelResultLabel.Text = "成功達成目標！進入奖励阶段！";
            _DisableRollButton();
            // 觸發奖励選擇
            RewardWindowManager.ShowRewardOptions();
        }
        else if (_currentAttempt >= _maxAttempts)
        {
            LevelResultLabel.Text = "挑戰失敗，請再試一次！";
            // 可以選擇重置當前關卡或是降低難度
            _currentAttempt = 0;
            _accumulatedScore = 0;
        }
        else
        {
            LevelResultLabel.Text = "繼續挑戰，選擇骰子再投擲！";
        }
        UpdateLevelInfoUI();
    }

    private bool IsValidCombination(List<Dice> dices)
    {
        var seen = new HashSet<int>();
        foreach (var d in dices)
        {
            if (!seen.Add(d.OriginalValue))
                return true; // 出現重複
        }
        return false;
    }

    // 找到相同兩顆作為條件骰，其餘為得分骰
    private int HighlightConditionAndScoreDices(List<Dice> dices)
    {
        // 統計出現次數
        var counts = new System.Collections.Generic.Dictionary<int, int>();
        foreach (var d in dices)
        {
            counts.TryGetValue(d.OriginalValue, out int c);
            counts[d.OriginalValue] = c + 1;
        }

        // 找到「最小」且出現次數 >= 2 的點數作為條件骰
        int conditionValue = -1;
        foreach (var kv in counts)
        {
            if (kv.Value >= 2 && (conditionValue == -1 || kv.Key < conditionValue))
            {
                conditionValue = kv.Key;
            }
        }

        int scoreSum = 0;
        int usedCondition = 0;

        // 視覺標示：條件骰灰色，得分骰醒目色
        foreach (var d in dices)
        {
            if (d.OriginalValue == conditionValue && usedCondition < 2)
            {
                d.SetToConditionType();
                usedCondition++;
            }
            else
            {
                d.SetToScoreType();
                scoreSum += d.OriginalValue;
            }
        }
        return scoreSum;
    }

    private void StartNewLevel()
    {
        UpdateLevelInfoUI();
        InitializePlayerDice();
        UpdateRollButtonUI();
        RollButton.Pressed += OnRollButtonPressed;
        RewardWindowManager.RewardConfirmed += OnRewardConfirmed;
    }

    // 當場景載入完成時調用 (類似於 Start 或 Initialize)
    public override void _Ready()
    {
        StartNewLevel();
    }

    [Export]
    public RewardWindowManager RewardWindowManager;

    private void OnRewardConfirmed(DiceData selectedDice)
    {
        GD.Print($"[Main] 玩家確認選擇: {selectedDice.Name}");

        // 進入下一關
        _goal += 5; // 每關增加5點目標分數
        _currentAttempt = 0;
        _accumulatedScore = 0;
        _currentLevel++;
        UpdateLevelInfoUI();

        ResultLabel.Text = "進入下一關！";
        _EnableRollButton();
    }

    // ========================================================================================
    //
    // DEPRECATED
    //
    //

    [Export]
    public Array<ModifierResource> ActiveModifiers = [];

    [Export]
    public ModifierResource DoubleSkill;

    [Export]
    public ModifierResource PlusOneSkill;

    //
    // Dice objects
    //

    [Export]
    public PackedScene DiceScene; // 骰子場景的模板
    private System.Collections.Generic.List<Dice> _activeDices = [];

    // End Dice objects

    [Export]
    public HBoxContainer PipelineContainer;

    [Export]
    public PackedScene ModifierCardScene; // 這是場景的模板

    //
    private static readonly int initHP = 50;

    [Export]
    public Label EnemyWeaknessLabel;

    private int _maxEnemyHP = initHP;
    private int _currentEnemyHP = initHP;
    private int _weaknessValue = 0; // 這個值在每一關重置，或者根據關卡難度增加

    // Player Info

    private int _maxPlayerHP = initHP;
    private int _currentPlayerHP = initHP;

    [Export]
    public Label PreviewDamageLabel;

    // Player Skills
    [Export]
    public HBoxContainer PlayerSkillsContainer;

    // Possible Skills
    [Export]
    public Array<ModifierResource> PossibleModifiers = []; // NOTE: 在 Inspector 裡把所有你寫好的 Resource 丟進這個陣列

    // Player Thinking Info
    private Dice _selectedDice = null;

    //
    private Random _random = new();

    private Phase _currentState;

    private void ChangePhase(Phase newState)
    {
        _currentState = newState;

        switch (_currentState)
        {
            case Phase.PlayerTurn:
                _EnableRollButton();
                ResultLabel.Text = "Your Turn! Dice the Roll!";
                PreviewDamageLabel.Text = "";
                break;

            case Phase.Processing:
                _DisableRollButton();
                break;
            case Phase.PlayerThinking:
                _DisableRollButton();
                break;
            case Phase.EnemyTurn:
                _DisableRollButton();
                PerformEnemyAction();
                break;
            case Phase.Victory:
                ResultLabel.Text = "Victory! You defeated the enemy!";
                _DisableRollButton();
                // 這裡可以播放一段勝利的動畫或顯示下一關按鈕
                break;
            case Phase.Defeat:
                ResultLabel.Text = "Defeat... The enemy was too strong.";
                _DisableRollButton();
                // 這裡可以播放一段哀傷的動畫或顯示重來按鈕
                break;
        }
    }

    private void SyncActiveSkills()
    {
        ActiveModifiers.Clear();

        int count = 0;
        foreach (var node in PlayerSkillsContainer.GetChildren())
        {
            count++;
            if (node is ModifierCard card && card.IsSelected)
            {
                ActiveModifiers.Add(card.SourceResource);
            }
        }

        GD.Print(
            $"[System] Sync 完成。 Player's skill count: {count}, 當前啟用的 Modifier 數量: {ActiveModifiers.Count}"
        );
    }

    private bool ProcessDamage(int totalDamage)
    {
        _currentEnemyHP -= totalDamage;
        if (_currentEnemyHP <= 0)
        {
            _currentEnemyHP = 0;
            return true;
        }
        return false;
    }

    private void UpdateUI()
    {
        LevelResultLabel.Text = $"Current HP: {_currentEnemyHP} / {_maxEnemyHP}";

        // 防止重複執行動畫，先建立新的 Tween
        Tween tween = GetTree().CreateTween();

        // 修正 Pivot 以便從中心縮放
        LevelResultLabel.PivotOffset = LevelResultLabel.Size / 2;

        // 縮放動畫：放大 -> 回彈
        tween.TweenProperty(LevelResultLabel, "scale", new Vector2(1.2f, 1.2f), 0.05f); // 放大 from 1.0 to 1.2
        tween.TweenProperty(LevelResultLabel, "scale", new Vector2(1.0f, 1.0f), 0.1f); // 縮回 to 1.0
    }

    private void UpdatePreviewDamage()
    {
        int totalDamage = 0;
        foreach (var d in _activeDices)
        {
            totalDamage += d.GetModifiedValue(); // 假設 Dice 類別有一個 OriginalValue 屬性來存儲當前的骰子點數
        }
        PreviewDamageLabel.Text = $"Preview Damage: {totalDamage}";
    }

    // Enemy action
    private async void PerformEnemyAction()
    {
        ResultLabel.Text = "Enemy is attacking...";

        // 模擬敵人思考/動畫時間
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

        // 這裡可以實作玩家的血條扣除
        // 1. 執行扣血
        int enemyDamage = 5;
        _currentPlayerHP -= enemyDamage;

        // 2. 更新 UI
        GD.Print($"[Battle] Player took {enemyDamage} damage. Current HP: {_currentPlayerHP}");

        // 3. 檢查玩家是否死亡 (Defeat)
        if (_currentPlayerHP <= 0)
        {
            _currentPlayerHP = 0;
            ChangePhase(Phase.Defeat);
        }
        else
        {
            // 沒死才輪到玩家
            ChangePhase(Phase.PlayerTurn);
        }
    }

    // End of a battle round

    private void ShowRewardOptions()
    {
        ResultLabel.Text = "Pick a New Skill!";

        // 1. 隨機洗牌或挑選三個
        GD.Print($"[Reward] Possible Modifiers Count: {PossibleModifiers.Count}");
        var options = PossibleModifiers.Duplicate(); // 複製一份
        options.Shuffle(); // 隨機打亂

        // 2. 顯示在 UI 上 (這裡我們重用之前的 AddModifierToVisualPipeline)
        // 但因為這是「領取獎勵」而非「已啟用的管線」，我們需要一個新的容器
        // 或是暫時清空 PipelineContainer 來顯示獎勵
        PipelineContainer.Modulate = new Color(1, 1, 0); // 視覺上提醒這是獎勵時間

        // 清空舊卡片
        foreach (var n in PipelineContainer.GetChildren())
            n.QueueFree();

        for (int i = 0; i < 3 && i < options.Count; i++)
        {
            AddRewardCard((ModifierResource)options[i]);
        }
    }

    private void AddRewardCard(ModifierResource res)
    {
        var card = ModifierCardScene.Instantiate<ModifierCard>();
        card.LinkModifierResource(res);
        PipelineContainer.AddChild(card);

        // 斷開原本的 Toggle 連結，改連到「領取」邏輯
        // 我們可以給 Card 加一個模式，或者直接在這裡覆蓋訊號
        card.ToggleStatusChanged += isActive => OnRewardPicked(res);
    }

    private void OnRewardPicked(ModifierResource res)
    {
        var card = ModifierCardScene.Instantiate<ModifierCard>();
        card.LinkModifierResource(res);
        GD.Print($"You picked: {res.ResourceName}");
        // Change the card's signal to now sync with the active modifiers, since it's now part of the player's permanent collection
        card.ToggleStatusChanged += isActive => SyncActiveSkills();
        // 加入玩家永久庫存
        PlayerSkillsContainer.AddChild(card);

        // 4. 清理 PipelineContainer 剩餘沒被選中的卡片
        foreach (var n in PipelineContainer.GetChildren())
        {
            n.QueueFree();
        }

        // 目前我們先簡單處理，重設場景
        ResetBattle();
    }

    private void ResetBattle()
    {
        _currentEnemyHP = _maxEnemyHP;

        // 清除 UI 並恢復狀態
        PipelineContainer.Modulate = new Color(1, 1, 1);
        ChangePhase(Phase.PlayerTurn);
    }
}
