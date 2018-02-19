﻿using ChilliSource.Cloud.Core;
using Ninject;
using Ninject.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ChilliSource.Cloud.Ninject.Tests
{
    public class ScopeContextTests : IDisposable
    {
        IKernel standardKernel;
        IScopeContextFactory scopeContextFactory;

        public ScopeContextTests()
        {
            standardKernel = new StandardKernel();
            RegisterServices(standardKernel, null);

            scopeContextFactory = ScopeContextHelper.CreateFactory(standardKernel, FactorySetup);
        }

        private void FactorySetup(IScopeContextFactorySetup binder)
        {
            binder.RegisterServices(RegisterServices);

            binder.RegisterSingletonType(typeof(CustomValue));
        }

        private void RegisterServices(IKernel kernel, Action<IBindingSyntax> scopeAction)
        {
            kernel.Bind<MyServiceA>().ToSelf().InScopeAction(scopeAction);
            kernel.Bind<MyServiceB>().ToSelf().InScopeAction(scopeAction);
        }

        [Fact]
        public void TestScopeContextInstances()
        {
            scopeContextFactory.Execute<MyServiceA>(svc =>
            {
                svc.DoStuff();

                var anotherInstance = svc.Resolver.Get<MyServiceA>();

                Assert.True(Object.ReferenceEquals(svc, anotherInstance));
            });
        }

        [Fact]
        public void TestScopeContextDependency()
        {
            MyServiceB.DoStuffCount = 0;

            scopeContextFactory.Execute<MyServiceA>(svc =>
            {
                svc.DoStuff();
            });

            Assert.True(MyServiceB.DoStuffCount > 0);
        }

        [Fact]
        public void TestScopeContextAutoDispose()
        {
            MyServiceA.DisposedCount = 0;
            MyServiceB.DisposedCount = 0;

            scopeContextFactory.Execute<MyServiceA>(svc =>
            {
                svc.DoStuff();
            });

            Assert.True(MyServiceA.DisposedCount > 0);
            Assert.True(MyServiceB.DisposedCount > 0);
        }

        [Fact]
        public void TestScopeContextSingletonValue()
        {
            scopeContextFactory.Execute<MyServiceA>(svc =>
            {
                Assert.True(svc.CustomValue == null);
            });

            scopeContextFactory.Execute<MyServiceA>(scope => scope.SetSingletonValue<CustomValue>(new CustomValue() { Value = 8 }), svc =>
            {
                Assert.True(svc.CustomValue != null && svc.CustomValue.Value == 8);
            });

            scopeContextFactory.Execute<MyServiceA>(scope => scope.SetSingletonValue<CustomValue>(new CustomValue() { Value = 123 }), svc =>
            {
                Assert.True(svc.CustomValue != null && svc.CustomValue.Value == 123);
            });

            scopeContextFactory.Execute<MyServiceA>(scope => scope.SetSingletonValue<CustomValue>(new CustomValue() { Value = 123 }), svc =>
            {
                Assert.True(svc.CustomValue != null && svc.CustomValue.Value == 123);
            });
        }

        [Fact]
        public void TestScopeContextTask()
        {
            var signal = new ManualResetEvent(false);

            scopeContextFactory.ExecuteAsync<MyServiceA>(scope => scope.SetSingletonValue<CustomValue>(new CustomValue() { Value = 321 }), svc =>
            {
                Assert.True(svc.CustomValue != null && svc.CustomValue.Value == 321);
                signal.Set();
            });

            signal.WaitOne();
        }

        public void Dispose()
        {
            standardKernel.Dispose();
            scopeContextFactory.Dispose();
        }

        public class MyServiceA : IDisposable
        {
            public static int DisposedCount;

            MyServiceB _service2;
            public CustomValue CustomValue { get; private set; }

            [Inject]
            public IServiceResolver Resolver { get; set; }

            public MyServiceA(MyServiceB service2, CustomValue value)
            {
                _service2 = service2;
                CustomValue = value;
            }

            public void DoStuff()
            {
                _service2.DoStuff();
            }

            public void Dispose()
            {
                DisposedCount++;
            }
        }

        public class CustomValue
        {
            public int Value { get; set; }
        }

        public class MyServiceB : IDisposable
        {
            public static int DisposedCount;
            public static int DoStuffCount;

            public void Dispose()
            {
                DisposedCount++;
            }

            internal void DoStuff()
            {
                DoStuffCount++;
            }
        }
    }
}
