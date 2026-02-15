using System;
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
    // 使用 [Export] 讓你在編輯器介面就能直接拖放節點引用，減少 GetNode 的硬編碼
    [Export]
    public Label ScoreLabel;

    [Export]
    public Label ResultLabel;

    [Export]
    public Button RollButton;

    [Export]
    public Button CommitButton;

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
    public GridContainer DiceContainer;

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

    // Enemy Info
    [Export]
    public ProgressBar EnemyHealthBar;

    [Export]
    public Label EnemyWeaknessLabel;

    private int _maxEnemyHP = initHP;
    private int _currentEnemyHP = initHP;
    private int _weaknessValue = 0; // 這個值在每一關重置，或者根據關卡難度增加

    // Player Info
    [Export]
    public ProgressBar PlayerHealthBar;
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

    private Random _random = new();

    private Phase _currentState;

    private void _DisableRollButton()
    {
        RollButton.Disabled = true;
    }

    private void _EnableRollButton()
    {
        RollButton.Disabled = false;
    }

    private void _DisableCommitButton()
    {
        CommitButton.Disabled = true;
    }

    private void _EnableCommitButton()
    {
        CommitButton.Disabled = false;
    }

    private void ChangePhase(Phase newState)
    {
        _currentState = newState;

        switch (_currentState)
        {
            case Phase.PlayerTurn:
                _EnableRollButton();
                _DisableCommitButton();
                ResultLabel.Text = "Your Turn! Dice the Roll!";
                PreviewDamageLabel.Text = "";
                break;

            case Phase.Processing:
                _DisableRollButton();
                _DisableCommitButton();
                break;
            case Phase.PlayerThinking:
                _DisableRollButton();
                _EnableCommitButton();
                ResultLabel.Text = "Choose your modifiers and commit your action.";
                break;
            case Phase.EnemyTurn:
                _DisableRollButton();
                PerformEnemyAction();
                break;
            case Phase.Victory:
                ResultLabel.Text = "Victory! You defeated the enemy!";
                _DisableRollButton();
                _DisableCommitButton();
                // 這裡可以播放一段勝利的動畫或顯示下一關按鈕
                // ShowRewardOptions();
                break;
            case Phase.Defeat:
                ResultLabel.Text = "Defeat... The enemy was too strong.";
                _DisableRollButton();
                // 這裡可以播放一段哀傷的動畫或顯示重來按鈕
                break;
        }
    }

    private void StartNewLevel()
    {
        // Initialize HP and weakness value for the new level
        // Initialize enemy/player health bar
        EnemyHealthBar.MaxValue = _maxEnemyHP;
        EnemyHealthBar.Value = _currentEnemyHP;
        PlayerHealthBar.MaxValue = _maxPlayerHP;
        PlayerHealthBar.Value = _currentPlayerHP;
        _weaknessValue = _random.Next(_maxEnemyHP / 4, _maxEnemyHP / 2); // 隨機設定弱點值，這會影響玩家的策略選擇
        EnemyWeaknessLabel.Text = $"Enemy Weakness: {_weaknessValue}";

        // Update the UI to reflect the new level's stats
        UpdateUI();

        // Change the battle state to PlayerTurn to start the new level
        // 這裡就是「信號 (Signal)」的串接
        // 在 C# 中，我們通常使用 += 語法來訂閱信號（事件）
        RollButton.Pressed += OnRollButtonPressed;
        CommitButton.Pressed += CommitPlayerAction;
        ChangePhase(Phase.PlayerTurn);
    }

    private void OnRollButtonPressed()
    {
        ChangePhase(Phase.Processing);
        // update the UI to show the rolled dice results
        // A. 清除舊的骰子
        foreach (var d in _activeDices)
            d.QueueFree();
        _activeDices.Clear();
        // B. 生成多顆骰子
        for (int i = 0; i < 5; i++)
        {
            // 擲骰
            int val = _random.Next(1, 7);

            // C. 實例化骰子 UI
            Dice diceUI = DiceScene.Instantiate<Dice>();
            DiceContainer.AddChild(diceUI);
            _activeDices.Add(diceUI);
            // D. 更新骰子視覺
            diceUI.SetValue(val);
        }
        UpdatePreviewDamage();
        ChangePhase(Phase.PlayerThinking);
    }

    private void CommitPlayerAction()
    {
        // NOTE: Come from player click the Commit button
        ChangePhase(Phase.Processing);

        // Apply the final dice results to the enemy's HP
        int totalDamage = 0;
        foreach (var d in _activeDices)
        {
            totalDamage += d.CurrentValue; // 假設 Dice 類別有一個 CurrentValue 屬性來存儲當前的骰子點數
        }
        // Check if the enemy is defeated, if so, change state to Victory
        var isDead = ProcessDamage(totalDamage);
        UpdateUI();
        // E. 進入敵人回合
        if (isDead)
        {
            ChangePhase(Phase.Victory);
        }
        else
        {
            ChangePhase(Phase.PlayerTurn);
        }
    }

    private void EndLevel() { }

    // 當場景載入完成時調用 (類似於 Start 或 Initialize)
    public override void _Ready()
    {
        StartNewLevel();
    }

    private void SyncActiveSkills()
    {
        ActiveModifiers.Clear();

        int count = 0;
        foreach (var node in PlayerSkillsContainer.GetChildren())
        {
            count++;
            if (node is ModifierCard card && card.IsActive)
            {
                ActiveModifiers.Add(card.SourceResource);
            }
        }

        GD.Print(
            $"[System] Sync 完成。 Player's skill count: {count}, 當前啟用的 Modifier 數量: {ActiveModifiers.Count}"
        );
    }

    private int rollDice()
    {
        // 1. 執行核心邏輯
        int roll = _random.Next(1, 7);
        int modifiedRoll = roll;
        foreach (var modifier in ActiveModifiers)
        {
            if (modifier != null)
            {
                int before = modifiedRoll;
                modifiedRoll = modifier.Apply(modifiedRoll);
                GD.Print($"[Pipeline] {modifier.GetType().Name}: {before} -> {modifiedRoll}");
            }
        }
        return modifiedRoll;
    }

    private bool ProcessDamage(int totalDamage)
    {
        _currentEnemyHP -= totalDamage;
        GetTree().CreateTween().TweenProperty(EnemyHealthBar, "value", _currentEnemyHP, 0.5f);
        if (_currentEnemyHP <= 0)
        {
            _currentEnemyHP = 0;
            return true;
        }
        return false;
    }

    private void UpdateUI()
    {
        ScoreLabel.Text = $"Current HP: {_currentEnemyHP} / {_maxEnemyHP}";

        // 防止重複執行動畫，先建立新的 Tween
        Tween tween = GetTree().CreateTween();

        // 修正 Pivot 以便從中心縮放
        ScoreLabel.PivotOffset = ScoreLabel.Size / 2;

        // 縮放動畫：放大 -> 回彈
        tween.TweenProperty(ScoreLabel, "scale", new Vector2(1.2f, 1.2f), 0.05f); // 放大 from 1.0 to 1.2
        tween.TweenProperty(ScoreLabel, "scale", new Vector2(1.0f, 1.0f), 0.1f); // 縮回 to 1.0
    }

    private void UpdatePreviewDamage()
    {
        int totalDamage = 0;
        foreach (var d in _activeDices)
        {
            totalDamage += d.CurrentValue; // 假設 Dice 類別有一個 CurrentValue 屬性來存儲當前的骰子點數
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
        GetTree().CreateTween().TweenProperty(PlayerHealthBar, "value", _currentPlayerHP, 0.3f);
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
        EnemyHealthBar.Value = _currentEnemyHP;

        // 清除 UI 並恢復狀態
        PipelineContainer.Modulate = new Color(1, 1, 1);
        ChangePhase(Phase.PlayerTurn);
    }
}
