namespace ConquerWeb.Models
{
    public class TopPlayer
    {
        public int Id { get; set; }
        public int TopType { get; set; }
        public string Name { get; set; }
        public short Level { get; set; }
        public short Job { get; set; }
        public string GuildName { get; set; }
        public byte Nobility { get; set; }
        public long Param { get; set; }
        public int VIPLevel { get; set; }
        public string Spouse { get; set; }
        public int Avatar { get; set; }

        public string JobName
        {
            get
            {
                switch (Job)
                {
                    case 10: return "InternTrojan";
                    case 11: return "Trojan";
                    case 12: return "VeteranTrojan";
                    case 13: return "TigerTrojan";
                    case 14: return "DragonTrojan";
                    case 15: return "TrojanMaster";
                    case 20: return "InternWarrior";
                    case 21: return "Warrior";
                    case 22: return "BrassWarrior";
                    case 23: return "SilverWarrior";
                    case 24: return "GoldWarrior";
                    case 25: return "WarriorMaster";
                    case 40: return "InternArcher";
                    case 41: return "Archer";
                    case 42: return "EagleArcher";
                    case 43: return "TigerArcher";
                    case 44: return "DragonArcher";
                    case 45: return "ArcherMaster";
                    case 190:
                    case 100: return "InternTaoist";
                    case 191: return "Taoist";
                    case 132: return "WaterTaoist";
                    case 133: return "WaterWizard";
                    case 134: return "WaterMaster";
                    case 135: return "WaterSaint";
                    case 142: return "FireTaoist";
                    case 143: return "FireWizard";
                    case 144: return "FireMaster";
                    case 145: return "FireSaint";
                    default:
                        if (Job > 0) return "Unknown";
                        return string.Empty;
                }
            }
        }
    }
}