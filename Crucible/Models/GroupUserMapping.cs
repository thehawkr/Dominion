namespace Crucible.Models
{
    public class GroupUserMapping
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
