public class BasicDice : DiceData
{
    public override string Name => "Basic";
    public override string DescriptionEn => "A standard six-sided dice.";
    public override string DescriptionZhTw => "標準六面骰子。";
}

public class Add1Dice : DiceData
{
    public override string Name => "Add 1";
    public override string DescriptionEn => "After this dice is rolled, add 1 to this dice.";
    public override string DescriptionZhTw => "擲出此骰子後，點數 +1。";
}

public class Add2Dice : DiceData
{
    public override string Name => "Add 2";
    public override string DescriptionEn => "After this dice is rolled, add 2 to this dice.";
    public override string DescriptionZhTw => "擲出此骰子後，點數 +2。";
}

public class Add3Dice : DiceData
{
    public override string Name => "Add 3";
    public override string DescriptionEn => "After this dice is rolled, add 3 to this dice.";
    public override string DescriptionZhTw => "擲出此骰子後，點數 +3。";
}
