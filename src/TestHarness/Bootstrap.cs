using FubuMVC.Core;
using FubuMVC.RegexUrlPolicy;
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

    public class Conventions : FubuRegistry
    {
        public Conventions()
        {
            Actions
                .FindBy(x => x
                    .IncludeTypeNamesSuffixed("Handler")
                    .IncludeMethodsPrefixed("Execute")
                    .Applies.ToAssemblyContainingType(GetType()));

            Routes
                .OverrideFolders()
                .UrlPolicy(RegexUrlPolicy.Create(
                    x => x.IgnoreAssemblyNamespace(GetType())
                        .IgnoreClassName()
                        .IgnoreMethodNames("Execute")
                        .ConstrainClassToGetEndingWith("GetHandler")
                        .ConstrainClassToPostEndingWith("PostHandler")
                        .ConstrainClassToPutEndingWith("PutHandler")
                        .ConstrainClassToDeleteEndingWith("DeleteHandler")
                        .ConstrainClassToUpdateEndingWith("UpdateHandler")));
        }
    }
}