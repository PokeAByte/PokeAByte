using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.ScriptTests;

public class CopyPropertiesTest
{
    [Fact]
    public async Task CopyProperties()
    {
        var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var driver = new TestDriver([0,1,2,3]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            new TestClientNotifier(),
            """
            <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GB" version="1.0.0">
                <memory>
                    <read start="0x00" end="0x04"/>
                </memory>
                <properties>
                    <original>
                        <property name="0" type="int" address="0x00" length="1" />
                        <property name="1" type="int" address="0x01" length="1" />
                        <property name="2" type="int" address="0x02" length="1" />
                        <property name="3" type="int" address="0x03" length="1" />
                        <property name="extra" type="string" value="Should not be copied." />
                    </original>
                    <copy>
                        <property name="0" type="int" />
                        <property name="1" type="int" />
                        <property name="2" type="int" />
                        <property name="3" type="int" />
                    </copy>
                </properties>
            </mapper>
            """,
            driver,
            Path.Combine(currentFolder, "scripts/copyproperties.js"),
            currentFolder
        );


        await instance.Read();
        await instance.Read();

        Assert.Equal(0, instance.Mapper.get_property_value("original.0"));
        Assert.Equal(1, instance.Mapper.get_property_value("original.1"));
        Assert.Equal(2, instance.Mapper.get_property_value("original.2"));
        Assert.Equal(3, instance.Mapper.get_property_value("original.3"));
        Assert.Equal("Should not be copied.", instance.Mapper.get_property_value("original.extra"));

        Assert.Equal(0, instance.Mapper.get_property_value("copy.0"));
        Assert.Equal(1, instance.Mapper.get_property("copy.0").Length);
        Assert.Equal(0u, instance.Mapper.get_property("copy.0").Address);

        Assert.Equal(1, instance.Mapper.get_property_value("copy.1"));
        Assert.Equal(1, instance.Mapper.get_property("copy.1").Length);
        Assert.Equal(1u, instance.Mapper.get_property("copy.1").Address);

        Assert.Equal(2, instance.Mapper.get_property_value("copy.2"));
        Assert.Equal(1, instance.Mapper.get_property("copy.2").Length);
        Assert.Equal(2u, instance.Mapper.get_property("copy.2").Address);

        Assert.Equal(3, instance.Mapper.get_property_value("copy.3"));
        Assert.Equal(1, instance.Mapper.get_property("copy.3").Length);
        Assert.Equal(3u, instance.Mapper.get_property("copy.3").Address);
        
        var error = Assert.Throws<Exception>(() => instance.Mapper.get_property_value("copy.extra"));
        Assert.Equal("copy.extra is not defined in properties.", error.Message);
    }
}