namespace Crucible.Models
{
    public class Writeup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Impact { get; set; }
        public string Description { get; set; }
        public string Solution { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
