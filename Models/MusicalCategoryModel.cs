namespace backend.Models
{
    public class MusicalCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public virtual MusicalCategory Parent { get; set; }
        public virtual ICollection<MusicalCategory> Children { get; set; }
    }

}
