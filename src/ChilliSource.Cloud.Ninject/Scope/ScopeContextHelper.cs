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
    /// <summary>
    /// Allows the creation of a scope context Factory
    /// </summary>
    public static class ScopeContextHelper
    {
        /// <summary>
        /// Delegate to bind services in the kernel for a specific scopeAction. <br/>
        ///  e.g. kernel.Bind&lt;MyServiceA&gt;().ToSelf().InScopeAction(scopeAction);
        /// </summary>
        /// <param name="kernel">A kernel</param>
        /// <param name="scopeAction">A scope action</param>               
        public delegate void RegisterServices(IKernel kernel, Action<IBindingSyntax> scopeAction);

        /// <summary>
        /// Creates a scope context factory
        /// </summary>
        /// <param name="defaultKernel">A default kernel</param>
        /// <param name="kernelBinder">A kernel binder delegate</param>
        /// <returns>Returns a scope context factory</returns>
        public static IScopeContextFactory CreateFactory(IKernel defaultKernel, Action<IScopeContextFactorySetup> setupAction)
        {
            var factory = new ScopeContextFactory(defaultKernel);
            setupAction(factory);

            return factory;
        }
    }
}
