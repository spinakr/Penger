namespace Domain.ValueObjects
{
    public class Symbol : ValueObject
    {
        public string Value { get; }
        public Symbol(string value)
        {
            IsTrue(value.Length <= 15, "Symbol cannot be longer than 15 characters");
            Value = Matches(value, @"^[a-zA-Z0-9\.\-\=]+$");
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}