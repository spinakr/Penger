namespace Domain.ValueObjects
{
    public class CurrencyType : Enumeration
    {
        public CurrencyType() { }
        private CurrencyType(int value, string displayName) : base(value, displayName) { }

        public static readonly CurrencyType NA = new CurrencyType(0, "");
        public static readonly CurrencyType USD = new CurrencyType(1, "USD");
        public static readonly CurrencyType EUR = new CurrencyType(2, "EUR");
        public static readonly CurrencyType GBP = new CurrencyType(3, "GBP");
        public static readonly CurrencyType JPY = new CurrencyType(4, "JPY");
        public static readonly CurrencyType CHF = new CurrencyType(5, "CHF");
        public static readonly CurrencyType NOK = new CurrencyType(6, "NOK");
        public static readonly CurrencyType SEK = new CurrencyType(7, "SEK");
    }
}