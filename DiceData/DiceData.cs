/// <summary>
/// 所有骰子資料的抽象基底類別。
/// 子類別需實作 Name 與各語系的 Description。
/// 未來獨特的骰子行為也在子類別中 override。
/// </summary>
public abstract class DiceData
{
    public abstract string Name { get; }
    public abstract string DescriptionEn { get; }
    public abstract string DescriptionZhTw { get; }

    /// <summary>
    /// 擲出此骰子並回傳結果數值。
    /// 每個子類別實作自己的擲骰行為。
    /// </summary>
    public abstract int Roll();

    /// <summary>
    /// 將骰子能力套用到原始擲骰值，回傳最終結果。
    /// 預設直接回傳 rawRoll，子類別可覆寫。
    /// </summary>
    public virtual int ApplyAbility(int rawRoll) => rawRoll;

    /// <summary>
    /// 從加權面池中隨機抽取一個結果。
    /// 例如 pool = [1,1,2,3,4,5,6] 代表 1 出現機率為 2/7。
    /// </summary>
    protected static int RollFromPool(int[] pool)
    {
        int index = Godot.GD.RandRange(0, pool.Length - 1);
        return pool[index];
    }
}
