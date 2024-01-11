using Admin.NET.Application.StrategyMode;
namespace Admin.NET.Application.Strategy;

/// <summary>
/// （策略）（密集库）AGV出库WMS自动推荐的库位（判断生产日期）
/// </summary>
public class AGVStrategyReturnRecommendStorageOutBoundJudgeTime : InAndOutBoundStrategy
{

    private readonly SqlSugarRepository<Entity.EG_WMS_Region> _Region = App.GetService<SqlSugarRepository<Entity.EG_WMS_Region>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_Storage> _Storage = App.GetService<SqlSugarRepository<Entity.EG_WMS_Storage>>();

    /// <summary>
    /// （策略）（密集库）AGV出库WMS自动推荐的库位（判断生产日期）
    /// </summary>
    /// <param name="MarterielNum">物料编号</param>
    /// <returns></returns>
    public override string AlgorithmInterface(string MarterielNum)
    {
        // 根据物料编号，得到这个物料属于那个区域
        var dataRegion = _Region.AsQueryable().Where(x => x.RegionMaterielNum == MarterielNum).ToList();
        // 用于保存每个区域里面的数据
        List<string> datastring = new List<string>();
        if (dataRegion == null || dataRegion.Count == 0)
        {
            throw Oops.Oh("区域未绑定物料");
        }
        for (int k = 0; k < dataRegion.Count; k++)
        {
            // 查询是否有正在进行中的任务库位的组别

            var dataStorageGroup = _Storage.AsQueryable()
                       .Where(a => a.TaskNo != null && a.RegionNum == dataRegion[k].RegionNum)
                       .Distinct()
                       .Select(a => new
                       {
                           a.StorageGroup,
                       })
                       .ToList();

            // 将有任务的组别保存
            string[] strings = new string[dataStorageGroup.Count];
            for (int i = 0; i < dataStorageGroup.Count; i++)
            {
                strings[i] = dataStorageGroup[i].StorageGroup;
            }

            // 查询所有的组别（排除不符合条件的组别）
            var getGroup = _Storage.AsQueryable()
                            .Where(x => !strings.Contains(x.StorageGroup) && x.StorageType == 0 &&
                                   x.StorageGroup != null && x.RegionNum == dataRegion[k].RegionNum)
                            .Distinct()
                            .Select(x => x.StorageGroup)
                            .ToList();

            // 如果这一组的最后一个的时间还没有达到，则这一组都用不了 
            string[] stringss = new string[getGroup.Count];
            var storagenum = new Entity.EG_WMS_Storage();
            for (int i = 0; i < getGroup.Count; i++)
            {
                var notStorageGroup = _Storage.AsQueryable()
                                     .Where(x => x.StorageGroup == getGroup[i] && x.StorageOccupy == 1)
                                     .OrderBy(x => x.StorageNum, OrderByType.Desc)
                                     .ToList();

                if (notStorageGroup.Count != 0)
                {
                    storagenum = notStorageGroup.Last();
                }

                // 这个组的最后一条数据不符合条件，把这个组别保存下来
                if (storagenum.StorageProductionDate.ToDateTime().AddHours(48) > DateTime.Now)
                {
                    stringss[i] = storagenum.StorageGroup.ToString();
                }

            }

            // 查询库位并且排除不符合条件的组别和库位

            var getStorage = _Storage.AsQueryable()
                     .Where(a => a.StorageStatus == 0 && a.StorageGroup != null
                     && a.StorageOccupy == 1 && a.RegionNum == dataRegion[k].RegionNum &&
                     !strings.Contains(a.StorageGroup) && !stringss.Contains(a.StorageGroup) &&
                     a.StorageProductionDate.ToDateTime().AddHours(48) < DateTime.Now)
                     .OrderBy(a => a.StorageNum, OrderByType.Asc)
                     .Select(a => new
                     {
                         a.StorageNum,
                         a.StorageGroup,
                     })
                     .ToList();

            // 将每个区域里面符合条件的库位保存
            foreach (var item in getStorage)
            {
                datastring.Add(item.StorageNum);
            }

        }

        // 将得到的库位重新进行排序，让最小编号的库位在前面
        List<int> dataInt = new List<int>();
        foreach (string s in datastring)
        {
            dataInt.Add(int.Parse(s));
        }
        dataInt.Sort();
        if (dataInt == null || dataInt.Count == 0)
        {
            //throw Oops.Oh("没有合适的库位");
            return "没有合适的库位";
        }

        return dataInt[0].ToString();

    }
}
