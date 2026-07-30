using System.Linq;
using System.Threading.Tasks;

namespace Catering
{
    public class CateringRecognizer
    {
        private readonly string[] _invalidEntres = new[]
        {
            "worm",
            "dirt",
            "rock",
            "mud",
            "earth",
            "virus"
        };

        private readonly string[] _invalidDrinks = new[]
        {
            "shake",
            "mud",
            "rain",
            "distilled"
        };

        public Task<bool> ValidateEntre(string entre)
        {
            var value = entre.ToLowerInvariant();
            var isValid = !_invalidEntres.Any(e => value.Contains(e));
            return Task.FromResult(isValid);
        }

        public Task<bool> ValidateDrink(string drink)
        {
            var value = drink.ToLowerInvariant();
            var isValid = !_invalidDrinks.Any(e => value.Contains(e));
            return Task.FromResult(isValid);
        }
    }
}
