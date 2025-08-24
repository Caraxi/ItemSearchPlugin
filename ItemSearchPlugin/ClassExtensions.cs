using System.ComponentModel;
using System.Reflection;
using Lumina.Excel.Sheets;

namespace ItemSearchPlugin {
    public static class ClassExtensions {
        public static bool HasClass(this ClassJobCategory cjc, uint classJobRowId) {
            return classJobRowId switch {
                0 => cjc.ADV,
                1 => cjc.GLA,
                2 => cjc.PGL,
                3 => cjc.MRD,
                4 => cjc.LNC,
                5 => cjc.ARC,
                6 => cjc.CNJ,
                7 => cjc.THM,
                8 => cjc.CRP,
                9 => cjc.BSM,
                10 => cjc.ARM,
                11 => cjc.GSM,
                12 => cjc.LTW,
                13 => cjc.WVR,
                14 => cjc.ALC,
                15 => cjc.CUL,
                16 => cjc.MIN,
                17 => cjc.BTN,
                18 => cjc.FSH,
                19 => cjc.PLD,
                20 => cjc.MNK,
                21 => cjc.WAR,
                22 => cjc.DRG,
                23 => cjc.BRD,
                24 => cjc.WHM,
                25 => cjc.BLM,
                26 => cjc.ACN,
                27 => cjc.SMN,
                28 => cjc.SCH,
                29 => cjc.ROG,
                30 => cjc.NIN,
                31 => cjc.MCH,
                32 => cjc.DRK,
                33 => cjc.AST,
                34 => cjc.SAM,
                35 => cjc.RDM,
                36 => cjc.BLU,
                37 => cjc.GNB,
                38 => cjc.DNC,
                39 => cjc.RPR,
                40 => cjc.SGE,
                41 => cjc.VPR,
                42 => cjc.PCT,
                _ => false,
            };
        }

        public enum CharacterSex {
            Male = 0,
            Female = 1,
            Either = 2,
            Both = 3
        };

        public static bool AllowsRaceSex(this EquipRaceCategory erc, uint raceId, CharacterSex sex) {
            return sex switch {
                CharacterSex.Both when (erc.Male == false || erc.Female == false) => false,
                CharacterSex.Either when (erc.Male == false && erc.Female == false) => false,
                CharacterSex.Female when erc.Female == false => false,
                CharacterSex.Male when erc.Male == false => false,
                _ => raceId switch {
                    0 => false,
                    1 => erc.Hyur,
                    2 => erc.Elezen,
                    3 => erc.Lalafell,
                    4 => erc.Miqote,
                    5 => erc.Roegadyn,
                    6 => erc.AuRa,
                    7 => erc.Hrothgar,
                    8 => erc.Viera,
                    _ => false
                }
            };
        }

        public static string DescriptionAttr<T>(this T source) {
            FieldInfo fi = source.GetType().GetField(source.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0) return attributes[0].Description;
            else return source.ToString();
        }

        public static string? GetExpac(string patch)
        {
            if (float.TryParse(patch, out float number))
            {
                switch (number)
                {
                    case >= 1 and <2:
                        return "1.0";
                        break;
                    case >= 2 and < 3:
                        return "ARR";
                        break;
                    case >= 3 and < 4:
                        return "HW";
                        break;
                    case >= 4 and < 5:
                        return "SB";
                        break;
                    case >= 5 and < 6:
                        return "ShB";
                        break;
                    case >= 6 and < 7:
                        return "EW";
                        break;
                    case >=7 and <8:
                        return "DT";
                        break;
                }
            }
            return patch;
        }

        // Absolutely shamelessly stolen from https://raw.githubusercontent.com/Kouzukii/ffxiv-whichpatchwasthat/refs/heads/main/WhichPatchWasThat/ItemPatchMapper.cs
        public static string? GetPatch(ulong id)
        {
            if (id < 2000000)
                id %= 500000;
			switch (id)
			{
				case >= 1 and <= 24:
				case >= 366 and <= 1675:
				case 1677:
				case >= 1680 and <= 1746:
				case >= 1749 and <= 1816:
				case >= 1819 and <= 1886:
				case >= 1889 and <= 1955:
				case >= 1958 and <= 2052:
				case >= 2055 and <= 2104:
				case >= 2106 and <= 2140:
				case >= 2142 and <= 2192:
				case >= 2195 and <= 2206:
				case >= 2209 and <= 2214:
				case 2219:
				case >= 2221 and <= 2300:
				case >= 2302 and <= 2306:
				case >= 2314 and <= 2649:
				case >= 2651 and <= 2945:
				case 2951:
				case >= 2958 and <= 2992:
				case >= 2994 and <= 3262:
				case >= 3274 and <= 3503:
				case >= 3515 and <= 3728:
				case >= 3740 and <= 3947:
				case >= 3959 and <= 4079:
				case >= 4091 and <= 4183:
				case >= 4196 and <= 4288:
				case >= 4301 and <= 4390:
				case >= 4404 and <= 4529:
				case >= 4542 and <= 5607:
				case >= 5609 and <= 5612:
				case >= 5614 and <= 5617:
				case >= 5619 and <= 5622:
				case >= 5624 and <= 5627:
				case >= 5629 and <= 5632:
				case >= 5634 and <= 5637:
				case >= 5639 and <= 5642:
				case >= 5644 and <= 5647:
				case >= 5649 and <= 5652:
				case >= 5654 and <= 5657:
				case >= 5659 and <= 5662:
				case >= 5664 and <= 5667:
				case >= 5669 and <= 5672:
				case >= 5674 and <= 5677:
				case >= 5679 and <= 5682:
				case >= 5684 and <= 5687:
				case >= 5689 and <= 5692:
				case >= 5694 and <= 5697:
				case >= 5699 and <= 5702:
				case >= 5704 and <= 5707:
				case >= 5709 and <= 5712:
				case >= 5714 and <= 5717:
				case >= 5719 and <= 5722:
				case >= 5724 and <= 5735:
				case >= 5737 and <= 5740:
				case >= 5742 and <= 5743:
				case >= 5745 and <= 5748:
				case 5752:
				case 5754:
				case >= 5756 and <= 5757:
				case >= 5760 and <= 5762:
				case >= 5764 and <= 5765:
				case >= 5768 and <= 5769:
				case 5771:
				case 5774:
				case 5776:
				case 5780:
				case 5783:
				case >= 5787 and <= 5789:
				case 5794:
				case 5796:
				case >= 5801 and <= 5802:
				case 5804:
				case 5806:
				case >= 5809 and <= 5810:
				case >= 5813 and <= 5941:
				case >= 5946 and <= 6031:
				case >= 6049 and <= 6108:
				case >= 6138 and <= 6158:
				case 6160:
				case 6162:
				case >= 6164 and <= 6179:
				case 6183:
				case >= 6185 and <= 6187:
				case >= 6189 and <= 6195:
				case >= 6197 and <= 6205:
				case 6207:
				case >= 6209 and <= 6210:
				case >= 6212 and <= 6216:
				case >= 6218 and <= 6224:
				case >= 6231 and <= 6269:
				case 13004:
					return "1.0";
				case 1678:
				case 1747:
				case 1817:
				case 1887:
				case 1956:
				case 2053:
				case 2105:
				case >= 2215 and <= 2216:
				case >= 2308 and <= 2309:
				case >= 2946 and <= 2950:
				case >= 3263 and <= 3267:
				case >= 3504 and <= 3508:
				case >= 3729 and <= 3733:
				case >= 3948 and <= 3952:
				case >= 4080 and <= 4084:
				case >= 4184 and <= 4189:
				case >= 4289 and <= 4294:
				case >= 4391 and <= 4396:
				case >= 4530 and <= 4535:
				case >= 6225 and <= 6230:
				case >= 6270 and <= 6319:
					return "2.0";
				case >= 5942 and <= 5945:
				case >= 6033 and <= 6048:
					return "2.05";
				case 25:
				case 1679:
				case 1748:
				case 1818:
				case 1888:
				case 1957:
				case 2054:
				case 2141:
				case >= 2207 and <= 2208:
				case >= 2217 and <= 2218:
				case 2301:
				case 2311:
				case >= 2952 and <= 2957:
				case >= 3268 and <= 3273:
				case >= 3509 and <= 3514:
				case >= 3734 and <= 3739:
				case >= 3953 and <= 3958:
				case >= 4085 and <= 4090:
				case >= 4190 and <= 4195:
				case >= 4295 and <= 4300:
				case >= 4397 and <= 4403:
				case >= 4536 and <= 4541:
				case 5744:
				case 5749:
				case 5751:
				case 5753:
				case 5755:
				case 5759:
				case 5767:
				case 5770:
				case 5773:
				case 5777:
				case 5779:
				case 5781:
				case 5786:
				case >= 5790 and <= 5791:
				case >= 5797 and <= 5799:
				case 5807:
				case 5812:
				case 6110:
				case 6112:
				case >= 6114 and <= 6115:
				case 6159:
				case 6161:
				case 6163:
				case 6180:
				case 6184:
				case 6188:
				case 6196:
				case 6208:
				case 6211:
				case 6217:
				case >= 6320 and <= 6498:
				case 6501:
				case >= 6503 and <= 6540:
				case >= 6542 and <= 6599:
				case >= 6601 and <= 6692:
				case >= 6704 and <= 7005:
				case >= 7007 and <= 7056:
					return "2.1";
				case 6111:
				case >= 6116 and <= 6137:
					return "2.16";
				case 6502:
				case 6600:
				case 7006:
					return "2.15";
				case 26:
				case >= 2193 and <= 2194:
				case 5736:
				case 5741:
				case 5750:
				case 5758:
				case 5763:
				case 5766:
				case 5772:
				case 5775:
				case 5778:
				case 5782:
				case >= 5784 and <= 5785:
				case >= 5792 and <= 5793:
				case 5795:
				case 5800:
				case 5803:
				case 5805:
				case 5808:
				case 5811:
				case >= 6499 and <= 6500:
				case >= 6693 and <= 6703:
				case >= 7059 and <= 7101:
				case >= 7108 and <= 7117:
				case >= 7122 and <= 7123:
				case >= 7126 and <= 7157:
				case >= 7159 and <= 7359:
				case >= 7444 and <= 7564:
				case 7566:
				case >= 7568 and <= 7798:
				case >= 7802 and <= 7862:
					return "2.2";
				case >= 7863 and <= 7885:
					return "2.28";
				case >= 7360 and <= 7443:
					return "2.25";
				case 27:
				case 6206:
				case >= 7102 and <= 7107:
				case >= 7118 and <= 7121:
				case 7158:
				case 7799:
				case >= 7894 and <= 7951:
				case 7958:
				case >= 7964 and <= 7967:
				case >= 7969 and <= 8051:
				case >= 8135 and <= 8156:
				case >= 8165 and <= 8173:
				case >= 8182 and <= 8188:
				case >= 8190 and <= 8199:
				case >= 8201 and <= 8205:
				case 8207:
				case >= 8209 and <= 8218:
				case >= 8224 and <= 8561:
				case >= 8563 and <= 8567:
				case >= 8570 and <= 8572:
					return "2.3";
				case 6113:
				case >= 7124 and <= 7125:
				case >= 8052 and <= 8110:
				case >= 8112 and <= 8115:
				case >= 8117 and <= 8118:
				case >= 8120 and <= 8123:
				case >= 8125 and <= 8127:
				case >= 8129 and <= 8134:
				case >= 8157 and <= 8163:
				case >= 8174 and <= 8180:
				case 8189:
				case 8208:
				case 8222:
				case 8562:
				case >= 8568 and <= 8569:
				case >= 8576 and <= 8581:
					return "2.35";
				case >= 8649 and <= 8659:
				case >= 8703 and <= 8716:
				case >= 8718 and <= 8720:
				case >= 8722 and <= 8724:
				case 8726:
				case >= 8732 and <= 8733:
				case >= 8743 and <= 8744:
					return "2.38";
				case 28:
				case 1676:
				case 7567:
				case >= 7886 and <= 7893:
				case >= 7952 and <= 7957:
				case >= 7959 and <= 7963:
				case 8200:
				case >= 8582 and <= 8648:
				case >= 8660 and <= 8668:
				case 8717:
				case >= 8752 and <= 8784:
				case >= 8786 and <= 8790:
				case >= 8793 and <= 8798:
				case >= 8805 and <= 8809:
				case >= 8811 and <= 8821:
				case >= 8831 and <= 8836:
				case 8841:
				case >= 8876 and <= 9031:
				case >= 9036 and <= 9047:
				case >= 9052 and <= 9057:
				case >= 9062 and <= 9067:
				case >= 9072 and <= 9077:
				case >= 9093 and <= 9267:
				case >= 9283 and <= 9289:
				case >= 9291 and <= 9317:
				case >= 9322 and <= 9344:
				case >= 9346 and <= 9350:
				case >= 9352 and <= 9372:
				case >= 9374 and <= 9375:
				case >= 9377 and <= 9378:
				case >= 9380 and <= 9461:
				case >= 9467 and <= 9490:
					return "2.4";
				case 8111:
				case 8116:
				case 8119:
				case 8124:
				case 8128:
				case 8164:
				case 8575:
				case >= 8669 and <= 8702:
				case 8721:
				case 8725:
				case 8745:
				case 8748:
				case 8750:
				case >= 8791 and <= 8792:
				case >= 8799 and <= 8802:
				case 8804:
				case 8810:
				case >= 8823 and <= 8828:
				case 8830:
				case >= 9268 and <= 9282:
				case 9290:
				case >= 9318 and <= 9321:
				case 9345:
				case >= 9491 and <= 9559:
					return "2.45";
				case 8734:
				case 8737:
				case 8739:
				case 8741:
				case 8785:
				case >= 8837 and <= 8838:
				case >= 8844 and <= 8875:
				case >= 9032 and <= 9035:
				case >= 9048 and <= 9051:
				case >= 9058 and <= 9061:
				case >= 9068 and <= 9071:
				case >= 9078 and <= 9092:
				case 9373:
				case 9376:
				case 9379:
				case >= 9717 and <= 9724:
				case >= 9726 and <= 9737:
				case >= 9739 and <= 9740:
				case >= 9743 and <= 9771:
				case >= 9901 and <= 10004:
				case >= 10032 and <= 10041:
				case >= 10047 and <= 10050:
				case >= 10052 and <= 10053:
				case >= 10071 and <= 10073:
				case 10076:
				case >= 10080 and <= 10082:
				case 10086:
				case >= 10090 and <= 10093:
				case 10096:
				case >= 10099 and <= 10110:
				case >= 10112 and <= 10124:
				case 10127:
				case >= 10132 and <= 10146:
				case >= 10152 and <= 10154:
					return "2.5";
				case 9725:
				case >= 10067 and <= 10070:
				case 10077:
				case 10087:
				case 10111:
					return "2.55";
				case 29:
				case 2307:
				case 6181:
				case 7565:
				case 9351:
				case >= 9560 and <= 9629:
				case 9738:
				case >= 9741 and <= 9742:
				case >= 9772 and <= 9853:
				case >= 9855 and <= 9856:
				case >= 9858 and <= 9859:
				case >= 9861 and <= 9862:
				case >= 9864 and <= 9865:
				case >= 9867 and <= 9868:
				case >= 9870 and <= 9871:
				case >= 9873 and <= 9874:
				case >= 9876 and <= 9877:
				case >= 9879 and <= 9880:
				case >= 9882 and <= 9885:
				case >= 9887 and <= 9890:
				case >= 9892 and <= 9893:
				case >= 9895 and <= 9900:
				case >= 10005 and <= 10024:
				case >= 10026 and <= 10031:
				case >= 10042 and <= 10046:
				case 10051:
				case >= 10054 and <= 10066:
				case >= 10078 and <= 10079:
				case >= 10083 and <= 10085:
				case >= 10088 and <= 10089:
				case >= 10094 and <= 10095:
				case >= 10097 and <= 10098:
				case 10125:
				case >= 10128 and <= 10131:
				case >= 10147 and <= 10151:
					return "2.51";
				case 30:
				case 2220:
				case 2650:
				case 2993:
				case 5608:
				case 5613:
				case 5618:
				case 5623:
				case 5628:
				case 5633:
				case 5638:
				case 5643:
				case 5648:
				case 5653:
				case 5658:
				case 5663:
				case 5668:
				case 5673:
				case 5678:
				case 5683:
				case 5688:
				case 5693:
				case 5698:
				case 5703:
				case 5708:
				case 5713:
				case 5718:
				case 5723:
				case 6032:
				case 6109:
				case 7968:
				case 8574:
				case >= 8727 and <= 8731:
				case >= 8839 and <= 8840:
				case >= 9462 and <= 9463:
				case >= 9645 and <= 9668:
				case >= 9670 and <= 9700:
				case >= 9702 and <= 9716:
				case 10025:
				case 10074:
				case >= 10155 and <= 10179:
				case >= 10307 and <= 10308:
				case 10310:
				case >= 10322 and <= 10324:
				case >= 10329 and <= 10331:
				case >= 10335 and <= 10373:
				case >= 10386 and <= 10675:
				case >= 10677 and <= 10682:
				case >= 10684 and <= 10689:
				case >= 10691 and <= 10696:
				case >= 10698 and <= 10703:
				case >= 10705 and <= 10710:
				case >= 10712 and <= 10717:
				case >= 10719 and <= 10724:
				case >= 10726 and <= 10731:
				case >= 10733 and <= 10738:
				case >= 10740 and <= 10745:
				case >= 10747 and <= 10752:
				case >= 10754 and <= 10759:
				case >= 10761 and <= 10766:
				case >= 10768 and <= 10773:
				case >= 10775 and <= 10780:
				case >= 10782 and <= 10787:
				case >= 10789 and <= 10794:
				case >= 10796 and <= 10801:
				case >= 10803 and <= 10808:
				case >= 10810 and <= 10815:
				case >= 10817 and <= 10822:
				case >= 10824 and <= 10829:
				case >= 10831 and <= 10836:
				case >= 10838 and <= 10843:
				case >= 10845 and <= 10850:
				case >= 10852 and <= 10857:
				case >= 10859 and <= 10864:
				case >= 10866 and <= 10871:
				case >= 10873 and <= 10878:
				case >= 10880 and <= 10885:
				case >= 10887 and <= 10892:
				case >= 10894 and <= 10899:
				case >= 10901 and <= 10906:
				case >= 10908 and <= 10913:
				case >= 10915 and <= 10920:
				case >= 10922 and <= 10927:
				case >= 10929 and <= 10934:
				case >= 10936 and <= 10941:
				case >= 10943 and <= 10948:
				case >= 10950 and <= 10955:
				case >= 10957 and <= 10962:
				case >= 10964 and <= 11447:
				case >= 11586 and <= 11737:
				case >= 11871 and <= 11917:
				case >= 11919 and <= 11921:
				case >= 11923 and <= 11925:
				case >= 11927 and <= 11929:
				case >= 11931 and <= 11933:
				case >= 11935 and <= 11937:
				case >= 11939 and <= 11941:
				case >= 11943 and <= 11945:
				case >= 11947 and <= 11949:
				case >= 11951 and <= 11953:
				case >= 11955 and <= 11971:
				case >= 11973 and <= 11999:
				case >= 12001 and <= 12012:
				case >= 12014 and <= 12015:
				case >= 12017 and <= 12018:
				case >= 12020 and <= 12021:
				case >= 12023 and <= 12024:
				case >= 12026 and <= 12027:
				case >= 12029 and <= 12030:
				case >= 12032 and <= 12033:
				case >= 12035 and <= 12039:
				case 12041:
				case >= 12044 and <= 12046:
				case >= 12048 and <= 12049:
				case >= 12051 and <= 12059:
				case 12061:
				case >= 12063 and <= 12064:
				case >= 12066 and <= 12069:
				case >= 12072 and <= 12075:
				case >= 12077 and <= 12092:
				case >= 12094 and <= 12103:
				case >= 12108 and <= 12123:
				case >= 12125 and <= 12132:
				case >= 12134 and <= 12141:
				case >= 12143 and <= 12150:
				case >= 12152 and <= 12159:
				case >= 12161 and <= 12168:
				case >= 12170 and <= 12177:
				case >= 12179 and <= 12186:
				case >= 12188 and <= 12195:
				case >= 12197 and <= 12204:
				case >= 12206 and <= 12212:
				case >= 12214 and <= 12253:
				case >= 12256 and <= 12279:
				case >= 12356 and <= 12511:
				case >= 12518 and <= 12522:
				case >= 12524 and <= 12526:
				case >= 12528 and <= 12566:
				case >= 12568 and <= 12572:
				case >= 12574 and <= 12583:
				case >= 12585 and <= 12592:
				case >= 12594 and <= 12616:
				case >= 12622 and <= 12632:
				case >= 12634 and <= 12651:
				case >= 12653 and <= 12673:
				case >= 12698 and <= 12710:
				case >= 12713 and <= 12837:
				case >= 12841 and <= 12929:
				case >= 12936 and <= 12945:
				case >= 12966 and <= 12973:
				case >= 12985 and <= 12989:
				case >= 12991 and <= 13003:
					return "3.0";
				case 10075:
				case >= 11448 and <= 11509:
				case 12047:
				case 12062:
				case 12104:
				case 12106:
				case 12254:
				case 12652:
				case >= 12674 and <= 12680:
					return "3.01";
				case 8803:
				case 8822:
				case 8829:
				case >= 10316 and <= 10321:
				case >= 10332 and <= 10333:
				case 12042:
				case 12065:
				case 12093:
				case >= 12711 and <= 12712:
				case 12990:
				case >= 13062 and <= 13063:
				case 13079:
				case >= 13098 and <= 13102:
				case >= 13111 and <= 13113:
					return "3.07";
				case 31:
				case 8573:
				case >= 8842 and <= 8843:
				case >= 9630 and <= 9644:
				case >= 10180 and <= 10306:
				case 10309:
				case 10311:
				case >= 10325 and <= 10327:
				case >= 10374 and <= 10385:
				case 10676:
				case 10683:
				case 10690:
				case 10697:
				case 10704:
				case 10711:
				case 10718:
				case 10725:
				case 10732:
				case 10739:
				case 10746:
				case 10753:
				case 10760:
				case 10767:
				case 10774:
				case 10781:
				case 10788:
				case 10795:
				case 10802:
				case 10809:
				case 10816:
				case 10823:
				case 10830:
				case 10837:
				case 10844:
				case 10851:
				case 10858:
				case 10865:
				case 10872:
				case 10879:
				case 10886:
				case 10893:
				case 10900:
				case 10907:
				case 10914:
				case 10921:
				case 10928:
				case 10935:
				case 10942:
				case 10949:
				case 10956:
				case 10963:
				case >= 11510 and <= 11585:
				case >= 11738 and <= 11870:
				case 11918:
				case 11922:
				case 11926:
				case 11930:
				case 11934:
				case 11938:
				case 11942:
				case 11946:
				case 11950:
				case 11954:
				case 11972:
				case 12000:
				case 12013:
				case 12016:
				case 12019:
				case 12022:
				case 12025:
				case 12028:
				case 12031:
				case 12034:
				case 12040:
				case 12060:
				case 12076:
				case 12105:
				case 12107:
				case 12255:
				case >= 12280 and <= 12355:
				case >= 12513 and <= 12517:
				case 12523:
				case 12527:
				case 12567:
				case 12573:
				case 12584:
				case 12593:
				case >= 12617 and <= 12621:
				case 12633:
				case >= 12681 and <= 12684:
				case 12838:
				case 12840:
				case >= 12931 and <= 12935:
				case >= 12946 and <= 12965:
				case >= 12974 and <= 12984:
				case >= 13005 and <= 13052:
					return "3.05";
				case 8738:
				case 8740:
				case 8742:
				case 8749:
				case 8751:
				case 9464:
				case 12050:
				case 12071:
				case >= 13055 and <= 13058:
				case >= 13064 and <= 13078:
				case 13084:
				case >= 13090 and <= 13097:
				case >= 13114 and <= 13222:
				case >= 13237 and <= 13284:
				case >= 13286 and <= 13297:
				case >= 13321 and <= 13334:
				case >= 13354 and <= 13566:
				case >= 13625 and <= 13631:
				case >= 13637 and <= 13638:
				case >= 13640 and <= 13708:
				case >= 13711 and <= 13717:
				case >= 13720 and <= 13723:
				case >= 13726 and <= 13740:
				case >= 13742 and <= 13775:
					return "3.1";
				case 12124:
				case 12133:
				case 12142:
				case 12151:
				case 12160:
				case 12169:
				case 12178:
				case 12187:
				case 12196:
				case 12205:
				case 12213:
				case >= 13080 and <= 13083:
				case >= 13223 and <= 13236:
				case >= 13298 and <= 13299:
				case >= 13567 and <= 13624:
				case >= 13632 and <= 13636:
				case >= 13987 and <= 13990:
					return "3.15";
				case 32:
				case 9465:
				case >= 13059 and <= 13061:
				case >= 13085 and <= 13089:
				case 13285:
				case >= 13300 and <= 13320:
				case 13639:
				case >= 13724 and <= 13725:
				case 13741:
				case >= 13790 and <= 13877:
				case >= 14003 and <= 14006:
				case >= 14014 and <= 14053:
				case >= 14058 and <= 14070:
				case >= 14080 and <= 14083:
				case >= 14085 and <= 14103:
				case >= 14106 and <= 14164:
				case >= 14176 and <= 14840:
				case >= 14850 and <= 14853:
				case >= 14868 and <= 14869:
				case >= 14884 and <= 14898:
				case >= 14900 and <= 14903:
				case >= 14924 and <= 14944:
				case >= 14955 and <= 14957:
				case >= 14959 and <= 14981:
				case >= 15097 and <= 15112:
					return "3.2";
				case 8206:
				case 9854:
				case 9857:
				case 9860:
				case 9863:
				case 9866:
				case 9869:
				case 9872:
				case 9875:
				case 9878:
				case 9881:
				case 9886:
				case 9891:
				case 9894:
				case 13104:
				case >= 13335 and <= 13337:
				case >= 13341 and <= 13342:
				case >= 13776 and <= 13789:
				case 14054:
				case 14084:
				case >= 14861 and <= 14867:
				case >= 14870 and <= 14883:
				case 14899:
				case >= 14904 and <= 14923:
				case 14958:
				case >= 15115 and <= 15129:
					return "3.25";
				case 15156:
				case 15163:
				case >= 15167 and <= 15222:
				case 15266:
				case 15422:
				case >= 15424 and <= 15425:
				case >= 15430 and <= 15431:
				case 15434:
				case 15443:
				case 15446:
				case 15448:
				case >= 15467 and <= 15471:
				case >= 15612 and <= 15614:
				case >= 15811 and <= 15812:
				case >= 15842 and <= 15844:
				case 15933:
				case >= 15938 and <= 15944:
				case 15946:
				case >= 16147 and <= 16151:
					return "3.35";
				case 9466:
				case 9669:
				case 9701:
				case 12839:
				case >= 13892 and <= 13926:
				case >= 14007 and <= 14013:
				case >= 14055 and <= 14057:
				case >= 14071 and <= 14079:
				case >= 14104 and <= 14105:
				case >= 14841 and <= 14849:
				case >= 15130 and <= 15133:
				case 15143:
				case >= 15145 and <= 15155:
				case >= 15157 and <= 15161:
				case >= 15164 and <= 15166:
				case >= 15223 and <= 15236:
				case 15265:
				case >= 15267 and <= 15283:
				case 15287:
				case 15291:
				case 15295:
				case 15299:
				case >= 15303 and <= 15421:
				case 15423:
				case 15427:
				case 15429:
				case >= 15432 and <= 15433:
				case >= 15435 and <= 15440:
				case 15442:
				case >= 15444 and <= 15445:
				case 15447:
				case >= 15450 and <= 15461:
				case >= 15472 and <= 15475:
				case >= 15478 and <= 15479:
				case >= 15505 and <= 15583:
				case >= 15615 and <= 15638:
				case >= 15645 and <= 15771:
				case 15779:
				case 15783:
				case 15787:
				case 15791:
				case 15795:
				case >= 15799 and <= 15810:
				case >= 15813 and <= 15841:
				case >= 15854 and <= 15872:
				case >= 15893 and <= 15918:
				case >= 15921 and <= 15932:
				case >= 15935 and <= 15936:
				case 15945:
				case >= 15947 and <= 15949:
					return "3.3";
				case 33:
				case >= 8735 and <= 8736:
				case >= 13709 and <= 13710:
				case >= 14171 and <= 14175:
				case >= 14854 and <= 14860:
				case >= 14945 and <= 14954:
				case 15095:
				case 15284:
				case 15288:
				case 15292:
				case 15296:
				case 15300:
				case >= 15462 and <= 15466:
				case >= 15476 and <= 15477:
				case >= 15495 and <= 15504:
				case >= 15584 and <= 15611:
				case >= 15639 and <= 15644:
				case >= 15772 and <= 15778:
				case 15780:
				case 15784:
				case 15788:
				case 15792:
				case 15796:
				case >= 15873 and <= 15892:
				case >= 15950 and <= 15956:
				case >= 15964 and <= 15965:
				case >= 15967 and <= 15977:
				case >= 15979 and <= 15984:
				case >= 15988 and <= 15991:
				case 16002:
				case 16009:
				case >= 16013 and <= 16026:
				case >= 16028 and <= 16030:
				case 16040:
				case 16049:
				case >= 16065 and <= 16066:
				case >= 16152 and <= 16165:
				case >= 16170 and <= 16558:
				case >= 16562 and <= 16564:
				case >= 16568 and <= 16575:
				case >= 16596 and <= 16611:
				case 16613:
				case >= 16616 and <= 16735:
				case >= 16742 and <= 16771:
				case >= 16778 and <= 16796:
				case >= 16799 and <= 16811:
				case >= 16816 and <= 16828:
				case >= 16831 and <= 16911:
				case >= 16926 and <= 16929:
					return "3.4";
				case 13339:
				case >= 15237 and <= 15250:
				case 15966:
				case 15978:
				case >= 15985 and <= 15987:
				case >= 15992 and <= 16001:
				case >= 16003 and <= 16008:
				case 16010:
				case 16064:
				case >= 16166 and <= 16169:
				case 16612:
				case >= 16614 and <= 16615:
				case >= 16736 and <= 16741:
				case 16777:
				case >= 16797 and <= 16798:
				case >= 16812 and <= 16815:
				case >= 16829 and <= 16830:
				case >= 16912 and <= 16925:
					return "3.45";
				case 10328:
				case >= 14982 and <= 14983:
				case 15096:
				case >= 15251 and <= 15264:
				case 15285:
				case 15289:
				case 15293:
				case 15297:
				case 15301:
				case 15428:
				case >= 15487 and <= 15491:
				case 15781:
				case 15785:
				case 15789:
				case 15793:
				case 15797:
				case >= 15957 and <= 15963:
				case 16027:
				case >= 16042 and <= 16048:
				case >= 16067 and <= 16145:
				case >= 16559 and <= 16560:
				case >= 16565 and <= 16567:
				case >= 16772 and <= 16775:
				case 16930:
				case >= 16932 and <= 16933:
				case >= 16935 and <= 16985:
				case 17000:
				case >= 17003 and <= 17041:
				case >= 17043 and <= 17136:
				case >= 17138 and <= 17164:
				case >= 17343 and <= 17469:
				case >= 17476 and <= 17490:
				case >= 17495 and <= 17499:
				case >= 17522 and <= 17523:
				case >= 17525 and <= 17527:
				case >= 17529 and <= 17548:
				case >= 17572 and <= 17574:
				case >= 17577 and <= 17593:
				case >= 17595 and <= 17603:
				case >= 17618 and <= 17625:
				case >= 17630 and <= 17656:
				case >= 17663 and <= 17664:
				case >= 17679 and <= 17683:
				case >= 17685 and <= 17686:
				case 17688:
				case >= 17690 and <= 17717:
					return "3.5";
				case >= 13878 and <= 13891:
				case 15286:
				case 15290:
				case 15294:
				case 15298:
				case 15302:
				case 15441:
				case >= 15480 and <= 15481:
				case 15782:
				case 15786:
				case 15790:
				case 15794:
				case 15798:
				case >= 16050 and <= 16063:
				case 16776:
				case 16934:
				case >= 16986 and <= 16999:
				case 17042:
				case >= 17244 and <= 17342:
				case >= 17471 and <= 17474:
				case >= 17500 and <= 17521:
				case 17524:
				case 17528:
				case >= 17549 and <= 17571:
				case 17575:
				case 17594:
				case 17626:
				case >= 17657 and <= 17662:
				case >= 17665 and <= 17678:
				case 17684:
				case 17687:
				case 17689:
				case >= 17723 and <= 17725:
				case >= 17727 and <= 17730:
					return "3.55";
				case >= 17165 and <= 17243:
				case >= 17491 and <= 17494:
				case 17576:
				case >= 17627 and <= 17629:
					return "3.56";
				case >= 15919 and <= 15920:
					return "3.57";
				case 35:
				case 16041:
				case >= 17844 and <= 17846:
				case >= 17861 and <= 17863:
				case >= 17868 and <= 17876:
				case 17966:
				case >= 17975 and <= 17976:
				case 17978:
				case >= 17996 and <= 18005:
				case >= 18969 and <= 19046:
				case >= 19104 and <= 19107:
				case >= 19118 and <= 19121:
				case >= 19203 and <= 19436:
				case >= 19499 and <= 19505:
				case 19810:
				case 19818:
				case 19836:
				case 19840:
				case 19876:
				case >= 19896 and <= 19900:
				case 19929:
				case 19935:
				case >= 19948 and <= 19949:
				case >= 19960 and <= 19961:
				case 19990:
				case 19992:
				case 19998:
				case 20005:
				case >= 20261 and <= 20271:
				case 20304:
				case >= 20475 and <= 20478:
				case 20532:
				case 20534:
				case 20537:
				case 20548:
				case 20556:
				case >= 20564 and <= 20566:
				case >= 20622 and <= 20625:
				case >= 20628 and <= 20636:
					return "4.05";
				case 34:
				case 13991:
				case >= 16031 and <= 16039:
				case 17137:
				case 17726:
				case >= 17740 and <= 17843:
				case >= 17847 and <= 17860:
				case >= 17864 and <= 17865:
				case >= 17877 and <= 17962:
				case 17965:
				case >= 17967 and <= 17974:
				case 17977:
				case >= 17979 and <= 17980:
				case >= 17982 and <= 17995:
				case >= 18006 and <= 18968:
				case >= 19047 and <= 19103:
				case >= 19109 and <= 19110:
				case >= 19123 and <= 19202:
				case >= 19506 and <= 19589:
				case >= 19612 and <= 19661:
				case >= 19727 and <= 19756:
				case >= 19767 and <= 19769:
				case >= 19771 and <= 19809:
				case >= 19811 and <= 19817:
				case >= 19819 and <= 19835:
				case >= 19837 and <= 19839:
				case >= 19841 and <= 19875:
				case >= 19877 and <= 19895:
				case >= 19901 and <= 19928:
				case >= 19930 and <= 19934:
				case >= 19936 and <= 19947:
				case >= 19950 and <= 19959:
				case >= 19962 and <= 19989:
				case 19991:
				case >= 19993 and <= 19997:
				case >= 19999 and <= 20004:
				case >= 20006 and <= 20260:
				case >= 20272 and <= 20303:
				case >= 20306 and <= 20309:
				case >= 20311 and <= 20439:
				case >= 20442 and <= 20474:
				case >= 20489 and <= 20531:
				case 20533:
				case 20536:
				case >= 20538 and <= 20542:
				case >= 20544 and <= 20547:
				case >= 20549 and <= 20555:
				case >= 20557 and <= 20562:
				case >= 20568 and <= 20621:
				case 20627:
				case >= 20637 and <= 20639:
				case >= 20642 and <= 20674:
				case 20677:
					return "4.0";
				case 19108:
				case >= 19111 and <= 19117:
				case 19122:
				case >= 19437 and <= 19498:
				case >= 19590 and <= 19600:
				case >= 19662 and <= 19716:
				case 20543:
				case >= 20675 and <= 20676:
					return "4.01";
				case 20305:
				case >= 20678 and <= 20679:
				case >= 21027 and <= 21032:
				case 21051:
				case 21192:
				case 21277:
				case 21307:
					return "4.15";
				case >= 8746 and <= 8747:
				case >= 13718 and <= 13719:
				case >= 15482 and <= 15486:
				case 15934:
				case 16561:
				case >= 16588 and <= 16595:
				case 16931:
				case 17002:
				case >= 17604 and <= 17617:
				case >= 17731 and <= 17739:
				case 17963:
				case 17981:
				case >= 19601 and <= 19611:
				case >= 19717 and <= 19726:
				case >= 19757 and <= 19766:
				case 19770:
				case 20310:
				case >= 20440 and <= 20441:
				case >= 20680 and <= 20745:
				case >= 20747 and <= 20817:
				case >= 20819 and <= 20922:
				case >= 20943 and <= 20958:
				case >= 20975 and <= 21026:
				case >= 21033 and <= 21034:
				case >= 21042 and <= 21048:
				case 21050:
				case >= 21052 and <= 21060:
				case >= 21062 and <= 21188:
				case >= 21193 and <= 21196:
				case >= 21198 and <= 21207:
				case >= 21274 and <= 21276:
				case >= 21278 and <= 21306:
				case >= 21317 and <= 21320:
					return "4.1";
				case 20818:
				case >= 20959 and <= 20974:
				case 21197:
					return "4.11";
				case 36:
				case >= 16578 and <= 16587:
				case >= 20479 and <= 20488:
				case 20535:
				case 20563:
				case 20567:
				case 20641:
				case 20746:
				case 21049:
				case >= 21189 and <= 21190:
				case >= 21208 and <= 21273:
				case >= 21321 and <= 21800:
				case >= 21804 and <= 21813:
				case >= 21815 and <= 21830:
				case >= 21833 and <= 21836:
				case >= 21839 and <= 21851:
				case >= 21854 and <= 21869:
				case >= 21871 and <= 21873:
				case >= 21875 and <= 21901:
				case >= 21903 and <= 21906:
				case >= 21908 and <= 21909:
				case >= 21911 and <= 21914:
				case 21916:
				case >= 21920 and <= 21941:
				case >= 22307 and <= 22356:
				case >= 22361 and <= 22366:
				case >= 22377 and <= 22404:
				case >= 22411 and <= 22451:
				case 22459:
				case >= 22462 and <= 22478:
				case >= 22481 and <= 22498:
				case >= 22500 and <= 22507:
					return "4.2";
				case 21035:
				case 21191:
				case >= 21801 and <= 21803:
				case >= 21831 and <= 21832:
				case 21852:
				case 21870:
				case 21874:
				case 21907:
				case >= 21917 and <= 21919:
				case >= 21942 and <= 22306:
				case >= 22358 and <= 22360:
				case >= 22452 and <= 22456:
				case 22461:
				case >= 22479 and <= 22480:
				case 22499:
				case >= 22508 and <= 22519:
				case 22522:
					return "4.25";
				case 22532:
				case >= 22538 and <= 22541:
				case 22566:
				case 22579:
				case >= 22581 and <= 22582:
				case >= 22977 and <= 22996:
				case >= 23023 and <= 23024:
				case 23029:
				case 23033:
				case >= 23044 and <= 23045:
				case 23048:
				case >= 23163 and <= 23164:
				case 23166:
				case 23177:
				case >= 23223 and <= 23225:
				case >= 23227 and <= 23228:
				case >= 23318 and <= 23320:
				case 23365:
				case 23369:
				case 23381:
					return "4.35";
				case 6541:
				case 17470:
				case 20640:
				case 21838:
				case 21902:
				case 21915:
				case >= 22367 and <= 22376:
				case 22460:
				case >= 22520 and <= 22521:
				case >= 22525 and <= 22531:
				case >= 22533 and <= 22537:
				case >= 22543 and <= 22560:
				case >= 22562 and <= 22564:
				case >= 22567 and <= 22570:
				case >= 22572 and <= 22578:
				case 22580:
				case >= 22584 and <= 22598:
				case >= 22616 and <= 22745:
				case >= 22748 and <= 22867:
				case >= 22884 and <= 22924:
				case >= 23001 and <= 23002:
				case >= 23013 and <= 23020:
				case 23022:
				case >= 23025 and <= 23026:
				case >= 23030 and <= 23032:
				case >= 23034 and <= 23037:
				case 23043:
				case 23047:
				case >= 23051 and <= 23075:
				case >= 23097 and <= 23119:
				case 23124:
				case >= 23143 and <= 23162:
				case 23165:
				case >= 23167 and <= 23174:
				case 23176:
				case >= 23178 and <= 23210:
				case >= 23220 and <= 23222:
				case 23226:
				case >= 23230 and <= 23308:
				case >= 23315 and <= 23317:
				case >= 23321 and <= 23341:
				case 23360:
				case 23364:
				case 23367:
				case >= 23370 and <= 23376:
				case 23380:
				case 23382:
					return "4.3";
				case 22542:
				case 22561:
				case 22571:
				case 22583:
				case >= 22599 and <= 22615:
				case >= 22925 and <= 22976:
				case >= 22997 and <= 23000:
				case >= 23027 and <= 23028:
				case 23046:
				case >= 23049 and <= 23050:
				case >= 23126 and <= 23142:
				case >= 23213 and <= 23219:
				case 23229:
				case 23309:
				case >= 23342 and <= 23359:
				case 23366:
				case >= 23377 and <= 23379:
				case >= 23383 and <= 23393:
					return "4.36";
				case 22565:
				case >= 22868 and <= 22883:
				case >= 23120 and <= 23123:
				case 23175:
					return "4.31";
				case 21814:
				case >= 22457 and <= 22458:
				case 23858:
				case >= 23881 and <= 23882:
				case >= 23888 and <= 23890:
				case 23907:
				case >= 23912 and <= 23913:
				case 23915:
				case >= 24000 and <= 24001:
				case >= 24007 and <= 24142:
				case 24144:
				case 24163:
				case 24219:
				case 24224:
				case 24233:
				case 24283:
				case >= 24285 and <= 24287:
				case >= 24312 and <= 24313:
				case 24339:
				case 24343:
					return "4.45";
				case 37:
				case 15449:
				case >= 21308 and <= 21316:
				case 21910:
				case >= 23003 and <= 23012:
				case 23021:
				case >= 23038 and <= 23042:
				case >= 23076 and <= 23096:
				case >= 23361 and <= 23362:
				case >= 23394 and <= 23857:
				case >= 23859 and <= 23865:
				case >= 23867 and <= 23880:
				case >= 23883 and <= 23887:
				case >= 23891 and <= 23906:
				case >= 23908 and <= 23911:
				case 23914:
				case >= 23916 and <= 23981:
				case >= 23984 and <= 23986:
				case >= 23989 and <= 23991:
				case >= 23993 and <= 23994:
				case >= 23997 and <= 23999:
				case >= 24002 and <= 24006:
				case 24143:
				case >= 24145 and <= 24147:
				case >= 24158 and <= 24162:
				case >= 24165 and <= 24218:
				case 24222:
				case >= 24226 and <= 24232:
				case 24234:
				case >= 24240 and <= 24265:
				case >= 24274 and <= 24282:
				case 24284:
				case >= 24288 and <= 24311:
				case >= 24316 and <= 24338:
				case >= 24340 and <= 24342:
					return "4.4";
				case 15144:
				case >= 17718 and <= 17722:
				case 21061:
				case 22405:
				case >= 22407 and <= 22410:
				case >= 22746 and <= 22747:
				case >= 23310 and <= 23314:
				case 23363:
				case 23983:
				case >= 23987 and <= 23988:
				case >= 23995 and <= 23996:
				case >= 24220 and <= 24221:
				case >= 24314 and <= 24315:
				case >= 24344 and <= 24347:
				case >= 24368 and <= 24487:
				case >= 24489 and <= 24534:
				case 24536:
				case >= 24538 and <= 24593:
				case >= 24599 and <= 24607:
				case >= 24612 and <= 24615:
				case >= 24622 and <= 24623:
				case 24625:
				case >= 24627 and <= 24628:
				case 24634:
				case 24636:
				case 24639:
				case 24642:
				case >= 24796 and <= 24799:
				case 24801:
				case >= 24803 and <= 24805:
				case >= 24821 and <= 24831:
				case >= 24859 and <= 24869:
				case 24872:
				case >= 24874 and <= 24876:
				case >= 24878 and <= 24879:
				case >= 24881 and <= 24900:
				case >= 24902 and <= 24903:
				case 24908:
				case >= 24910 and <= 24989:
				case >= 24999 and <= 25000:
				case 25002:
				case >= 25005 and <= 25006:
				case >= 25008 and <= 25037:
				case >= 25054 and <= 25056:
				case >= 25058 and <= 25066:
				case >= 25070 and <= 25077:
				case >= 25084 and <= 25086:
					return "4.5";
				case >= 20931 and <= 20932:
				case 24488:
				case 24535:
				case 24626:
				case >= 24630 and <= 24631:
				case 24635:
				case 24640:
				case >= 24643 and <= 24792:
				case >= 24806 and <= 24820:
				case >= 24832 and <= 24858:
				case >= 24870 and <= 24871:
				case 24873:
				case >= 24904 and <= 24907:
				case >= 24996 and <= 24998:
				case 25001:
				case >= 25067 and <= 25069:
					return "4.55";
				case 24164:
				case 24537:
				case >= 24594 and <= 24598:
				case >= 24608 and <= 24611:
				case >= 24617 and <= 24621:
				case 24624:
				case >= 24637 and <= 24638:
				case 24641:
				case 24794:
				case 24800:
				case 24802:
				case 24877:
				case 24880:
				case 24901:
				case 24909:
				case >= 24990 and <= 24995:
				case 25007:
				case >= 25078 and <= 25083:
				case >= 27919 and <= 27920:
					return "4.56";
				case 38:
				case 15162:
				case 23982:
				case 23992:
				case >= 24148 and <= 24157:
				case 24629:
				case >= 24632 and <= 24633:
				case 25057:
				case >= 25180 and <= 25467:
				case >= 25628 and <= 26427:
				case >= 26533 and <= 26778:
				case >= 26780 and <= 26781:
				case >= 26783 and <= 26784:
				case >= 26787 and <= 26788:
				case >= 26790 and <= 26794:
				case >= 26796 and <= 26801:
				case >= 26803 and <= 26810:
				case >= 26819 and <= 26891:
				case >= 26903 and <= 26934:
				case >= 27077 and <= 27286:
				case >= 27288 and <= 27291:
				case >= 27294 and <= 27305:
				case >= 27312 and <= 27392:
				case >= 27410 and <= 27693:
				case >= 27696 and <= 27715:
				case >= 27720 and <= 27736:
				case >= 27738 and <= 27742:
				case >= 27745 and <= 27759:
				case 27761:
				case >= 27763 and <= 27790:
				case >= 27797 and <= 27843:
				case >= 27849 and <= 27882:
				case >= 27893 and <= 27914:
				case 27917:
				case >= 27921 and <= 27937:
				case >= 27940 and <= 27955:
				case >= 27957 and <= 27975:
				case >= 27978 and <= 27983:
				case >= 27985 and <= 27990:
				case >= 27992 and <= 27994:
				case 28062:
					return "5.0";
				case 39:
				case >= 24266 and <= 24273:
				case >= 24348 and <= 24367:
				case >= 25468 and <= 25627:
				case >= 26428 and <= 26532:
				case 26779:
				case 26782:
				case 26785:
				case 26789:
				case 26802:
				case >= 26811 and <= 26818:
				case >= 26892 and <= 26902:
				case >= 26935 and <= 27014:
				case 27287:
				case >= 27292 and <= 27293:
				case >= 27306 and <= 27311:
				case >= 27400 and <= 27403:
				case >= 27405 and <= 27408:
				case >= 27694 and <= 27695:
				case >= 27716 and <= 27719:
				case 27737:
				case >= 27743 and <= 27744:
				case 27760:
				case 27762:
				case >= 27791 and <= 27796:
				case >= 27844 and <= 27848:
				case >= 27883 and <= 27892:
				case 27918:
				case >= 27938 and <= 27939:
				case 27956:
				case >= 27976 and <= 27977:
				case 27984:
				case 27991:
				case >= 27995 and <= 27999:
					return "5.05";
				case 26795:
				case >= 27015 and <= 27076:
				case >= 27393 and <= 27399:
				case 27404:
				case 27409:
				case >= 27915 and <= 27916:
				case 28061:
					return "5.01";
				case 22406:
				case 23866:
				case >= 24236 and <= 24239:
				case >= 28063 and <= 28072:
				case >= 28081 and <= 28115:
				case 28117:
				case 28121:
				case 28123:
				case >= 28126 and <= 28136:
				case >= 28139 and <= 28147:
				case 28149:
				case >= 28151 and <= 28156:
				case 28158:
				case >= 28162 and <= 28186:
				case >= 28189 and <= 28481:
				case >= 28501 and <= 28507:
				case >= 28509 and <= 28555:
				case >= 28558 and <= 28594:
				case 28612:
				case >= 28614 and <= 28622:
				case >= 28624 and <= 28627:
				case >= 28629 and <= 28634:
				case >= 28636 and <= 28641:
				case >= 28648 and <= 28650:
				case >= 28652 and <= 28889:
				case >= 28892 and <= 28895:
				case >= 28897 and <= 28924:
					return "5.1";
				case 28116:
				case 28120:
				case 28148:
				case 28150:
				case >= 28160 and <= 28161:
				case >= 28492 and <= 28496:
				case 28613:
				case 28623:
				case 28635:
				case >= 28642 and <= 28647:
				case >= 28890 and <= 28891:
				case >= 29980 and <= 29982:
					return "5.15";
				case 28079:
				case 28118:
					return "5.18";
				case >= 28124 and <= 28125:
				case 28628:
				case >= 28963 and <= 28964:
				case 28971:
				case 28979:
				case 28989:
				case 28995:
				case 29403:
				case 29707:
				case >= 29792 and <= 29947:
				case 29992:
				case >= 29994 and <= 30033:
				case >= 30047 and <= 30054:
				case 30089:
				case >= 30094 and <= 30095:
				case 30100:
				case 30104:
				case >= 30110 and <= 30130:
				case >= 30259 and <= 30262:
				case 30267:
				case >= 30269 and <= 30271:
				case >= 30278 and <= 30281:
					return "5.21";
				case 40:
				case 24235:
				case >= 25003 and <= 25004:
				case 26786:
				case >= 28073 and <= 28078:
				case 28080:
				case 28119:
				case 28122:
				case 28137:
				case 28187:
				case 28508:
				case >= 28595 and <= 28599:
				case >= 28925 and <= 28962:
				case >= 28965 and <= 28967:
				case >= 28969 and <= 28970:
				case 28972:
				case >= 28974 and <= 28977:
				case >= 28980 and <= 28981:
				case >= 28983 and <= 28988:
				case >= 28990 and <= 28994:
				case >= 28996 and <= 29402:
				case >= 29404 and <= 29611:
				case >= 29681 and <= 29706:
				case >= 29708 and <= 29791:
				case >= 29948 and <= 29979:
				case >= 29983 and <= 29991:
				case >= 30034 and <= 30046:
				case >= 30055 and <= 30088:
				case >= 30090 and <= 30093:
				case >= 30096 and <= 30098:
				case 30101:
				case 30103:
				case >= 30105 and <= 30109:
				case >= 30131 and <= 30135:
				case >= 30246 and <= 30258:
				case >= 30263 and <= 30266:
				case 30268:
				case 30272:
				case >= 30274 and <= 30277:
					return "5.2";
				case 28968:
				case 28973:
				case 28978:
				case 28982:
				case >= 29612 and <= 29680:
				case 29993:
				case >= 30136 and <= 30245:
				case 30273:
					return "5.25";
				case 28188:
				case 30341:
				case 30362:
				case >= 30364 and <= 30412:
				case >= 30424 and <= 30594:
				case >= 30596 and <= 30714:
				case >= 30750 and <= 30758:
				case >= 30803 and <= 30860:
				case >= 30863 and <= 30869:
				case >= 30872 and <= 30874:
				case >= 30877 and <= 30883:
				case >= 30970 and <= 31100:
				case >= 31136 and <= 31183:
				case >= 31320 and <= 31323:
				case >= 31329 and <= 31338:
				case 31340:
				case >= 31342 and <= 31343:
				case >= 31346 and <= 31350:
				case >= 31394 and <= 31397:
				case 31400:
				case >= 31404 and <= 31405:
				case >= 31408 and <= 31572:
				case 31577:
				case >= 31649 and <= 31663:
				case >= 31670 and <= 31673:
				case >= 31676 and <= 31681:
				case >= 31684 and <= 31713:
					return "5.3";
				case 30102:
				case >= 30282 and <= 30340:
				case >= 30342 and <= 30361:
				case 30363:
				case >= 30419 and <= 30423:
				case >= 30715 and <= 30749:
				case >= 30767 and <= 30802:
				case 30861:
				case 30870:
				case 30876:
				case >= 30884 and <= 30969:
				case >= 31101 and <= 31135:
				case 31326:
				case >= 31344 and <= 31345:
				case >= 31351 and <= 31393:
				case 31402:
				case 31407:
				case >= 31573 and <= 31576:
				case >= 31631 and <= 31648:
				case >= 31664 and <= 31669:
					return "5.35";
				case >= 30413 and <= 30418:
				case >= 30762 and <= 30766:
				case 30862:
				case 30871:
				case 30875:
				case >= 31184 and <= 31319:
				case >= 31324 and <= 31325:
				case >= 31327 and <= 31328:
				case 31341:
				case 31401:
				case 31406:
				case >= 31578 and <= 31630:
				case >= 31674 and <= 31675:
					return "5.31";
				case 41:
				case >= 25038 and <= 25053:
				case 28157:
				case >= 28556 and <= 28557:
				case >= 28600 and <= 28610:
				case 28651:
				case 30099:
				case 31398:
				case >= 31682 and <= 31683:
				case >= 31772 and <= 31912:
				case >= 32049 and <= 32161:
				case >= 32202 and <= 32209:
				case 32212:
				case 32214:
				case 32219:
				case >= 32221 and <= 32224:
				case >= 32227 and <= 32228:
				case >= 32230 and <= 32237:
				case >= 32241 and <= 32249:
				case >= 32252 and <= 32639:
				case >= 32798 and <= 32803:
				case >= 32806 and <= 32823:
				case 32825:
				case 32830:
				case >= 32836 and <= 32837:
				case 32839:
				case >= 32843 and <= 32845:
				case >= 32847 and <= 32849:
				case 32856:
				case >= 32858 and <= 32865:
				case >= 32875 and <= 32881:
				case >= 32934 and <= 32955:
				case >= 32961 and <= 33013:
				case 33015:
				case >= 33021 and <= 33036:
				case 33038:
				case >= 33040 and <= 33042:
				case 33110:
				case >= 33115 and <= 33125:
				case >= 33129 and <= 33135:
				case >= 33139 and <= 33143:
				case >= 33145 and <= 33153:
					return "5.4";
				case >= 30759 and <= 30761:
				case 31339:
				case 31403:
				case >= 31714 and <= 31771:
				case >= 31913 and <= 32048:
				case >= 32162 and <= 32201:
				case >= 32210 and <= 32211:
				case 32213:
				case >= 32215 and <= 32218:
				case >= 32225 and <= 32226:
				case 32229:
				case >= 32239 and <= 32240:
				case >= 32250 and <= 32251:
				case >= 32640 and <= 32797:
				case 32824:
				case >= 32826 and <= 32827:
				case 32829:
				case >= 32831 and <= 32835:
				case >= 32840 and <= 32841:
				case 32846:
				case 32855:
				case 32857:
				case >= 32866 and <= 32869:
				case >= 32871 and <= 32874:
				case >= 32882 and <= 32933:
				case >= 32956 and <= 32960:
				case 33014:
				case >= 33016 and <= 33020:
				case 33037:
				case 33039:
				case >= 33043 and <= 33109:
				case >= 33111 and <= 33114:
				case >= 33126 and <= 33128:
				case >= 33136 and <= 33137:
				case 33144:
					return "5.41";
				case 32828:
				case >= 33239 and <= 33244:
				case 33254:
				case >= 33257 and <= 33259:
				case 33265:
				case 33271:
				case >= 33281 and <= 33285:
				case 33291:
				case >= 33294 and <= 33297:
				case >= 33335 and <= 33337:
				case 33340:
				case >= 33462 and <= 33479:
				case >= 33613 and <= 33647:
				case 33672:
				case 33676:
				case 33684:
				case 33689:
				case 33700:
				case 33706:
				case >= 33710 and <= 33711:
				case >= 33715 and <= 33750:
				case >= 33757 and <= 33817:
				case >= 33820 and <= 33837:
				case 33840:
				case 33845:
				case 33884:
				case 33886:
					return "5.55";
				case 28896:
				case 30595:
				case 31399:
				case 32220:
				case 32838:
				case 32850:
				case >= 32852 and <= 32854:
				case >= 33154 and <= 33238:
				case >= 33245 and <= 33253:
				case >= 33255 and <= 33256:
				case >= 33260 and <= 33264:
				case >= 33266 and <= 33270:
				case >= 33272 and <= 33280:
				case >= 33286 and <= 33290:
				case >= 33292 and <= 33293:
				case >= 33298 and <= 33334:
				case >= 33338 and <= 33339:
				case 33341:
				case >= 33356 and <= 33461:
				case >= 33480 and <= 33612:
				case >= 33648 and <= 33671:
				case >= 33673 and <= 33675:
				case 33677:
				case >= 33685 and <= 33688:
				case >= 33690 and <= 33699:
				case >= 33701 and <= 33705:
				case >= 33707 and <= 33709:
				case >= 33712 and <= 33714:
				case >= 33751 and <= 33756:
				case >= 33818 and <= 33819:
				case >= 33838 and <= 33839:
				case >= 33841 and <= 33844:
				case >= 33846 and <= 33883:
				case 33885:
				case >= 33887 and <= 33912:
					return "5.5";
				case 42:
				case 24225:
				case 32238:
				case >= 32804 and <= 32805:
				case 32842:
				case 32851:
				case 32870:
				case >= 33354 and <= 33355:
				case >= 33913 and <= 35019:
				case >= 35320 and <= 35555:
				case 35558:
				case >= 35560 and <= 35567:
				case >= 35569 and <= 35571:
				case >= 35573 and <= 35574:
				case >= 35576 and <= 35577:
				case >= 35579 and <= 35583:
				case >= 35588 and <= 35607:
				case >= 35626 and <= 35733:
				case >= 35744 and <= 35794:
				case >= 35797 and <= 35822:
				case 35827:
				case >= 35831 and <= 35852:
				case >= 35857 and <= 35867:
				case >= 35872 and <= 36007:
				case >= 36009 and <= 36028:
				case >= 36036 and <= 36066:
				case >= 36077 and <= 36098:
				case >= 36104 and <= 36108:
				case >= 36114 and <= 36117:
				case >= 36121 and <= 36172:
				case >= 36174 and <= 36186:
				case >= 36188 and <= 36200:
				case >= 36202 and <= 36212:
				case >= 36214 and <= 36217:
				case >= 36223 and <= 36231:
				case >= 36237 and <= 36251:
				case >= 36253 and <= 36340:
				case >= 36342 and <= 36367:
				case >= 36370 and <= 36627:
				case >= 36629 and <= 36631:
				case >= 36633 and <= 36636:
				case >= 36638 and <= 36654:
					return "6.0";
				case 43:
				case >= 35020 and <= 35319:
				case >= 35556 and <= 35557:
				case 35559:
				case 35568:
				case 35572:
				case 35575:
				case 35578:
				case >= 35584 and <= 35587:
				case >= 35618 and <= 35625:
				case >= 35734 and <= 35743:
				case 35796:
				case >= 35823 and <= 35826:
				case >= 35828 and <= 35830:
				case >= 35868 and <= 35871:
				case 36008:
				case >= 36029 and <= 36035:
				case >= 36067 and <= 36076:
				case >= 36099 and <= 36103:
				case >= 36109 and <= 36113:
				case 36173:
				case 36187:
				case 36201:
				case 36213:
				case >= 36218 and <= 36222:
				case >= 36232 and <= 36236:
				case 36252:
				case 36341:
				case >= 36368 and <= 36369:
				case 36628:
				case 36637:
					return "6.05";
				case >= 35608 and <= 35617:
				case >= 36118 and <= 36120:
				case 36656:
				case 36658:
				case >= 36679 and <= 36809:
				case >= 36811 and <= 36843:
				case >= 36849 and <= 36850:
				case >= 36852 and <= 36860:
				case >= 36863 and <= 36904:
				case 36906:
				case >= 36909 and <= 36910:
				case >= 36912 and <= 36913:
				case >= 36916 and <= 36942:
				case >= 36963 and <= 37334:
				case >= 37337 and <= 37349:
				case >= 37353 and <= 37357:
				case >= 37359 and <= 37363:
				case >= 37365 and <= 37366:
				case >= 37368 and <= 37382:
				case >= 37386 and <= 37389:
				case >= 37391 and <= 37399:
				case >= 37401 and <= 37413:
				case >= 37416 and <= 37418:
				case >= 37420 and <= 37492:
					return "6.1";
				case 36657:
				case >= 36659 and <= 36678:
				case >= 36844 and <= 36848:
				case 36851:
				case 36907:
				case >= 37335 and <= 37336:
				case >= 37351 and <= 37352:
				case 37358:
				case 37364:
				case 37367:
				case >= 37383 and <= 37385:
				case 37400:
				case 37414:
				case 37419:
				case 37493:
					return "6.15";
				case 36810:
				case >= 36943 and <= 36962:
					return "6.11";
				case 44:
				case >= 33678 and <= 33680:
				case 36632:
				case 36862:
				case 36905:
				case 36915:
				case >= 37549 and <= 37853:
				case >= 37856 and <= 38210:
				case >= 38212 and <= 38222:
				case >= 38228 and <= 38232:
				case >= 38238 and <= 38275:
				case >= 38348 and <= 38399:
				case >= 38421 and <= 38428:
				case >= 38433 and <= 38434:
				case >= 38436 and <= 38444:
				case >= 38446 and <= 38448:
				case 38450:
				case >= 38454 and <= 38455:
				case 38457:
				case 38460:
				case >= 38463 and <= 38464:
				case >= 38467 and <= 38532:
				case 38536:
				case 38538:
				case >= 38540 and <= 38558:
				case >= 38560 and <= 38569:
				case >= 38571 and <= 38585:
				case 38587:
				case 38589:
				case 38591:
				case >= 38593 and <= 38594:
				case 38599:
				case >= 38604 and <= 38620:
				case >= 38622 and <= 38631:
				case >= 38633 and <= 38638:
				case >= 38640 and <= 38690:
				case >= 38697 and <= 38714:
					return "6.2";
				case 37854:
				case >= 38223 and <= 38227:
				case >= 38276 and <= 38347:
				case >= 38400 and <= 38420:
				case >= 38429 and <= 38432:
				case 38435:
				case 38445:
				case >= 38451 and <= 38452:
				case 38456:
				case >= 38461 and <= 38462:
				case >= 38465 and <= 38466:
				case >= 38533 and <= 38535:
				case 38537:
				case 38539:
				case 38559:
				case 38570:
				case 38586:
				case 38588:
				case 38592:
				case >= 38595 and <= 38598:
				case >= 38600 and <= 38603:
				case 38621:
				case 38632:
				case 38639:
				case 38691:
					return "6.25";
				case >= 35853 and <= 35856:
				case 37390:
					return "6.28";
				case 23125:
				case 38211:
				case >= 38233 and <= 38237:
				case 38453:
				case >= 38458 and <= 38459:
				case >= 38810 and <= 38841:
				case >= 38890 and <= 38939:
				case >= 38948 and <= 38950:
				case >= 38953 and <= 39143:
				case >= 39224 and <= 39302:
				case >= 39308 and <= 39328:
				case >= 39349 and <= 39367:
				case >= 39369 and <= 39370:
				case >= 39373 and <= 39375:
				case >= 39379 and <= 39387:
				case 39390:
				case >= 39392 and <= 39395:
				case >= 39401 and <= 39419:
				case >= 39421 and <= 39425:
				case >= 39427 and <= 39470:
				case 39472:
				case >= 39474 and <= 39478:
				case >= 39481 and <= 39484:
				case >= 39487 and <= 39495:
				case >= 39497 and <= 39503:
				case >= 39509 and <= 39575:
				case >= 39578 and <= 39579:
				case >= 39582 and <= 39596:
				case 39598:
				case >= 39600 and <= 39612:
				case >= 39616 and <= 39629:
					return "6.3";
				case 38951:
				case >= 39164 and <= 39183:
					return "6.31";
				case 36911:
				case >= 38715 and <= 38809:
				case >= 38842 and <= 38889:
				case >= 38940 and <= 38947:
				case 38952:
				case >= 39144 and <= 39163:
				case >= 39184 and <= 39223:
				case >= 39329 and <= 39348:
				case 39368:
				case >= 39371 and <= 39372:
				case >= 39376 and <= 39378:
				case >= 39388 and <= 39389:
				case 39391:
				case >= 39396 and <= 39400:
				case 39420:
				case 39426:
				case 39471:
				case 39473:
				case >= 39479 and <= 39480:
				case >= 39485 and <= 39486:
				case >= 39504 and <= 39508:
				case >= 39576 and <= 39577:
				case 39580:
				case 39597:
				case 39599:
				case >= 39613 and <= 39615:
					return "6.35";
				case 45:
				case >= 33681 and <= 33683:
				case 36861:
				case >= 39303 and <= 39307:
				case 39496:
				case >= 39630 and <= 40356:
				case >= 40358 and <= 40361:
				case >= 40363 and <= 40385:
				case >= 40387 and <= 40451:
				case >= 40456 and <= 40499:
				case >= 40502 and <= 40657:
				case >= 40659 and <= 40660:
				case >= 40662 and <= 40663:
				case 40665:
				case >= 40671 and <= 40704:
				case >= 40706 and <= 40752:
				case 40754:
				case >= 40756 and <= 40764:
					return "6.4";
				case 40386:
				case >= 40500 and <= 40501:
				case 40664:
				case 40705:
				case 40755:
					return "6.48";
				case >= 15134 and <= 15142:
				case 37415:
				case 38449:
				case 38590:
				case 40357:
				case 40658:
				case 40661:
				case 40667:
				case 40753:
				case >= 40765 and <= 40931:
				case >= 41033 and <= 41077:
				case >= 41082 and <= 41087:
				case 41089:
				case >= 41091 and <= 41097:
				case >= 41102 and <= 41105:
				case >= 41107 and <= 41108:
				case 41111:
				case 41113:
				case >= 41130 and <= 41136:
				case >= 41138 and <= 41139:
				case >= 41141 and <= 41142:
				case >= 41149 and <= 41179:
				case >= 41307 and <= 41315:
				case >= 41322 and <= 41348:
				case >= 41367 and <= 41369:
				case >= 41371 and <= 41375:
				case 41380:
				case >= 41386 and <= 41394:
				case 41396:
				case >= 41401 and <= 41406:
				case >= 41413 and <= 41424:
				case >= 41426 and <= 41428:
				case >= 41458 and <= 41460:
				case 41468:
				case >= 41473 and <= 41477:
				case >= 41481 and <= 41483:
				case >= 41489 and <= 41490:
				case >= 41493 and <= 41494:
				case >= 41496 and <= 41497:
				case >= 41501 and <= 41502:
				case >= 41507 and <= 41553:
				case >= 41565 and <= 41586:
				case >= 41592 and <= 41618:
				case >= 41630 and <= 41657:
				case >= 41659 and <= 41661:
				case >= 41664 and <= 41677:
				case 41709:
					return "6.5";
				case >= 41078 and <= 41080:
				case >= 41180 and <= 41304:
				case >= 41376 and <= 41379:
				case >= 41381 and <= 41383:
				case 41385:
				case >= 41430 and <= 41442:
				case >= 41462 and <= 41463:
				case >= 41465 and <= 41467:
				case 41471:
				case >= 41479 and <= 41480:
				case 41486:
				case 41491:
				case 41495:
				case >= 41499 and <= 41500:
				case >= 41560 and <= 41564:
				case >= 41587 and <= 41591:
				case >= 41628 and <= 41629:
				case >= 41662 and <= 41663:
				case >= 41679 and <= 41700:
				case >= 41707 and <= 41708:
				case 41796:
					return "6.51";
				case 36908:
				case >= 40932 and <= 41032:
				case 41088:
				case 41106:
				case 41114:
				case >= 41144 and <= 41145:
				case >= 41305 and <= 41306:
				case 41370:
				case 41384:
				case 41395:
				case >= 41407 and <= 41412:
				case 41425:
				case 41429:
				case 41457:
				case >= 41469 and <= 41470:
				case >= 41503 and <= 41506:
				case 41658:
					return "6.55";
				case 41081:
				case 41397:
				case 41478:
				case 41484:
				case 41487:
				case >= 41555 and <= 41559:
				case >= 41619 and <= 41622:
				case >= 41701 and <= 41706:
				case 41797:
					return "6.58";
				case 46:
				case 41090:
				case >= 41109 and <= 41110:
				case >= 41115 and <= 41129:
				case 41143:
				case >= 41146 and <= 41147:
				case >= 41316 and <= 41318:
				case >= 41349 and <= 41357:
				case >= 41398 and <= 41400:
				case >= 41455 and <= 41456:
				case 41485:
				case 41488:
				case 41492:
				case 41678:
				case >= 41717 and <= 41793:
				case >= 41801 and <= 41804:
				case >= 41806 and <= 41812:
				case >= 41814 and <= 41821:
				case 41823:
				case >= 41829 and <= 42814:
				case >= 43178 and <= 43526:
				case >= 43537 and <= 43540:
				case >= 43556 and <= 43563:
				case >= 43565 and <= 43571:
				case >= 43573 and <= 43585:
				case >= 43587 and <= 43588:
				case >= 43590 and <= 43596:
				case >= 43598 and <= 43599:
				case >= 43601 and <= 43604:
				case >= 43606 and <= 43871:
				case >= 43873 and <= 43945:
				case >= 43953 and <= 44122:
				case >= 44131 and <= 44132:
				case >= 44134 and <= 44140:
				case >= 44157 and <= 44161:
				case >= 44167 and <= 44169:
				case >= 44185 and <= 44239:
				case >= 44241 and <= 44250:
				case 44256:
				case >= 44258 and <= 44259:
				case >= 44261 and <= 44268:
				case >= 44270 and <= 44304:
				case >= 44310 and <= 44322:
				case >= 44324 and <= 44333:
					return "7.0";
				case 47:
				case 40362:
				case >= 41099 and <= 41101:
				case 41112:
				case 41137:
				case 41140:
				case 41464:
				case 41805:
				case 41822:
				case >= 41824 and <= 41828:
				case >= 42870 and <= 43177:
				case >= 43527 and <= 43536:
				case >= 43549 and <= 43555:
				case 43572:
				case 43597:
				case 43600:
				case >= 43946 and <= 43952:
				case >= 44123 and <= 44130:
				case 44133:
				case >= 44141 and <= 44156:
				case >= 44162 and <= 44166:
				case >= 44170 and <= 44184:
				case 44240:
				case >= 44251 and <= 44255:
				case 44257:
				case 44269:
				case 44309:
				case 44323:
					return "7.05";
				case >= 42815 and <= 42869:
				case >= 43541 and <= 43548:
				case 43586:
				case 43872:
				case >= 44305 and <= 44308:
					return "7.01";
				case 40666:
				case 41472:
				case >= 44334 and <= 44338:
				case 44492:
				case >= 44500 and <= 44501:
				case 44507:
				case >= 44605 and <= 44639:
				case >= 44655 and <= 44660:
				case >= 44850 and <= 44864:
				case >= 44868 and <= 44869:
				case >= 44888 and <= 44892:
				case >= 44915 and <= 44924:
				case >= 44930 and <= 44933:
				case >= 45074 and <= 45078:
					return "7.15";
				case >= 40452 and <= 40455:
				case >= 41319 and <= 41321:
				case >= 41358 and <= 41366:
				case 41461:
				case 41813:
				case 43564:
				case 43589:
				case >= 44339 and <= 44352:
				case >= 44365 and <= 44490:
				case >= 44493 and <= 44497:
				case 44499:
				case 44502:
				case 44506:
				case 44508:
				case >= 44510 and <= 44604:
				case >= 44640 and <= 44654:
				case >= 44661 and <= 44666:
				case >= 44676 and <= 44720:
				case >= 44744 and <= 44849:
				case >= 44865 and <= 44867:
				case >= 44870 and <= 44887:
				case >= 44895 and <= 44914:
				case >= 44925 and <= 44929:
				case >= 44934 and <= 45017:
				case >= 45021 and <= 45040:
				case >= 45047 and <= 45073:
				case >= 45079 and <= 45570:
				case >= 45573 and <= 45576:
				case 45579:
				case 45590:
					return "7.1";
				case 45018:
					return "7.16";
				case >= 44721 and <= 44743:
				case >= 45019 and <= 45020:
				case 45577:
					return "7.11";
				case 48:
				case 22357:
				case 44260:
				case >= 44353 and <= 44364:
				case 44491:
				case 44498:
				case 44503:
				case >= 44668 and <= 44675:
				case >= 45571 and <= 45572:
				case >= 45968 and <= 46086:
				case >= 46283 and <= 46735:
				case >= 47906 and <= 47923:
				case 47928:
				case >= 47938 and <= 47959:
				case 47961:
				case 47971:
				case 47974:
				case 47976:
				case >= 47980 and <= 47981:
				case 47984:
				case 47986:
				case >= 47988 and <= 47999:
				case 48004:
				case >= 48006 and <= 48007:
				case >= 48086 and <= 48089:
				case >= 48097 and <= 48104:
				case >= 48106 and <= 48135:
				case >= 48145 and <= 48152:
				case 48155:
				case 48159:
				case >= 48173 and <= 48193:
				case 48201:
				case 48203:
				case >= 48214 and <= 48219:
				case >= 48222 and <= 48226:
				case >= 48228 and <= 48229:
				case 48231:
				case >= 48727 and <= 48731:
				case >= 48737 and <= 48741:
					return "7.2";
				case >= 45041 and <= 45044:
				case >= 47728 and <= 47905:
				case >= 47924 and <= 47927:
				case 47962:
				case >= 47967 and <= 47970:
				case 47972:
				case 47975:
				case >= 47977 and <= 47979:
				case 47983:
				case 47987:
				case >= 48000 and <= 48003:
				case >= 48008 and <= 48085:
				case 48090:
				case 48096:
				case 48105:
				case >= 48137 and <= 48144:
				case >= 48156 and <= 48157:
				case >= 48161 and <= 48162:
				case 48202:
				case >= 48204 and <= 48209:
				case 48230:
				case 48736:
				case 48742:
				case >= 48745 and <= 48749:
					return "7.25";
				case 44505:
				case 44509:
				case 45046:
				case >= 45586 and <= 45587:
				case >= 45591 and <= 45967:
				case >= 46181 and <= 46184:
				case >= 46279 and <= 46282:
				case >= 46849 and <= 46859:
				case >= 46972 and <= 46973:
				case 46975:
				case 46977:
				case 46983:
				case >= 47096 and <= 47107:
				case >= 47220 and <= 47221:
				case 47223:
				case 47225:
				case 47229:
				case >= 47344 and <= 47345:
				case >= 47347 and <= 47603:
				case >= 47716 and <= 47723:
				case >= 47929 and <= 47937:
				case 47966:
				case 47985:
				case >= 48091 and <= 48092:
				case 48136:
				case >= 48153 and <= 48154:
				case 48158:
				case 48160:
				case >= 48163 and <= 48172:
				case >= 48210 and <= 48213:
				case >= 48220 and <= 48221:
				case 48227:
				case >= 48232 and <= 48726:
				case >= 48732 and <= 48735:
				case >= 48743 and <= 48744:
				case 48750:
					return "7.21";
				case 43605:
				case >= 46087 and <= 46179:
				case >= 46185 and <= 46262:
				case >= 46736 and <= 46848:
				case >= 46860 and <= 46971:
				case 46974:
				case 46976:
				case >= 46978 and <= 46982:
				case >= 46984 and <= 47005:
				case >= 47108 and <= 47219:
				case 47222:
				case 47224:
				case >= 47226 and <= 47228:
				case >= 47230 and <= 47340:
				case 47346:
				case >= 47604 and <= 47715:
				case >= 47725 and <= 47727:
				case 47960:
				case >= 47963 and <= 47965:
				case 48005:
				case >= 48194 and <= 48198:
				case >= 48994 and <= 49121:
					return "7.3";
			}

			return "0";
        }


    }
}
