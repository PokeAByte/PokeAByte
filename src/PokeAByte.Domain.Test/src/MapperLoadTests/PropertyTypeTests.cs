using System;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.MapperLoadTests;

public class PropertyAttributeTests
{
    [Fact]
    public async Task ThrowsOnBogusDataType()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        var exception = Assert.Throws<Exception>(() =>
        {
            return MapperTestHelper.CreateTestInstance(
                clientnotifier,
                """
                <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GBA">
                    <memory>
                        <read start="0x00" end="0x01"/>
                    </memory>
                    <properties>
                        <test>
                            <property name="0" type="boolean" length="1" address="0x00" />
                        </test>
                    </properties>
                </mapper>
                """,
                driver
            );
        });

        Assert.Equal(
            """Unable to parse test.0. <property name="0" type="boolean" length="1" address="0" />""",
            exception.Message
        );
    }
    
    [Fact]
    public async Task AcceptsValidTypes()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            """
            <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GBA">
                <properties>
                    <test>
                        <property name="0" type="binaryCodedDecimal" />
                        <property name="1" type="bitArray" />
                        <property name="2" type="bool" />
                        <property name="3" type="bit" />
                        <property name="4" type="int" />
                        <property name="5" type="string" />
                        <property name="6" type="uint" />
                        <property name="7" type="byteArray" />
                    </test>
                </properties>
            </mapper>
            """,
            driver
        );
        Assert.Equal(8, instance.Mapper.Properties.Count);
    }

    [Fact]
    public async Task ThrowsOnInvalidAddress()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        var exception = Assert.Throws<Exception>(() =>
        {
            return MapperTestHelper.CreateTestInstance(
                clientnotifier,
                """
                <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GBA">
                    <properties>
                        <test>
                            <property name="0" type="bool" length="1" address="0x100000000000" />
                        </test>
                    </properties>
                </mapper>
                """,
                driver
            );
        });

        Assert.Equal(
            """Unable to translate 0x100000000000 into a memory address / uint32.""",
            exception.Message
        );
    }

    [Fact]
    public async Task ParsesAddressVariables()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            """
            <mapper 
                id="8213664d-a1e8-4d4c-a569-1c57be540d46" 
                name="TestMapper" 
                platform="GBA"
                xmlns:var="https://schemas.pokeabyte.io/attributes/var">
            >
                <classes>
                    <custom>
                        <property name="math" address="{address} + 0x0F" type="int" />
                    </custom>
                </classes>
                <properties>
                    <just>
                        <class name="incredible" type="custom" var:address="0x01"/>
                    </just>
                </properties>
            </mapper>
            """,
            driver
        );
        Assert.Equal("1 + 15", instance.Mapper.Properties["just.incredible.math"].OriginalAddressString);
    }
}
