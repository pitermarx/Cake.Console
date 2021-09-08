using System;
using System.Collections.Generic;
using Cake.Core;
using Cake.Core.Composition;
using Cake.Core.Configuration;
using Cake.Core.Diagnostics;
using Cake.Core.Modules;
using Cake.NuGet;
using Microsoft.Extensions.DependencyInjection;

namespace Cake.Console.Internals
{
    internal sealed class CakeContainer : ICakeContainerRegistrar
    {
        private readonly List<Builder> services;

        public CakeContainer()
        {
            services = new List<Builder>();
            new CoreModule().Register(this);
            new NuGetModule().Register(this);

            this.RegisterType<CakeConfigurationProvider>().AsSelf().Singleton();

            // Logging
            this.RegisterType<CakeReportPrinter>().As<ICakeReportPrinter>().Singleton();
            this.RegisterType<CakeConsole>().As<IConsole>().Singleton();
            this.RegisterType<CakeConsoleLog>().As<ICakeLog>().Singleton();
        }

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

        public IServiceCollection BuildServiceCollection()
        {
            IServiceCollection collection = new ServiceCollection();
            foreach (var serv in services)
            {
                var description = serv.BuildServiceDescriptor();
                collection.Add(description);
            }

            return collection;
        }

        internal sealed class Builder : ICakeRegistrationBuilder
        {
            private Type impl;
            private object instance;
            private Type type;
            private ServiceLifetime lifetime = ServiceLifetime.Singleton;

            ICakeRegistrationBuilder ICakeRegistrationBuilder.As(Type type) { this.type = type; return this; }

            ICakeRegistrationBuilder ICakeRegistrationBuilder.AsSelf() { type = impl; return this; }

            ICakeRegistrationBuilder ICakeRegistrationBuilder.Singleton() { lifetime = ServiceLifetime.Singleton; return this; }

            ICakeRegistrationBuilder ICakeRegistrationBuilder.Transient() { lifetime = ServiceLifetime.Transient; return this; }

            public static Builder Transient(Type t) => new() { impl = t, type = t, lifetime = ServiceLifetime.Transient };

            public static Builder Singleton<T>() => new() { impl = typeof(T), type = typeof(T) };

            public static Builder Singleton<T, TImpl>() => new() { impl = typeof(TImpl), type = typeof(T) };

            public static Builder Singleton<T>(T value) => new() { impl = typeof(T), type = typeof(T), instance = value };

            public ServiceDescriptor BuildServiceDescriptor()
            {
                if (instance != null) return ServiceDescriptor.Describe(type, f => instance, lifetime);

                return ServiceDescriptor.Describe(type, impl, lifetime);
            }
        }
    }
}
