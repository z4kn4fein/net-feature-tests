﻿using System;
using System.Collections.Generic;
using System.Linq;

using LinFu.IoC;
using LinFu.IoC.Configuration;
using LinFu.IoC.Interfaces;

namespace IoC.Framework.Feature.Tests.Adapters {
    // thanks a lot to Philip Laureano for this adapter
    public class LinFuAdapter : FrameworkAdapterBase {
        private readonly IServiceContainer _container;

        public LinFuAdapter() {
            _container = new ServiceContainer();
            _container.LoadFrom(AppDomain.CurrentDomain.BaseDirectory, "LinFu*.dll");
        }

        public override void AddSingleton(Type serviceType, Type componentType, string key) {
            this.AddService(serviceType, componentType, key, LifecycleType.Singleton);
        }

        public override void AddTransient(Type serviceType, Type componentType, string key) {
            // ashmind: Specifying OncePerRequest explicitly breaks MustHave.PropertyDependency test,
            // which is either a bug or my misunderstanding on how things work.
            // On the other hand, if I do not specify the lifestyle explicitly, 
            // _container.AddService(key, serviceType, componentType) matches instance registration
            // overload with componentType as an serviceInstance.
            this.AddService(serviceType, componentType, key, LifecycleType.OncePerRequest);
        }

        private void AddService(Type serviceType, Type componentType, string key, LifecycleType lifecycle) {
            if (key == null) {
                _container.AddService(serviceType, componentType, lifecycle);
                return;
            }

            _container.AddService(key, serviceType, componentType, lifecycle);
        }

        public override void AddInstance(Type serviceType, object instance, string key) {
            if (key == null) {
                _container.AddService(serviceType, instance);
                return;
            }

            _container.AddService(key, serviceType, instance);
        }

        protected override object DoGetInstance(Type serviceType, string key) {
            if (key == null)
                return _container.GetService(serviceType);

            return _container.GetService(key, serviceType);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType) {
            return _container.GetServices(info => serviceType.IsAssignableFrom(info.ServiceType))
                             .Select(info => info.Object);
        }

        public override object CreateInstance(Type componentType) {
            return _container.AutoCreate(componentType);
        }
    }
}