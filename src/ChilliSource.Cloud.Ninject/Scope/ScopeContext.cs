using ChilliSource.Cloud.Core;
using Ninject;
using Ninject.Extensions.NamedScope;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliSource.Cloud.Ninject
{
    internal class ScopeContext : IScopeContext
    {
        NamedScope _scope;
        IServiceResolver _resolver;

        public ScopeContext(NamedScope scope)
        {
            _scope = scope;
            _resolver = scope.Get<IServiceResolver>();
        }

        public T Get<T>()
        {
            return _resolver.Get<T>();
        }

        public T GetSingletonValue<T>()
        {
            return (T)_resolver.Get<InScopeValuesHolder>().GetSingletonValue(typeof(T));
        }

        public void SetSingletonValue<T>(T value)
        {
            _resolver.Get<InScopeValuesHolder>().SetSingletonValue(typeof(T), value);
        }

        bool disposed = false;
        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            _scope.Dispose();
        }
    }
}
