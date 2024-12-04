using DoctorsWebApplication.Data;
using DoctorsWebApplication.Search;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoctorsWebApplication.Helpers
{
    public class ListHelper
    {
        private readonly DoctorsDatabase2023Context? _context;
        private readonly IPageSearchData? _searchData;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "<Pending>")]
        private Dictionary<string, string> _countries = new Dictionary<string, string>();

        public ListHelper()
        {
        
        }

        public ListHelper(DoctorsDatabase2023Context context, IPageSearchData searchData)
        {
            _context = context;
            _searchData = searchData;
        }

        public SelectList BuildCountriesList(string? selectedCountry)
        {
            //get the list of countries an run up a query
            var countrySL = from c in CountriesList
                            select new { CountryCode = c.Key, Country = c.Value };

            //Put the list in a state I can use it in the view
            return new SelectList(countrySL, "Country", "Country", selectedCountry);
        }

        public Dictionary<string, string> CountriesList
        {
            get
            {
                return CountryList() ?? throw new ApplicationException("The countries dictionary needs to be initialised.");
            }
        }
        private Dictionary<string, string> CountryList()
        {
#pragma warning disable IDE0028 // Simplify collection initialization
            _countries ??= new Dictionary<string, string>();
#pragma warning restore IDE0028 // Simplify collection initialization

            if (_countries.Count > 0)
            {
                return _countries;
            }

            _countries.Add("AF", "Afghanistan");
            _countries.Add("AL", "Albania");
            _countries.Add("DZ", "Algeria");
            _countries.Add("AS", "American Samoa");
            _countries.Add("AD", "Andorra");
            _countries.Add("AO", "Angola");
            _countries.Add("AI", "Anguilla");
            _countries.Add("AQ", "Antarctica");
            _countries.Add("AG", "Antigua And Barbuda");
            _countries.Add("AR", "Argentina");
            _countries.Add("AM", "Armenia");
            _countries.Add("AW", "Aruba");
            _countries.Add("AU", "Australia");
            _countries.Add("AT", "Austria");
            _countries.Add("AZ", "Azerbaijan");

            _countries.Add("BS", "Bahamas, The");
            _countries.Add("BH", "Bahrain");
            _countries.Add("BD", "Bangladesh");
            _countries.Add("BB", "Barbados");
            _countries.Add("BY", "Belarus");
            _countries.Add("BE", "Belgium");
            _countries.Add("BZ", "Belize");
            _countries.Add("BJ", "Benin");
            _countries.Add("BM", "Bermuda");
            _countries.Add("BT", "Bhutan");
            _countries.Add("BO", "Bolivia");
            _countries.Add("BA", "Bosnia and Herzegovina");
            _countries.Add("BW", "Botswana");
            _countries.Add("BV", "Bouvet Island");
            _countries.Add("BR", "Brazil");
            _countries.Add("IO", "British Indian Ocean Territory");
            _countries.Add("BN ", "Brunei");
            _countries.Add("BG ", "Bulgaria");
            _countries.Add("BF ", "Burkina Faso");
            _countries.Add("BI", "Burundi");

            _countries.Add("KH", "Cambodia");
            _countries.Add("CM", "Cameroon");
            _countries.Add("CA", "Canada");
            _countries.Add("CV", "Cape Verde");
            _countries.Add("KY ", "Cayman Islands");
            _countries.Add("CF", "Central African Republic");
            _countries.Add("TD", "Chad");
            _countries.Add("CL", "Chile");
            _countries.Add("CN", "China");
            _countries.Add("CX", "Christmas Island");
            _countries.Add("CC ", "Cocos(Keeling) Islands");
            _countries.Add("CO ", "Colombia");
            _countries.Add("KM ", "Comoros");
            _countries.Add("CG ", "Congo");
            _countries.Add("CD ", "Congo(DRC)");
            _countries.Add("CK", "Cook Islands");
            _countries.Add("CR ", "Costa Rica");
            _countries.Add("CI", "Côte d'Ivoire");
            _countries.Add("HR", "Croatia");
            _countries.Add("CU", "Cuba");
            _countries.Add("CY", "Cyprus");
            _countries.Add("CZ", "Czech Republic");

            _countries.Add("DK", "Denmark");
            _countries.Add("DJ", "Djibouti");
            _countries.Add("DM", "Dominica");
            _countries.Add("DO", "Dominican Republic");

            _countries.Add("EC", "Ecuador");
            _countries.Add("EG", "Egypt");
            _countries.Add("SV", "El Salvador");
            _countries.Add("GQ", "Equatorial Guinea");
            _countries.Add("ER", "Eritrea");
            _countries.Add("EE", "Estonia");
            _countries.Add("ET", "Ethiopia");

            _countries.Add("FK", "Falkland Islands(Islas Malvinas)");
            _countries.Add("FO", "Faroe Islands");
            _countries.Add("FJ", "Fiji Islands");
            _countries.Add("FI", "Finland");
            _countries.Add("FR", "France");
            _countries.Add("GF", "French Guiana");
            _countries.Add("PF", "French Polynesia");
            _countries.Add("TF", "French Southern and Antarctic Lands");

            _countries.Add("GA", "Gabon");
            _countries.Add("GM", "Gambia, The");
            _countries.Add("GE", "Georgia");
            _countries.Add("DE", "Germany");
            _countries.Add("GH", "Ghana");
            _countries.Add("GI", "Gibraltar");
            _countries.Add("GR", "Greece");
            _countries.Add("GL", "Greenland");
            _countries.Add("GD", "Grenada");
            _countries.Add("GP", "Guadeloupe");
            _countries.Add("GU", "Guam");
            _countries.Add("GT", "Guatemala");
            _countries.Add("GN", "Guinea");
            _countries.Add("GW", "Guinea-Bissau");
            _countries.Add("GY", "Guyana");

            _countries.Add("HT", "Haiti");
            _countries.Add("HM", "Heard Island and McDonald Islands");
            _countries.Add("HN", "Honduras");
            _countries.Add("HK", "Hong Kong SAR");
            _countries.Add("HU", "Hungary");

            _countries.Add("IS", "Iceland");
            _countries.Add("IN", "India");
            _countries.Add("ID", "Indonesia");
            _countries.Add("IR", "Iran");
            _countries.Add("IQ", "Iraq");
            _countries.Add("IE", "Ireland");
            _countries.Add("IL", "Israel");
            _countries.Add("IT", "Italy");

            _countries.Add("JM", "Jamaica");
            _countries.Add("JP", "Japan");
            _countries.Add("JO", "Jordan");

            _countries.Add("KZ", "Kazakhstan");
            _countries.Add("KE", "Kenya");
            _countries.Add("KI", "Kiribati");
            _countries.Add("KR", "Korea");
            _countries.Add("KW", "Kuwait");
            _countries.Add("KG", "Kyrgyzstan");

            _countries.Add("LA", "Laos");
            _countries.Add("LV", "Latvia");
            _countries.Add("LB", "Lebanon");
            _countries.Add("LS", "Lesotho");
            _countries.Add("LR", "Liberia");
            _countries.Add("LY", "Libya");
            _countries.Add("LI", "Liechtenstein");
            _countries.Add("LT", "Lithuania");
            _countries.Add("LU", "Luxembourg");

            _countries.Add("MO", "Macao SAR");
            _countries.Add("MK", "Macedonia, Former Yugoslav Republic of");
            _countries.Add("MG", "Madagascar");
            _countries.Add("MW", "Malawi");
            _countries.Add("MY", "Malaysia");
            _countries.Add("MV", "Maldives");
            _countries.Add("ML", "Mali");
            _countries.Add("MT", "Malta");
            _countries.Add("MH", "Marshall Islands");
            _countries.Add("MQ", "Martinique");
            _countries.Add("MR", "Mauritania");
            _countries.Add("MU", "Mauritius");
            _countries.Add("YT", "Mayotte");
            _countries.Add("MX", "Mexico");
            _countries.Add("FM", "Micronesia");
            _countries.Add("MD", "Moldova");
            _countries.Add("MC", "Monaco");
            _countries.Add("MN", "Mongolia");
            _countries.Add("MS", "Montserrat");
            _countries.Add("MA", "Morocco");
            _countries.Add("MZ", "Mozambique");
            _countries.Add("MM", "Myanmar");

            _countries.Add("NA", "Namibia");
            _countries.Add("NR", "Nauru");
            _countries.Add("NP", "Nepal");
            _countries.Add("NL", "Netherlands");
            _countries.Add("AN", "Netherlands Antilles");
            _countries.Add("NC", "New Caledonia");
            _countries.Add("NZ", "New Zealand");
            _countries.Add("NI", "Nicaragua");
            _countries.Add("NE", "Niger");
            _countries.Add("NG", "Nigeria");
            _countries.Add("NU", "Niue");
            _countries.Add("NF", "Norfolk Island");
            _countries.Add("KP", "North Korea");
            _countries.Add("MP", "Northern Mariana Islands");
            _countries.Add("NO", "Norway");

            _countries.Add("OM", "Oman");

            _countries.Add("PK", "Pakistan");
            _countries.Add("PW", "Palau");
            _countries.Add("PS", "Palestinian Authority");
            _countries.Add("PA", "Panama");
            _countries.Add("PG", "Papua New Guinea");
            _countries.Add("PY", "Paraguay");
            _countries.Add("PE", "Peru");
            _countries.Add("PH", "Philippines");
            _countries.Add("PN", "Pitcairn Islands");
            _countries.Add("PL", "Poland");
            _countries.Add("PT", "Portugal");
            _countries.Add("PR", "Puerto Rico");

            _countries.Add("QA", "Qatar");

            _countries.Add("RE", "Réunion");
            _countries.Add("RO", "Romania");
            _countries.Add("RU", "Russia");
            _countries.Add("RW", "Rwanda");

            _countries.Add("SH", "Saint Helena");
            _countries.Add("KN", "Saint Kitts and Nevis");
            _countries.Add("LC", "Saint Lucia");
            _countries.Add("PM", "Saint Pierre and Miquelon");
            _countries.Add("VC", "Saint Vincent and the Grenadine");
            _countries.Add("WS", "Samoa");
            _countries.Add("SM", "San Marino");
            _countries.Add("ST", "São Tomé and Príncipe");
            _countries.Add("SA", "Saudi Arabia");
            _countries.Add("SN", "Senegal");
            _countries.Add("CS", "Serbia and Montenegro");
            _countries.Add("SC", "Seychelles");
            _countries.Add("SL", "Sierra Leone");
            _countries.Add("SG", "Singapore");
            _countries.Add("SK", "Slovakia");
            _countries.Add("SI", "Slovenia");
            _countries.Add("SB", "Solomon Islands");
            _countries.Add("SO", "Somalia");
            _countries.Add("ZA", "South Africa");
            _countries.Add("GS", "South Georgia and the,South,Sandwich Islands");
            _countries.Add("ES", "Spain");
            _countries.Add("LK", "Sri Lanka");
            _countries.Add("SD", "Sudan");
            _countries.Add("SR", "Suriname");
            _countries.Add("SJ", "Svalbard and Jan Mayen");
            _countries.Add("SZ", "Swaziland");
            _countries.Add("SE", "Sweden");
            _countries.Add("CH", "Switzerland");
            _countries.Add("SY", "Syria");

            _countries.Add("TW", "Taiwan");
            _countries.Add("TJ", "Tajikistan");
            _countries.Add("TZ", "Tanzania");
            _countries.Add("TH", "Thailand");
            _countries.Add("TL", "Timor - Leste");
            _countries.Add("TG", "Togo");
            _countries.Add("TK", "Tokelau");
            _countries.Add("TO", "Tonga");
            _countries.Add("TT", "Trinidad and,Tobago");
            _countries.Add("TN", "Tunisia");
            _countries.Add("TR", "Turkey");
            _countries.Add("TM", "Turkmenistan");
            _countries.Add("TC", "Turks and Caicos Islands");
            _countries.Add("TV", "Tuvalu");

            _countries.Add("UM", "U.S.Minor Outlying Islands");
            _countries.Add("UG", "Uganda");
            _countries.Add("UA", "Ukraine");
            _countries.Add("AE", "United Arab Emirates");
            _countries.Add("GB", "United Kingdom");
            _countries.Add("US", "United States");
            _countries.Add("UY", "Uruguay");
            _countries.Add("UZ", "Uzbekistan");

            _countries.Add("VU", "Vanuatu");
            _countries.Add("VA", "Vatican City");
            _countries.Add("VE", "Venezuela");
            _countries.Add("VN", "Vietnam");
            _countries.Add("VG", "Virgin Islands, British");
            _countries.Add("VI", "Virgin Islands, U.S.");

            _countries.Add("WF", "Wallis and Futuna");

            _countries.Add("YE", "Yemen");

            _countries.Add("ZM", "Zambia");
            _countries.Add("ZW", "Zimbabwe");

            return _countries;
        }
    }
}
