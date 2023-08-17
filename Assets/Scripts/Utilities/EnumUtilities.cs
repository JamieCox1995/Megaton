using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public static class EnumUtilities
{
    public static bool HasFlag<TEnum>(this TEnum value, TEnum flag) where TEnum : struct
    {
        var type = typeof(TEnum);

        if (!type.IsEnum) throw new InvalidOperationException("Please ensure the type parameter \"TEnum\" is an enumerated type.");

        var underlyingType = Enum.GetUnderlyingType(type);

        if (underlyingType == typeof(byte))
        {
            var numericValue = Convert.ToByte(value);
            var numericFlag = Convert.ToByte(flag);

            return (numericValue & numericFlag) == numericFlag;
        }
        else if (underlyingType == typeof(sbyte))
        {
            var numericValue = Convert.ToSByte(value);
            var numericFlag = Convert.ToSByte(flag);

            return (numericValue & numericFlag) == numericFlag;
        }
        else if (underlyingType == typeof(short))
        {
            var numericValue = Convert.ToInt16(value);
            var numericFlag = Convert.ToInt16(flag);

            return (numericValue & numericFlag) == numericFlag;
        }
        else if (underlyingType == typeof(ushort))
        {
            var numericValue = Convert.ToUInt16(value);
            var numericFlag = Convert.ToUInt16(flag);

            return (numericValue & numericFlag) == numericFlag;
        }
        else if (underlyingType == typeof(int))
        {
            var numericValue = Convert.ToInt32(value);
            var numericFlag = Convert.ToInt32(flag);

            return (numericValue & numericFlag) == numericFlag;
        }
        else if (underlyingType == typeof(uint))
        {
            var numericValue = Convert.ToUInt32(value);
            var numericFlag = Convert.ToUInt32(flag);

            return (numericValue & numericFlag) == numericFlag;
        }
        else if (underlyingType == typeof(long))
        {
            var numericValue = Convert.ToInt64(value);
            var numericFlag = Convert.ToInt64(flag);

            return (numericValue & numericFlag) == numericFlag;
        }
        else
        {
            var numericValue = Convert.ToUInt64(value);
            var numericFlag = Convert.ToUInt64(flag);

            return (numericValue & numericFlag) == numericFlag;
        }
    }
}

