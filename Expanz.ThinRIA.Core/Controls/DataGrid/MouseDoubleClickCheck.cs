using System.Windows.Input;
using System.Windows;
using System;

namespace Expanz.ThinRIA.Controls
{
    public static class MouseDoubleClickCheck
    {
        #region Member Variables
        private static DateTime _lastClicked = DateTime.Now.AddSeconds(-2);
        private static Point _lastMousePosition = new Point(1, 1); 
        #endregion

        #region Constants
        private const double MousePositionTolerance = 20; 
        #endregion

        #region Public Methods
        /// <summary>
        /// function to check whether the mouse is double clicked or not.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsDoubleClick(this MouseButtonEventArgs e)
        {
            bool isDoubleClick = ((DateTime.Now.Subtract(_lastClicked) < TimeSpan.FromMilliseconds(500)) && (e.GetPosition(null).GetDistance(_lastMousePosition) < MousePositionTolerance));

            if (isDoubleClick)
                _lastClicked = DateTime.Now.AddSeconds(-2); // To prevent double double-clicks
            else
                _lastClicked = DateTime.Now;

            _lastMousePosition = e.GetPosition(null);

            return isDoubleClick;
        }

        /// <summary>
        /// function to get the distance
        /// </summary>
        /// <param name="current"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static double GetDistance(this Point current, Point other)
        {
            double x = current.X - other.X;
            double y = current.Y - other.Y;
            return Math.Sqrt(x * x + y * y);
        } 
        #endregion
    }
}