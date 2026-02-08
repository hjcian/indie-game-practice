using System;
using Godot;
using Godot.Collections;

public enum BattleState
{
    PlayerTurn,
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
    public Button DoubleSkillButton;

    [Export]
    public Button PlusOneSkillButton;

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

    // Enemy HP
    [Export]
    public ProgressBar EnemyHealthBar;
    private int _maxEnemyHP = 100;
    private int _currentEnemyHP = 100;

    // Player HP
    [Export]
    public ProgressBar PlayerHealthBar;
    private int _maxPlayerHP = 100;
    private int _currentPlayerHP = 100;

    private bool _isDoubled = false;
    private bool _isPlusOned = false;
    private Random _random = new();

    private BattleState _currentState;

    private void _DisablePlayerButtons()
    {
        RollButton.Disabled = true;
        DoubleSkillButton.Disabled = true;
        PlusOneSkillButton.Disabled = true;
    }

    private void _EnablePlayerButtons()
    {
        RollButton.Disabled = false;
        DoubleSkillButton.Disabled = false;
        PlusOneSkillButton.Disabled = false;
    }

    private void ChangeBattleState(BattleState newState)
    {
        _currentState = newState;

        switch (_currentState)
        {
            case BattleState.PlayerTurn:
                _EnablePlayerButtons();
                ResultLabel.Text = "Your Turn! Choose your modifiers and Roll.";
                break;

            case BattleState.Processing:
                _DisablePlayerButtons();
                break;

            case BattleState.EnemyTurn:
                _DisablePlayerButtons();
                PerformEnemyAction();
                break;
            case BattleState.Victory:
                ResultLabel.Text = "Victory! You defeated the enemy!";
                _DisablePlayerButtons();
                // 這裡可以播放一段勝利的動畫或顯示下一關按鈕
                break;
            case BattleState.Defeat:
                ResultLabel.Text = "Defeat... The enemy was too strong.";
                _DisablePlayerButtons();
                // 這裡可以播放一段哀傷的動畫或顯示重來按鈕
                break;
        }
    }

    // 當場景載入完成時調用 (類似於 Start 或 Initialize)
    public override void _Ready()
    {
        // Initialize enemy/player health bar
        EnemyHealthBar.MaxValue = _maxEnemyHP;
        EnemyHealthBar.Value = _currentEnemyHP;
        PlayerHealthBar.MaxValue = _maxPlayerHP;
        PlayerHealthBar.Value = _currentPlayerHP;

        UpdateUI();
        // 這裡就是「信號 (Signal)」的串接
        // 在 C# 中，我們通常使用 += 語法來訂閱信號（事件）
        RollButton.Pressed += OnRollButtonPressed;
        DoubleSkillButton.Pressed += OnDoubleSkillButtonPressed;
        DoubleSkillButton.Text = "Double: disabled"; // 初始化按鈕文字

        PlusOneSkillButton.Pressed += OnPlusOneSkillButtonPressed;
        PlusOneSkillButton.Text = "Plus one: disabled"; // 初始化按鈕文字
    }

    private void AddModifierToVisualPipeline(ModifierResource modifier)
    {
        if (ModifierCardScene == null)
        {
            GD.PushError("錯誤：ModifierCardScene 尚未在 Inspector 中綁定！");
            return;
        }

        // 1. 實例化 (像 new 一個物件，但它是場景)
        var card = ModifierCardScene.Instantiate<ModifierCard>();

        // 2. Link the card with its ModifierResource data (這裡可以直接傳整個 Resource，或是只傳必要的資訊)
        card.LinkModifierResource(modifier);

        // 3. 掛載到 UI 容器下
        PipelineContainer.AddChild(card);

        card.ToggleStatusChanged += (bool isActive) => SyncActiveModifiers();
    }

    private void SyncActiveModifiers()
    {
        ActiveModifiers.Clear();

        foreach (var node in PipelineContainer.GetChildren())
        {
            if (node is ModifierCard card && card.IsActive)
            {
                ActiveModifiers.Add(card.SourceResource);
            }
        }

        GD.Print($"[System] Sync 完成。當前啟用的 Modifier 數量: {ActiveModifiers.Count}");
    }

    private void OnPlusOneSkillButtonPressed()
    {
        if (!_isPlusOned)
        {
            if (PlusOneSkill == null)
            {
                GD.PushError("錯誤：PlusOneSkill 尚未在 Inspector 中綁定！");
                return;
            }
            _isPlusOned = true;
            ActiveModifiers.Add(PlusOneSkill);
            AddModifierToVisualPipeline(PlusOneSkill);
            GD.Print(
                $"[System] 注入成功：{PlusOneSkill.ResourceName}, 當前 Modifier 數量: {ActiveModifiers.Count}"
            );
        }
        PlusOneSkillButton.Text = "Applied: Plus One Effect Injected";
        UpdateUI();
    }

    private void OnDoubleSkillButtonPressed()
    {
        if (!_isDoubled)
        {
            if (DoubleSkill == null)
            {
                GD.PushError("錯誤：DoubleSkill 尚未在 Inspector 中綁定！");
                return;
            }
            _isDoubled = true;
            ActiveModifiers.Add(DoubleSkill);
            AddModifierToVisualPipeline(DoubleSkill);
            GD.Print(
                $"[System] 注入成功：{DoubleSkill.ResourceName}, 當前 Modifier 數量: {ActiveModifiers.Count}"
            );
        }
        DoubleSkillButton.Text = "Applied: Double Effect Injected";
        UpdateUI();
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

    private void OnRollButtonPressed()
    {
        ChangeBattleState(BattleState.Processing);

        // A. 清除舊的骰子
        foreach (var d in _activeDices)
            d.QueueFree();
        _activeDices.Clear();
        // B. 生成多顆骰子 (例如一次擲 3 顆)

        int totalDamage = 0;
        for (int i = 0; i < 3; i++)
        {
            // 實例化骰子
            Dice diceUI = DiceScene.Instantiate<Dice>();
            DiceContainer.AddChild(diceUI);
            _activeDices.Add(diceUI);
            // 擲骰並顯示結果
            int rollValue = rollDice();

            // D. 更新骰子視覺
            diceUI.SetValue(rollValue);
            totalDamage += rollValue; // 累加到總分
        }
        var isDead = ProcessDamage(totalDamage);
        UpdateUI();
        // E. 進入敵人回合
        if (isDead)
        {
            ChangeBattleState(BattleState.Victory);
        }
        else
        {
            ChangeBattleState(BattleState.EnemyTurn);
        }
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

    // Enemy action
    private async void PerformEnemyAction()
    {
        ResultLabel.Text = "Enemy is attacking...";

        // 模擬敵人思考/動畫時間
        await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

        // 這裡可以實作玩家的血條扣除
        // 1. 執行扣血
        int enemyDamage = 33;
        _currentPlayerHP -= enemyDamage;

        // 2. 更新 UI
        GetTree().CreateTween().TweenProperty(PlayerHealthBar, "value", _currentPlayerHP, 0.3f);
        GD.Print($"[Battle] Player took {enemyDamage} damage. Current HP: {_currentPlayerHP}");

        // 3. 檢查玩家是否死亡 (Defeat)
        if (_currentPlayerHP <= 0)
        {
            _currentPlayerHP = 0;
            ChangeBattleState(BattleState.Defeat);
        }
        else
        {
            // 沒死才輪到玩家
            ChangeBattleState(BattleState.PlayerTurn);
        }
    }
}
