using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PokeAByte.Domain.Test.TypeReadTests;

public class InvalidReferenceTest
{
    [Fact]
    public async Task UndefinedReference()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([0]);
        var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            """
            <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GB" version="1.0.0">
                <memory>
                    <read start="0x00" end="0x01"/>
                </memory>
                <properties>
                    <test>
                        <property name="string" type="string" address="0x00" length="1" />
                    </test>
                </properties>
            </mapper>
            """,
            driver
        );

        await instance.Read();
        await instance.Read();
        await instance.DisposeAsync();
        Assert.NotEmpty(clientnotifier.Errors);
        Assert.Equal("Error", clientnotifier.Errors.Last().Title);
        Assert.Equal(
            "The given key 'defaultCharacterMap' was not present in the dictionary.",
            clientnotifier.Errors.Last().Detail
        );
    }

    [Fact]
    public async Task InvalidKey()
    {
        var clientnotifier = new TestClientNotifier();
        var driver = new TestDriver([1]);
        var instance = MapperTestHelper.CreateTestInstance(
            clientnotifier,
            """
            <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="GB" version="1.0.0">
                <memory>
                    <read start="0x00" end="0x01"/>
                </memory>
                <properties>
                    <test>
                        <property name="0" type="int" reference="number_word" address="0x00" length="1" />
                    </test>
                </properties>
                <references>
                    <number_word>
                        <entry key="0x01" value="One" />	
                        <entry key="0x02" value="Two" />
                        <entry key="0x03" value="Three" />
                        <entry key="0x03" value="Trois" />
                    </number_word>
                </references>
            </mapper>
            """,
            driver
        );

        await instance.Read();
        await instance.Read();
        Assert.Empty(clientnotifier.Errors);
        Assert.Equal("One", instance.Mapper.get_property_value("test.0"));

        await driver.WriteBytes(0, [3]);
        await instance.Read();

        Assert.NotEmpty(clientnotifier.Errors);
        Assert.Equal("Error", clientnotifier.Errors.Last().Title);
        Assert.Equal(
            "Can not determine reference value. The glossary for 'number_word' contains more than one value for the key '3' (0x3).",
            clientnotifier.Errors.Last().Detail
        );

        await instance.DisposeAsync();
    }
}