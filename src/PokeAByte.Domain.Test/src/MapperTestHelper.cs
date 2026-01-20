using System.Linq;
using PokeAByte.Domain.Interfaces;
using PokeAByte.Domain.Logic;
using Voyager.UnitTestLogger;

namespace PokeAByte.Domain.Test;

public static class MapperTestHelper
{
    public static SpyLog<PokeAByteInstance> InstanceLogger = new SpyLog<PokeAByteInstance>();
    public static SpyLog<ScriptConsole> ScriptLogger = new SpyLog<ScriptConsole>();
    public static string CreateMapper(string properties, string memoryStart, string memoryEnd, string system)
    {
        return $"""
        <mapper id="8213664d-a1e8-4d4c-a569-1c57be540d46" name="TestMapper" platform="{system}" version="1.0.0">
            <memory>
                <read start="{memoryStart}" end="{memoryEnd}"/>
            </memory>
            <properties>
                <test>
                    {properties}
                </test>
            </properties>
            <references>
                <digits>
                    <entry key="1" value="one" />
                    <entry key="2" value="two" />
                    <entry key="3" value="three" />
                </digits>
                <defaultCharacterMap>
                    <entry key="0" />
                    <entry key="1" value="a" />
                    <entry key="2" value="b" />
                    <entry key="3" value="c" />
                    <entry key="4" value="d" />
                    <entry key="5" value="e" />
                    <entry key="6" value="f" />
                    <entry key="7" value="g" />
                    <entry key="8" value="h" />
                    <entry key="9" value="i" />
                    <entry key="10" value="j" />
                    <entry key="11" value="k" />
                    <entry key="12" value="l" />
                    <entry key="13" value="m" />
                    <entry key="14" value="n" />
                    <entry key="15" value="o" />
                    <entry key="16" value="p" />
                    <entry key="17" value="q" />
                    <entry key="18" value="r" />
                    <entry key="19" value="s" />
                    <entry key="20" value="t" />
                    <entry key="21" value="u" />
                    <entry key="22" value="v" />
                    <entry key="23" value="w" />
                    <entry key="24" value="x" />
                    <entry key="25" value="y" />
                    <entry key="26" value="z" />
                    <entry key="27" value=" " />
                </defaultCharacterMap>
            </references>
        </mapper>
        """;
    }

    public static SpyData[] GetLogData()
    {
        return InstanceLogger.GetSpyData().ToArray();
    }

    public static PokeAByteInstance CreateTestInstance(
        IClientNotifier notifier,
        MapperContent mapperData,
        IPokeAByteDriver driver
    )
    {
        InstanceLogger = new SpyLog<PokeAByteInstance>();
        ScriptLogger = new SpyLog<ScriptConsole>();

        return new PokeAByteInstance(
            InstanceLogger,
            new ScriptConsole(ScriptLogger),
            notifier,
            mapperData,
            driver
        );
    }

    public static PokeAByteInstance CreateTestInstance(
        IClientNotifier notifier,
        string mapperContent,
        IPokeAByteDriver driver,
        string scriptPath = null,
        string scriptRoot = null
    )
    {
        InstanceLogger = new SpyLog<PokeAByteInstance>();
        ScriptLogger = new SpyLog<ScriptConsole>();
        return new PokeAByteInstance(
            InstanceLogger,
            new ScriptConsole(ScriptLogger),
            notifier,
            new MapperContent("test.xml", mapperContent, scriptPath, scriptRoot),
            driver
        );
    }
}
