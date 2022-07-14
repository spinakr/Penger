namespace Domain.ValueObjects
{
    public class Amount : ValueObject
    {
        public double Value { get; }
        public Amount(double value)
        {
            // IsTrue(value >= 0, "Amount cannot be negative");
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static Amount operator +(Amount a1, Amount a2)
        {
            return new Amount(a1.Value + a2.Value);
        }
        public static Amount operator -(Amount a1, Amount a2)
        {
            return new Amount(a1.Value - a2.Value);
        }

        public override string ToString()
        {
            return $"{Math.Round(Value, 2)}";
        }
    }
}