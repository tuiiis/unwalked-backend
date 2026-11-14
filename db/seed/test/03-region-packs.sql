INSERT INTO region_pack (region_id, version, built_at, size_bytes, sha256, object_key, is_current)
SELECT region.id, pack.version, pack.built_at, pack.size_bytes, pack.sha256, pack.object_key, pack.is_current
FROM region
CROSS JOIN (VALUES
    (1, timestamptz '2026-10-15 03:00:00+00', 48211337::bigint, '2ac11329557b2628d50c48636e8e5d99d077b0dc727c599c1b0f86d074080d1e', 'packs/vilnius/v1.pack', false),
    (2, timestamptz '2026-11-01 03:00:00+00', 49002113::bigint, '975e88bf1588ed07325d4c6252e0d30d7fd13ad15052af7501d9788ecbb1c8b0', 'packs/vilnius/v2.pack', true)
) AS pack (version, built_at, size_bytes, sha256, object_key, is_current)
WHERE region.code = 'vilnius'
ON CONFLICT (region_id, version) DO NOTHING;
