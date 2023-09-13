namespace Cdk.Core
{
    public class Output
    {
        public string Name { get; }
        public string Value { get; }

        public Output(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
