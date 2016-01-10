namespace AspNetDependencyInjectionVS
{
    using System;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Http;
    using Microsoft.Extensions.DependencyInjection;
    using StructureMap;

    public class Startup
    {
        public static Guid InstanceId = Guid.NewGuid();

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // see https://github.com/structuremap/structuremap.dnx

            var container = new Container(c => c.AddRegistry<MyRegistry>());

            container.Populate(services);

            return container.GetInstance<IServiceProvider>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseIISPlatformHandler();
            
            app.Run(async context =>
            {
                var singleton1 = context.RequestServices.GetService<ISingleton>();
                var singleton2 = context.RequestServices.GetService<ISingleton>();
                var scoped1 = context.RequestServices.GetService<IScoped>();
                var scoped2 = context.RequestServices.GetService<IScoped>();
                var transient1 = context.RequestServices.GetService<ITransient>();
                var transient2 = context.RequestServices.GetService<ITransient>();
                var instance1 = context.RequestServices.GetService<IInstance>();
                var instance2 = context.RequestServices.GetService<IInstance>();
                
                await context.Response.WriteAsync(
                    "<table>" +
                    $"<tr><td>Singleton 1 and Singleton 2 are the same instance:</td><td> {object.ReferenceEquals(singleton1, singleton2)}</td></tr>" +
                    $"<tr><td>Instance 1 and Instance 2 are the same instance:</td><td> {object.ReferenceEquals(instance1, instance2)}</td></tr>" +
                    $"<tr><td>Scoped 1 and Scoped 2 are the same instance:</td><td> {object.ReferenceEquals(scoped1, scoped2)}</td></tr>" +
                    $"<tr><td>Transient 1 and Transient 2 are the same instance:</td><td> {object.ReferenceEquals(transient1, transient2)}</td></tr>" +
                    "</table><br><br><table>" +
                    $"<tr><td>Singleton Id:</td><td> {singleton1.Id}</td></tr>" +
                    $"<tr><td>Instance Id:</td><td> {instance1.Id}</td></tr>" +
                    $"<tr><td>_instanceId:</td><td> {InstanceId}</td></tr>" +
                    $"<tr><td>Scoped Id:</td><td> {scoped1.Id}</td></tr>" +
                    $"<tr><td>Transient 1 Id:</td><td> {transient1.Id}</td></tr>" +
                    $"<tr><td>Transient 2 Id:</td><td> {transient2.Id}</td></tr>" +
                    "</table>");
            });
        }
    }

    public interface IService
    {
        Guid Id { get; }
    }

    public interface ISingleton : IService
    {
    }

    public interface IScoped : IService
    {
    }

    public interface ITransient : IService
    {
    }

    public interface IInstance : IService
    {
    }

    public class MyService : ISingleton, ITransient, IScoped
    {
        public Guid Id { get; }

        public MyService()
        {
            Id = Guid.NewGuid();
        }
    }

    public class MyServiceInstance : IInstance
    {
        public Guid Id { get; }

        public MyServiceInstance(Guid id)
        {
            Id = id;
        }
    }

    public class MyServiceRegistry : Registry
    {
        // see http://structuremap.github.io/object-lifecycle/supported-lifecycles/
        public MyServiceRegistry()
        {
            For<ISingleton>().Singleton().Use<MyService>();
            For<IScoped>().ContainerScoped().Use<MyService>();
            For<ITransient>().AlwaysUnique().Use<MyService>();
        }
    }

    public class MyInstanceRegistry : Registry
    {
        public MyInstanceRegistry()
        {
            For<IInstance>().Use(new MyServiceInstance(Startup.InstanceId));
        }
    }
}