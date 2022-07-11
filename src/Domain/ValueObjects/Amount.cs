namespace Domain.ValueObjects
{
    public class Amount
    {
        public double Value { get; }
        public Amount(double value)
        {
            IsTrue(value >= 0, "Amount cannot be negative");
            Value = value;
        }
    }
}