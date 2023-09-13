namespace Cdk.Core
{
    public readonly struct Parameter
    {
        public string Name { get; }
        public string Description { get; }
        public object? DefaultValue { get; }
        public bool IsSecure { get; }

        public Parameter(string name, string description, object? defaultValue = default, bool isSecure = false)
        {
            Name = name;
            Description = description;
            DefaultValue = defaultValue;
            IsSecure = isSecure;
        }
    }
}
