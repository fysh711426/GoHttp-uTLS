package main

import (
	tls "github.com/refraction-networking/utls"
)

func GetClientHelloID(id int) *tls.ClientHelloID {
	// tls.HelloGolang
	if id == 10_001_00 {
		return &tls.HelloGolang
	}

	// tls.HelloCustom
	if id == 11_001_00 {
		return &tls.HelloCustom
	}

	// tls.HelloRandomized
	if id == 12_001_00 {
		return &tls.HelloRandomized
	}
	if id == 12_002_00 {
		return &tls.HelloRandomizedALPN
	}
	if id == 12_003_00 {
		return &tls.HelloRandomizedNoALPN
	}

	// tls.HelloFirefox
	if id == 13_055_00 {
		return &tls.HelloFirefox_55
	}
	if id == 13_056_00 {
		return &tls.HelloFirefox_56
	}
	if id == 13_063_00 {
		return &tls.HelloFirefox_63
	}
	if id == 13_065_00 {
		return &tls.HelloFirefox_65
	}
	if id == 13_099_00 {
		return &tls.HelloFirefox_99
	}
	if id == 13_102_00 {
		return &tls.HelloFirefox_102
	}
	if id == 13_105_00 {
		return &tls.HelloFirefox_105
	}

	// tls.HelloChrome
	if id == 14_058_00 {
		return &tls.HelloChrome_58
	}
	if id == 14_062_00 {
		return &tls.HelloChrome_62
	}
	if id == 14_070_00 {
		return &tls.HelloChrome_70
	}
	if id == 14_072_00 {
		return &tls.HelloChrome_72
	}
	if id == 14_083_00 {
		return &tls.HelloChrome_83
	}
	if id == 14_087_00 {
		return &tls.HelloChrome_87
	}
	if id == 14_096_00 {
		return &tls.HelloChrome_96
	}
	if id == 14_100_00 {
		return &tls.HelloChrome_100
	}
	if id == 14_102_00 {
		return &tls.HelloChrome_102
	}

	// tls.HelloIOS
	if id == 15_011_01 {
		return &tls.HelloIOS_11_1
	}
	if id == 15_012_01 {
		return &tls.HelloIOS_12_1
	}
	if id == 15_013_00 {
		return &tls.HelloIOS_13
	}
	if id == 15_014_00 {
		return &tls.HelloIOS_14
	}

	// tls.HelloAndroid
	if id == 16_011_00 {
		return &tls.HelloAndroid_11_OkHttp
	}

	// tls.HelloEdge
	if id == 17_085_00 {
		return &tls.HelloEdge_85
	}
	if id == 17_106_00 {
		return &tls.HelloEdge_106
	}

	// tls.HelloSafari
	if id == 18_016_00 {
		return &tls.HelloSafari_16_0
	}

	// tls.Hello360
	if id == 19_007_05 {
		return &tls.Hello360_7_5
	}
	if id == 19_011_00 {
		return &tls.Hello360_11_0
	}

	// tls.HelloQQ
	if id == 20_011_01 {
		return &tls.HelloQQ_11_1
	}
	return nil
}
