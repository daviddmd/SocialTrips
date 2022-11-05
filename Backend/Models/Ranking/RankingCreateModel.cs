using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.Ranking
{
    public class RankingCreateModel
    {
        [Required]
        public string Description { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid Format")]
        public string Color { get; set; }
        [Required]
        public double MinimumKilometers { get; set; }
    }
}
