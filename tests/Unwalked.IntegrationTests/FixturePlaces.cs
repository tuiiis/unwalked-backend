namespace Unwalked.IntegrationTests;

public static class FixturePlaces
{
    public const string Region = "vilnius";

    public const double CathedralLatitude = 54.6858;
    public const double CathedralLongitude = 25.2877;

    public const string Csv = """
        id,name,category,alternate_categories,confidence,lon,lat,sources
        cathedral,Vilnius Cathedral,church,"[""landmark""]",0.99,25.2877,54.6858,fixture
        cafe-near,Cathedral Square Cafe,cafe,,0.90,25.2885,54.6862,fixture
        museum-old-town,Old Town Museum,museum,"[""cafe""]",0.80,25.2890,54.6820,fixture
        bakery-uzupis,Uzupis Bakery,bakery,,0.70,25.2960,54.6810,fixture
        park-vingis,Vingis Park,park,,0.95,25.2420,54.6830,fixture
        cafe-antakalnis,Antakalnis Cafe,cafe,,0.60,25.3150,54.7000,fixture
        no-category,Nameless,,,0.50,25.2880,54.6860,fixture
        """;
}
