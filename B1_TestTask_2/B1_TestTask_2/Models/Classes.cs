using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_TestTask_2.Models
{
    public class Classes
    {
        [Key]
        public int ClassNumber { get; set; }
        public string ClassName { get; set; }

        public int FileId { get; set; }
        public Files File { get; set; }
        public ICollection<Accounts> Accounts { get; set; }
    }
}
