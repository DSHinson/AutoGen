using Generator.Attributes;

namespace TestAuoGen.Api.models_for_auto_gen
{
    [AutoGenRepository]
    [AutoGenController]
    public class ShoppingCart
    {
        public Guid Id { get; set; }
        public List<string> CartItems { get; set; }
    }
}
