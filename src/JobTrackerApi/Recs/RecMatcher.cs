using System.Text.RegularExpressions;

// Decides which postings are worth pinning: early-career software roles near me
public static partial class RecMatcher
{
    private static readonly string[] OrangeCounty =
        ["irvine", "costa mesa", "newport beach", "santa ana", "tustin", "anaheim", "orange county", "san clemente",
         "laguna", "lake forest", "aliso viejo", "huntington beach", "mission viejo", "fullerton", "orange, ca"];

    private static readonly string[] SanDiego =
        ["san diego", "carlsbad", "la jolla", "del mar", "oceanside", "escondido", "sorrento"];

    private static readonly string[] BayArea =
        ["san francisco", "bay area", "oakland", "san mateo", "palo alto", "mountain view", "sunnyvale",
         "san jose", "menlo park", "redwood city", "santa clara", "south san francisco"];

    // 0 = not a match, otherwise how close it is to the top of my list
    public static int Score(string title, string location)
    {
        if (!IsEarlyCareerSoftwareRole(title)) return 0;
        return LocationPriority(location);
    }

    public static bool IsEarlyCareerSoftwareRole(string title)
    {
        var t = title.ToLowerInvariant();
        if (TooSenior().IsMatch(t)) return false;
        if (!TechRole().IsMatch(t)) return false;
        return EarlyCareer().IsMatch(t);
    }

    // True when a posting looks like a job already on the menu (same company, same role)
    public static bool IsSameJob(string recCompany, string recTitle, string jobCompany, string jobRole)
    {
        if (Normalize(recCompany) != Normalize(jobCompany)) return false;
        var title = Normalize(recTitle);
        var role = Normalize(jobRole);
        return title.Length > 0 && role.Length > 0 && (title.Contains(role) || role.Contains(title));
    }

    // "Software Engineer I - Growth (USA)" -> "software engineer i growth usa"
    public static string Normalize(string text)
    {
        return Spaces().Replace(NotLetters().Replace(text.ToLowerInvariant(), " "), " ").Trim();
    }

    // Orange County first, then San Diego, then SF or remote in the US
    public static int LocationPriority(string location)
    {
        var l = location.ToLowerInvariant();
        if (OrangeCounty.Any(l.Contains)) return 3;
        if (SanDiego.Any(l.Contains)) return 2;
        if (BayArea.Any(l.Contains)) return 1;
        if (l.Contains("remote") && (InUs().IsMatch(l) || !l.Contains(','))) return 1;
        return 0;
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NotLetters();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();

    [GeneratedRegex(@"\b(us|usa|u\.s\.|united states|north america)\b")]
    private static partial Regex InUs();

    [GeneratedRegex(@"\b(senior|sr\.?|staff|principal|lead|manager|director|head|intern|internship|architect|ii|iii|iv|phd)\b")]
    private static partial Regex TooSenior();

    [GeneratedRegex(@"\b(software|engineer|engineering|developer|sde|swe)\b")]
    private static partial Regex TechRole();

    [GeneratedRegex(@"\b(new grad|new college grad|college grad|university grad|graduate|early career|early-career|entry level|entry-level|junior|jr\.?|associate|apprentice|i|1|2026|2027)\b")]
    private static partial Regex EarlyCareer();
}
