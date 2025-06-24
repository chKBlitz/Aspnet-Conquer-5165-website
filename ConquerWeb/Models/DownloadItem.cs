using System;

namespace ConquerWeb.Models
{
    public class DownloadItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string File_Name { get; set; }
        public decimal File_Size_MB { get; set; }
        public string Download_Url { get; set; }
        public DateTime Publish_Date { get; set; }
        public string Category { get; set; }
    }
}