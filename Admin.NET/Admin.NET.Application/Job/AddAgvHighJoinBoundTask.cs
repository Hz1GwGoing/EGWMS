namespace Admin.NET.Application.Job;
internal class AddAgvHighJoinBoundTask : IJob
{
    #region 关系注入
    private readonly IServiceProvider _serviceProvider;
    #endregion

    #region 构造函数
    public AddAgvHighJoinBoundTask(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    #endregion

    public Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using var serviceScope = _serviceProvider.CreateScope();

        // 查询库位上是否有料箱

        




        throw new NotImplementedException();


    }
}
