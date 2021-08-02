using System;

namespace IAutoM8.Global.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Keeps specific DateTime not less then provided min value
        /// </summary>
        public static DateTime ClampBottom(this DateTime date, DateTime min)
            => date > min ? date : min;

        /// <summary>
        /// Keeps specific DateTime not greater then provided max value
        /// </summary>
        public static DateTime ClampTop(this DateTime date, DateTime max)
            => date > max ? max : date;

        /// <summary>
        /// Keeps DateTime in a specific range
        /// </summary>
        public static DateTime Clamp(this DateTime date, DateTime min, DateTime max)
            => ClampTop(ClampBottom(date, min), max);
    }
}
