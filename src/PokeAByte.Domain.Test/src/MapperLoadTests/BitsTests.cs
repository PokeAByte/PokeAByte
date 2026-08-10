using System;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.MapperLoadTests;

public class BitsTests
{
    [Fact]
    public async Task AcceptsRangeBits()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0b11111111]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" bits="0-3" address="0x00" />""", 
                "0x00", 
                "0x01", 
                "GBA"
            ),
            driver
        );

        await instance.Read();

        Assert.Equal(0b1111, instance.Mapper.get_property_value("test.0"));
    }

    [Fact]
    public async Task AcceptsBitArray()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0b01010101]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" bits="0,2,4,6" address="0x00" />""", 
                "0x00", 
                "0x01", 
                "GBA"
            ),
            driver
        );

        await instance.Read();

        Assert.Equal(0b1111, instance.Mapper.get_property_value("test.0"));
    }

    [Fact]
    public async Task AcceptsIndex()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0b10000000]);
        await using var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" bits="7" address="0x00" />""", 
                "0x00", 
                "0x01", 
                "GBA"
            ),
            driver
        );

        await instance.Read();

        Assert.Equal(1, instance.Mapper.get_property_value("test.0"));
    }

    [Fact]
    public async Task RejectsFaultyRange()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0b10000000]);

        var exception = Assert.Throws<Exception>(() => MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" bits="0-a" address="0x00" />""", 
                "0x00", 
                "0x01", 
                "GBA"
            ),
            driver
        ));

        Assert.Equal(
            """Unable to parse test.0. <property name="0" type="int" length="1" bits="0-a" address="0" />""",
            exception.Message
        );
        Assert.Equal(
            """Invalid format for attribute Bits (0-a).""",
            exception.InnerException.Message
        );
    }

    [Fact]
    public async Task RejectsFaultyArray()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0b10000000]);

        var exception = Assert.Throws<Exception>(() => MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" bits="0,a,4,6" address="0x00" />""", 
                "0x00", 
                "0x01", 
                "GBA"
            ),
            driver
        ));

        Assert.Equal(
            """Unable to parse test.0. <property name="0" type="int" length="1" bits="0,a,4,6" address="0" />""",
            exception.Message
        );
        Assert.Equal(
            """Invalid format for attribute Bits (0,a,4,6).""",
            exception.InnerException.Message
        );
    }

    [Fact]
    public async Task RejectsFaultyIndex()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0b10000000]);

        var exception = Assert.Throws<Exception>(() => MapperTestHelper.CreateTestInstance(
            clientnotifier,
            MapperTestHelper.CreateMapper(
                """<property name="0" type="int" length="1" bits="a" address="0x00" />""", 
                "0x00", 
                "0x01", 
                "GBA"
            ),
            driver
        ));

        Assert.Equal(
            """Unable to parse test.0. <property name="0" type="int" length="1" bits="a" address="0" />""",
            exception.Message
        );
        Assert.Equal(
            """Invalid format for attribute Bits (a).""",
            exception.InnerException.Message
        );
    }
}
