// x2 Skew Dices

public class Skew1x2Dice : DiceData
{
    public override string Name => "Skew 1: x2";
    public override string DescriptionEn => "Number 1 has double chance to appear.";
    public override string DescriptionZhTw => "數字 1 出現的機率提升為兩倍。";

    public override int Roll() => RollFromPool([1, 1, 2, 3, 4, 5, 6]);
}

public class Skew2x2Dice : DiceData
{
    public override string Name => "Skew 2: x2";
    public override string DescriptionEn => "Number 2 has double chance to appear.";
    public override string DescriptionZhTw => "數字 2 出現的機率提升為兩倍。";

    public override int Roll() => RollFromPool([1, 2, 2, 3, 4, 5, 6]);
}

public class Skew3x2Dice : DiceData
{
    public override string Name => "Skew 3: x2";
    public override string DescriptionEn => "Number 3 has double chance to appear.";
    public override string DescriptionZhTw => "數字 3 出現的機率提升為兩倍。";

    public override int Roll() => RollFromPool([1, 2, 3, 3, 4, 5, 6]);
}

public class Skew4x2Dice : DiceData
{
    public override string Name => "Skew 4: x2";
    public override string DescriptionEn => "Number 4 has double chance to appear.";
    public override string DescriptionZhTw => "數字 4 出現的機率提升為兩倍。";

    public override int Roll() => RollFromPool([1, 2, 3, 4, 4, 5, 6]);
}

public class Skew5x2Dice : DiceData
{
    public override string Name => "Skew 5: x2";
    public override string DescriptionEn => "Number 5 has double chance to appear.";
    public override string DescriptionZhTw => "數字 5 出現的機率提升為兩倍。";

    public override int Roll() => RollFromPool([1, 2, 3, 4, 5, 5, 6]);
}

public class Skew6x2Dice : DiceData
{
    public override string Name => "Skew 6: x2";
    public override string DescriptionEn => "Number 6 has double chance to appear.";
    public override string DescriptionZhTw => "數字 6 出現的機率提升為兩倍。";

    public override int Roll() => RollFromPool([1, 2, 3, 4, 5, 6, 6]);
}

// x3 Skew Dices

public class Skew1x3Dice : DiceData
{
    public override string Name => "Skew 1: x3";
    public override string DescriptionEn => "Number 1 has triple chance to appear.";
    public override string DescriptionZhTw => "數字 1 出現的機率提升為三倍。";

    public override int Roll() => RollFromPool([1, 1, 1, 2, 3, 4, 5, 6]);
}

public class Skew2x3Dice : DiceData
{
    public override string Name => "Skew 2: x3";
    public override string DescriptionEn => "Number 2 has triple chance to appear.";
    public override string DescriptionZhTw => "數字 2 出現的機率提升為三倍。";

    public override int Roll() => RollFromPool([1, 2, 2, 2, 3, 4, 5, 6]);
}

public class Skew3x3Dice : DiceData
{
    public override string Name => "Skew 3: x3";
    public override string DescriptionEn => "Number 3 has triple chance to appear.";
    public override string DescriptionZhTw => "數字 3 出現的機率提升為三倍。";

    public override int Roll() => RollFromPool([1, 2, 3, 3, 3, 4, 5, 6]);
}

public class Skew4x3Dice : DiceData
{
    public override string Name => "Skew 4: x3";
    public override string DescriptionEn => "Number 4 has triple chance to appear.";
    public override string DescriptionZhTw => "數字 4 出現的機率提升為三倍。";

    public override int Roll() => RollFromPool([1, 2, 3, 4, 4, 4, 5, 6]);
}

public class Skew5x3Dice : DiceData
{
    public override string Name => "Skew 5: x3";
    public override string DescriptionEn => "Number 5 has triple chance to appear.";
    public override string DescriptionZhTw => "數字 5 出現的機率提升為三倍。";

    public override int Roll() => RollFromPool([1, 2, 3, 4, 5, 5, 5, 6]);
}

public class Skew6x3Dice : DiceData
{
    public override string Name => "Skew 6: x3";
    public override string DescriptionEn => "Number 6 has triple chance to appear.";
    public override string DescriptionZhTw => "數字 6 出現的機率提升為三倍。";

    public override int Roll() => RollFromPool([1, 2, 3, 4, 5, 6, 6, 6]);
}
