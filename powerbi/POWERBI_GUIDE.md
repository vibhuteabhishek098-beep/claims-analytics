# Power BI dashboard (build it yourself, about 30-45 minutes)

A `.pbix` file cannot be generated outside Power BI Desktop, so this is a step-by-step guide.
Build it in Power BI Desktop (free, Windows) and save your own `ClaimsDashboard.pbix` in this folder.

## 1. Load data
Home > Get data > Text/CSV: load `data/claims_enriched.csv` and `data/fraud_flags.csv`.
In Power Query set `claim_date` to Date and `claim_amount` to Decimal number. Close & Apply.

## 2. Relationship
Model view: link `claims_enriched[claim_id]` to `fraud_flags[claim_id]` (one-to-one).

## 3. DAX measures (Modeling > New measure)
```DAX
Total Claims     = COUNTROWS(claims_enriched)
Total Amount     = SUM(claims_enriched[claim_amount])
Approved Amount  = CALCULATE([Total Amount], claims_enriched[status] = "Approved")
Approval Rate %  = DIVIDE(CALCULATE([Total Claims], claims_enriched[status] = "Approved"), [Total Claims])
Avg Claim        = DIVIDE([Total Amount], [Total Claims])
Flagged Claims   = CALCULATE(COUNTROWS(fraud_flags), fraud_flags[high_amount_flag] = 1 || fraud_flags[repeat_claimant_flag] = 1)
Flag Rate %      = DIVIDE([Flagged Claims], [Total Claims])
```

## 4. Report page layout
- 4 KPI cards: Total Claims, Approved Amount, Approval Rate %, Flagged Claims
- Line chart: `claim_month` (axis) vs Total Claims and Approved Amount
- Bar chart: Total Amount by `policy_type`
- Map or bar: Total Claims by `state`
- Table: flagged claims (claim_id, customer_id, policy_type, claim_amount, amount_vs_type_avg), filtered to flagged
- Slicers: policy_type, status, state

## 5. Save a screenshot as `powerbi/dashboard.png` for your GitHub README.
