using Generator.Attributes;

namespace TestAuoGen.Api.models_for_auto_gen
{
    [AutoGenRepository]
    [AutoGenController]
    public class User
    {
        public Guid Id { get; set; }
        public string? Name { get; set; } = null;
    }
}
