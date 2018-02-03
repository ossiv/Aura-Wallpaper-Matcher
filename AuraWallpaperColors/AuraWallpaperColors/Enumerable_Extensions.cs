using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraWallpaperColors
{
    public static class Enumerable_Extensions
    {
        public static IEnumerable<(T, int)> Enumerate<T>(this IEnumerable<T> @this)
        {
            return @this.Select((v, i) => (v, i));
        }
    }
}
