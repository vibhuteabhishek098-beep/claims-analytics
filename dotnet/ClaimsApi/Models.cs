namespace ClaimsApi;

public record Claim(int ClaimId, string ClaimDate, string ClaimMonth, double Amount, string Status,
                    string PolicyType, int CustomerId, string State);

public record FlaggedClaim(int ClaimId, int CustomerId, string PolicyType, string State, string ClaimDate,
                           double Amount, string Status, double AmountVsTypeAvg,
                           bool HighAmountFlag, bool RepeatClaimantFlag);

public record Summary(int TotalClaims, double TotalAmount, double ApprovalRatePct, int FlaggedClaims);
