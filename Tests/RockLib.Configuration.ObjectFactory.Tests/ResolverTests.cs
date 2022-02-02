using RockLib.Configuration.ObjectFactory;
using System.Reflection;
using Xunit;

namespace Tests
{
   public static class ResolverTests
   {
#pragma warning disable CA1034 // Nested types should not be visible
      public class WhenCanResolvePropertyReturnsTrue
      {
         [Fact]
         public void CanResolveInterfaceMethodReturnsTrue()
         {
            var bar = new Bar();
            IResolver resolver = new Resolver(t => bar, t => true);
            Assert.True(resolver.CanResolve(BarParameter));
         }

         public class GivenResolvePropertyReturnsNonNull
         {
            [Fact]
            public void TryResolveInterfaceMethodReturnsTrue()
            {
               var bar = new Bar();
               IResolver resolver = new Resolver(t => bar, t => true);
               Assert.True(resolver.TryResolve(BarParameter, out var dummy));
            }

            [Fact]
            public void TryResolveInterfaceMethodAssignsTheValueToTheOutParameter()
            {
               var bar = new Bar();
               IResolver resolver = new Resolver(t => bar, t => true);
               resolver.TryResolve(BarParameter, out var resolved);
               Assert.Same(bar, resolved);
            }
         }

         public class GivenResolvePropertyReturnsNull
         {
            [Fact]
            public void TryResolveInterfaceMethodReturnsFalse()
            {
               IResolver resolver = new Resolver(t => null!, t => true);
               Assert.False(resolver.TryResolve(BarParameter, out var dummy));
            }

            [Fact]
            public void TryResolveInterfaceMethodAssignsNullToTheOutParameter()
            {
               IResolver resolver = new Resolver(t => null!, t => true);
               resolver.TryResolve(BarParameter, out var resolved);
               Assert.Null(resolved);
            }
         }
      }

      public class WhenCanResolvePropertyReturnsFalse
      {
         [Fact]
         public void CanResolveInterfaceMethodReturnsFalse()
         {
            var bar = new Bar();
            IResolver resolver = new Resolver(t => bar, t => false);
            Assert.False(resolver.CanResolve(BarParameter));
         }

         [Fact]
         public void TryResolveInterfaceMethodReturnsFalse()
         {
            var bar = new Bar();
            IResolver resolver = new Resolver(t => bar, t => false);
            Assert.False(resolver.TryResolve(BarParameter, out var dummy));
         }

         [Fact]
         public void TryResolveInterfaceMethodAssignsNullToTheOutParameter()
         {
            var bar = new Bar();
            IResolver resolver = new Resolver(t => bar, t => false);
            resolver.TryResolve(BarParameter, out var resolved);
            Assert.Null(resolved);
         }
      }

      public class Empty
      {
         [Fact]
         public void CanResolveReturnsFalse()
         {
            Assert.False(Resolver.Empty.CanResolve(BarParameter));
         }

         [Fact]
         public void ResolveReturnsNull()
         {
            Assert.Null(Resolver.Empty.Resolve(BarParameter));
         }
      }

      public static class Constructor1
      {
         public class WhenResolveReturnsNonNull
         {
            [Fact]
            public void CanResolveReturnsTrue()
            {
               var bar = new Bar();
               var resolver = new Resolver(t => bar);
               Assert.True(resolver.CanResolve(BarParameter));
            }

            [Fact]
            public void ResolveReturnsNonNull()
            {
               var bar = new Bar();
               var resolver = new Resolver(t => bar);
               Assert.NotNull(resolver.Resolve(BarParameter));
            }
         }

         public class WhenResolveReturnsNull
         {
            [Fact]
            public void CanResolveReturnsFalse()
            {
               var resolver = new Resolver(t => null!);
               Assert.False(resolver.CanResolve(BarParameter));
            }

            [Fact]
            public void ResolveReturnsNull()
            {
               Resolver resolver = new Resolver(t => null!);
               Assert.Null(resolver.Resolve(BarParameter));
            }
         }
      }

      public static class Constructor2
      {
         public class WhenResolveReturnsNonNull
         {
            [Fact]
            public void ResolveReturnsTheValue()
            {
               var bar = new Bar();
               var resolver = new Resolver(t => bar, t => true);
               Assert.Same(bar, resolver.Resolve(BarParameter));
            }
         }

         public class WhenResolveReturnsNull
         {
            [Fact]
            public void ResolveReturnsNull()
            {
               Resolver resolver = new Resolver(t => null!, t => false);
               Assert.Null(resolver.Resolve(BarParameter));
            }
         }

         public class WhenCanResolveReturnsTrue
         {
            [Fact]
            public void CanResolveReturnsTrue()
            {
               var bar = new Bar();
               var resolver = new Resolver(t => bar, t => true);
               Assert.True(resolver.CanResolve(BarParameter));
            }
         }

         public class WhenCanResolveReturnsFalse
         {
            [Fact]
            public void CanResolveReturnsFalse()
            {
               var resolver = new Resolver(t => null!, t => false);
               Assert.False(resolver.CanResolve(BarParameter));
            }
         }
      }

      public static  class Constructor3
      {
         public class WhenResolveNamedReturnsNonNull
         {
            [Fact]
            public void ResolveReturnsTheValue()
            {
               var bar = new Bar();
               var wrongBar = new Bar();
               var resolver = new Resolver(t => wrongBar, (t, n) => bar);
               Assert.Same(bar, resolver.Resolve(BarParameter));
            }

            [Fact]
            public void CanResolveReturnsTrue()
            {
               var bar = new Bar();
               var resolver = new Resolver(t => null!, (t, n) => bar);
               Assert.True(resolver.CanResolve(BarParameter));
            }
         }

         public class WhenResolveNamedReturnsNullAndResolveReturnsNonNull
         {
            [Fact]
            public void ResolveReturnsTheValue()
            {
               var bar = new Bar();
               var resolver = new Resolver(t => bar, (t, n) => null!);
               Assert.Same(bar, resolver.Resolve(BarParameter));
            }

            [Fact]
            public void CanResolveReturnsTrue()
            {
               var bar = new Bar();
               var resolver = new Resolver(t => bar, (t, n) => null!);
               Assert.True(resolver.CanResolve(BarParameter));
            }
         }

         public class WhenResolveNamedReturnsNullAndResolveReturnsNull
         {
            [Fact]
            public void ResolveReturnsNull()
            {
               var resolver = new Resolver(t => null!, (t, n) => null!);
               Assert.Null(resolver.Resolve(BarParameter));
            }

            [Fact]
            public void CanResolveReturnsFalse()
            {
               var resolver = new Resolver(t => null!, (t, n) => null!);
               Assert.False(resolver.CanResolve(BarParameter));
            }
         }
      }

      private static ParameterInfo BarParameter => typeof(Foo).GetConstructors()[0].GetParameters()[0];

      public class Foo
      {
         public Foo(IBar bar) => Bar = bar;
         public IBar Bar { get; }
      }

#pragma warning disable CA1040 // Avoid empty interfaces
      public interface IBar { }
#pragma warning restore CA1040 // Avoid empty interfaces

      public class Bar : IBar { }
#pragma warning restore CA1034 // Nested types should not be visible
   }
}
