// Utf8Json.Internal.DoubleConversion.DoubleToStringConverter
using System;
using System.Globalization;
using Utf8Json.Internal;
using Utf8Json.Internal.DoubleConversion;

internal static class DoubleToStringConverter
{
	private enum FastDtoaMode
	{
		FAST_DTOA_SHORTEST,
		FAST_DTOA_SHORTEST_SINGLE
	}

	private enum DtoaMode
	{
		SHORTEST,
		SHORTEST_SINGLE
	}

	private enum Flags
	{
		NO_FLAGS = 0,
		EMIT_POSITIVE_EXPONENT_SIGN = 1,
		EMIT_TRAILING_DECIMAL_POINT = 2,
		EMIT_TRAILING_ZERO_AFTER_POINT = 4,
		UNIQUE_ZERO = 8
	}

	[ThreadStatic]
	private static byte[] decimalRepBuffer;

	[ThreadStatic]
	private static byte[] exponentialRepBuffer;

	[ThreadStatic]
	private static byte[] toStringBuffer;

	private static readonly byte[] infinity_symbol_ = StringEncoding.UTF8.GetBytes(double.PositiveInfinity.ToString());

	private static readonly byte[] nan_symbol_ = StringEncoding.UTF8.GetBytes(double.NaN.ToString());

	private static readonly Flags flags_ = (Flags)9;

	private static readonly char exponent_character_ = 'E';

	private static readonly int decimal_in_shortest_low_ = -4;

	private static readonly int decimal_in_shortest_high_ = 15;

	private const int kBase10MaximalLength = 17;

	private const int kFastDtoaMaximalLength = 17;

	private const int kFastDtoaMaximalSingleLength = 9;

	private const int kMinimalTargetExponent = -60;

	private const int kMaximalTargetExponent = -32;

	private static readonly uint[] kSmallPowersOfTen = new uint[11]
	{
		0u,
		1u,
		10u,
		100u,
		1000u,
		10000u,
		100000u,
		1000000u,
		10000000u,
		100000000u,
		1000000000u
	};

	private static byte[] GetDecimalRepBuffer(int size)
	{
		if (decimalRepBuffer == null)
		{
			decimalRepBuffer = new byte[size];
		}
		return decimalRepBuffer;
	}

	private static byte[] GetExponentialRepBuffer(int size)
	{
		if (exponentialRepBuffer == null)
		{
			exponentialRepBuffer = new byte[size];
		}
		return exponentialRepBuffer;
	}

	private static byte[] GetToStringBuffer()
	{
		if (toStringBuffer == null)
		{
			toStringBuffer = new byte[24];
		}
		return toStringBuffer;
	}

	public static int GetBytes(ref byte[] buffer, int offset, float value)
	{
		StringBuilder result_builder = new StringBuilder(buffer, offset);
		if (!ToShortestIeeeNumber(value, ref result_builder, DtoaMode.SHORTEST_SINGLE))
		{
			throw new InvalidOperationException("not support float value:" + value);
		}
		buffer = result_builder.buffer;
		return result_builder.offset - offset;
	}

	public static int GetBytes(ref byte[] buffer, int offset, double value)
	{
		StringBuilder result_builder = new StringBuilder(buffer, offset);
		if (!ToShortestIeeeNumber(value, ref result_builder, DtoaMode.SHORTEST))
		{
			throw new InvalidOperationException("not support double value:" + value);
		}
		buffer = result_builder.buffer;
		return result_builder.offset - offset;
	}

	private static bool RoundWeed(byte[] buffer, int length, ulong distance_too_high_w, ulong unsafe_interval, ulong rest, ulong ten_kappa, ulong unit)
	{
		ulong num = distance_too_high_w - unit;
		ulong num2 = distance_too_high_w + unit;
		while (rest < num && unsafe_interval - rest >= ten_kappa && (rest + ten_kappa < num || num - rest >= rest + ten_kappa - num))
		{
			buffer[length - 1]--;
			rest += ten_kappa;
		}
		if (rest < num2 && unsafe_interval - rest >= ten_kappa && (rest + ten_kappa < num2 || num2 - rest > rest + ten_kappa - num2))
		{
			return false;
		}
		if (2 * unit <= rest)
		{
			return rest <= unsafe_interval - 4 * unit;
		}
		return false;
	}

	private static void BiggestPowerTen(uint number, int number_bits, out uint power, out int exponent_plus_one)
	{
		int num = (number_bits + 1) * 1233 >> 12;
		num++;
		if (number < kSmallPowersOfTen[num])
		{
			num--;
		}
		power = kSmallPowersOfTen[num];
		exponent_plus_one = num;
	}

	private static bool DigitGen(DiyFp low, DiyFp w, DiyFp high, byte[] buffer, out int length, out int kappa)
	{
		ulong num = 1uL;
		DiyFp b = new DiyFp(low.f - num, low.e);
		DiyFp a = new DiyFp(high.f + num, high.e);
		DiyFp diyFp = DiyFp.Minus(ref a, ref b);
		DiyFp diyFp2 = new DiyFp((ulong)(1L << -w.e), w.e);
		uint num2 = (uint)(a.f >> -diyFp2.e);
		ulong num3 = a.f & (diyFp2.f - 1);
		BiggestPowerTen(num2, 64 - -diyFp2.e, out uint power, out int exponent_plus_one);
		kappa = exponent_plus_one;
		length = 0;
		while (kappa > 0)
		{
			int num4 = (int)(num2 / power);
			buffer[length] = (byte)(48 + num4);
			length++;
			num2 %= power;
			kappa--;
			ulong num5 = ((ulong)num2 << -diyFp2.e) + num3;
			if (num5 < diyFp.f)
			{
				return RoundWeed(buffer, length, DiyFp.Minus(ref a, ref w).f, diyFp.f, num5, (ulong)power << -diyFp2.e, num);
			}
			power /= 10u;
		}
		do
		{
			num3 *= 10;
			num *= 10;
			diyFp.f *= 10uL;
			int num6 = (int)(num3 >> -diyFp2.e);
			buffer[length] = (byte)(48 + num6);
			length++;
			num3 &= diyFp2.f - 1;
			kappa--;
		}
		while (num3 >= diyFp.f);
		return RoundWeed(buffer, length, DiyFp.Minus(ref a, ref w).f * num, diyFp.f, num3, diyFp2.f, num);
	}

	private static bool Grisu3(double v, FastDtoaMode mode, byte[] buffer, out int length, out int decimal_exponent)
	{
		DiyFp a = new Utf8Json.Internal.DoubleConversion.Double(v).AsNormalizedDiyFp();
		DiyFp out_m_minus;
		DiyFp out_m_plus;
		switch (mode)
		{
			case FastDtoaMode.FAST_DTOA_SHORTEST:
				new Utf8Json.Internal.DoubleConversion.Double(v).NormalizedBoundaries(out out_m_minus, out out_m_plus);
				break;
			case FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE:
				new Utf8Json.Internal.DoubleConversion.Single((float)v).NormalizedBoundaries(out out_m_minus, out out_m_plus);
				break;
			default:
				throw new Exception("Invalid Mode.");
		}
		int min_exponent = -60 - (a.e + 64);
		int max_exponent = -32 - (a.e + 64);
		PowersOfTenCache.GetCachedPowerForBinaryExponentRange(min_exponent, max_exponent, out DiyFp power, out int decimal_exponent2);
		DiyFp w = DiyFp.Times(ref a, ref power);
		DiyFp low = DiyFp.Times(ref out_m_minus, ref power);
		DiyFp high = DiyFp.Times(ref out_m_plus, ref power);
		int kappa;
		bool result = DigitGen(low, w, high, buffer, out length, out kappa);
		decimal_exponent = -decimal_exponent2 + kappa;
		return result;
	}

	private static bool FastDtoa(double v, FastDtoaMode mode, byte[] buffer, out int length, out int decimal_point)
	{
		bool flag = false;
		int decimal_exponent = 0;
		if ((uint)mode <= 1u)
		{
			flag = Grisu3(v, mode, buffer, out length, out decimal_exponent);
			if (flag)
			{
				decimal_point = length + decimal_exponent;
			}
			else
			{
				decimal_point = -1;
			}
			return flag;
		}
		throw new Exception("unreachable code.");
	}

	private static bool HandleSpecialValues(double value, ref StringBuilder result_builder)
	{
		Utf8Json.Internal.DoubleConversion.Double @double = new Utf8Json.Internal.DoubleConversion.Double(value);
		if (@double.IsInfinite())
		{
			if (infinity_symbol_ == null)
			{
				return false;
			}
			if (value < 0.0)
			{
				result_builder.AddCharacter(45);
			}
			result_builder.AddString(infinity_symbol_);
			return true;
		}
		if (@double.IsNan())
		{
			if (nan_symbol_ == null)
			{
				return false;
			}
			result_builder.AddString(nan_symbol_);
			return true;
		}
		return false;
	}

	private static bool ToShortestIeeeNumber(double value, ref StringBuilder result_builder, DtoaMode mode)
	{
		if (new Utf8Json.Internal.DoubleConversion.Double(value).IsSpecial())
		{
			return HandleSpecialValues(value, ref result_builder);
		}
		byte[] array = GetDecimalRepBuffer(18);
		if (!DoubleToAscii(value, mode, 0, array, out bool sign, out int length, out int point))
		{
			string str = value.ToString("G17", CultureInfo.InvariantCulture);
			result_builder.AddStringSlow(str);
			return true;
		}
		bool flag = (flags_ & Flags.UNIQUE_ZERO) != 0;
		if (sign && (value != 0.0 || !flag))
		{
			result_builder.AddCharacter(45);
		}
		int num = point - 1;
		if (decimal_in_shortest_low_ <= num && num < decimal_in_shortest_high_)
		{
			CreateDecimalRepresentation(array, length, point, Math.Max(0, length - point), ref result_builder);
		}
		else
		{
			CreateExponentialRepresentation(array, length, num, ref result_builder);
		}
		return true;
	}

	private static void CreateDecimalRepresentation(byte[] decimal_digits, int length, int decimal_point, int digits_after_point, ref StringBuilder result_builder)
	{
		if (decimal_point <= 0)
		{
			result_builder.AddCharacter(48);
			if (digits_after_point > 0)
			{
				result_builder.AddCharacter(46);
				result_builder.AddPadding(48, -decimal_point);
				result_builder.AddSubstring(decimal_digits, length);
				int count = digits_after_point - -decimal_point - length;
				result_builder.AddPadding(48, count);
			}
		}
		else if (decimal_point >= length)
		{
			result_builder.AddSubstring(decimal_digits, length);
			result_builder.AddPadding(48, decimal_point - length);
			if (digits_after_point > 0)
			{
				result_builder.AddCharacter(46);
				result_builder.AddPadding(48, digits_after_point);
			}
		}
		else
		{
			result_builder.AddSubstring(decimal_digits, decimal_point);
			result_builder.AddCharacter(46);
			result_builder.AddSubstring(decimal_digits, decimal_point, length - decimal_point);
			int count2 = digits_after_point - (length - decimal_point);
			result_builder.AddPadding(48, count2);
		}
		if (digits_after_point == 0)
		{
			if ((flags_ & Flags.EMIT_TRAILING_DECIMAL_POINT) != 0)
			{
				result_builder.AddCharacter(46);
			}
			if ((flags_ & Flags.EMIT_TRAILING_ZERO_AFTER_POINT) != 0)
			{
				result_builder.AddCharacter(48);
			}
		}
	}

	private static void CreateExponentialRepresentation(byte[] decimal_digits, int length, int exponent, ref StringBuilder result_builder)
	{
		result_builder.AddCharacter(decimal_digits[0]);
		if (length != 1)
		{
			result_builder.AddCharacter(46);
			result_builder.AddSubstring(decimal_digits, 1, length - 1);
		}
		result_builder.AddCharacter((byte)exponent_character_);
		if (exponent < 0)
		{
			result_builder.AddCharacter(45);
			exponent = -exponent;
		}
		else if ((flags_ & Flags.EMIT_POSITIVE_EXPONENT_SIGN) != 0)
		{
			result_builder.AddCharacter(43);
		}
		if (exponent == 0)
		{
			result_builder.AddCharacter(48);
			return;
		}
		byte[] array = GetExponentialRepBuffer(6);
		array[5] = 0;
		int num = 5;
		while (exponent > 0)
		{
			array[--num] = (byte)(48 + exponent % 10);
			exponent /= 10;
		}
		result_builder.AddSubstring(array, num, 5 - num);
	}

	private static bool DoubleToAscii(double v, DtoaMode mode, int requested_digits, byte[] vector, out bool sign, out int length, out int point)
	{
		if (new Utf8Json.Internal.DoubleConversion.Double(v).Sign() < 0)
		{
			sign = true;
			v = 0.0 - v;
		}
		else
		{
			sign = false;
		}
		if (v == 0.0)
		{
			vector[0] = 48;
			length = 1;
			point = 1;
			return true;
		}
		switch (mode)
		{
			case DtoaMode.SHORTEST:
				return FastDtoa(v, FastDtoaMode.FAST_DTOA_SHORTEST, vector, out length, out point);
			case DtoaMode.SHORTEST_SINGLE:
				return FastDtoa(v, FastDtoaMode.FAST_DTOA_SHORTEST_SINGLE, vector, out length, out point);
			default:
				{
					bool flag = false;
					throw new Exception("Unreachable code.");
				}
		}
	}
}
