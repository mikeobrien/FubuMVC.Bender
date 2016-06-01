FubuMVC Bender
=============

[![Nuget](http://img.shields.io/nuget/v/FubuMVC.Bender.svg?style=flat)](http://www.nuget.org/packages/FubuMVC.Bender/) [![TeamCity Build Status](https://img.shields.io/teamcity/http/build.mikeobrien.net/s/fububender.svg?style=flat)](http://build.mikeobrien.net/viewType.html?buildTypeId=fububender&guest=1)

This library integrates [Bender](https://github.com/mikeobrien/Bender) with [FubuMVC](http://mvc.fubu-project.org/). 

Installation
------------

    PM> Install-Package FubuMVC.Bender  

Usage
------------

To use Bender with FubuMVC simply import it in your `FubuRegistry`:

```csharp
public class Conventions : FubuRegistry
{
    public Conventions()
    {
        ...
        Import<FubuBender>();
    }
}
```

The DSL allows you to configure the data that is bound to the model after serialization. The default is route parameters only. 

```csharp
Import<FubuBender>(x => x
    .Bindings(y => y.BindCookies().BindFiles()));
```

You can also configure Bender:

```csharp
Import<FubuBender>(x => x
    .Configure(y => y.UseJsonCamelCaseNaming()));
```

License
------------

MIT License