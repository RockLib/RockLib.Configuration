using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RockLib.Configuration.UnitTests
{
    public class CompositeSectionTests
    {
        [Fact]
        public void CompositeSectionCombinesSections()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo.bar:garply:baz", "123" },
                    { "foo_bar:garply:qux", "abc" },
                }).Build();

            var foobarSection = config.GetCompositeSection("foo.bar", "foo_bar");

            foobarSection["garply:baz"].Should().Be("123");
            foobarSection["garply:qux"].Should().Be("abc");

            var garplySection = foobarSection.GetSection("garply");
            garplySection["baz"].Should().Be("123");
            garplySection["qux"].Should().Be("abc");

            var children = foobarSection.GetChildren().ToList();
            children.Count.Should().Be(1);
            children[0]["baz"].Should().Be("123");
            children[0]["qux"].Should().Be("abc");
        }

        [Fact]
        public void CompositeSectionPathOrderIsSignificant()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "foo_bar:garply:baz", "abc" }, // Both of these settings have
                    { "foo.bar:garply:baz", "123" }, // the same composite path.
                }).Build();

            // Since the dot section is the first key, its value should be selected.
            var foobarSection = config.GetCompositeSection("foo.bar", "foo_bar");

            foobarSection["garply:baz"].Should().Be("123");

            var garplySection = foobarSection.GetSection("garply");
            garplySection["baz"].Should().Be("123");

            var children = foobarSection.GetChildren().ToList();
            children.Count.Should().Be(1);
            children[0]["baz"].Should().Be("123");
        }
    }
}
