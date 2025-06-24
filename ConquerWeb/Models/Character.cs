using System;

namespace ConquerWeb.Models
{
    public class Character
    {
        public int UID { get; set; }
        public string Name { get; set; }
        public short Level { get; set; }
        public long Experience { get; set; }
        public string Spouse { get; set; }
        public short Body { get; set; }
        public short Face { get; set; }
        public short Hair { get; set; }
        public long Silvers { get; set; }
        public long WHSilvers { get; set; }
        public int CPs { get; set; }
        public int GuildID { get; set; }
        public int Version { get; set; }
        public int Map { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public byte Job { get; set; }
        public byte PreviousJob1 { get; set; }
        public byte PreviousJob2 { get; set; }
        public short Strength { get; set; }
        public short Agility { get; set; }
        public short Vitality { get; set; }
        public short Spirit { get; set; }
        public short ExtraStats { get; set; }
        public short Life { get; set; }
        public short Mana { get; set; }
        public int VirtuePoints { get; set; }
        public int DBScrolls { get; set; }
        public int VIPLevelToReceive { get; set; }
        public int VIPDaysToReceive { get; set; }
        public int DoubleExp { get; set; }
        public string WHPassword { get; set; }
        public int VIPLevel { get; set; }
        public int VIP { get; set; }
        public int PumpkinPoints { get; set; }
        public int TreasurePoints { get; set; }
        public int CTBPoints { get; set; }
        public int MetScrolls { get; set; }
        public int DragonGems { get; set; }
        public int PhoenixGems { get; set; }
        public int RainbowGems { get; set; }
        public int KylinGems { get; set; }
        public int FuryGems { get; set; }
        public int VioletGems { get; set; }
        public int MoonGems { get; set; }
        public int TortoiseGems { get; set; }
        public int Dragonballs { get; set; }
        public int Paradises { get; set; }
        public int GarmentToken { get; set; }
        public int OnlineTime { get; set; }
        public int CurrentKills { get; set; }
        public byte Nobility { get; set; }
        public int PKPoints { get; set; }
        public int VotePoints { get; set; }
        public int ClassicPoints { get; set; }
        public int PassiveSkills { get; set; }
        public int HeavensBlessing { get; set; }
        public DateTime? LastLogin { get; set; }
        public int BotjailedTime { get; set; }
        public byte MutedRecord { get; set; }
        public int MutedTime { get; set; }
        public int CurrentHonor { get; set; }
        public int TotalHonor { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }

        public string JobName
        {
            get
            {
                switch (Job)
                {
                    case 1: return "Warrior";
                    case 2: return "Trojan";
                    case 3: return "Archer";
                    case 4: return "Water Taoist";
                    case 5: return "Fire Taoist";
                    case 6: return "Ninja";
                    case 7: return "Monk";
                    case 8: return "Pirate";
                    case 9: return "Dragon Warrior";
                    case 10: return "Thunder Taoist";
                    default:
                        if (Job > 0) return "Unknown";
                        return string.Empty;
                }
            }
        }
    }
}