using System;
using Godot;
using Godot.Collections;

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

    [Export]
    public HBoxContainer PipelineContainer;

    [Export]
    public PackedScene ModifierCardScene; // 這是場景的模板

    private int _currentScore = 0;
    private bool _isDoubled = false;
    private bool _isPlusOned = false;
    private Random _random = new();

    // 當場景載入完成時調用 (類似於 Start 或 Initialize)
    public override void _Ready()
    {
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

        // 2. 設定資料
        // 注意：這裡建議在 ModifierResource 加一個變數來存顯示名稱，或是直接用 GetType().Name
        card.SetName(modifier.GetType().Name);

        // 3. 掛載到 UI 容器下
        PipelineContainer.AddChild(card);
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

    private void OnRollButtonPressed()
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
        _currentScore += modifiedRoll;

        // 2. 更新視覺反饋
        ResultLabel.Text = $"Last Roll: {roll}, Modified: {modifiedRoll}";
        UpdateUI();

        // 3. 簡單的遊戲終點判斷
        if (_currentScore >= 50)
        {
            ResultLabel.Text = "Status: Target Reached! (Winner)";
            RollButton.Disabled = true;
        }
    }

    private void UpdateUI()
    {
        ScoreLabel.Text = $"Current Score: {_currentScore} / 50";

        // 防止重複執行動畫，先建立新的 Tween
        Tween tween = GetTree().CreateTween();

        // 修正 Pivot 以便從中心縮放
        ScoreLabel.PivotOffset = ScoreLabel.Size / 2;

        // 縮放動畫：放大 -> 回彈
        tween.TweenProperty(ScoreLabel, "scale", new Vector2(1.2f, 1.2f), 0.05f); // 放大 from 1.0 to 1.2
        tween.TweenProperty(ScoreLabel, "scale", new Vector2(1.0f, 1.0f), 0.1f); // 縮回 to 1.0
    }
}
