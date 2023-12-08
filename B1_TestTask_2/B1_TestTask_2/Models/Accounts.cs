using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_TestTask_2.Models
{
    public class Accounts
    {
        public int Id { get; set; }
        public int AccountNumber { get; set; }
        public int ClassId { get; set; }
        public int AccountGroupId { get; set; }

        public Classes Class { get; set; }
        public AccountDetails AccountDetails { get; set; }
        public AccountGroups AccountGroups { get; set; }
    }
}
