using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_TestTask_2.Models
{
    public class Files
    {
        [Key]
        public int Id { get; set; }
        public string FileName { get; set; }

        public ICollection<Classes> Classes { get; set; }
    }
}
