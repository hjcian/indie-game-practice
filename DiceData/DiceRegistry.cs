using System.Collections.Generic;

/// <summary>
/// 遊戲中所有可用骰子的集中管理。
/// RewardWindowManager 從此 Registry 隨機抽取骰子顯示給玩家。
/// </summary>
public static class DiceRegistry
{
    public static readonly BasicDice Basic = new BasicDice();

    public static readonly IReadOnlyList<DiceData> All = new List<DiceData>
    {
        // Basic
        new Add1Dice(),
        new Add2Dice(),
        new Add3Dice(),
        // Probability Skew x2
        new Skew1x2Dice(),
        new Skew2x2Dice(),
        new Skew3x2Dice(),
        new Skew4x2Dice(),
        new Skew5x2Dice(),
        new Skew6x2Dice(),
        // Probability Skew x3
        new Skew1x3Dice(),
        new Skew2x3Dice(),
        new Skew3x3Dice(),
        new Skew4x3Dice(),
        new Skew5x3Dice(),
        new Skew6x3Dice(),
    };
}
