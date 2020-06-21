using System.Text.Json;

namespace CommonServices
{
    public static class JsonExtensions
    {
        public static string Gsp(this JsonElement element, string propertyName)
        {
            return element.GetProperty(propertyName).GetString();
        }
    }
}