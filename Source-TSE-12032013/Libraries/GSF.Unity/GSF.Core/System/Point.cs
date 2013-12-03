using MS.Internal;
using System;
using System.ComponentModel;
using System.Runtime;
namespace System.Windows
{
    /// <summary>Represents an x- and y-coordinate pair in two-dimensional space.</summary>
    [Serializable]
    public struct Point : IFormattable
    {
        internal double _x;
        internal double _y;
        /// <summary>Gets or sets the <see cref="P:System.Windows.Point.X" />-coordinate value of this <see cref="T:System.Windows.Point" /> structure. </summary>
        /// <returns>The <see cref="P:System.Windows.Point.X" />-coordinate value of this <see cref="T:System.Windows.Point" /> structure.  The default value is 0.</returns>
        public double X
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._x;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._x = value;
            }
        }
        /// <summary>Gets or sets the <see cref="P:System.Windows.Point.Y" />-coordinate value of this <see cref="T:System.Windows.Point" />. </summary>
        /// <returns>The <see cref="P:System.Windows.Point.Y" />-coordinate value of this <see cref="T:System.Windows.Point" /> structure.  The default value is 0.</returns>
        public double Y
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._y;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._y = value;
            }
        }
        /// <summary>Compares two <see cref="T:System.Windows.Point" /> structures for equality. </summary>
        /// <returns>true if both the <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> coordinates of <paramref name="point1" /> and <paramref name="point2" /> are equal; otherwise, false.</returns>
        /// <param name="point1">The first <see cref="T:System.Windows.Point" /> structure to compare.</param>
        /// <param name="point2">The second <see cref="T:System.Windows.Point" /> structure to compare.</param>
        public static bool operator ==(Point point1, Point point2)
        {
            return point1.X == point2.X && point1.Y == point2.Y;
        }
        /// <summary>Compares two <see cref="T:System.Windows.Point" /> structures for inequality. </summary>
        /// <returns>true if <paramref name="point1" /> and <paramref name="point2" /> have different <see cref="P:System.Windows.Point.X" /> or <see cref="P:System.Windows.Point.Y" /> coordinates; false if <paramref name="point1" /> and <paramref name="point2" /> have the same <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> coordinates.</returns>
        /// <param name="point1">The first point to compare.</param>
        /// <param name="point2">The second point to compare.</param>
        public static bool operator !=(Point point1, Point point2)
        {
            return !(point1 == point2);
        }
        /// <summary>Compares two <see cref="T:System.Windows.Point" /> structures for equality. </summary>
        /// <returns>true if <paramref name="point1" /> and <paramref name="point2" /> contain the same <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> values; otherwise, false.</returns>
        /// <param name="point1">The first point to compare.</param>
        /// <param name="point2">The second point to compare.</param>
        public static bool Equals(Point point1, Point point2)
        {
            return point1.X.Equals(point2.X) && point1.Y.Equals(point2.Y);
        }
        /// <summary>Determines whether the specified <see cref="T:System.Object" /> is a <see cref="T:System.Windows.Point" /> and whether it contains the same coordinates as this <see cref="T:System.Windows.Point" />. </summary>
        /// <returns>true if <paramref name="o" /> is a <see cref="T:System.Windows.Point" /> and contains the same <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> values as this <see cref="T:System.Windows.Point" />; otherwise, false.</returns>
        /// <param name="o">The <see cref="T:System.Object" /> to compare.</param>
        public override bool Equals(object o)
        {
            if (o == null || !(o is Point))
            {
                return false;
            }
            Point point = (Point)o;
            return Point.Equals(this, point);
        }
        /// <summary>Compares two <see cref="T:System.Windows.Point" /> structures for equality.</summary>
        /// <returns>true if both <see cref="T:System.Windows.Point" /> structures contain the same <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> values; otherwise, false.</returns>
        /// <param name="value">The point to compare to this instance.</param>
        public bool Equals(Point value)
        {
            return Point.Equals(this, value);
        }
        /// <summary>Returns the hash code for this <see cref="T:System.Windows.Point" />.</summary>
        /// <returns>The hash code for this <see cref="T:System.Windows.Point" /> structure.</returns>
        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode();
        }
        /// <summary>Constructs a <see cref="T:System.Windows.Point" /> from the specified <see cref="T:System.String" />.</summary>
        /// <returns>The equivalent <see cref="T:System.Windows.Point" /> structure. </returns>
        /// <param name="source">A string representation of a point.</param>
        /// <exception cref="T:System.FormatException">
        ///   <paramref name="source" /> is not composed of two comma- or space-delimited double values.</exception>
        /// <exception cref="T:System.InvalidOperationException">
        ///   <paramref name="source" /> does not contain two numbers.-or-<paramref name="source" /> contains too many delimiters.</exception>
        public static Point Parse(string source)
        {
            Point result = new Point();
            return result;
        }
        /// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Point" />. </summary>
        /// <returns>A <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> values of this <see cref="T:System.Windows.Point" /> structure.</returns>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override string ToString()
        {
            return this.ConvertToString(null, null);
        }
        /// <summary>Creates a <see cref="T:System.String" /> representation of this <see cref="T:System.Windows.Point" />. </summary>
        /// <returns>A <see cref="T:System.String" /> containing the <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> values of this <see cref="T:System.Windows.Point" /> structure.</returns>
        /// <param name="provider">Culture-specific formatting information.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public string ToString(IFormatProvider provider)
        {
            return this.ConvertToString(null, provider);
        }
        /// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. For a description of this member, see <see cref="M:System.IFormattable.ToString(System.String,System.IFormatProvider)" />.</summary>
        /// <returns>A string containing the value of the current instance in the specified format. </returns>
        /// <param name="format">The string specifying the format to use. -or- null to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
        /// <param name="provider">The IFormatProvider to use to format the value. -or- null to obtain the numeric format information from the current locale setting of the operating system.</param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        string IFormattable.ToString(string format, IFormatProvider provider)
        {
            return this.ConvertToString(format, provider);
        }
        internal string ConvertToString(string format, IFormatProvider provider)
        {
            return string.Format(provider, string.Concat(new string[]
			{
				"{1:",
				format,
				"}{0}{2:",
				format,
				"}"
			}), new object[]
			{
				",",
				this._x,
				this._y
			});
        }
        /// <summary>Creates a new <see cref="T:System.Windows.Point" /> structure that contains the specified coordinates. </summary>
        /// <param name="x">The x-coordinate of the new <see cref="T:System.Windows.Point" /> structure. </param>
        /// <param name="y">The y-coordinate of the new <see cref="T:System.Windows.Point" /> structure. </param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Point(double x, double y)
        {
            this._x = x;
            this._y = y;
        }
        /// <summary>Offsets a point's <see cref="P:System.Windows.Point.X" /> and <see cref="P:System.Windows.Point.Y" /> coordinates by the specified amounts.</summary>
        /// <param name="offsetX">The amount to offset the point's<see cref="P:System.Windows.Point.X" /> coordinate. </param>
        /// <param name="offsetY">The amount to offset thepoint's <see cref="P:System.Windows.Point.Y" /> coordinate.</param>
        public void Offset(double offsetX, double offsetY)
        {
            this._x += offsetX;
            this._y += offsetY;
        }
    }
}
