using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace GameHook.IntegrationTests
{
    // TODO: Write badge write tests to flip different badges to true / false.

    [TestClass]
    public class FullXmlTests : BaseTest
    {
        [Ignore]
        [TestMethod]
        public async Task Property_OK_BinaryCodedDecimal()
        {
            await Task.CompletedTask;
        }

        [Ignore]
        [TestMethod]
        public async Task Property_OK_BitFieldProperty()
        {
            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task Property_OK_BitProperty()
        {
            await Load_GBC_PokemonCrystal();

            var mapper = await GameHookClient.GetMapperAsync();
            var value_1 = mapper.Properties.Single(x => x.Path == "player.badges.1");
            var value_2 = (await GameHookClient.GetPropertiesAsync()).Single(x => x.Path == "player.badges.1");
            var value_3 = await GameHookClient.GetPropertyAsync("player.badges.1");

            GameHookAssert.ArePropertiesEqual(new OpenAPI.GameHook.PropertyModel
            {
                Address = 0xD857,
                Bytes = new int[] { 0xFF },
                Bits = "1",
                Path = "player.badges.1",
                Length = 1,
                Type = "bool",
                IsFrozen = false,
                Description = "hiveBadge",
                Value = true
            }, value_1);

            GameHookAssert.ArePropertiesEqual(value_1, value_2);
            GameHookAssert.ArePropertiesEqual(value_1, value_3);
        }

        [Ignore]
        [TestMethod]
        public async Task Property_OK_BooleanProperty()
        {
            await Task.CompletedTask;
        }

        [TestMethod]
        public async Task Property_OK_Integer_A()
        {
            await Load_GBC_PokemonCrystal();

            var mapper = await GameHookClient.GetMapperAsync();
            var value_1 = mapper.Properties.Single(x => x.Path == "player.team_count");
            var value_2 = (await GameHookClient.GetPropertiesAsync()).Single(x => x.Path == "player.team_count");
            var value_3 = await GameHookClient.GetPropertyAsync("player.team_count");

            GameHookAssert.ArePropertiesEqual(new OpenAPI.GameHook.PropertyModel
            {
                Address = 0xDCD7,
                Bytes = new int[] { 0x01 },
                Bits = null,
                Path = "player.team_count",
                Length = 1,
                Type = "int",
                IsFrozen = false,
                Value = (long)1
            }, value_1);

            GameHookAssert.ArePropertiesEqual(value_1, value_2);
            GameHookAssert.ArePropertiesEqual(value_1, value_3);
        }

        [TestMethod]
        public async Task Property_OK_Integer_B()
        {
            await Load_GBC_PokemonCrystal();

            var mapper = await GameHookClient.GetMapperAsync();
            var value_1 = mapper.Properties.Single(x => x.Path == "bag.money");
            var value_2 = (await GameHookClient.GetPropertiesAsync()).Single(x => x.Path == "bag.money");
            var value_3 = await GameHookClient.GetPropertyAsync("bag.money");

            GameHookAssert.ArePropertiesEqual(new OpenAPI.GameHook.PropertyModel
            {
                Address = 0xD84E,
                Bytes = new int[] { 0x07, 0xB0, 0x93 },
                Bits = null,
                Path = "bag.money",
                Length = 3,
                Type = "int",
                IsFrozen = false,
                Value = (long)503955
            }, value_1);

            GameHookAssert.ArePropertiesEqual(value_1, value_2);
            GameHookAssert.ArePropertiesEqual(value_1, value_3);
        }

        [TestMethod]
        public async Task Property_OK_Reference()
        {
            await Load_GBC_PokemonCrystal();

            var mapper = await GameHookClient.GetMapperAsync();
            var value_1 = mapper.Properties.Single(x => x.Path == "player.gender");
            var value_2 = (await GameHookClient.GetPropertiesAsync()).Single(x => x.Path == "player.gender");
            var value_3 = await GameHookClient.GetPropertyAsync("player.gender");

            GameHookAssert.ArePropertiesEqual(new OpenAPI.GameHook.PropertyModel
            {
                Address = 0xD472,
                Bytes = new int[] { 0x01 },
                Bits = null,
                Path = "player.gender",
                Length = 1,
                Type = "int",
                IsFrozen = false,
                Reference = "gender",
                Value = "Female"
            }, value_1);

            GameHookAssert.ArePropertiesEqual(value_1, value_2);
            GameHookAssert.ArePropertiesEqual(value_1, value_3);
        }

        [TestMethod]
        public async Task Property_OK_String()
        {
            await Load_GBC_PokemonCrystal();

            var mapper = await GameHookClient.GetMapperAsync();
            var playerName = mapper.Properties.Single(x => x.Path == "player.name");
            var playerName2 = (await GameHookClient.GetPropertiesAsync()).Single(x => x.Path == "player.name");
            var playerName3 = await GameHookClient.GetPropertyAsync("player.name");

            GameHookAssert.ArePropertiesEqual(new OpenAPI.GameHook.PropertyModel
            {
                Address = 0xD47D,
                Bytes = new int[] { 0x92, 0xA7, 0xA4, 0xAB, 0xAB, 0xB9, 0x50, 0x50, 0x00, 0x00, 0x00 },
                Bits = null,
                Path = "player.name",
                Length = 11,
                Type = "string",
                Reference = "defaultCharacterMap",
                IsFrozen = false,
                Value = "Shellz"
            }, playerName);

            GameHookAssert.ArePropertiesEqual(playerName, playerName2);
            GameHookAssert.ArePropertiesEqual(playerName, playerName3);
        }

        [Ignore]
        [TestMethod]
        public async Task Property_OK_UnsignedInteger()
        {
            await Task.CompletedTask;
        }

        // TODO: Test writing a value.
        // TODO: Test writing a string that is 8 characters with AAA. Should insert AAA + termination character at the end.
        // TODO: Test writing a string that is 8 characters with AAAAAAAA. Should insert the termination character at the end, replacing the last A.
    }
}