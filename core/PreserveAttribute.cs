namespace FashionApp.core
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class PreserveAttribute : Attribute
    {
        public bool AllMembers { get; set; }
        public bool Conditional { get; set; }
        public string[] MemberNames { get; set; } = Array.Empty<string>();
        public string[] TypeNames { get; set; } = Array.Empty<string>();
        public PreserveAttribute() { }
        public PreserveAttribute(string typeName)
        {
            TypeNames = new[] { typeName };
        }
        public PreserveAttribute(string typeName, string memberName)
        {
            TypeNames = new[] { typeName };
            MemberNames = new[] { memberName };
        }
    }
}
