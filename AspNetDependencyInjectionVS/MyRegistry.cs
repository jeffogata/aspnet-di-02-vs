namespace AspNetDependencyInjectionVS
{
    using StructureMap;

    public class MyRegistry : Registry
    {
        public MyRegistry()
        {
            Scan(scan =>
            {
                // TheCallingAssembly is not available - see https://github.com/structuremap/structuremap.dnx/issues/5
                scan.AssemblyContainingType<Startup>();
                scan.LookForRegistries();
            });
        }
    }
}