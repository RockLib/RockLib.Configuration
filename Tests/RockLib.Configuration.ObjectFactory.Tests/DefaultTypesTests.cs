using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests
{
   public class DefaultTypesTests
   {
      [Fact]
      public void CanAddByDeclaringTypeAndMemberName()
      {
         // Register a default type by declaring type and member name when you need
         // different properties of the same type to be have different default types.

         // Call the Add method for each default type that needs to be registered.
         var first = new DefaultTypes();
         first.Add(typeof(Foo), "bar", typeof(Bar));
         first.Add(typeof(Foo), "baz", typeof(Baz));

         // The Add method returns 'this', so you can chain them together:
         var second = new DefaultTypes()
             .Add(typeof(Foo), "bar", typeof(Bar))
             .Add(typeof(Foo), "baz", typeof(Baz));

         // List initialization syntax works also.
         var third = new DefaultTypes
            {
                { typeof(Foo), "bar", typeof(Bar) },
                { typeof(Foo), "baz", typeof(Baz) }
            };

         // All three instances represent the same thing. Verify that their
         // contents are the same.

         // DefaultTypes implements IEnumerable<KeyValuePair<string, Type>>
         var firstList = first.ToList();
         var secondList = second.ToList();
         var thirdList = third.ToList();

         Assert.Equal(firstList.Count, secondList.Count);
         Assert.Equal(secondList.Count, thirdList.Count);

         for (int i = 0; i < firstList.Count; i++)
         {
            Assert.Equal(firstList[i].Key, secondList[i].Key);
            Assert.Equal(firstList[i].Value, secondList[i].Value);

            Assert.Equal(secondList[i].Key, thirdList[i].Key);
            Assert.Equal(secondList[i].Value, thirdList[i].Value);
         }
      }

      [Fact]
      public void CanAddByTargetType()
      {
         // Register a default type by target type when you want all properties of the
         // target type to have the same default type.

         // Call the Add method for each default type that needs to be registered.
         var first = new DefaultTypes();
         first.Add(typeof(IBar), typeof(Bar));
         first.Add(typeof(IBaz), typeof(Baz));

         // The Add method returns 'this', so you can chain them together:
         var second = new DefaultTypes()
             .Add(typeof(IBar), typeof(Bar))
             .Add(typeof(IBaz), typeof(Baz));

         // List initialization syntax works also.
         var third = new DefaultTypes
            {
                { typeof(IBar), typeof(Bar) },
                { typeof(IBaz), typeof(Baz) }
            };

         // All three instances represent the same thing. Verify that their
         // contents are the same.

         // DefaultTypes implements IEnumerable<KeyValuePair<string, Type>>
         var firstList = first.ToList();
         var secondList = second.ToList();
         var thirdList = third.ToList();

         Assert.Equal(firstList.Count, secondList.Count);
         Assert.Equal(secondList.Count, thirdList.Count);

         for (int i = 0; i < firstList.Count; i++)
         {
            Assert.Equal(firstList[i].Key, secondList[i].Key);
            Assert.Equal(firstList[i].Value, secondList[i].Value);

            Assert.Equal(secondList[i].Key, thirdList[i].Key);
            Assert.Equal(secondList[i].Value, thirdList[i].Value);
         }
      }

      [Fact]
      public void TryGetByDeclaringTypeAndMemberNameReturnsTrueWhenThereIsAMatch()
      {
         var defaultTypes = new DefaultTypes()
             .Add(typeof(Foo), "bar", typeof(Bar))
             .Add(typeof(Foo), "baz", typeof(Baz));

         Assert.True(defaultTypes.TryGet(typeof(Foo), "bar", out var defaultFooBar));
         Assert.Equal(typeof(Bar), defaultFooBar);

         Assert.True(defaultTypes.TryGet(typeof(Foo), "baz", out var defaultFooBaz));
         Assert.Equal(typeof(Baz), defaultFooBaz);
      }

      [Fact]
      public void TryGetByDeclaringTypeAndMemberNameReturnsFalseWhenThereNotIsAMatch()
      {
         var defaultTypes = new DefaultTypes();

         Assert.False(defaultTypes.TryGet(typeof(Foo), "bar", out var defaultFooBar));
         Assert.Null(defaultFooBar);

         Assert.False(defaultTypes.TryGet(typeof(Foo), "baz", out var defaultFooBaz));
         Assert.Null(defaultFooBaz);
      }

      [Fact]
      public void TryGetByTargetTypeReturnsTrueWhenThereIsAMatch()
      {
         var defaultTypes = new DefaultTypes()
             .Add(typeof(IBar), typeof(Bar))
             .Add(typeof(IBaz), typeof(Baz));

         Assert.True(defaultTypes.TryGet(typeof(IBar), out var defaultFooBar));
         Assert.Equal(typeof(Bar), defaultFooBar);

         Assert.True(defaultTypes.TryGet(typeof(IBaz), out var defaultFooBaz));
         Assert.Equal(typeof(Baz), defaultFooBaz);
      }

      [Fact]
      public void TryGetByTargetTypeReturnsFalseWhenThereNotIsAMatch()
      {
         var defaultTypes = new DefaultTypes();

         Assert.False(defaultTypes.TryGet(typeof(IBar), out var defaultFooBar));
         Assert.Null(defaultFooBar);

         Assert.False(defaultTypes.TryGet(typeof(IBaz), out var defaultFooBaz));
         Assert.Null(defaultFooBaz);
      }

      [Fact]
      public void GivenNullDeclaringTypeThrowsArgumentNullException()
      {
         var defaultTypes = new DefaultTypes();

         Assert.Throws<ArgumentNullException>(() => defaultTypes.Add(null!, "bar", typeof(Bar)));
      }

      [Fact]
      public void GivenNullMemberNameThrowsArgumentNullException()
      {
         var defaultTypes = new DefaultTypes();

         Assert.Throws<ArgumentNullException>(() => defaultTypes.Add(typeof(Foo), null!, typeof(Bar)));
      }

      [Fact]
      public void GivenNullDefaultType1ThrowsArgumentNullException()
      {
         var defaultTypes = new DefaultTypes();

         Assert.Throws<ArgumentNullException>(() => defaultTypes.Add(typeof(Foo), "bar", null!));
      }

      [Fact]
      public void GivenNullTargetTypeThrowsArgumentNullException()
      {
         var defaultTypes = new DefaultTypes();

         Assert.Throws<ArgumentNullException>(() => defaultTypes.Add(null!, typeof(Bar)));
      }

      [Fact]
      public void GivenNullDefaultType2ThrowsArgumentNullException()
      {
         var defaultTypes = new DefaultTypes();

         Assert.Throws<ArgumentNullException>(() => defaultTypes.Add(typeof(IBar), null!));
      }

      [Fact]
      public void GivenNoMatchingMembersThrowsArgumentException()
      {
         var defaultTypes = new DefaultTypes();

         var actual = Assert.Throws<ArgumentException>(() => defaultTypes.Add(typeof(Foo), "qux", typeof(Qux)));

#if DEBUG
            var expected = Exceptions.NoMatchingMembers(typeof(Foo), "qux");
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenDefaultTypeIsNotAssignableToMemberTypeThrowsArgumentException()
      {
         var defaultTypes = new DefaultTypes();

         var actual = Assert.Throws<ArgumentException>(() => defaultTypes.Add(typeof(Foo), "bar", typeof(Qux)));

#if DEBUG
            var expected = Exceptions.DefaultTypeNotAssignableToMembers(typeof(Foo), "bar", typeof(Qux), new List<Member> { new Member("Bar", typeof(IBar), MemberType.Property) });
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenDefaultTypeIsNotAssignableToTargetTypeThrowsArgumentException()
      {
         var defaultTypes = new DefaultTypes();

         var actual = Assert.Throws<ArgumentException>(() => defaultTypes.Add(typeof(IBar), typeof(Qux)));

#if DEBUG
            var expected = Exceptions.DefaultTypeIsNotAssignableToTargetType(typeof(IBar), typeof(Qux));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenAbstractDefaultType1ThrowsArgumentException()
      {
         var defaultTypes = new DefaultTypes();

         var actual = Assert.Throws<ArgumentException>(() => defaultTypes.Add(typeof(Foo), "bar", typeof(AbstractBar)));

#if DEBUG
            var expected = Exceptions.DefaultTypeCannotBeAbstract(typeof(AbstractBar));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

      [Fact]
      public void GivenAbstractDefaultType2ThrowsArgumentException()
      {
         var defaultTypes = new DefaultTypes();

         var actual = Assert.Throws<ArgumentException>(() => defaultTypes.Add(typeof(IBar), typeof(AbstractBar)));

#if DEBUG
            var expected = Exceptions.DefaultTypeCannotBeAbstract(typeof(AbstractBar));
            Assert.Equal(expected.Message, actual.Message);
#endif
      }

#pragma warning disable CA1812
      private class Foo
      {
         public IBar? Bar { get; set; }
         public IBaz? Baz { get; set; }
      }

      private interface IBar { }
      private class Bar : IBar { }

      private interface IBaz { }
      private class Baz : IBaz { }

      private class Qux { }

      private abstract class AbstractBar : IBar { }
#pragma warning restore CA1812
   }
}
