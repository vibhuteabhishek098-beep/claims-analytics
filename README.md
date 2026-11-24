# Insurance Claims Analytics (SQL + Power BI + .NET)

End-to-end mini project on synthetic insurance data: design the SQL layer, expose results through a .NET Web API,
and visualise them in Power BI. It mirrors the claims and fraud-analytics work done by insurance technology teams.

## Architecture
```
Python (seeded data) -> SQL database (3 tables, 2 views) -> CSV exports -> .NET 8 Web API
                                                                      \-> Power BI dashboard
```

## What's inside
| Folder | Contents |
|---|---|
| `sql/` | Schema with keys, checks and indexes; views `vw_claims_enriched` and `vw_fraud_flags` (CTEs, window functions `LAG`, `RANK`); four analysis queries (monthly trend, loss ratio, state ranking, flagged claims) |
| `python/` | `build_db.py` creates the database and CSVs (500 customers, ~660 policies, ~250 claims, seed 42); `run_queries.py` prints query results |
| `dotnet/ClaimsApi/` | ASP.NET Core 8 minimal API: `/api/summary`, `/api/claims/flagged?policyType=`, `/api/claims/by-state`, `/api/claims/{id}`; `--selftest` mode |
| `powerbi/` | Dashboard guide with DAX measures and visuals |

## Fraud flags (in SQL)
1. **High amount:** claim greater than 3x the average for its policy type.
2. **Repeat claimant:** 3 claims by the same customer within 90 days (`LAG` over a customer partition).

## Run it
```bash
python python/build_db.py          # creates data/claims.db and the CSVs
python python/run_queries.py       # prints the analysis queries
cd dotnet/ClaimsApi
dotnet run -- --selftest           # data-layer check, expects SELFTEST PASSED
dotnet run                         # starts the API (default http://localhost:5000)
```
Requires Python 3 and the .NET 8 SDK. No third-party packages. The SQL is portable and runs on SQLite;
for SQL Server change `TEXT`/`REAL` to `VARCHAR`/`DECIMAL`, `substr` to `SUBSTRING`,
and the `julianday` difference to `DATEDIFF`.

## Notes
All data is synthetic. The API reads the CSV exports of the SQL views to stay dependency-free; connecting it
directly to SQL Server with `Microsoft.Data.SqlClient` is the natural next step.
