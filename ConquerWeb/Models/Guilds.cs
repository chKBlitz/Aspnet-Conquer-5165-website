namespace ConquerWeb.Models
{
    public class Guild
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int LeaderID { get; set; }
        public string LeaderName { get; set; }
        public string Bulletin { get; set; }
        public long Fund { get; set; }
        public int Wins { get; set; }
        public short Members { get; set; }
        public string LastWinner { get; set; }
    }
}