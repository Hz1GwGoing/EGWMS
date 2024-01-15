using Admin.NET.Application.Strategy;
using Admin.NET.Application.StrategyMode;

namespace Admin.NET.Application;

/// <summary>
/// 策略调用类
/// </summary>
internal class StrategyClient
{
    InAndOutBoundStrategy _strategy;

    public StrategyClient(InAndOutBoundStrategy strategy)
    {
        this._strategy = strategy;
    }

    /// <summary>
    /// 动态实现接口
    /// </summary>
    /// <param name="materielnum">物料编号</param>
    /// <returns></returns>
    public string ContextInterface(string materielnum)
    {
        return _strategy.AlgorithmInterface(materielnum);
    }


}
