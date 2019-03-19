using System;
using System.Linq;
using Autofac;
using Karambolo.Common;
using Prism.Ioc;

namespace GettingStarted.Infrastructure
{
    class AutofacContainerExtension : IContainerExtension<IContainer>
    {
        public ContainerBuilder Builder { get; }

        public IContainer Instance { get; private set; }

        public bool SupportsModules => false;

        public AutofacContainerExtension()
            : this(new ContainerBuilder()) { }

        public AutofacContainerExtension(ContainerBuilder builder)
        {
            Builder = builder;
        }

        public void FinalizeExtension()
        {
            if (Instance == null)
                Instance = Builder.Build();
        }

        public bool IsRegistered(Type type)
        {
            throw new NotSupportedException();
        }

        public bool IsRegistered(Type type, string name)
        {
            throw new NotSupportedException();
        }

        public IContainerRegistry RegisterInstance(Type type, object instance)
        {
            Builder.RegisterInstance(instance).As(type);
            return this;
        }

        public IContainerRegistry RegisterInstance(Type type, object instance, string name)
        {
            Builder.RegisterInstance(instance).Named(name, type);
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type from, Type to)
        {
            Builder.RegisterType(to).As(from).SingleInstance();
            return this;
        }

        public IContainerRegistry RegisterSingleton(Type from, Type to, string name)
        {
            Builder.RegisterType(to).Named(name, from).SingleInstance();
            return this;
        }

        public IContainerRegistry Register(Type from, Type to)
        {
            Builder.RegisterType(to).As(from);
            return this;
        }

        public IContainerRegistry Register(Type from, Type to, string name)
        {
            Builder.RegisterType(to).Named(name, from);
            return this;
        }

        public object Resolve(Type type)
        {
            return Instance.Resolve(type);
        }

        public object Resolve(Type type, params (Type Type, object Instance)[] parameters)
        {
            return Instance.Resolve(type, parameters.Select(param => new TypedParameter(param.Type, param.Instance)));
        }

        public object Resolve(Type type, string name)
        {
            return Instance.ResolveNamed(name, type);
        }

        public object Resolve(Type type, string name, params (Type Type, object Instance)[] parameters)
        {
            return Instance.ResolveNamed(name, type, parameters.Select(param => new TypedParameter(param.Type, param.Instance)));
        }

        public object ResolveViewModelForView(object view, Type viewModelType)
        {
            return Instance.Resolve(viewModelType);
        }
    }
}
