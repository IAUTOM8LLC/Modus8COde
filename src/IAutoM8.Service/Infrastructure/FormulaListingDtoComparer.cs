using IAutoM8.Service.Formula.Dto;

namespace IAutoM8.Service.Infrastructure
{
    public class FormulaListingDtoComparer
    {
        public bool Equals(FormulaListingDto x, FormulaListingDto y)
        {
            if (object.ReferenceEquals(x, y))
                return true;

            if (x == null || y == null)
                return false;

            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(FormulaListingDto obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
