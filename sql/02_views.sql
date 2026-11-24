-- Flat view used by reports and the .NET API
CREATE VIEW vw_claims_enriched AS
SELECT c.claim_id,
       c.claim_date,
       substr(c.claim_date,1,7) AS claim_month,
       c.claim_amount,
       c.status,
       p.policy_id,
       p.policy_type,
       p.annual_premium,
       cu.customer_id,
       cu.state
FROM claims c
JOIN policies  p  ON p.policy_id   = c.policy_id
JOIN customers cu ON cu.customer_id = p.customer_id;

-- Fraud indicators: (1) amount far above the average for that policy type,
-- (2) same customer filing 3+ claims within 90 days.
CREATE VIEW vw_fraud_flags AS
WITH type_avg AS (
    SELECT policy_type, AVG(claim_amount) AS avg_amount
    FROM vw_claims_enriched
    GROUP BY policy_type
),
ordered AS (
    SELECT e.*,
           LAG(claim_date, 2) OVER (PARTITION BY customer_id ORDER BY claim_date) AS claim_date_2_back
    FROM vw_claims_enriched e
)
SELECT o.claim_id, o.customer_id, o.policy_type, o.state, o.claim_date, o.claim_amount, o.status,
       ROUND(o.claim_amount / t.avg_amount, 2) AS amount_vs_type_avg,
       CASE WHEN o.claim_amount > 3 * t.avg_amount THEN 1 ELSE 0 END AS high_amount_flag,
       CASE WHEN o.claim_date_2_back IS NOT NULL
                 AND julianday(o.claim_date) - julianday(o.claim_date_2_back) <= 90
            THEN 1 ELSE 0 END AS repeat_claimant_flag
FROM ordered o
JOIN type_avg t ON t.policy_type = o.policy_type;
