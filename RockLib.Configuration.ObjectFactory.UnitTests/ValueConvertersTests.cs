using RockLib.Configuration.ObjectFactory;
using System;
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

            // Use this Add method when you want to register a function that returns
            // the converted value.

            // Call the Add method for each converter that needs to be registered.
            var valueConverters = new ValueConverters();
            valueConverters.Add(typeof(Foo), "bar", value => new Bar(int.Parse(value)));
            valueConverters.Add(typeof(Foo), "baz", ParseBaz);

            // The Add method returns 'this', so you can chain them together:
            valueConverters = new ValueConverters()
                .Add(typeof(Foo), "bar", value => new Bar(int.Parse(value)))
                .Add(typeof(Foo), "baz", ParseBaz);

            // List initialization syntax works also.
            valueConverters = new ValueConverters
            {
                { typeof(Foo), "bar", value => new Bar(int.Parse(value)) },
                { typeof(Foo), "baz", ParseBaz }
            };
        }

        [Fact]
        public void CanAddConverterTypeByDeclaringTypeAndMemberName()
        {
            // Register a converter by declaring type and member name when you need
            // different properties of the same type to use different converters.

            // Use this Add method when you want to register a type that declares
            // a 'static T Convert(string)' method.

            // Call the Add method for each converter that needs to be registered.
            var valueConverters = new ValueConverters();
            valueConverters.Add(typeof(Foo), "bar", typeof(BarConverter));
            valueConverters.Add(typeof(Foo), "baz", typeof(BazConverter));

            // The Add method returns 'this', so you can chain them together:
            valueConverters = new ValueConverters()
                .Add(typeof(Foo), "bar", typeof(BarConverter))
                .Add(typeof(Foo), "baz", typeof(BazConverter));

            // List initialization syntax works also.
            valueConverters = new ValueConverters
            {
                { typeof(Foo), "bar", typeof(BarConverter) },
                { typeof(Foo), "baz", typeof(BazConverter) }
            };
        }

        [Fact]
        public void CanAddConvertFuncByTargetType()
        {
            // Register a converter by target type when you want all properties of the
            // target type to use the same converter.

            // Use this Add method when you want to register a function that returns
            // the converted value.

            // Call the Add method for each converter that needs to be registered.
            var valueConverters = new ValueConverters();
            valueConverters.Add(typeof(Bar), value => new Bar(int.Parse(value)));
            valueConverters.Add(typeof(Baz), ParseBaz);

            // The Add method returns 'this', so you can chain them together:
            valueConverters = new ValueConverters()
                .Add(typeof(Bar), value => new Bar(int.Parse(value)))
                .Add(typeof(Baz), ParseBaz);

            // List initialization syntax works also.
            valueConverters = new ValueConverters
            {
                { typeof(Bar), value => new Bar(int.Parse(value)) },
                { typeof(Baz), ParseBaz }
            };
        }

        [Fact]
        public void CanAddConverterTypeByTargetType()
        {
            // Register a converter by target type when you want all properties of the
            // target type to use the same converter.

            // Use this Add method when you want to register a type that declares
            // a 'static T Convert(string)' method.

            // Call the Add method for each converter that needs to be registered.
            var valueConverters = new ValueConverters();
            valueConverters.Add(typeof(Bar), typeof(BarConverter));
            valueConverters.Add(typeof(Baz), typeof(BazConverter));

            // The Add method returns 'this', so you can chain them together:
            valueConverters = new ValueConverters()
                .Add(typeof(Bar), typeof(BarConverter))
                .Add(typeof(Baz), typeof(BazConverter));

            // List initialization syntax works also.
            valueConverters = new ValueConverters
            {
                { typeof(Bar), typeof(BarConverter) },
                { typeof(Baz), typeof(BazConverter) }
            };
        }

        [Fact]
        public void TryGetByDeclaringTypeAndMemberNameReturnsTrueWhenThereIsAMatch()
        {
            var valueConverters = new ValueConverters()
                .Add(typeof(Foo), "bar", typeof(BarConverter))
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
        public void TheConvertFuncReturnedByTryGetCallsTheConvertMethodFromTheConvertTypePassedToTheAddByDeclaringTypeAndMemberNameMethod()
        {
            var valueConverters = new ValueConverters()
                .Add(typeof(Foo), "bar", typeof(CountingBarConverter));

            Assert.True(valueConverters.TryGet(typeof(Foo), "bar", out var retrievedConvertBar));

            Assert.Equal(0, CountingBarConverter.InvocationCount);

            var converted = retrievedConvertBar("123");

            Assert.Equal(1, CountingBarConverter.InvocationCount);
            Assert.IsType<Bar>(converted);
            Assert.Equal(123, ((Bar)converted).Baz);
        }

        private static class CountingBarConverter
        {
            public static int InvocationCount { get; private set; }

            private static Bar Convert(string value)
            {
                InvocationCount++;
                return new Bar(int.Parse(value));
            }
        }

        [Fact]
        public void TheConvertFuncReturnedByTryGetCallsTheConvertMethodFromTheConvertTypePassedToTheAddByTargetTypeMethod()
        {
            var valueConverters = new ValueConverters()
                .Add(typeof(Baz), typeof(CountingBazConverter));

            Assert.True(valueConverters.TryGet(typeof(Baz), out var retrievedConvertBaz));

            Assert.Equal(0, CountingBazConverter.InvocationCount);

            var converted = retrievedConvertBaz("123.45,-76.543");

            Assert.Equal(1, CountingBazConverter.InvocationCount);
            Assert.IsType<Baz>(converted);
            Assert.Equal(123.45, ((Baz)converted).Latitude);
            Assert.Equal(-76.543, ((Baz)converted).Longitude);
        }

        private static class CountingBazConverter
        {
            public static int InvocationCount { get; private set; }

            private static Baz Convert(string value)
            {
                InvocationCount++;
                return ParseBaz(value);
            }
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
    }
}
