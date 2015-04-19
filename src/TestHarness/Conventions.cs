using FubuMVC.Bender;
using FubuMVC.Core;
using FubuMVC.RegexUrlPolicy;

namespace TestHarness
{
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

            Import<FubuBender>(x => x
                .Bindings(y => y.BindCookies().BindFiles())
                .Configure(y => y.UseJsonCamelCaseNaming()));
        }
    }
}