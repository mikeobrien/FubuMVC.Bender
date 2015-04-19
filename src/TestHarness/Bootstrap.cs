using FubuMVC.Core;
using FubuMVC.StructureMap3;
using TestHarness;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(Bootstrap), "Start")]

namespace TestHarness
{
    public class Bootstrap
    {
        public static void Start()
        {
            FubuApplication.For<Conventions>()
                .StructureMap().Bootstrap();
        }
    }
}