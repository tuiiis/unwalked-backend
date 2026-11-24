namespace Unwalked.Infrastructure.Areas;

/// <summary>
/// Maps OpenStreetMap tags to category codes of the catalogue. Tags without a mapping are skipped.
/// </summary>
public static class OsmCategories
{
    private static readonly Dictionary<(string Key, string Value), string> Codes = new()
    {
        [("amenity", "cafe")] = "cafe",
        [("amenity", "restaurant")] = "restaurant",
        [("amenity", "bar")] = "bar",
        [("amenity", "pub")] = "pub",
        [("amenity", "fast_food")] = "fast_food_restaurant",
        [("amenity", "ice_cream")] = "ice_cream_shop",
        [("amenity", "library")] = "library",
        [("amenity", "theatre")] = "theatre_venue",
        [("amenity", "cinema")] = "movie_theater",
        [("amenity", "arts_centre")] = "cultural_center",
        [("amenity", "marketplace")] = "farmers_market",
        [("leisure", "park")] = "park",
        [("leisure", "garden")] = "botanical_garden",
        [("leisure", "playground")] = "playground",
        [("leisure", "nature_reserve")] = "nature_reserve",
        [("tourism", "museum")] = "museum",
        [("tourism", "gallery")] = "art_gallery",
        [("tourism", "zoo")] = "zoo",
        [("tourism", "artwork")] = "sculpture_statue",
        [("historic", "monument")] = "monument",
        [("historic", "memorial")] = "monument",
        [("historic", "castle")] = "castle",
        [("place", "square")] = "public_plaza",
        [("shop", "bakery")] = "bakery",
        [("shop", "books")] = "bookstore",
        [("shop", "gift")] = "gift_shop",
        [("shop", "second_hand")] = "second_hand_store",
        [("shop", "antiques")] = "antique_store",
        [("shop", "art")] = "arts_crafts_and_hobby_store",
        [("shop", "craft")] = "arts_crafts_and_hobby_store",
        [("shop", "chocolate")] = "chocolatier",
        [("shop", "coffee")] = "coffee_roastery",
        [("shop", "tea")] = "tea_room",
        [("shop", "deli")] = "delicatessen",
    };

    private static readonly Dictionary<string, string> Worship = new()
    {
        ["christian"] = "christian_place_of_worship",
        ["jewish"] = "jewish_place_of_worship",
        ["muslim"] = "muslim_place_of_worship",
        ["buddhist"] = "buddhist_place_of_worship",
    };

    /// <summary>
    /// Tag keys the source is asked for.
    /// </summary>
    public static IReadOnlyCollection<string> Keys { get; } = Codes.Keys.Select(key => key.Key).Append("amenity").Distinct().ToArray();

    /// <summary>
    /// Values asked for under a key.
    /// </summary>
    public static IEnumerable<string> Values(string key) =>
        Codes.Keys.Where(pair => pair.Key == key).Select(pair => pair.Value).Concat(key == "amenity" ? ["place_of_worship"] : []);

    /// <summary>
    /// The catalogue category for an element's tags, or null when none applies.
    /// </summary>
    public static string? For(IReadOnlyDictionary<string, string> tags)
    {
        if (tags.TryGetValue("amenity", out var amenity) && amenity == "place_of_worship")
        {
            return tags.TryGetValue("religion", out var religion) && Worship.TryGetValue(religion, out var code) ? code : "religious_organization";
        }

        foreach (var ((key, value), code) in Codes)
        {
            if (tags.TryGetValue(key, out var tagged) && tagged == value)
            {
                return code;
            }
        }

        return null;
    }
}
