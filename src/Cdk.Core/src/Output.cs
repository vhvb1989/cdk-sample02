namespace Cdk.Core
{
    public class Output
    {
        public string Name { get; }
        public string Value { get; }
        public bool IsLiteral { get; }

        public Output(string name, string value, bool isLiteral = false)
        {
            Name = name;
            Value = value;
            IsLiteral = isLiteral;
        }
    }
}
