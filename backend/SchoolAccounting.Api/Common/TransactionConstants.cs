namespace SchoolAccounting.Api.Common;

public static class TransactionConstants
{
    // Transaction number prefixes
    public const string TxnPrefix = "TXN-";
    public const string RevPrefix = "REV-";
    public const string RcpPrefix = "RCP-";

    // Transaction / ledger types
    public const string TypeIncome = "income";
    public const string TypeExpense = "expense";

    // Journal entry types
    public const string EntryDebit = "debit";
    public const string EntryCredit = "credit";

    // Journal classification
    public const string JournalTransaction = "transaction";
    public const string JournalClosing = "closing";
    public const string JournalOpening = "opening";
}
