using System;
using System.Collections.Generic;
using System.Linq;
using Cake.Console.HostBuilderBehaviours;
using Cake.Core;
using Cake.Core.Composition;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Core.Modules;
using Cake.Core.Scripting;
using Cake.NuGet;
using Microsoft.Extensions.DependencyInjection;

namespace Cake.Console.Internals
{
    internal sealed class CakeContainer : ICakeContainerRegistrar
    {
        private readonly List<Builder> services = new();

        public ICakeRegistrationBuilder RegisterInstance<T>(T instance)
            where T : class
        {
            var registration = Builder.Singleton(instance);
            services.Add(registration);
            return registration;
        }

        public ICakeRegistrationBuilder RegisterType(Type type)
        {
            var registration = Builder.Transient(type);
            services.Add(registration);
            return registration;
        }

        public IServiceProvider Build(string[] args)
        {
            RegisterServices(args);

            IServiceCollection collection = new ServiceCollection();
            foreach (var serv in services)
            {
                collection.Add(serv.BuildServiceDescriptor());
            }
            return collection.BuildServiceProvider();
        }

        private void RegisterServices(string[] args)
        {
            new CoreModule().Register(this);
            new NuGetModule().Register(this);

            services.Add(Builder.Singleton<ICakeReportPrinter, CakeReportPrinter>());
            services.Add(Builder.Singleton<IConsole, CakeConsole>());
            services.Add(Builder.Singleton<ICakeLog>(s =>
            {
                var v = s.GetService<ICakeConfiguration>().GetValue("verbosity");
                var verbosity = Enum.TryParse<Verbosity>(v, true, out var verb)
                    ? verb
                    : Verbosity.Normal;

                var console = s.GetService<IConsole>();
                return new CakeBuildLog(console, verbosity);
            }));

            // naive arg parsing
            var dic = args
                .Select(a => a.Replace("-", string.Empty).Split("="))
                .ToDictionary(
                    pair => pair[0],
                    pair => pair.Length > 1 ? pair[1] : "true");

            services.Add(Builder.Singleton<CakeConfigurationProvider>());
            services.Add(Builder.Singleton<ICakeArguments>(new CakeArguments(dic.ToLookup(v => v.Key, v => v.Value))));
            services.Add(Builder.Singleton(s => s.GetService<CakeConfigurationProvider>().CreateConfiguration(".", dic)));

            services.Add(Builder.Singleton<IScriptHost, CakeHost>());

            services.Add(Builder.Singleton<IHostBuilderBehaviour, WorkingDirectoryBehaviour>());
            services.Add(Builder.Singleton<IHostBuilderBehaviour, ToolInstallerBehaviour>());
            services.Add(Builder.Singleton<IHostBuilderBehaviour, TaskRegisteringBehaviour>());
        }

        internal sealed class Builder : ICakeRegistrationBuilder
        {
            private Type impl;
            private Func<IServiceProvider, object> factory;
            private object instance;
            private Type type;
            private ServiceLifetime lifetime = ServiceLifetime.Singleton;

            ICakeRegistrationBuilder ICakeRegistrationBuilder.As(Type type)
            {
                this.type = type;
                return this;
            }

            ICakeRegistrationBuilder ICakeRegistrationBuilder.AsSelf()
            {
                type = impl;
                return this;
            }

            ICakeRegistrationBuilder ICakeRegistrationBuilder.Singleton()
            {
                lifetime = ServiceLifetime.Singleton;
                return this;
            }

            ICakeRegistrationBuilder ICakeRegistrationBuilder.Transient()
            {
                lifetime = ServiceLifetime.Transient;
                return this;
            }

            public static Builder Transient(Type t) => new() { impl = t, type = t, lifetime = ServiceLifetime.Transient };

            public static Builder Singleton<T>() => new() { impl = typeof(T), type = typeof(T) };

            public static Builder Singleton<T, TImpl>() => new() { impl = typeof(TImpl), type = typeof(T) };

            public static Builder Singleton<T>(T value) => new() { impl = typeof(T), type = typeof(T), instance = value };

            public static Builder Singleton<T>(Func<IServiceProvider, T> value) => new() { impl = typeof(T), type = typeof(T), factory = s => value(s) };


            public ServiceDescriptor BuildServiceDescriptor()
            {
                if (instance != null) return ServiceDescriptor.Describe(type, f => instance, lifetime);

                if (factory != null) return ServiceDescriptor.Describe(type, factory, lifetime);

                return ServiceDescriptor.Describe(type, impl, lifetime);
            }
        }
    }
}
