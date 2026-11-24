-- Q1: Monthly claims count, total paid and approval rate
SELECT claim_month,
       COUNT(*) AS claims,
       ROUND(SUM(CASE WHEN status='Approved' THEN claim_amount ELSE 0 END),2) AS approved_amount,
       ROUND(100.0 * SUM(CASE WHEN status='Approved' THEN 1 ELSE 0 END) / COUNT(*),1) AS approval_rate_pct
FROM vw_claims_enriched
GROUP BY claim_month
ORDER BY claim_month;

-- Q2: Loss ratio by policy type (approved claims / premium collected)
SELECT p.policy_type,
       ROUND(SUM(p.annual_premium),2) AS premium,
       ROUND(COALESCE(SUM(a.paid),0),2) AS claims_paid,
       ROUND(COALESCE(SUM(a.paid),0) / SUM(p.annual_premium),3) AS loss_ratio
FROM policies p
LEFT JOIN (SELECT policy_id, SUM(claim_amount) AS paid
           FROM claims WHERE status='Approved' GROUP BY policy_id) a
       ON a.policy_id = p.policy_id
GROUP BY p.policy_type
ORDER BY loss_ratio DESC;

-- Q3: Claims by state, ranked
SELECT state, COUNT(*) AS claims, ROUND(AVG(claim_amount),2) AS avg_claim,
       RANK() OVER (ORDER BY COUNT(*) DESC) AS rank_by_volume
FROM vw_claims_enriched
GROUP BY state;

-- Q4: Flagged claims for review
SELECT * FROM vw_fraud_flags
WHERE high_amount_flag = 1 OR repeat_claimant_flag = 1
ORDER BY amount_vs_type_avg DESC
LIMIT 20;
