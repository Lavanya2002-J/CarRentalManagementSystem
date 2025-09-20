using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalManagementSystem.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }
        [Required] 
        [ForeignKey("Customer")]
        public int CustomerID { get; set; }
        [Required]
        [ForeignKey("Car")]
        public Guid CarID { get; set; }

        public string Ratings { get; set; }
        public string FeedBack { get; set; }
    }
}
