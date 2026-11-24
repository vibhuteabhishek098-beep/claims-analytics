-- Insurance Claims Analytics: schema (portable SQL; runs on SQLite, minor type changes for SQL Server)
DROP VIEW IF EXISTS vw_claims_enriched;
DROP VIEW IF EXISTS vw_fraud_flags;
DROP TABLE IF EXISTS claims;
DROP TABLE IF EXISTS policies;
DROP TABLE IF EXISTS customers;

CREATE TABLE customers (
    customer_id INTEGER PRIMARY KEY,
    full_name   TEXT NOT NULL,
    state       TEXT NOT NULL,
    birth_year  INTEGER NOT NULL
);

CREATE TABLE policies (
    policy_id   INTEGER PRIMARY KEY,
    customer_id INTEGER NOT NULL REFERENCES customers(customer_id),
    policy_type TEXT NOT NULL CHECK (policy_type IN ('Auto','Home','Health','Life')),
    start_date  TEXT NOT NULL,
    annual_premium REAL NOT NULL CHECK (annual_premium > 0)
);

CREATE TABLE claims (
    claim_id    INTEGER PRIMARY KEY,
    policy_id   INTEGER NOT NULL REFERENCES policies(policy_id),
    claim_date  TEXT NOT NULL,
    claim_amount REAL NOT NULL CHECK (claim_amount > 0),
    status      TEXT NOT NULL CHECK (status IN ('Approved','Rejected','Pending'))
);

CREATE INDEX ix_policies_customer ON policies(customer_id);
CREATE INDEX ix_claims_policy     ON claims(policy_id);
CREATE INDEX ix_claims_date       ON claims(claim_date);
