using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace B1_TestTask_1.Models
{
    public class StringsData
    {
        [Key]
        public int id { get; set; }

        public DateTime DateColumn { get; set; }
        public string LatinCharsColumn { get; set; }
        public string RussianCharsColumn { get; set; }
        public int IntegerColumn { get; set; }
        public double DoubleColumn { get; set; }
    }
}
