INSTALL spatial; LOAD spatial; INSTALL httpfs; LOAD httpfs;
SET s3_region = 'us-west-2';
COPY (
  SELECT id,
         names."primary" AS name,
         taxonomy."primary" AS category,
         array_to_string(taxonomy.alternates, ';') AS alternate_categories,
         confidence,
         ST_X(geometry) AS lon,
         ST_Y(geometry) AS lat
  FROM read_parquet('s3://overturemaps-us-west-2/release/2026-09-23.1/theme=places/type=place/*', hive_partitioning = 1)
  WHERE bbox.xmin BETWEEN 25.15 AND 25.40
    AND bbox.ymin BETWEEN 54.62 AND 54.78
) TO 'vilnius_places.csv' (HEADER, DELIMITER ',');
