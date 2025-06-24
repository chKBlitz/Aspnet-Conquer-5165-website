using System;

namespace ConquerWeb.Models
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public DateTime Publish_Date { get; set; }
    }
}