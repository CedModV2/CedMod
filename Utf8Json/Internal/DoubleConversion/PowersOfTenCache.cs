// Decompiled with JetBrains decompiler
// Type: Utf8Json.Internal.DoubleConversion.PowersOfTenCache
// Assembly: Assembly-CSharp, Version=1.2.3.4, Culture=neutral, PublicKeyToken=null
// MVID: 4FF70443-CA06-4035-B3D1-98CFA9EE67BF
// Assembly location: D:\steamgames\steamapps\common\SCP Secret Laboratory Dedicated Server\SCPSL_Data\Managed\Assembly-CSharp.dll

using System;

namespace Utf8Json.Internal.DoubleConversion
{
  internal static class PowersOfTenCache
  {
    private static readonly CachedPower[] kCachedPowers = new CachedPower[87]
    {
      new CachedPower(18054884314459144840UL, (short) -1220, (short) -348),
      new CachedPower(13451937075301367670UL, (short) -1193, (short) -340),
      new CachedPower(10022474136428063862UL, (short) -1166, (short) -332),
      new CachedPower(14934650266808366570UL, (short) -1140, (short) -324),
      new CachedPower(11127181549972568877UL, (short) -1113, (short) -316),
      new CachedPower(16580792590934885855UL, (short) -1087, (short) -308),
      new CachedPower(12353653155963782858UL, (short) -1060, (short) -300),
      new CachedPower(18408377700990114895UL, (short) -1034, (short) -292),
      new CachedPower(13715310171984221708UL, (short) -1007, (short) -284),
      new CachedPower(10218702384817765436UL, (short) -980, (short) -276),
      new CachedPower(15227053142812498563UL, (short) -954, (short) -268),
      new CachedPower(11345038669416679861UL, (short) -927, (short) -260),
      new CachedPower(16905424996341287883UL, (short) -901, (short) -252),
      new CachedPower(12595523146049147757UL, (short) -874, (short) -244),
      new CachedPower(9384396036005875287UL, (short) -847, (short) -236),
      new CachedPower(13983839803942852151UL, (short) -821, (short) -228),
      new CachedPower(10418772551374772303UL, (short) -794, (short) -220),
      new CachedPower(15525180923007089351UL, (short) -768, (short) -212),
      new CachedPower(11567161174868858868UL, (short) -741, (short) -204),
      new CachedPower(17236413322193710309UL, (short) -715, (short) -196),
      new CachedPower(12842128665889583758UL, (short) -688, (short) -188),
      new CachedPower(9568131466127621947UL, (short) -661, (short) -180),
      new CachedPower(14257626930069360058UL, (short) -635, (short) -172),
      new CachedPower(10622759856335341974UL, (short) -608, (short) -164),
      new CachedPower(15829145694278690180UL, (short) -582, (short) -156),
      new CachedPower(11793632577567316726UL, (short) -555, (short) -148),
      new CachedPower(17573882009934360870UL, (short) -529, (short) -140),
      new CachedPower(13093562431584567480UL, (short) -502, (short) -132),
      new CachedPower(9755464219737475723UL, (short) -475, (short) -124),
      new CachedPower(14536774485912137811UL, (short) -449, (short) -116),
      new CachedPower(10830740992659433045UL, (short) -422, (short) -108),
      new CachedPower(16139061738043178685UL, (short) -396, (short) -100),
      new CachedPower(12024538023802026127UL, (short) -369, (short) -92),
      new CachedPower(17917957937422433684UL, (short) -343, (short) -84),
      new CachedPower(13349918974505688015UL, (short) -316, (short) -76),
      new CachedPower(9946464728195732843UL, (short) -289, (short) -68),
      new CachedPower(14821387422376473014UL, (short) -263, (short) -60),
      new CachedPower(11042794154864902060UL, (short) -236, (short) -52),
      new CachedPower(16455045573212060422UL, (short) -210, (short) -44),
      new CachedPower(12259964326927110867UL, (short) -183, (short) -36),
      new CachedPower(18268770466636286478UL, (short) -157, (short) -28),
      new CachedPower(13611294676837538539UL, (short) -130, (short) -20),
      new CachedPower(10141204801825835212UL, (short) -103, (short) -12),
      new CachedPower(15111572745182864684UL, (short) -77, (short) -4),
      new CachedPower(11258999068426240000UL, (short) -50, (short) 4),
      new CachedPower(16777216000000000000UL, (short) -24, (short) 12),
      new CachedPower(12500000000000000000UL, (short) 3, (short) 20),
      new CachedPower(9313225746154785156UL, (short) 30, (short) 28),
      new CachedPower(13877787807814456755UL, (short) 56, (short) 36),
      new CachedPower(10339757656912845936UL, (short) 83, (short) 44),
      new CachedPower(15407439555097886824UL, (short) 109, (short) 52),
      new CachedPower(11479437019748901445UL, (short) 136, (short) 60),
      new CachedPower(17105694144590052135UL, (short) 162, (short) 68),
      new CachedPower(12744735289059618216UL, (short) 189, (short) 76),
      new CachedPower(9495567745759798747UL, (short) 216, (short) 84),
      new CachedPower(14149498560666738074UL, (short) 242, (short) 92),
      new CachedPower(10542197943230523224UL, (short) 269, (short) 100),
      new CachedPower(15709099088952724970UL, (short) 295, (short) 108),
      new CachedPower(11704190886730495818UL, (short) 322, (short) 116),
      new CachedPower(17440603504673385349UL, (short) 348, (short) 124),
      new CachedPower(12994262207056124023UL, (short) 375, (short) 132),
      new CachedPower(9681479787123295682UL, (short) 402, (short) 140),
      new CachedPower(14426529090290212157UL, (short) 428, (short) 148),
      new CachedPower(10748601772107342003UL, (short) 455, (short) 156),
      new CachedPower(16016664761464807395UL, (short) 481, (short) 164),
      new CachedPower(11933345169920330789UL, (short) 508, (short) 172),
      new CachedPower(17782069995880619868UL, (short) 534, (short) 180),
      new CachedPower(13248674568444952270UL, (short) 561, (short) 188),
      new CachedPower(9871031767461413346UL, (short) 588, (short) 196),
      new CachedPower(14708983551653345445UL, (short) 614, (short) 204),
      new CachedPower(10959046745042015199UL, (short) 641, (short) 212),
      new CachedPower(16330252207878254650UL, (short) 667, (short) 220),
      new CachedPower(12166986024289022870UL, (short) 694, (short) 228),
      new CachedPower(18130221999122236476UL, (short) 720, (short) 236),
      new CachedPower(13508068024458167312UL, (short) 747, (short) 244),
      new CachedPower(10064294952495520794UL, (short) 774, (short) 252),
      new CachedPower(14996968138956309548UL, (short) 800, (short) 260),
      new CachedPower(11173611982879273257UL, (short) 827, (short) 268),
      new CachedPower(16649979327439178909UL, (short) 853, (short) 276),
      new CachedPower(12405201291620119593UL, (short) 880, (short) 284),
      new CachedPower(9242595204427927429UL, (short) 907, (short) 292),
      new CachedPower(13772540099066387757UL, (short) 933, (short) 300),
      new CachedPower(10261342003245940623UL, (short) 960, (short) 308),
      new CachedPower(15290591125556738113UL, (short) 986, (short) 316),
      new CachedPower(11392378155556871081UL, (short) 1013, (short) 324),
      new CachedPower(16975966327722178521UL, (short) 1039, (short) 332),
      new CachedPower(12648080533535911531UL, (short) 1066, (short) 340)
    };
    public const int kCachedPowersOffset = 348;
    public const double kD_1_LOG2_10 = 0.301029995663981;
    public const int kDecimalExponentDistance = 8;
    public const int kMinDecimalExponent = -348;
    public const int kMaxDecimalExponent = 340;

    public static void GetCachedPowerForBinaryExponentRange(
      int min_exponent,
      int max_exponent,
      out DiyFp power,
      out int decimal_exponent)
    {
      int num = 64;
      int index = (348 + (int) Math.Ceiling((double) (min_exponent + num - 1) * 0.301029995663981) - 1) / 8 + 1;
      CachedPower kCachedPower = PowersOfTenCache.kCachedPowers[index];
      decimal_exponent = (int) kCachedPower.decimal_exponent;
      power = new DiyFp(kCachedPower.significand, (int) kCachedPower.binary_exponent);
    }

    public static void GetCachedPowerForDecimalExponent(
      int requested_exponent,
      out DiyFp power,
      out int found_exponent)
    {
      int index = (requested_exponent + 348) / 8;
      CachedPower kCachedPower = PowersOfTenCache.kCachedPowers[index];
      power = new DiyFp(kCachedPower.significand, (int) kCachedPower.binary_exponent);
      found_exponent = (int) kCachedPower.decimal_exponent;
    }
  }
}
