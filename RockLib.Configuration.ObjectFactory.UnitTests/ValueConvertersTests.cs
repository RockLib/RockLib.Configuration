using RockLib.Configuration.ObjectFactory;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests
{
    public class ValueConvertersTests
    {
        [Fact]
        public void CanAddConvertFuncByDeclaringTypeAndMemberName()
        {
            // Register a converter by declaring type and member name when you need
            // different properties of the same type to use different converters.

            // Call the Add method for each converter that needs to be registered.
            var first = new ValueConverters();
            first.Add(typeof(Foo), "bar", value => new Bar(int.Parse(value)));
            first.Add(typeof(Foo), "baz", ParseBaz);

            // The Add method returns 'this', so you can chain them together:
            var second = new ValueConverters()
                .Add(typeof(Foo), "bar", value => new Bar(int.Parse(value)))
                .Add(typeof(Foo), "baz", ParseBaz);

            // List initialization syntax works also.
            var third = new ValueConverters
            {
                { typeof(Foo), "bar", value => new Bar(int.Parse(value)) },
                { typeof(Foo), "baz", ParseBaz }
            };

            // All three instances represent the same thing. Verify that their
            // contents are the same.

            // ValueConverters implements IEnumerable<KeyValuePair<string, Type>>
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
        public void CanAddConvertFuncByTargetType()
        {
            // Register a converter by target type when you want all properties of the
            // target type to use the same converter.

            // Call the Add method for each converter that needs to be registered.
            var first = new ValueConverters();
            first.Add(typeof(Bar), value => new Bar(int.Parse(value)));
            first.Add(typeof(Baz), ParseBaz);

            // The Add method returns 'this', so you can chain them together:
            var second = new ValueConverters()
                .Add(typeof(Bar), value => new Bar(int.Parse(value)))
                .Add(typeof(Baz), ParseBaz);

            // List initialization syntax works also.
            var third = new ValueConverters
            {
                { typeof(Bar), value => new Bar(int.Parse(value)) },
                { typeof(Baz), ParseBaz }
            };

            // All three instances represent the same thing. Verify that their
            // contents are the same.

            // ValueConverters implements IEnumerable<KeyValuePair<string, Type>>
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
            var valueConverters = new ValueConverters()
                .Add(typeof(Foo), "bar", value => new Bar(int.Parse(value)))
                .Add(typeof(Foo), "baz", ParseBaz);

            Assert.True(valueConverters.TryGet(typeof(Foo), "bar", out var convertBar));
            Assert.True(valueConverters.TryGet(typeof(Foo), "baz", out var convertBaz));
        }

        [Fact]
        public void TryGetByDeclaringTypeAndMemberNameReturnsFalseWhenThereNotIsAMatch()
        {
            var valueConverters = new ValueConverters();

            Assert.False(valueConverters.TryGet(typeof(Foo), "bar", out var convertBar));
            Assert.False(valueConverters.TryGet(typeof(Foo), "baz", out var convertBaz));
        }

        [Fact]
        public void TryGetByTargetTypeReturnsTrueWhenThereIsAMatch()
        {
            var valueConverters = new ValueConverters()
                .Add(typeof(Bar), value => new Bar(int.Parse(value)))
                .Add(typeof(Baz), ParseBaz);

            Assert.True(valueConverters.TryGet(typeof(Bar), out var convertBar));
            Assert.True(valueConverters.TryGet(typeof(Baz), out var convertBaz));
        }

        [Fact]
        public void TryGetByTargetTypeReturnsFalseWhenThereNotIsAMatch()
        {
            var valueConverters = new ValueConverters();

            Assert.False(valueConverters.TryGet(typeof(Bar), out var convertBar));
            Assert.False(valueConverters.TryGet(typeof(Baz), out var convertBaz));
        }

        [Fact]
        public void TheConvertFuncReturnedByTryGetCallsTheConvertFuncPassedToTheAddByDeclaringTypeAndMemberNameMethod()
        {
            var invocationCount = 0;
            Func<string, Bar> registeredConvertBar = value =>
            {
                invocationCount++;
                return new Bar(int.Parse(value));
            };

            var valueConverters = new ValueConverters()
                .Add(typeof(Foo), "bar", registeredConvertBar);

            Assert.True(valueConverters.TryGet(typeof(Foo), "bar", out var retrievedConvertBar));

            Assert.Equal(0, invocationCount);

            var converted = retrievedConvertBar("123");

            Assert.Equal(1, invocationCount);
            Assert.IsType<Bar>(converted);
            Assert.Equal(123, ((Bar)converted).Baz);
        }

        [Fact]
        public void TheConvertFuncReturnedByTryGetCallsTheConvertFuncPassedToTheAddByTargetTypeMethod()
        {
            var invocationCount = 0;
            Func<string, Baz> registeredConvertBaz = value =>
            {
                invocationCount++;
                return ParseBaz(value);
            };

            var valueConverters = new ValueConverters()
                .Add(typeof(Baz), registeredConvertBaz);

            Assert.True(valueConverters.TryGet(typeof(Baz), out var retrievedConvertBaz));

            Assert.Equal(0, invocationCount);

            var converted = retrievedConvertBaz("123.45,-76.543");

            Assert.Equal(1, invocationCount);
            Assert.IsType<Baz>(converted);
            Assert.Equal(123.45, ((Baz)converted).Latitude);
            Assert.Equal(-76.543, ((Baz)converted).Longitude);
        }

        [Fact]
        public void GivenNullDeclaringType_ThrowsArgumentNullException()
        {
            var valueConverters = new ValueConverters();

            Assert.Throws<ArgumentNullException>(() => valueConverters.Add(null, "bar", value => new Bar(int.Parse(value))));
        }

        [Fact]
        public void GivenNullMemberName_ThrowsArgumentNullException()
        {
            var valueConverters = new ValueConverters();

            Assert.Throws<ArgumentNullException>(() => valueConverters.Add(typeof(Foo), null, value => new Bar(int.Parse(value))));
        }

        [Fact]
        public void GivenNullConvertFunc1_ThrowsArgumentNullException()
        {
            var valueConverters = new ValueConverters();

            Assert.Throws<ArgumentNullException>(() => valueConverters.Add<Bar>(typeof(Foo), "bar", null));
        }

        [Fact]
        public void GivenNullTargetType_ThrowsArgumentNullException()
        {
            var valueConverters = new ValueConverters();

            Assert.Throws<ArgumentNullException>(() => valueConverters.Add(null, value => new Bar(int.Parse(value))));
        }

        [Fact]
        public void GivenNullConvertFunc2_ThrowsArgumentNullException()
        {
            var valueConverters = new ValueConverters();

            Assert.Throws<ArgumentNullException>(() => valueConverters.Add<Bar>(typeof(Bar), null));
        }

        [Fact]
        public void GivenNoMatchingMembers_ThrowsArgumentException()
        {
            var valueConverters = new ValueConverters();

            var actual = Assert.Throws<ArgumentException>(() => valueConverters.Add(typeof(Foo), "qux", value => new Bar(int.Parse(value))));

#if DEBUG
            var expected = Exceptions.NoMatchingMembers(typeof(Foo), "qux");
            Assert.Equal(expected.Message, actual.Message);
#endif
        }

        [Fact]
        public void GivenReturnTypeOfFunctionIsNotAssignableToMemberType_ThrowsArgumentException()
        {
            var valueConverters = new ValueConverters();

            var actual = Assert.Throws<ArgumentException>(() => valueConverters.Add(typeof(Foo), "bar", value => new Qux()));

#if DEBUG
            var expected = Exceptions.ReturnTypeOfConvertFuncNotAssignableToMembers(typeof(Foo), "bar", typeof(Qux), new List<Member> { new Member("Bar", typeof(Bar), MemberType.Property) });
            Assert.Equal(expected.Message, actual.Message);
#endif
        }
        
        [Fact]
        public void GivenReturnTypeOfFunctionIsNotAssignableToTargetType_ThrowsArgumentException()
        {
            var valueConverters = new ValueConverters();

            var actual = Assert.Throws<ArgumentException>(() => valueConverters.Add(typeof(Bar), value => new Qux()));

#if DEBUG
            var expected = Exceptions.ReturnTypeOfConvertFuncIsNotAssignableToTargetType(typeof(Bar), typeof(Qux));
            Assert.Equal(expected.Message, actual.Message);
#endif
        }

        private static Baz ParseBaz(string value)
        {
            var split = value.Split(',');
            return new Baz(double.Parse(split[0]), double.Parse(split[1]));
        }

        private static class BarConverter
        {
            private static Bar Convert(string value) => new Bar(int.Parse(value));
        }

        private static class BazConverter
        {
            private static Baz Convert(string value) => ParseBaz(value);
        }

        private class Foo
        {
            public Bar Bar { get; set; }
            public Baz Baz { get; set; }
        }

        private struct Bar
        {
            public Bar(int baz) => Baz = baz;
            public int Baz { get; }
        }

        private struct Baz
        {
            public Baz(double latitude, double longitude)
            {
                Latitude = latitude;
                Longitude = longitude;
            }
            public double Latitude { get; }
            public double Longitude { get; }
        }

        private struct Qux { }
    }
}
