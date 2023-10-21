using AllDone.Infra.Dispatch;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace AllDone.Infra.Tests.Dispatch;

[TestFixture]
public class OperationDispatchBuilderTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task MinimalPipeline_ExecuteOperationAsync()
    {
        var serviceProvider = BuildServiceProviderWithOperationDispatch(build => {});
        var dispatch = serviceProvider.GetRequiredService<OperationDispatch>();  

        var response = await dispatch.ExecuteOperationAsync<ReqeustOne, ResponseOne>(new ReqeustOne());

        var service = serviceProvider.GetRequiredService<MyService>();
        
        response.ShouldNotBeNull();
        service.Log.ShouldBe(new[] { "MyService.One" });
    }

    private ServiceProvider BuildServiceProviderWithOperationDispatch(Action<IOperationDispatchBuild> build)
    {
        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddSingleton<MyService>();
        serviceCollection.AddOperationDispatch<MyService>(
            (service, map) => {
                map.MapMethod<ReqeustOne, ResponseOne>(service.One);
                map.MapMethod<ReqeustTwo, ResponseTwo>(service.Two);
            },
            build
        );

        var serviceProvider = serviceCollection.BuildServiceProvider();
        return serviceProvider;
    }

    private class MyService
    {
        public Task<ResponseOne> One(ReqeustOne request) 
        {
            Log.Add("MyService.One");
            return Task.FromResult(new ResponseOne());
        }

        public Task<ResponseTwo> Two(ReqeustTwo request) 
        {
            Log.Add("MyService.Two");
            return Task.FromResult(new ResponseTwo());
        }

        public List<string> Log { get; } = new();
    }

    private record ReqeustOne();
    private record ResponseOne();
    private record ReqeustTwo();
    private record ResponseTwo();
}
