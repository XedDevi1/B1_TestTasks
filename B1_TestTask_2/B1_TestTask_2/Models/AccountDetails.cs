using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace B1_TestTask_2.Models
{
    public class AccountDetails
    {
        public int Id { get; set; }
        public decimal ActiveOpeningBalance { get; set; }
        public decimal PassiveOpeningBalance { get; set; }
        public decimal DebitTurnover { get; set; }
        public decimal LoanTurnover { get; set; }
        public decimal ActiveClosingBalance { get; set; }
        public decimal PassiveClosingBalance { get; set; }

        public Accounts Account { get; set; }
    }
}
