using Admin.NET.Application.Strategy;
using Admin.NET.Application.StrategyMode;

namespace Admin.NET.Application;

/// <summary>
/// 策略调用类
/// </summary>
internal class StrategyClient
{
    InAndOutBoundStrategy strategy;

    public StrategyClient(InAndOutBoundStrategy strategy)
    {
        this.strategy = strategy;
    }

    /// <summary>
    /// 动态实现接口
    /// </summary>
    /// <param name="type">指定参数</param>
    /// <param name="materielnum">物料编号</param>
    /// <returns></returns>
    public InAndOutBoundStrategy ContextInterface(string type, string materielnum)
    {
        switch (type)
        {
            case "INA":
                return new AGVStrategyReturnRecommEndStorage(materielnum);
            default:
                return null;

        }
    }


}
