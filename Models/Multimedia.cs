using System.ComponentModel.DataAnnotations;

namespace multimedia_storage.Models
{
    public class Multimedia
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string extension { get; set; }
        public double size { get; set; }
        public string location { get; set; }
        public string image { get; set; }

    }
}
