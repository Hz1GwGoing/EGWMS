using Admin.NET.Application.StrategyMode;
namespace Admin.NET.Application.Strategy;

/// <summary>
/// （策略）（密集库）AGV入库WMS自动推荐的库位
/// </summary>
public class AGVStrategyReturnRecommEndStorage : InAndOutBoundStrategy
{
    private readonly SqlSugarRepository<Entity.EG_WMS_Region> _Region = App.GetService<SqlSugarRepository<Entity.EG_WMS_Region>>();
    private readonly SqlSugarRepository<Entity.EG_WMS_Storage> _Storage = App.GetService<SqlSugarRepository<Entity.EG_WMS_Storage>>();

    /// <summary>
    /// （策略）（密集库）AGV入库WMS自动推荐的库位
    /// </summary>
    /// <param name="MarterielNum"></param>
    /// <returns></returns>
    public override string AlgorithmInterface(string MarterielNum)
    { // 根据物料编号，得到这个物料属于那个区域
        var dataRegion = _Region.AsQueryable().Where(x => x.RegionMaterielNum == MarterielNum).ToList();

        if (dataRegion == null || dataRegion.Count == 0)
        {
            throw Oops.Oh("区域未绑定物料");
        }

        for (int k = 0; k < dataRegion.Count; k++)
        {

            #region 用于新区域时，第一次入库推荐使用（可能需要修改）

            // 一开始初始化数据，第一个开始
            var data = _Storage.AsQueryable()
                     .Where(x => x.RegionNum == dataRegion[k].RegionNum && x.StorageOccupy == 0 && x.StorageStatus == 0)
                     .OrderBy(x => x.StorageNum, OrderByType.Desc)
                     .Select(x => new
                     {
                         x.StorageNum
                     })
                     .ToList();

            // 区域库位总数
            int datacount = _Storage.AsQueryable()
                        .Where(x => x.RegionNum == dataRegion[k].RegionNum)
                        .Count();

            if (data.Count == datacount)
            {
                return data[0].StorageNum.ToString();
            }

            #endregion


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

            // 查询库位并且排除不符合条件的组别和库位

            var getStorage = _Storage.AsQueryable()
                     .Where(a => a.StorageStatus == 0 && a.StorageGroup != null
                     && a.StorageOccupy == 0 && a.RegionNum == dataRegion[k].RegionNum && !strings.Contains(a.StorageGroup))
                     .OrderBy(a => a.StorageNum, OrderByType.Desc)
                     .Select(a => new
                     {
                         a.StorageNum,
                         a.StorageGroup,
                     })
                     .ToList();


            // 得到组别
            var getStorageGroup = _Storage.AsQueryable()
                                 .Where(a => a.StorageStatus == 0 && a.StorageGroup != null
                                 && a.StorageOccupy == 0 && a.RegionNum == dataRegion[k].RegionNum && !strings.Contains(a.StorageGroup))
                                 .OrderBy(a => a.StorageNum, OrderByType.Desc)
                                 .Distinct()
                                 .Select(a => new
                                 {
                                     a.StorageGroup,
                                 })
                                 .ToList();


            for (int i = 0; i < getStorageGroup.Count; i++)
            {
                // 查询得到当前组已经占用的库位

                var AlreadyOccupied = _Storage.AsQueryable()
                         .Where(x => x.StorageGroup == getStorageGroup[i].StorageGroup && x.StorageOccupy == 1)
                         .Select(it => new
                         {
                             it.StorageNum,
                         })
                         .ToList();

                // 如果当前组没有占用的库位

                if (AlreadyOccupied.Count == 0)
                {
                    var datalist = _Storage.AsQueryable()
                         .Where(x => x.StorageGroup == getStorageGroup[i].StorageGroup && x.StorageStatus == 0)
                         // TODO、没有验证
                         .OrderBy(x => x.StorageNum, OrderByType.Desc)
                         .Select(it => new
                         {
                             it.StorageNum,
                         })
                         .ToList();

                    return datalist[0].StorageNum;
                }

                // 如果在确实有占用的库位
                if (AlreadyOccupied.Count != 0)
                {
                    // 查询得到当前组别下最末尾一个有占用的库位
                    var allStorageOccupy = _Storage.AsQueryable()
                             .Where(x => x.StorageOccupy == 1 && x.StorageGroup == getStorageGroup[i].StorageGroup)
                             .OrderBy(a => a.StorageNum, OrderByType.Desc)
                             .Select(a => new
                             {
                                 a.StorageNum,
                                 a.StorageGroup,
                             })
                             .ToList()
                             .Last();

                    // 得到这个组别下所有的未占用库位
                    var GetGroupOccupyNot = _Storage.AsQueryable()
                                  .Where(x => x.StorageOccupy == 0 && x.StorageStatus == 0 &&
                                         x.StorageGroup == allStorageOccupy.StorageGroup)
                                  .OrderBy(x => x.StorageNum, OrderByType.Desc)
                                  .Select(it => new
                                  {
                                      it.StorageNum,
                                  })
                                  .ToList();

                    // 依次判断符合条件的数据
                    // 当前组别最后一个被占用的库位编号
                    int lastOccupyNum = allStorageOccupy.StorageNum.ToInt();

                    for (int j = 0; j < GetGroupOccupyNot.Count; j++)
                    {
                        if (GetGroupOccupyNot[j].StorageNum.ToInt() < lastOccupyNum)
                        {
                            return GetGroupOccupyNot[j].StorageNum;
                        }
                    }
                }
                else
                {
                    //throw Oops.Oh("没有合适的库位");
                    return "没有合适的库位";

                }
            }
        }

        // 如果没有则返回错误
        return "没有合适的库位";

        //throw Oops.Oh("没有合适的库位");
    }
}
