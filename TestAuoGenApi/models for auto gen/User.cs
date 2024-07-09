using Generator.Attributes;

namespace TestAuoGenApi.models_for_auto_gen
{
    [AutoGenController]
    public class User
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } = null;
    }
}
