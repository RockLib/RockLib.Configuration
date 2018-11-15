using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Xunit;

namespace RockLib.Configuration.AppConfigTests
{
    public class ConfigTests
    {
        [Fact]
        public void AppSetting_WillPullValueFromAppConfigFile()
        {
            var value = Config.AppSettings["Key100"];

            Assert.Equal("Key100_Value", value);
        }

        [Fact]
        public void AppSetting_WhenValueNotInAppSetting_WillThrowException()
        {
            Assert.Throws<KeyNotFoundException>(() => Config.AppSettings["KeyNotFound"]);
        }

        [Fact]
        public void ElementWithValueMapsCorrectly()
        {
            var section = Config.Root.GetSection("element_with_value");

            Assert.Equal("element_with_value", section.Key);
            Assert.Equal("element_with_value", section.Path);
            Assert.Equal("123", section.Value);
            Assert.Empty(section.GetChildren());
        }

        [Fact]
        public void MismatchedCaseWorksAsExpected()
        {
            var section = Config.Root.GetSection("CASE_MISMATCH");

            Assert.Equal("CASE_MISMATCH", section.Key);
            Assert.Equal("CASE_MISMATCH", section.Path);
            
            Assert.Null(section.Value);

            var children = section.GetChildren().ToArray();
            Assert.NotEmpty(children);

            Assert.Equal("foo", children[0].Key);
            Assert.Equal("CASE_MISMATCH:foo", children[0].Path);
            Assert.Equal("123", children[0].Value);
        }

        [Fact]
        public void ElementWithAttributesMapsCorrectly()
        {
            var section = Config.Root.GetSection("element_with_attributes");

            Assert.Equal("element_with_attributes", section.Key);
            Assert.Equal("element_with_attributes", section.Path);
            Assert.Null(section.Value);

            var children = section.GetChildren().ToArray();
            Assert.Equal(2, children.Length);

            var foo = children.SingleOrDefault(c => c.Key == "foo");
            var bar = children.SingleOrDefault(c => c.Key == "bar");

            Assert.NotNull(foo);
            Assert.NotNull(bar);

            Assert.Equal("element_with_attributes:foo", foo.Path);
            Assert.Equal("234", foo.Value);
            Assert.Empty(foo.GetChildren());

            Assert.Equal("element_with_attributes:bar", bar.Path);
            Assert.Equal("345", bar.Value);
            Assert.Empty(bar.GetChildren());
        }

        [Fact]
        public void ElementWithDistinctChildNamesMapsCorrectly()
        {
            var section = Config.Root.GetSection("element_with_distinct_child_names");

            var children = section.GetChildren().ToArray();
            Assert.Equal(2, children.Length);

            var foo = children.SingleOrDefault(c => c.Key == "foo");
            var garply = children.SingleOrDefault(c => c.Key == "garply");

            Assert.NotNull(foo);
            Assert.NotNull(garply);

            Assert.Equal("element_with_distinct_child_names:foo", foo.Path);
            Assert.Null(foo.Value);

            children = foo.GetChildren().ToArray();
            Assert.Equal(2, children.Length);

            var bar = children.SingleOrDefault(c => c.Key == "bar");
            var baz = children.SingleOrDefault(c => c.Key == "baz");

            Assert.NotNull(bar);
            Assert.NotNull(baz);

            Assert.Equal("element_with_distinct_child_names:foo:bar", bar.Path);
            Assert.Equal("456", bar.Value);
            Assert.Empty(bar.GetChildren());

            Assert.Equal("element_with_distinct_child_names:foo:baz", baz.Path);
            Assert.Null(baz.Value);

            children = baz.GetChildren().ToArray();
            Assert.Single(children);

            var qux = children.SingleOrDefault(c => c.Key == "qux");

            Assert.NotNull(qux);

            Assert.Equal("element_with_distinct_child_names:foo:baz:qux", qux.Path);
            Assert.Equal("567", qux.Value);
            Assert.Empty(qux.GetChildren());

            Assert.Equal("element_with_distinct_child_names:garply", garply.Path);
            Assert.Equal("678", garply.Value);
            Assert.Empty(garply.GetChildren());
        }

        [Fact]
        public void ElementWithMultipleChildrenWithTheSameNameMapsCorrectly()
        {
            var section = Config.Root.GetSection("element_with_multiple_children_with_the_same_name");

            var children = section.GetChildren().ToArray();
            Assert.Equal(2, children.Length);

            var foo = children.SingleOrDefault(c => c.Key == "foo");
            var bar = children.SingleOrDefault(c => c.Key == "bar");

            Assert.NotNull(foo);
            Assert.NotNull(bar);

            Assert.Equal("element_with_multiple_children_with_the_same_name:foo", foo.Path);
            Assert.Null(foo.Value);

            children = foo.GetChildren().ToArray();
            Assert.Equal(2, children.Length);

            Assert.Equal("0", children[0].Key);
            Assert.Equal("element_with_multiple_children_with_the_same_name:foo:0", children[0].Path);
            Assert.Equal("789", children[0].Value);
            Assert.Empty(children[0].GetChildren());

            Assert.Equal("1", children[1].Key);
            Assert.Equal("element_with_multiple_children_with_the_same_name:foo:1", children[1].Path);
            Assert.Equal("890", children[1].Value);
            Assert.Empty(children[1].GetChildren());

            Assert.Equal("element_with_multiple_children_with_the_same_name:bar", bar.Path);
            Assert.Null(bar.Value);

            children = bar.GetChildren().ToArray();
            Assert.Single(children);

            var baz = children[0];

            Assert.Equal("baz", baz.Key);
            Assert.Equal("element_with_multiple_children_with_the_same_name:bar:baz", baz.Path);
            Assert.Null(baz.Value);

            children = baz.GetChildren().ToArray();
            Assert.Equal(2, children.Length);

            Assert.Equal("0", children[0].Key);
            Assert.Equal("element_with_multiple_children_with_the_same_name:bar:baz:0", children[0].Path);
            Assert.Null(children[0].Value);

            var grandchildren = children[0].GetChildren().ToArray();
            Assert.Single(grandchildren);

            var qux = grandchildren[0];

            Assert.Equal("qux", qux.Key);
            Assert.Equal("element_with_multiple_children_with_the_same_name:bar:baz:0:qux", qux.Path);
            Assert.Equal("901", qux.Value);
            Assert.Empty(qux.GetChildren());

            Assert.Equal("1", children[1].Key);
            Assert.Equal("element_with_multiple_children_with_the_same_name:bar:baz:1", children[1].Path);
            Assert.Null(children[1].Value);

            grandchildren = children[1].GetChildren().ToArray();
            Assert.Single(grandchildren);

            qux = grandchildren[0];

            Assert.Equal("qux", qux.Key);
            Assert.Equal("element_with_multiple_children_with_the_same_name:bar:baz:1:qux", qux.Path);
            Assert.Equal("012", qux.Value);
            Assert.Empty(qux.GetChildren());
        }

        [Fact]
        public void MultipleTopLevelElementsMapCorrectly()
        {
            var section = Config.Root.GetSection("multiple_top_level_elements");

            var children = section.GetChildren().ToArray();
            Assert.Equal(2, children.Length);

            Assert.Equal("0", children[0].Key);
            Assert.Equal("multiple_top_level_elements:0", children[0].Path);
            Assert.Null(children[0].Value);

            var grandchildren = children[0].GetChildren().ToArray();
            Assert.Equal(2, grandchildren.Length);

            var foo = grandchildren.SingleOrDefault(c => c.Key == "foo");
            var bar = grandchildren.SingleOrDefault(c => c.Key == "bar");

            Assert.NotNull(foo);
            Assert.NotNull(bar);

            Assert.Equal("multiple_top_level_elements:0:foo", foo.Path);
            Assert.Equal("1234", foo.Value);
            Assert.Empty(foo.GetChildren());

            Assert.Equal("multiple_top_level_elements:0:bar", bar.Path);
            Assert.Equal("2345", bar.Value);
            Assert.Empty(bar.GetChildren());

            Assert.Equal("1", children[1].Key);
            Assert.Equal("multiple_top_level_elements:1", children[1].Path);
            Assert.Null(children[1].Value);

            grandchildren = children[1].GetChildren().ToArray();
            Assert.Equal(2, grandchildren.Length);

            foo = grandchildren.SingleOrDefault(c => c.Key == "foo");
            bar = grandchildren.SingleOrDefault(c => c.Key == "bar");

            Assert.NotNull(foo);
            Assert.NotNull(bar);

            Assert.Equal("multiple_top_level_elements:1:foo", foo.Path);
            Assert.Equal("3456", foo.Value);
            Assert.Empty(foo.GetChildren());

            Assert.Equal("multiple_top_level_elements:1:bar", bar.Path);
            Assert.Equal("4567", bar.Value);
            Assert.Empty(bar.GetChildren());
        }

        [Fact]
        public void ReloadTest()
        {
            try
            {
                var section = Config.Root.GetSection("element_to_reload");

                Assert.Equal("123", section.Value);

                var waitHandle = new AutoResetEvent(false);

                ChangeToken.OnChange(section.GetReloadToken, () => waitHandle.Set());

                WriteConfig("456");

                waitHandle.WaitOne();

                Assert.Equal("456", section.Value);
            }
            finally
            {
                WriteConfig("123");
            }
        }

        private static void WriteConfig(string value)
        {
            var filePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;

            var doc = new XmlDocument();

            using (var reader = new StreamReader(filePath))
                doc.Load(reader);

            var navigator = doc.CreateNavigator();

            var node = navigator.SelectSingleNode("/configuration/element_to_reload/value");
            node.SetValue(value);

            doc.Save(filePath);
        }
    }
}
