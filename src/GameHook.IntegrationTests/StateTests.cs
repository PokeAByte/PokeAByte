using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GameHook.IntegrationTests
{
    [TestClass]
    public class StateTests : BaseTest
    {
        // Red/Blue State Tests
        [TestMethod]
        public async Task RedBlue_State_NoPkmn_OK()
        {
            await Load_GB_PokemonRed(2);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "No Pokemon");
            mapper.AssertAreEqual("player.team.0.level", 0xD18C, [0x00], 0);
        }
        [TestMethod]
        public async Task RedBlue_State_Overworld_OK()
        {
            await Load_GB_PokemonRed(3);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Overworld");
            mapper.AssertAreEqual("battle.mode", 0xD057, [0x00], null);
        }
        [TestMethod]
        public async Task RedBlue_State_ToBattle_OK()
        {
            await Load_GB_PokemonRed(4);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "To Battle");
            mapper.AssertAreEqual("battle.other.battle_start", 0xCCF5, [0x00], 0);
        }
        [TestMethod]
        public async Task RedBlue_State_Battle_OK()
        {
            await Load_GB_PokemonRed(5);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Battle");
        }
        [TestMethod]
        public async Task RedBlue_State_FromBattle_OK()
        {
            await Load_GB_PokemonRed(6);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "From Battle");
            mapper.AssertAreEqual("battle.other.low_health_alarm", 0xCCF6, [0x01], "Disabled");
        }
        // Red/Blue-Deprecated State Tests
        [TestMethod]
        public async Task RedBlueDeprecated_State_NoPkmn_OK()
        {
            await Load_GB_PokemonRedDeprecated(2);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "No Pokemon");
            mapper.AssertAreEqual("player.team.0.level", 0xD18C, [0x00], 0);
        }
        [TestMethod]
        public async Task RedBlueDeprecated_State_Overworld_OK()
        {
            await Load_GB_PokemonRedDeprecated(3);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Overworld");
            mapper.AssertAreEqual("battle.type", 0xD057, [0x00], "None");
        }
        [TestMethod]
        public async Task RedBlueDeprecated_State_ToBattle_OK()
        {
            await Load_GB_PokemonRedDeprecated(4);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "To Battle");
            mapper.AssertAreEqual("battle.turnInfo.battleStart", 0xCCF5, [0x00], 0);
        }
        [TestMethod]
        public async Task RedBlueDeprecated_State_Battle_OK()
        {
            await Load_GB_PokemonRedDeprecated(5);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Battle");
        }
        [TestMethod]
        public async Task RedBlueDeprecated_State_FromBattle_OK()
        {
            await Load_GB_PokemonRedDeprecated(6);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "From Battle");
            mapper.AssertAreEqual("battle.lowHealthAlarm", 0xCCF6, [0x01], "Disabled");
        }
        // Yellow State Tests
        [TestMethod]
        public async Task Yellow_State_NoPkmn_OK()
        {
            await Load_GB_PokemonYellow(1);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "No Pokemon");
            mapper.AssertAreEqual("player.team.0.level", 0xD18B, [0x00], 0);
        }
        [TestMethod]
        public async Task Yellow_State_Overworld_OK()
        {
            await Load_GB_PokemonYellow(2);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Overworld");
            mapper.AssertAreEqual("battle.mode", 0xD056, [0x00], null);
        }
        [TestMethod]
        public async Task Yellow_State_ToBattle_OK()
        {
            await Load_GB_PokemonYellow(3);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "To Battle");
            mapper.AssertAreEqual("battle.other.battle_start", 0xCCF5, [0x00], 0);
        }
        [TestMethod]
        public async Task Yellow_State_Battle_OK()
        {
            await Load_GB_PokemonYellow(4);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Battle");
        }
        [TestMethod]
        public async Task Yellow_State_FromBattle_OK()
        {
            await Load_GB_PokemonYellow(5);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "From Battle");
            mapper.AssertAreEqual("battle.other.low_health_alarm", 0xCCF6, [0x01], "Disabled");
        }
        // Yellow-Deprecated State Tests
        [TestMethod]
        public async Task YellowDeprecated_State_NoPkmn_OK()
        {
            await Load_GB_PokemonYellowDeprecated(1);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "No Pokemon");
            mapper.AssertAreEqual("player.team.0.level", 0xD18B, [0x00], 0);
        }
        [TestMethod]
        public async Task YellowDeprecated_State_Overworld_OK()
        {
            await Load_GB_PokemonYellowDeprecated(2);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Overworld");
            mapper.AssertAreEqual("battle.type", 0xD056, [0x00], "None");
        }
        [TestMethod]
        public async Task YellowDeprecated_State_ToBattle_OK()
        {
            await Load_GB_PokemonYellowDeprecated(3);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "To Battle");
            mapper.AssertAreEqual("battle.turnInfo.battleStart", 0xCCF5, [0x00], 0);
        }
        [TestMethod]
        public async Task YellowDeprecated_State_Battle_OK()
        {
            await Load_GB_PokemonYellowDeprecated(4);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Battle");
        }
        [TestMethod]
        public async Task YellowDeprecated_State_FromBattle_OK()
        {
            await Load_GB_PokemonYellowDeprecated(5);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "From Battle");
            mapper.AssertAreEqual("battle.lowHealthAlarm", 0xCCF6, [0x01], "Disabled");
        }
        // Gold/Silver Tests
        [TestMethod]
        public async Task Gold_State_NoPkmn_OK()
        {
            await Load_GBC_PokemonGold(2);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "No Pokemon");
            mapper.AssertAreEqual("player.team.0.level", 0xDA49, [0x00], 0);
        }
        [TestMethod]
        public async Task Gold_State_Overworld_OK()
        {
            await Load_GBC_PokemonGold(3);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Overworld");
            mapper.AssertAreEqual("battle.mode", 0xD116, [0x00], null);
        }
        // Mapper logic needs work for this state to function correctly
        // [TestMethod]
        // public async Task Gold_State_ToBattle_OK()
        // {
        //     await Load_GBC_PokemonGold(4);
        // 
        //     var mapper = await GameHookClient.GetMapperAsync();
        // 
        //     mapper.AssertAreEqual("meta.state", "To Battle");
        // }
        [TestMethod]
        public async Task Gold_State_Battle_OK()
        {
            await Load_GBC_PokemonGold(5);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Battle");
        }
        // [TestMethod]
        // public async Task Gold_State_FromBattle_OK()
        // {
        //     await Load_GBC_PokemonGold(6);
        // 
        //     var mapper = await GameHookClient.GetMapperAsync();
        // 
        //     mapper.AssertAreEqual("meta.state", "From Battle");
        //     mapper.AssertAreEqual("battle.other.lowHealthAlarm", 0xC6FD, [0x01], "Disabled");
        // }
        //Platinum State Tests
        [TestMethod]
        public async Task Platinum_State_NoPkmn_OK()
        {
            await Load_NDS_PokemonPlatinum(2);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "No Pokemon");
        }
        [TestMethod]
        public async Task Platinum_State_Overworld_OK()
        {
            await Load_NDS_PokemonPlatinum(3);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Overworld");
        }
        [TestMethod]
        public async Task Platinum_State_Battle_OK()
        {
            await Load_NDS_PokemonPlatinum(4);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Battle");
        }
        //HeartGold State Tests
        [TestMethod]
        public async Task HeartGold_State_NoPkmn_OK()
        {
            await Load_NDS_PokemonHeartGold(2);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "No Pokemon");
        }
        [TestMethod]
        public async Task HeartGold_State_Overworld_OK()
        {
            await Load_NDS_PokemonHeartGold(1);

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Overworld");
        }
        [TestMethod]
        public async Task HeartGold_State_Battle_OK()
        {
            await Load_NDS_PokemonHeartGold();

            var mapper = await GameHookClient.GetMapperAsync();

            mapper.AssertAreEqual("meta.state", "Battle");
        }
    }
}
