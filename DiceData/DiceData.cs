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
}
