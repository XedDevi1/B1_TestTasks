namespace B1_TestTask_2.ViewModels
{
    public class AccountDisplayModel
    {
        public int AccountNumber { get; set; }
        public int AccountGroup { get; set; }
        public string ClassName { get; set; }
        public decimal ActiveOpeningBalance { get; set; }
        public decimal PassiveOpeningBalance { get; set; }
        public decimal DebitTurnover { get; set; }
        public decimal LoanTurnover { get; set; }
        public decimal ActiveClosingBalance { get; set; }
        public decimal PassiveClosingBalance { get; set; }
        public bool IsGroupSummary { get; set; }
        public bool IsClassSummary { get; set; }
        public bool IsClassHeader { get; set; }
        public string DisplayText { get; set; }
    }
}
