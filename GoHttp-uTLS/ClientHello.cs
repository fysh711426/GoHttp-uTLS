namespace GoHttp_uTLS
{
    public enum ClientHello
    {
        // HelloGolang
        HelloGolang = 10_001_00,

        // HelloCustom
        HelloCustom = 11_001_00,

        // HelloRandomized
        HelloRandomized = 12_001_00,
        HelloRandomizedALPN = 12_002_00,
        HelloRandomizedNoALPN = 12_003_00,

        // HelloFirefox
        HelloFirefox_55 = 13_055_00,
        HelloFirefox_56 = 13_056_00,
        HelloFirefox_63 = 13_063_00,
        HelloFirefox_65 = 13_065_00,
        HelloFirefox_99 = 13_099_00,
        HelloFirefox_102 = 13_102_00,
        HelloFirefox_105 = 13_105_00,
        HelloFirefox_Auto = HelloFirefox_105,

        // HelloChrome
        HelloChrome_58 = 14_058_00,
        HelloChrome_62 = 14_062_00,
        HelloChrome_70 = 14_070_00,
        HelloChrome_72 = 14_072_00,
        HelloChrome_83 = 14_083_00,
        HelloChrome_87 = 14_087_00,
        HelloChrome_96 = 14_096_00,
        HelloChrome_100 = 14_100_00,
        HelloChrome_102 = 14_102_00,
        HelloChrome_Auto = HelloChrome_102,

        // HelloIOS
        HelloIOS_11_1 = 15_011_01,
        HelloIOS_12_1 = 15_012_01,
        HelloIOS_13 = 15_013_00,
        HelloIOS_14 = 15_014_00,
        HelloIOS_Auto = HelloIOS_14,

        // HelloAndroid
        HelloAndroid_11_OkHttp = 16_011_00,

        // HelloEdge
        HelloEdge_85 = 17_085_00,
        HelloEdge_106 = 17_106_00,
        HelloEdge_Auto = HelloEdge_85,

        // HelloSafari
        HelloSafari_16_0 = 18_016_00,
        HelloSafari_Auto = HelloSafari_16_0,

        // Hello360
        Hello360_7_5 = 19_007_05,
        Hello360_11_0 = 19_011_00,
        Hello360_Auto = Hello360_7_5,

        // HelloQQ
        HelloQQ_11_1 = 20_011_01,
        HelloQQ_Auto = HelloQQ_11_1
    }
}