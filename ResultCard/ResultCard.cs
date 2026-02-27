using Godot;

public partial class ResultCard : PanelContainer
{
    private Label _diceTypeLabel;
    private Dice _diceNode;
    private Label _modifiedValueLabel;

    public int RawValue { get; private set; }
    public int FinalValue { get; private set; }

    public override void _Ready()
    {
        _diceTypeLabel = GetNode<Label>("%DiceTypeLabel");
        _diceNode = GetNode<Dice>("%Dice");
        _modifiedValueLabel = GetNode<Label>("%ModifiedValueLabel");
    }

    /// <summary>
    /// 設定骰子資料與原始擲骰結果，自動套用能力並更新 UI。
    /// </summary>
    public void Setup(DiceData diceData, int rawRoll)
    {
        RawValue = rawRoll;
        FinalValue = diceData.ApplyAbility(rawRoll);

        _diceTypeLabel.Text = diceData.Name;
        _diceNode.SetValue(rawRoll);

        if (FinalValue != RawValue)
            _modifiedValueLabel.Text = $"→ {FinalValue}";
        else
            _modifiedValueLabel.Visible = false;
    }

    /// <summary>
    /// 將內部 Dice 標示為條件骰（灰色）。
    /// </summary>
    public void SetToConditionType() => _diceNode.SetToConditionType();

    /// <summary>
    /// 將內部 Dice 標示為得分骰（醒目色）。
    /// </summary>
    public void SetToScoreType() => _diceNode.SetToScoreType();
}
