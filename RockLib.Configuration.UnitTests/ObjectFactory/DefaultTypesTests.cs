using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections.Generic;
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
            var defaultTypes = new DefaultTypes();
            defaultTypes.Add(typeof(Foo), "bar", typeof(Bar));
            defaultTypes.Add(typeof(Foo), "baz", typeof(Baz));

            // The Add method returns 'this', so you can chain them together:
            defaultTypes = new DefaultTypes()
                .Add(typeof(Foo), "bar", typeof(Bar))
                .Add(typeof(Foo), "baz", typeof(Baz));

            // List initialization syntax works also.
            defaultTypes = new DefaultTypes
            {
                { typeof(Foo), "bar", typeof(Bar) },
                { typeof(Foo), "baz", typeof(Baz) }
            };
        }

        [Fact]
        public void CanAddByTargetType()
        {
            // Register a default type by target type when you want all properties of the
            // target type to have the same default type.

            // Call the Add method for each default type that needs to be registered.
            var defaultTypes = new DefaultTypes();
            defaultTypes.Add(typeof(IBar), typeof(Bar));
            defaultTypes.Add(typeof(IBaz), typeof(Baz));

            // The Add method returns 'this', so you can chain them together:
            defaultTypes = new DefaultTypes()
                .Add(typeof(IBar), typeof(Bar))
                .Add(typeof(IBaz), typeof(Baz));

            // List initialization syntax works also.
            defaultTypes = new DefaultTypes
            {
                { typeof(IBar), typeof(Bar) },
                { typeof(IBaz), typeof(Baz) }
            };
        }

        [Fact]
        public void TryGetByDeclaringTypeAndMemberNameReturnsTrueWhenThereIsAMatch()
        {
            var defaultTypes = new DefaultTypes()
                .Add(typeof(Foo), "bar", typeof(Bar))
                .Add(typeof(Foo), "baz", typeof(Baz));

            Assert.True(defaultTypes.TryGet(typeof(Foo), "bar", out Type defaultFooBar));
            Assert.Equal(typeof(Bar), defaultFooBar);

            Assert.True(defaultTypes.TryGet(typeof(Foo), "baz", out Type defaultFooBaz));
            Assert.Equal(typeof(Baz), defaultFooBaz);
        }

        [Fact]
        public void TryGetByDeclaringTypeAndMemberNameReturnsFalseWhenThereNotIsAMatch()
        {
            var defaultTypes = new DefaultTypes();

            Assert.False(defaultTypes.TryGet(typeof(Foo), "bar", out Type defaultFooBar));
            Assert.Null(defaultFooBar);

            Assert.False(defaultTypes.TryGet(typeof(Foo), "baz", out Type defaultFooBaz));
            Assert.Null(defaultFooBaz);
        }

        [Fact]
        public void TryGetByTargetTypeReturnsTrueWhenThereIsAMatch()
        {
            var defaultTypes = new DefaultTypes()
                .Add(typeof(IBar), typeof(Bar))
                .Add(typeof(IBaz), typeof(Baz));

            Assert.True(defaultTypes.TryGet(typeof(IBar), out Type defaultFooBar));
            Assert.Equal(typeof(Bar), defaultFooBar);

            Assert.True(defaultTypes.TryGet(typeof(IBaz), out Type defaultFooBaz));
            Assert.Equal(typeof(Baz), defaultFooBaz);
        }

        [Fact]
        public void TryGetByTargetTypeReturnsFalseWhenThereNotIsAMatch()
        {
            var defaultTypes = new DefaultTypes();

            Assert.False(defaultTypes.TryGet(typeof(IBar), out Type defaultFooBar));
            Assert.Null(defaultFooBar);

            Assert.False(defaultTypes.TryGet(typeof(IBaz), out Type defaultFooBaz));
            Assert.Null(defaultFooBaz);
        }

        [Fact]
        public void GivenNullDeclaringType_ThrowsArgumentNullException()
        {
            var defaultTypes = new DefaultTypes();

            Assert.Throws<ArgumentNullException>(() => defaultTypes.Add(null, "bar", typeof(Bar)));
        }

        [Fact]
        public void GivenNullMemberName_ThrowsArgumentNullException()
        {
            var defaultTypes = new DefaultTypes();

            Assert.Throws<ArgumentNullException>(() => defaultTypes.Add(typeof(Foo), null, typeof(Bar)));
        }

        [Fact]
        public void GivenNullDefaultType1_ThrowsArgumentNullException()
        {
            var defaultTypes = new DefaultTypes();

            Assert.Throws<ArgumentNullException>(() => defaultTypes.Add(typeof(Foo), "bar", null));
        }

        [Fact]
        public void GivenNullTargetType_ThrowsArgumentNullException()
        {
            var defaultTypes = new DefaultTypes();

            Assert.Throws<ArgumentNullException>(() => defaultTypes.Add(null, typeof(Bar)));
        }

        [Fact]
        public void GivenNullDefaultType2_ThrowsArgumentNullException()
        {
            var defaultTypes = new DefaultTypes();

            Assert.Throws<ArgumentNullException>(() => defaultTypes.Add(typeof(IBar), null));
        }

        [Fact]
        public void GivenNoMatchingMembers_ThrowsArgumentException()
        {
            var defaultTypes = new DefaultTypes();

            var actual = Assert.Throws<ArgumentException>(() => defaultTypes.Add(typeof(Foo), "qux", typeof(Qux)));

#if DEBUG
            var expected = Exceptions.DefaultTypeHasNoMatchingMembers(typeof(Foo), "qux");
            Assert.Equal(expected.Message, actual.Message);
#endif
        }

        [Fact]
        public void GivenDefaultTypeIsNotAssignableToMemberType_ThrowsArgumentException()
        {
            var defaultTypes = new DefaultTypes();

            var actual = Assert.Throws<ArgumentException>(() => defaultTypes.Add(typeof(Foo), "bar", typeof(Qux)));

#if DEBUG
            var expected = Exceptions.DefaultTypeNotAssignableToMembers(typeof(Foo), "bar", typeof(Qux), new List<Member> { new Member("Bar", typeof(IBar), MemberType.Property) });
            Assert.Equal(expected.Message, actual.Message);
#endif
        }

        [Fact]
        public void GivenDefaultTypeIsNotAssignableToTargetType_ThrowsArgumentException()
        {
            var defaultTypes = new DefaultTypes();

            var actual = Assert.Throws<ArgumentException>(() => defaultTypes.Add(typeof(IBar), typeof(Qux)));

#if DEBUG
            var expected = Exceptions.DefaultTypeIsNotAssignableToTargetType(typeof(IBar), typeof(Qux));
            Assert.Equal(expected.Message, actual.Message);
#endif
        }

        private class Foo
        {
            public IBar Bar { get; set; }
            public IBaz Baz { get; set; }
        }

        private interface IBar { }
        private class Bar : IBar { }

        private interface IBaz { }
        private class Baz : IBaz { }

        private class Qux { }
    }
}
