namespace Admin.NET.Application.StrategyMode;

/// <summary>
/// 出入库策略模式
/// </summary>
public abstract class InAndOutBoundStrategy
{
    /// <summary>
    /// 出入库策略
    /// </summary>
    /// <param name="MarterielNum">物料编号</param>
    /// <returns></returns>
    public abstract string AlgorithmInterface(string MarterielNum);
}
