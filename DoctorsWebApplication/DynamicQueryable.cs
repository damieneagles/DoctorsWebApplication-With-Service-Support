using System.Linq.Dynamic;
using System.Linq.Dynamic.Core;
using System.Text;

namespace DoctorsWebApplication
{
    public static class DynamicQueryable
    {
        public static IQueryable Sort(this IQueryable collection, string sortBy, string direction)
        {
            if (string.IsNullOrEmpty(sortBy))
                throw new ArgumentNullException(nameof(sortBy));
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(sortBy);
                sb.Append(" ");
                sb.Append(direction);
                return collection.OrderBy(sb.ToString().Trim());
            }
        }
    }
}
