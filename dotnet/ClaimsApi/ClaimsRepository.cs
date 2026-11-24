using System.Globalization;

namespace ClaimsApi;

/// <summary>Loads the CSV exports produced from the SQL views (vw_claims_enriched, vw_fraud_flags).</summary>
public class ClaimsRepository
{
    public IReadOnlyList<Claim> Claims { get; }
    public IReadOnlyList<FlaggedClaim> Flags { get; }

    public ClaimsRepository(string dataDir)
    {
        Claims = ReadCsv(Path.Combine(dataDir, "claims_enriched.csv"), c => new Claim(
            int.Parse(c["claim_id"]), c["claim_date"], c["claim_month"], Dbl(c["claim_amount"]), c["status"],
            c["policy_type"], int.Parse(c["customer_id"]), c["state"]));

        Flags = ReadCsv(Path.Combine(dataDir, "fraud_flags.csv"), c => new FlaggedClaim(
            int.Parse(c["claim_id"]), int.Parse(c["customer_id"]), c["policy_type"], c["state"], c["claim_date"],
            Dbl(c["claim_amount"]), c["status"], Dbl(c["amount_vs_type_avg"]),
            c["high_amount_flag"] == "1", c["repeat_claimant_flag"] == "1"));
    }

    static double Dbl(string s) => double.Parse(s, CultureInfo.InvariantCulture);

    static List<T> ReadCsv<T>(string path, Func<Dictionary<string, string>, T> map)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"Missing {path}. Run python/build_db.py first.");
        var lines = File.ReadAllLines(path);
        var header = lines[0].Split(',');
        var rows = new List<T>();
        foreach (var line in lines.Skip(1).Where(l => l.Length > 0))
        {
            var parts = line.Split(',');
            var dict = header.Zip(parts, (h, v) => (h, v)).ToDictionary(x => x.h, x => x.v);
            rows.Add(map(dict));
        }
        return rows;
    }

    public Summary GetSummary()
    {
        var approved = Claims.Count(c => c.Status == "Approved");
        return new Summary(
            Claims.Count,
            Math.Round(Claims.Sum(c => c.Amount), 2),
            Claims.Count == 0 ? 0 : Math.Round(100.0 * approved / Claims.Count, 1),
            Flags.Count(f => f.HighAmountFlag || f.RepeatClaimantFlag));
    }

    public IEnumerable<FlaggedClaim> GetFlagged(string? policyType = null) =>
        Flags.Where(f => (f.HighAmountFlag || f.RepeatClaimantFlag) &&
                         (policyType is null || f.PolicyType.Equals(policyType, StringComparison.OrdinalIgnoreCase)))
             .OrderByDescending(f => f.AmountVsTypeAvg);

    public IEnumerable<object> ByState() =>
        Claims.GroupBy(c => c.State)
              .Select(g => new { State = g.Key, Claims = g.Count(), AvgAmount = Math.Round(g.Average(c => c.Amount), 2) })
              .OrderByDescending(x => x.Claims);
}
