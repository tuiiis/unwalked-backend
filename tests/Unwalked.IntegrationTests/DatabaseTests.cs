using Npgsql;

namespace Unwalked.IntegrationTests;

public class DatabaseTests
{
    [Test]
    public async Task MaskedAccountView_HidesEmailFromAnalyst()
    {
        var id = Guid.NewGuid();
        var email = $"walker-{id:N}@example.test";
        await ExecuteAsync(
            PostgisEnvironment.MigratorConnectionString,
            $"INSERT INTO user_account (id, email, password_hash, created_at) VALUES ('{id}', '{email}', 'hash', now())");

        var masked = await ScalarAsync<string>(
            PostgisEnvironment.AnalystConnectionString,
            $"SELECT email FROM v_user_account_masked WHERE id = '{id}'");

        Assert.That(masked, Is.EqualTo("w***@example.test"));
    }

    [TestCase("SELECT email FROM user_account", "user_account")]
    [TestCase("SELECT image FROM achievement_share", "achievement_share")]
    [TestCase("SELECT name FROM place", "place")]
    [TestCase("CALL aggregate_feedback(current_date)", "aggregate_feedback")]
    public async Task AnalystLogin_CannotReachPersonalDataOrMaintenance(string sql, string target)
    {
        var error = await CatchAsync(PostgisEnvironment.AnalystConnectionString, sql);

        Assert.That(error.SqlState, Is.EqualTo(PostgresErrorCodes.InsufficientPrivilege), error.MessageText);
        Assert.That(error.MessageText, Does.Contain(target));
    }

    [TestCase("CALL aggregate_feedback(current_date)", "aggregate_feedback")]
    [TestCase("SELECT * FROM feedback_daily_total", "feedback_daily_total")]
    [TestCase("UPDATE region_pack SET is_current = false", "region_pack")]
    public async Task ApiLogin_CannotRunMaintenanceOrPublishPacks(string sql, string target)
    {
        var error = await CatchAsync(PostgisEnvironment.ApiConnectionString, sql);

        Assert.That(error.SqlState, Is.EqualTo(PostgresErrorCodes.InsufficientPrivilege), error.MessageText);
        Assert.That(error.MessageText, Does.Contain(target));
    }

    [Test]
    public async Task PublishingPack_RetiresPreviousCurrentPackOfSameRegion()
    {
        var region = await CreateRegionAsync();
        await InsertPackAsync(region, 1, isCurrent: true);
        await InsertPackAsync(region, 2, isCurrent: true);

        var current = await ScalarAsync<int>(
            PostgisEnvironment.ApiConnectionString,
            $"SELECT version FROM v_region_pack_index WHERE region_code = '{region}'");
        var currentCount = await ScalarAsync<long>(
            PostgisEnvironment.MigratorConnectionString,
            $"SELECT count(*) FROM region_pack JOIN region ON region.id = region_id WHERE code = '{region}' AND is_current");

        Assert.Multiple(() =>
        {
            Assert.That(current, Is.EqualTo(2));
            Assert.That(currentCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task RegionPack_RejectsMalformedChecksum()
    {
        var region = await CreateRegionAsync();

        var error = await CatchAsync(
            PostgisEnvironment.MigratorConnectionString,
            $"""
            INSERT INTO region_pack (region_id, version, built_at, size_bytes, sha256, object_key, is_current)
            SELECT id, 1, now(), 10, repeat('Z', 64), '{region}/bad.pack', false FROM region WHERE code = '{region}'
            """);

        Assert.That(error.SqlState, Is.EqualTo(PostgresErrorCodes.CheckViolation));
        Assert.That(error.ConstraintName, Is.EqualTo("ck_region_pack_sha256_hex"));
    }

    [Test]
    public async Task Region_WithPlacesCannotBeDeleted()
    {
        var error = await CatchAsync(PostgisEnvironment.MigratorConnectionString, $"DELETE FROM region WHERE code = '{FixturePlaces.Region}'");

        Assert.That(error.SqlState, Is.EqualTo(PostgresErrorCodes.ForeignKeyViolation));
        Assert.That(error.ConstraintName, Is.EqualTo("fk_place_region"));
    }

    [Test]
    public async Task Region_WithBuildingsCannotBeDeleted()
    {
        var region = await CreateRegionAsync();
        await InsertBuildingAsync(region, "SRID=4326;MULTIPOLYGON(((0.1 0.1, 0.2 0.1, 0.2 0.2, 0.1 0.2, 0.1 0.1)))");

        var error = await CatchAsync(PostgisEnvironment.MigratorConnectionString, $"DELETE FROM region WHERE code = '{region}'");

        Assert.That(error.SqlState, Is.EqualTo(PostgresErrorCodes.ForeignKeyViolation));
        Assert.That(error.ConstraintName, Is.EqualTo("fk_building_region"));
    }

    [Test]
    public async Task Building_RejectsSelfIntersectingFootprint()
    {
        var region = await CreateRegionAsync();

        var error = await CatchAsync(
            PostgisEnvironment.MigratorConnectionString,
            BuildingInsertSql(region, "SRID=4326;MULTIPOLYGON(((0.1 0.1, 0.2 0.2, 0.2 0.1, 0.1 0.2, 0.1 0.1)))"));

        Assert.That(error.SqlState, Is.EqualTo(PostgresErrorCodes.CheckViolation));
        Assert.That(error.ConstraintName, Is.EqualTo("ck_building_footprint_valid"));
    }

    [Test]
    public async Task ApiLogin_FindsBuildingContainingStay()
    {
        var region = await CreateRegionAsync();
        var externalId = await InsertBuildingAsync(region, "SRID=4326;MULTIPOLYGON(((0.3 0.3, 0.3004 0.3, 0.3004 0.3004, 0.3 0.3004, 0.3 0.3)))");

        var found = await ScalarAsync<string>(
            PostgisEnvironment.ApiConnectionString,
            "SELECT external_id FROM building WHERE ST_Intersects(footprint, 'SRID=4326;POINT(0.3002 0.3002)'::geography)");

        Assert.That(found, Is.EqualTo(externalId));
    }

    [Test]
    public async Task PlaceDetail_ExposesOpeningHours()
    {
        await ExecuteAsync(
            PostgisEnvironment.MigratorConnectionString,
            "UPDATE place SET opening_hours = 'Mo-Fr 08:00-20:00' WHERE id = (SELECT min(id) FROM place)");

        var hours = await ScalarAsync<string>(
            PostgisEnvironment.ApiConnectionString,
            "SELECT opening_hours FROM v_place_detail WHERE id = (SELECT min(id) FROM place)");

        Assert.That(hours, Is.EqualTo("Mo-Fr 08:00-20:00"));
    }

    [Test]
    public async Task Place_RejectsBlankOpeningHours()
    {
        var error = await CatchAsync(
            PostgisEnvironment.MigratorConnectionString,
            "UPDATE place SET opening_hours = '  ' WHERE id = (SELECT min(id) FROM place)");

        Assert.That(error.SqlState, Is.EqualTo(PostgresErrorCodes.CheckViolation));
        Assert.That(error.ConstraintName, Is.EqualTo("ck_place_opening_hours_not_blank"));
    }

    [Test]
    public async Task DeletingAccount_RemovesItsShares()
    {
        var owner = Guid.NewGuid();
        await ExecuteAsync(
            PostgisEnvironment.MigratorConnectionString,
            $"""
            INSERT INTO user_account (id, email, password_hash, created_at) VALUES ('{owner}', 'owner-{owner:N}@example.test', 'hash', now());
            INSERT INTO achievement_share (id, owner_id, achievement_name, achieved_on, image, image_content_type, created_at)
            VALUES (gen_random_uuid(), '{owner}', 'First route', current_date, '\x89504e47'::bytea, 'image/png', now());
            """);

        await ExecuteAsync(PostgisEnvironment.ApiConnectionString, $"DELETE FROM user_account WHERE id = '{owner}'");

        var shares = await ScalarAsync<long>(
            PostgisEnvironment.MigratorConnectionString,
            $"SELECT count(*) FROM achievement_share WHERE owner_id = '{owner}'");
        Assert.That(shares, Is.Zero);
    }

    [Test]
    public async Task AggregateFeedback_MovesOldReactionsIntoDailyTotals()
    {
        const string day = "2001-03-04";
        await InsertFeedbackAsync(day, "walking_route", "accepted", "cafe", 2);
        await InsertFeedbackAsync(day, "walking_route", "accepted", null, 1);
        await ExecuteAsync(PostgisEnvironment.MigratorConnectionString, "CALL aggregate_feedback('2001-03-05')");
        await InsertFeedbackAsync(day, "walking_route", "accepted", null, 2);
        await ExecuteAsync(PostgisEnvironment.MigratorConnectionString, "CALL aggregate_feedback('2001-03-05')");

        var raw = await ScalarAsync<long>(
            PostgisEnvironment.MigratorConnectionString,
            $"SELECT count(*) FROM recommendation_feedback WHERE received_on = '{day}'");
        var withCategory = await ScalarAsync<int>(
            PostgisEnvironment.AnalystConnectionString,
            $"SELECT event_count FROM feedback_daily_total WHERE received_on = '{day}' AND category_id IS NOT NULL");
        var withoutCategory = await ScalarAsync<int>(
            PostgisEnvironment.AnalystConnectionString,
            $"SELECT event_count FROM feedback_daily_total WHERE received_on = '{day}' AND category_id IS NULL");

        Assert.Multiple(() =>
        {
            Assert.That(raw, Is.Zero);
            Assert.That(withCategory, Is.EqualTo(2));
            Assert.That(withoutCategory, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task FeedbackSummary_CountsRawAndAggregatedReactions()
    {
        await InsertFeedbackAsync("2001-05-01", "unvisited_cell", "visited", null, 2);
        var before = await SummaryCountAsync("unvisited_cell", "visited");
        await ExecuteAsync(PostgisEnvironment.MigratorConnectionString, "CALL aggregate_feedback('2001-05-02')");
        await InsertFeedbackAsync("2001-05-03", "unvisited_cell", "visited", null, 1);

        var after = await SummaryCountAsync("unvisited_cell", "visited");

        Assert.That(after, Is.EqualTo(before + 1));
    }

    [Test]
    public async Task TestDataSeed_LoadsAndCanBeRepeated()
    {
        await using var dataSource = NpgsqlDataSource.Create(PostgisEnvironment.MigratorConnectionString);
        var runner = new Infrastructure.Seeding.SqlScriptRunner(dataSource);

        var applied = await runner.RunDirectoryAsync(PostgisEnvironment.SeedDirectory("test"), CancellationToken.None);
        await runner.RunDirectoryAsync(PostgisEnvironment.SeedDirectory("test"), CancellationToken.None);

        var accounts = await ScalarAsync<long>(
            PostgisEnvironment.MigratorConnectionString,
            "SELECT count(*) FROM user_account WHERE email LIKE '%@example.test' AND email IN ('ona@example.test', 'jonas@example.test', 'rasa@example.test')");
        var currentPack = await ScalarAsync<int>(
            PostgisEnvironment.ApiConnectionString,
            "SELECT version FROM v_region_pack_index WHERE region_code = 'vilnius'");
        Assert.Multiple(() =>
        {
            Assert.That(applied, Has.Count.EqualTo(3));
            Assert.That(accounts, Is.EqualTo(3));
            Assert.That(currentPack, Is.EqualTo(2));
        });
    }

    private static async Task<long> SummaryCountAsync(string type, string action) =>
        await ScalarAsync<long?>(
            PostgisEnvironment.AnalystConnectionString,
            $"SELECT event_count FROM v_feedback_summary WHERE recommendation_type = '{type}' AND action = '{action}'") ?? 0;

    private static async Task InsertFeedbackAsync(string day, string type, string action, string? category, int count)
    {
        var categoryId = category is null ? "NULL" : $"(SELECT id FROM category WHERE code = '{category}')";
        await ExecuteAsync(
            PostgisEnvironment.ApiConnectionString,
            $"""
            INSERT INTO recommendation_feedback (event_id, recommendation_type, action, category_id, received_on)
            SELECT gen_random_uuid(), '{type}', '{action}', {categoryId}, '{day}' FROM generate_series(1, {count})
            """);
    }

    private static async Task<string> InsertBuildingAsync(string region, string footprint)
    {
        var externalId = $"bld-{Guid.NewGuid():N}";
        await ExecuteAsync(PostgisEnvironment.MigratorConnectionString, BuildingInsertSql(region, footprint, externalId));
        return externalId;
    }

    private static string BuildingInsertSql(string region, string footprint, string? externalId = null) =>
        $"""
        INSERT INTO building (external_id, footprint, h3_r11, region_id)
        SELECT '{externalId ?? $"bld-{Guid.NewGuid():N}"}', '{footprint}', 0, id FROM region WHERE code = '{region}'
        """;

    private static async Task<string> CreateRegionAsync()
    {
        var code = $"test-{Guid.NewGuid():N}"[..20];
        await ExecuteAsync(
            PostgisEnvironment.MigratorConnectionString,
            $"INSERT INTO region (code, name, bounds) VALUES ('{code}', 'Test region', 'SRID=4326;POLYGON((0 0, 1 0, 1 1, 0 1, 0 0))')");
        return code;
    }

    private static Task InsertPackAsync(string region, int version, bool isCurrent) =>
        ExecuteAsync(
            PostgisEnvironment.MigratorConnectionString,
            $"""
            INSERT INTO region_pack (region_id, version, built_at, size_bytes, sha256, object_key, is_current)
            SELECT id, {version}, now(), 1000, repeat('a', 64), '{region}/v{version}.pack', {isCurrent} FROM region WHERE code = '{region}'
            """);

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<T> ScalarAsync<T>(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return value is null or DBNull ? default! : (T)value;
    }

    private static async Task<PostgresException> CatchAsync(string connectionString, string sql)
    {
        try
        {
            await ExecuteAsync(connectionString, sql);
        }
        catch (PostgresException error)
        {
            return error;
        }

        throw new AssertionException($"Expected a database error for: {sql}");
    }
}
