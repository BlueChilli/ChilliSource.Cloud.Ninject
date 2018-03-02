using ChilliSource.Cloud.Core;
using Ninject;
using Ninject.Extensions.ChildKernel;
using Ninject.Extensions.ContextPreservation;
using Ninject.Extensions.NamedScope;
using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Ninject
{
    internal class ScopeContextFactory : IScopeContextFactorySetup, IScopeContextFactory
    {
        readonly string _scopeName;

        ChildKernel _contextKernel;
        List<Type> _singletonTypes;

        internal ScopeContextFactory(IKernel defaultKernel)
        {
            var thisFactory = this;

            _singletonTypes = new List<Type>();
            _scopeName = typeof(ScopeContextFactory).FullName;
            _contextKernel = new ChildKernel(defaultKernel, new NinjectSettings() { AllowNullInjection = true });

            _contextKernel.GetBindings(typeof(IKernel)).ToList().ForEach(b =>
                _contextKernel.RemoveBinding(b));

            _contextKernel.Bind<ScopeValidation>().ToMethod(ctx => new ScopeValidation(thisFactory)).InNamedScope(_scopeName);
            _contextKernel.Bind<InScopeValuesHolder>().ToSelf().InNamedScope(_scopeName);            
        }

        public void RegisterSingletonType(Type type)
        {
            _singletonTypes.Add(type);

            _contextKernel.Bind(type).ToMethod(ctx => ctx.ContextPreservingGet<InScopeValuesHolder>().GetSingletonValue(type)).InNamedScope(_scopeName);
        }

        internal void ValidateSingletonType(Type type)
        {
            if (!_singletonTypes.Contains(type))
                throw new ApplicationException($"Type {type.FullName} not registered as singleton type.");
        }

        public void RegisterServices(ScopeContextHelper.RegisterServices registerServicesAction)
        {
            registerServicesAction(_contextKernel, (syntax) => NamedScopeExtensionMethods.InNamedScope((dynamic)syntax, _scopeName));

            _contextKernel.GetBindings(typeof(IServiceResolver)).ToList().ForEach(b =>
                _contextKernel.RemoveBinding(b));
            _contextKernel.Bind<IServiceResolver>().ToMethod(ctx => new NinjectDependecyResolver(ctx.GetContextPreservingResolutionRoot())).InNamedScope(_scopeName);
        }

        public void Register(Action<IKernel> action)
        {
            action(_contextKernel);
        }

        public IScopeContext CreateScope()
        {
            return new ScopeContext(_contextKernel.CreateNamedScope(_scopeName));
        }

        bool disposed = false;
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            _contextKernel.Dispose();
        }
    }

    internal class ScopeValidation
    {
        ScopeContextFactory _factory;
        public ScopeValidation(ScopeContextFactory factory)
        {
            _factory = factory;
        }

        internal void ValidateSingletonType(Type type)
        {
            _factory.ValidateSingletonType(type);
        }
    }
}
