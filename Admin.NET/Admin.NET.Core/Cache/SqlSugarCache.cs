﻿// 麻省理工学院许可证
//
// 版权所有 (c) 2021-2023 zuohuaijun，大名科技（天津）有限公司  联系电话/微信：18020030720  QQ：515096995
//
// 特此免费授予获得本软件的任何人以处理本软件的权利，但须遵守以下条件：在所有副本或重要部分的软件中必须包括上述版权声明和本许可声明。
//
// 软件按“原样”提供，不提供任何形式的明示或暗示的保证，包括但不限于对适销性、适用性和非侵权的保证。
// 在任何情况下，作者或版权持有人均不对任何索赔、损害或其他责任负责，无论是因合同、侵权或其他方式引起的，与软件或其使用或其他交易有关。

namespace Admin.NET.Core;

/// <summary>
/// SqlSugar二级缓存
/// </summary>
public class SqlSugarCache : ICacheService
{
    /// <summary>
    /// 内存缓存
    /// </summary>
    //private static readonly ICache _cache = App.GetService(typeof(ICache)) as ICache;
    private static readonly ICache _cache = Cache.Default;

    public void Add<V>(string key, V value)
    {
        _cache.Set(key, value);
    }

    public void Add<V>(string key, V value, int cacheDurationInSeconds)
    {
        _cache.Set(key, value, cacheDurationInSeconds);
    }

    public bool ContainsKey<V>(string key)
    {
        return _cache.ContainsKey(key);
    }

    public V Get<V>(string key)
    {
        return _cache.Get<V>(key);
    }

    public IEnumerable<string> GetAllKey<V>()
    {
        return _cache.Keys;
    }

    public V GetOrCreate<V>(string cacheKey, Func<V> create, int cacheDurationInSeconds = int.MaxValue)
    {
        if (!_cache.TryGetValue<V>(cacheKey, out V value))
        {
            value = create();
            _cache.Set(cacheKey, value, cacheDurationInSeconds);
        }
        return value;
    }

    public void Remove<V>(string key)
    {
        _cache.Remove(key);
    }
}