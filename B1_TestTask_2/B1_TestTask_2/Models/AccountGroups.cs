using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_TestTask_2.Models
{
    public class AccountGroups
    {
        public int Id { get; set; }
        public int AccountGroup { get; set; }

        public ICollection<Accounts> Accounts { get; set; }
    }
}
