INSERT INTO region (code, name, bounds) VALUES
    ('vilnius', 'Vilnius', 'SRID=4326;POLYGON((25.15 54.62, 25.40 54.62, 25.40 54.78, 25.15 54.78, 25.15 54.62))')
ON CONFLICT (code) DO NOTHING;
