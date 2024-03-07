using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ww.Utilities.Extensions
{
	[DebuggerStepThrough]
	public static class EnumExtensions
	{
		#region Code taken from http://somewebguy.wordpress.com/2010/02/23/enumeration-extensions-2/
		#region Extension Methods
		/// <summary>
		/// Adds an enumerated type and returns the new value.
		/// </summary>
		public static T AddFlag<T>(this Enum value, T append)
		{
			Type type = value.GetType();

			// Determine the Values
			object result = value;
			_Value parsed = new _Value(append, type);
			if (parsed.Signed is long)
			{
				result = Convert.ToInt64(value) | (long)parsed.Signed;
			}
			else if (parsed.Unsigned is ulong)
			{
				result = Convert.ToUInt64(value) | (ulong)parsed.Unsigned;
			}

			// Return the Final Value
			return (T)Enum.Parse(type, result.ToString());
		}

		/// <summary>
		/// Removes an enumerated type and returns the new value.
		/// </summary>
		public static T RemoveFlag<T>(this Enum value, T remove)
		{
			Type type = value.GetType();

			// Determine the Values
			object result = value;
			_Value parsed = new _Value(remove, type);
			if (parsed.Signed is long)
			{
				result = Convert.ToInt64(value) & ~(long)parsed.Signed;
			}
			else if (parsed.Unsigned is ulong)
			{
				result = Convert.ToUInt64(value) & ~(ulong)parsed.Unsigned;
			}

			// Return the Final Value
			return (T)Enum.Parse(type, result.ToString());
		}

		/// <summary>
		/// Checks if an enumerated type contains a value. This is built-in to .NET 4.0 and greater.
		/// </summary>
		public static bool HasFlag<T>(this Enum value, T check)
		{
			Type type = value.GetType();

			// Determine the Values
			_Value parsed = new _Value(check, type);
			if (parsed.Signed is long)
			{
				return (Convert.ToInt64(value) & (long)parsed.Signed) == (long)parsed.Signed;
			}
			else if (parsed.Unsigned is ulong)
			{
				return (Convert.ToUInt64(value) & (ulong)parsed.Unsigned) == (ulong)parsed.Unsigned;
			}
			else
			{
				return false;
			}
		}

		public static bool HasAnyFlag(this Enum value, IEnumerable<Enum> values)
		{
			return values.Any(x => value.HasFlag(x));
		}
		public static bool HasAnyFlag(this Enum value, params Enum[] values)
		{
			return values.Any(x => value.HasFlag(x));
		}

		public static bool HasAllFlags(this Enum value, IEnumerable<Enum> values)
		{
			return values.All(x => value.HasFlag(x));
		}
		public static bool HasAllFlags(this Enum value, params Enum[] values)
		{
			return values.All(x => value.HasFlag(x));
		}
		#endregion

		#region Helper Classes
		// Class to simplfy narrowing values between a ulong and long since either value should cover any lesser value.
		private class _Value
		{
			// Cached comparisons for tye to use.
			private static Type _UInt64 = typeof(ulong);
			private static Type _UInt32 = typeof(long);

			public long? Signed;
			public ulong? Unsigned;

			public _Value(object value, Type type)
			{

				// Make sure it is even an enum to work with.
				if (!type.IsEnum)
				{
					throw new ArgumentException("Value provided is not an enumerated type!");
				}

				// Then check for the enumerated value.
				Type compare = Enum.GetUnderlyingType(type);

				// If this is an unsigned long then the only value that can hold it would be a ulong...
				if (compare.Equals(_Value._UInt32) || compare.Equals(_Value._UInt64))
				{
					this.Unsigned = Convert.ToUInt64(value);
				}
				// Otherwise, a long should cover anything else...
				else
				{
					this.Signed = Convert.ToInt64(value);
				}
			}
		}
		#endregion
		#endregion

		public static string FlagsForDatabase(this Enum flags)
		{
			return GetFlags(flags, true).Select(x => Convert.ToInt32(x)).ToDatabaseList();
		}

		public static IEnumerable<Enum> GetFlags(this Enum input, bool ignoreZero = false)
		{
			if (ignoreZero)
			{
				foreach (Enum value in Enum.GetValues(input.GetType()))
				{
					if (Convert.ToInt32(value) != 0 && input.HasFlag(value))
					{
						yield return value;
					}
				}
			}
			else
			{
				foreach (Enum value in Enum.GetValues(input.GetType()))
				{
					if (input.HasFlag(value))
					{
						yield return value;
					}
				}
			}
		}

        public static T GetRandomValue<T>(this Enum e)
        {
            Array values = Enum.GetValues(typeof(T));
            T type = (T)values.GetValue(RandomThreadSafe.Next(values.Length - 1));
            return type;
        }
    }

    #region Code adapted from https://gist.github.com/1002735
    [DebuggerStepThrough]
    public class EnumLabelAttribute : Attribute
    {
        public string Label { get; private set; }
        public string Abbreviation { get; private set; }
        public EnumLabelAttribute(string label) { 
            Label = label;
            Abbreviation = null;
        }
        public EnumLabelAttribute(string label, string abbreviation)
        {
            Label = label;
            Abbreviation = abbreviation;
        }
    }

    [DebuggerStepThrough]
    public class EnumTagAttribute : Attribute
    {
        public object Tag { get; private set; }
        public EnumTagAttribute(object tag)
        {
            Tag = tag;
        }
    }

    [DebuggerStepThrough]
    public static class EnumAttributeExtension
    {
        public static string GetLabel(this Enum e)
        {
            if (e == null) return null;
            var labelBuilder = new StringBuilder();
            Type type = e.GetType();

            // If this is a Flags enum with multiple values in it, is not 0, and is not a predefined set of multiple values...
            if (type.IsDefined(typeof(FlagsAttribute), false) && Convert.ToInt32(e) != 0 && !Enum.IsDefined(type, e))
            {
                List<Enum> values = e.GetFlags(ignoreZero: true).ToList();
                if (values.Count > 0)
                {
                    foreach (Enum value in values)
                    {
                        GetLabel(value, type, labelBuilder);
                        labelBuilder.Append(" | ");
                    }
                    labelBuilder.Length -= 3;
                }
            }
            else
            {
                GetLabel(e, type, labelBuilder);
            }

            return labelBuilder.ToString();
        }
        private static void GetLabel(Enum e, Type type, StringBuilder labelBuilder)
        {
            var attr = UtilityFunctions.GetAttribute<EnumLabelAttribute>(type, e.ToString());
            if (attr == null)
            {
                labelBuilder.Append(e);
            }
            else if (attr.Abbreviation != null)
            {
                labelBuilder.Append(attr.Abbreviation + " - " + attr.Label);
            }
            else
            {
                labelBuilder.Append(attr.Label);
            }
        }

        public static string GetAbbreviation(this Enum e)
        {
            if (e == null) return null;
            var attr = UtilityFunctions.GetAttribute<EnumLabelAttribute>(e.GetType(), e.ToString());
            if (attr == null)
            {
                return e.ToString();
            }
            if (attr.Abbreviation != null)
            {
                return attr.Abbreviation;
            }
            return attr.Label;
        }

        public static object GetTag(this Enum e)
        {
            if (e == null) return null;
            var attr = UtilityFunctions.GetAttribute<EnumTagAttribute>(e.GetType(), e.ToString());
            if (attr == null) return null;
            return attr.Tag;
        }
        public static string GetTagAsString(this Enum e)
        {
            return GetTag(e) as string;
        }
    }
    #endregion
}
