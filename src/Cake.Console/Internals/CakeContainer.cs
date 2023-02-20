using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Cli;
using Cake.Console.CommandApp;
using Cake.Console.HostBuilderBehaviours;
using Cake.Core;
using Cake.Core.Composition;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Core.Modules;
using Cake.DotNetTool.Module;
using Cake.NuGet;
using Microsoft.Extensions.DependencyInjection;
using IBuilder = Cake.Core.Composition.ICakeRegistrationBuilder;

namespace Cake.Console.Internals
{
    internal sealed class CakeContainer : ICakeContainerRegistrar, Spectre.Console.Cli.ITypeRegistrar
    {
        private readonly List<Builder> services;

        public CakeContainer()
        {
            services = new List<Builder>();
            new CoreModule().Register(this);
            new NuGetModule().Register(this);
            new DotNetToolModule().Register(this);

            // Configuration will also contain the arguments
            this.RegisterType<CakeConfigurationProvider>().AsSelf().Singleton();
            RegisterFactory(provider =>
            {
                var cakeArgs = provider.GetRequiredService<ICakeArguments>();
                var argDic = cakeArgs.GetArguments().ToDictionary(a => a.Key, a => a.Value.First());
                return provider.GetRequiredService<CakeConfigurationProvider>().CreateConfiguration(".", argDic);
            }).As<ICakeConfiguration>().Singleton();

            // Logging
            this.RegisterType<CakeReportPrinter>().As<ICakeReportPrinter>().Singleton();
            this.RegisterType<CakeConsole>().As<IConsole>().Singleton();
            this.RegisterType<CakeConsoleLog>().As<ICakeLog>().Singleton();

            // behaviours
            this.RegisterType<WorkingDirectoryBehaviour>().As<IHostBuilderBehaviour>().Singleton();
            this.RegisterType<ToolInstallerBehaviour>().As<IHostBuilderBehaviour>().Singleton();
            this.RegisterType<TaskRegisteringBehaviour>().As<IHostBuilderBehaviour>().Singleton();

            // features
            this.RegisterType<VersionResolver>().As<IVersionResolver>().Singleton();
            this.RegisterType<VersionFeature>().As<ICakeVersionFeature>().Singleton();
            this.RegisterType<InfoFeature>().As<ICakeInfoFeature>().Singleton();
        }

        public IBuilder RegisterInstance<T>(T instance)
            where T : class
        {
            var registration = new Builder(typeof(T), instance);
            services.Add(registration);
            return registration;
        }

        public IBuilder RegisterType(Type type)
        {
            var registration = new Builder(type);
            services.Add(registration);
            return registration.Transient();
        }

        public IBuilder RegisterFactory(Func<IServiceProvider, object> factory)
        {
            var registration = new Builder(typeof(object), factory);
            services.Add(registration);
            return registration;
        }

        public IServiceCollection AsServiceCollection()
        {
            IServiceCollection collection = new ServiceCollection();
            foreach (var serv in services)
            {
                var description = serv.BuildServiceDescriptor();
                collection.Add(description);
            }

            return collection;
        }

        void Spectre.Console.Cli.ITypeRegistrar.Register(Type service, Type implementation)
            => RegisterType(implementation).As(service).Singleton();

        void Spectre.Console.Cli.ITypeRegistrar.RegisterInstance(Type service, object implementation)
            => RegisterInstance(implementation).As(service).Singleton();

        void Spectre.Console.Cli.ITypeRegistrar.RegisterLazy(Type service, Func<object> factory)
            => RegisterFactory(_ => factory()).As(service).Singleton();

        Spectre.Console.Cli.ITypeResolver Spectre.Console.Cli.ITypeRegistrar.Build()
            => new TypeResolver(AsServiceCollection().BuildServiceProvider());

        internal class Builder : IBuilder
        {
            private readonly Type implementationType;
            private readonly object? implementationInstance;
            private Type serviceType;
            private ServiceLifetime lifetime;

            public Builder(Type impl, object? instance = null)
            {
                implementationType = impl;
                implementationInstance = instance;
                serviceType = impl;
                lifetime = ServiceLifetime.Singleton;
            }

            public IBuilder As(Type type) { serviceType = type; return this; }

            public IBuilder AsSelf() => As(implementationType);

            public IBuilder Singleton() { lifetime = ServiceLifetime.Singleton; return this; }

            public IBuilder Transient() { lifetime = ServiceLifetime.Transient; return this; }

            public ServiceDescriptor BuildServiceDescriptor()
            {
                return implementationInstance switch
                {
                    Func<IServiceProvider, object> factory => ServiceDescriptor.Describe(serviceType, factory, lifetime),
                    object obj => ServiceDescriptor.Describe(serviceType, _ => obj, lifetime),
                    null => ServiceDescriptor.Describe(serviceType, implementationType, lifetime)
                };
            }
        }
    }
}
