using CommonRepositories;

namespace EsDb.Model
{
    public class Session : BaseEntity
    {
        public Session()
        {
            Consultations = new HashSet<Consultation>();
            Reviews = new HashSet<Review>();
        }

        public string SessionId { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; }
        public DateTime? LastActivityDate { get; set; }

        public virtual ICollection<Consultation> Consultations { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
