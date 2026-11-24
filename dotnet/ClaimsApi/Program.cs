using ClaimsApi;

var dataDir = Environment.GetEnvironmentVariable("CLAIMS_DATA_DIR")
              ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data"));
var repo = new ClaimsRepository(dataDir);

// Self-check mode: validates the data layer without starting a server (used as a quick test).
if (args.Contains("--selftest"))
{
    var s = repo.GetSummary();
    var flagged = repo.GetFlagged().ToList();
    var failures = new List<string>();
    if (s.TotalClaims <= 0) failures.Add("no claims loaded");
    if (s.ApprovalRatePct is < 0 or > 100) failures.Add("approval rate out of range");
    if (flagged.Count != s.FlaggedClaims) failures.Add("flagged count mismatch");
    if (flagged.Any(f => !f.HighAmountFlag && !f.RepeatClaimantFlag)) failures.Add("unflagged claim in flagged list");
    if (flagged.Zip(flagged.Skip(1)).Any(p => p.First.AmountVsTypeAvg < p.Second.AmountVsTypeAvg)) failures.Add("flagged list not sorted");
    if (repo.GetFlagged("auto").Any(f => f.PolicyType != "Auto")) failures.Add("policy type filter broken");
    Console.WriteLine($"claims={s.TotalClaims} flagged={s.FlaggedClaims} approval={s.ApprovalRatePct}%");
    Console.WriteLine(failures.Count == 0 ? "SELFTEST PASSED" : "SELFTEST FAILED: " + string.Join("; ", failures));
    return failures.Count == 0 ? 0 : 1;
}

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/api/summary", () => repo.GetSummary());
app.MapGet("/api/claims/flagged", (string? policyType) => repo.GetFlagged(policyType));
app.MapGet("/api/claims/by-state", () => repo.ByState());
app.MapGet("/api/claims/{id:int}", (int id) =>
    repo.Claims.FirstOrDefault(c => c.ClaimId == id) is { } claim ? Results.Ok(claim) : Results.NotFound());

app.Run();
return 0;
