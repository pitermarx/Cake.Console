using System;
using Cake.Core.Composition;
using Cake.Core.Packaging;

namespace Cake.Console
{
    public static class CakeHostBuilderExtensions
    {
        public static CakeHostBuilder WithNugetTool(this CakeHostBuilder builder, string id, string version)
        {
            builder.WithTool(new PackageReference(new Uri($"nuget:?package={id}&version={version}")));
            return builder;
        }

        public static CakeHostBuilder RegisterTasks<T>(this CakeHostBuilder builder) where T : ICakeTasks
        {
            builder.Configure(s => s.RegisterType(typeof(T)).As<ICakeTasks>());
            return builder;
        }

        public static void Singleton<T>(this ICakeContainerRegistrar registrar)
        {
            registrar.RegisterType(typeof(T)).AsSelf().Singleton();
        }
    }
}