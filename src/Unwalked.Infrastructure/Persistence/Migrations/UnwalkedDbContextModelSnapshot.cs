using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Unwalked.Infrastructure.Persistence;

#nullable disable

namespace Unwalked.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(UnwalkedDbContext))]
    partial class UnwalkedDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "postgis");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Unwalked.Domain.Feedback.FeedbackDailyTotal", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("character varying(16)")
                        .HasColumnName("action");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("integer")
                        .HasColumnName("category_id");

                    b.Property<int>("EventCount")
                        .HasColumnType("integer")
                        .HasColumnName("event_count");

                    b.Property<DateOnly>("ReceivedOn")
                        .HasColumnType("date")
                        .HasColumnName("received_on");

                    b.Property<string>("RecommendationType")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("recommendation_type");

                    b.HasKey("Id")
                        .HasName("pk_feedback_daily_total");

                    b.HasIndex("CategoryId")
                        .HasDatabaseName("ix_feedback_daily_total_category_id");

                    b.HasIndex("ReceivedOn", "RecommendationType", "Action", "CategoryId")
                        .IsUnique()
                        .HasDatabaseName("ux_feedback_daily_total_day_type_action_category");

                    NpgsqlIndexBuilderExtensions.AreNullsDistinct(b.HasIndex("ReceivedOn", "RecommendationType", "Action", "CategoryId"), false);

                    b.ToTable("feedback_daily_total", null, t =>
                        {
                            t.HasCheckConstraint("ck_feedback_daily_total_action", "action IN ('accepted', 'dismissed', 'visited')");

                            t.HasCheckConstraint("ck_feedback_daily_total_count_positive", "event_count > 0");

                            t.HasCheckConstraint("ck_feedback_daily_total_type", "recommendation_type IN ('nearby_new_place', 'along_habitual_route', 'unvisited_cell', 'walking_route')");
                        });
                });

            modelBuilder.Entity("Unwalked.Domain.Feedback.RecommendationFeedback", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("character varying(16)")
                        .HasColumnName("action");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("integer")
                        .HasColumnName("category_id");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uuid")
                        .HasColumnName("event_id");

                    b.Property<DateOnly>("ReceivedOn")
                        .HasColumnType("date")
                        .HasColumnName("received_on");

                    b.Property<string>("RecommendationType")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("recommendation_type");

                    b.HasKey("Id")
                        .HasName("pk_recommendation_feedback");

                    b.HasIndex("CategoryId")
                        .HasDatabaseName("ix_recommendation_feedback_category_id");

                    b.HasIndex("EventId")
                        .IsUnique()
                        .HasDatabaseName("ix_recommendation_feedback_event_id");

                    b.HasIndex("ReceivedOn", "RecommendationType")
                        .HasDatabaseName("ix_recommendation_feedback_received_on_recommendation_type");

                    b.ToTable("recommendation_feedback", null, t =>
                        {
                            t.HasCheckConstraint("ck_recommendation_feedback_action", "action IN ('accepted', 'dismissed', 'visited')");

                            t.HasCheckConstraint("ck_recommendation_feedback_type", "recommendation_type IN ('nearby_new_place', 'along_habitual_route', 'unvisited_cell', 'walking_route')");
                        });
                });

            modelBuilder.Entity("Unwalked.Domain.Places.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("code");

                    b.HasKey("Id")
                        .HasName("pk_category");

                    b.HasIndex("Code")
                        .IsUnique()
                        .HasDatabaseName("ix_category_code");

                    b.ToTable("category", null, t =>
                        {
                            t.HasCheckConstraint("ck_category_code_format", "code ~ '^[a-z0-9_]{1,64}$'");
                        });
                });

            modelBuilder.Entity("Unwalked.Domain.Places.Place", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<decimal>("Confidence")
                        .HasPrecision(4, 3)
                        .HasColumnType("numeric(4,3)")
                        .HasColumnName("confidence");

                    b.Property<string>("ExternalId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasColumnName("external_id");

                    b.Property<long>("H3R11")
                        .HasColumnType("bigint")
                        .HasColumnName("h3_r11");

                    b.Property<Point>("Location")
                        .IsRequired()
                        .HasColumnType("geography (point, 4326)")
                        .HasColumnName("location");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)")
                        .HasColumnName("name");

                    b.Property<int>("PrimaryCategoryId")
                        .HasColumnType("integer")
                        .HasColumnName("primary_category_id");

                    b.Property<int>("RegionId")
                        .HasColumnType("integer")
                        .HasColumnName("region_id");

                    b.HasKey("Id")
                        .HasName("pk_place");

                    b.HasIndex("ExternalId")
                        .IsUnique()
                        .HasDatabaseName("ix_place_external_id");

                    b.HasIndex("H3R11")
                        .HasDatabaseName("ix_place_h3_r11");

                    b.HasIndex("Location")
                        .HasDatabaseName("ix_place_location");

                    NpgsqlIndexBuilderExtensions.HasMethod(b.HasIndex("Location"), "gist");

                    b.HasIndex("PrimaryCategoryId")
                        .HasDatabaseName("ix_place_primary_category_id");

                    b.HasIndex("RegionId")
                        .HasDatabaseName("ix_place_region_id");

                    b.ToTable("place", null, t =>
                        {
                            t.HasCheckConstraint("ck_place_confidence_range", "confidence BETWEEN 0 AND 1");

                            t.HasCheckConstraint("ck_place_name_not_blank", "btrim(name) <> ''");
                        });
                });

            modelBuilder.Entity("Unwalked.Domain.Places.PlaceAlternateCategory", b =>
                {
                    b.Property<long>("PlaceId")
                        .HasColumnType("bigint")
                        .HasColumnName("place_id");

                    b.Property<int>("CategoryId")
                        .HasColumnType("integer")
                        .HasColumnName("category_id");

                    b.HasKey("PlaceId", "CategoryId")
                        .HasName("pk_place_alternate_category");

                    b.HasIndex("CategoryId", "PlaceId")
                        .HasDatabaseName("ix_place_alternate_category_category_id_place_id");

                    b.ToTable("place_alternate_category", (string)null);
                });

            modelBuilder.Entity("Unwalked.Domain.Regions.Region", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Polygon>("Bounds")
                        .IsRequired()
                        .HasColumnType("geography (polygon, 4326)")
                        .HasColumnName("bounds");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("code");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasColumnName("name");

                    b.HasKey("Id")
                        .HasName("pk_region");

                    b.HasIndex("Code")
                        .IsUnique()
                        .HasDatabaseName("ix_region_code");

                    b.ToTable("region", null, t =>
                        {
                            t.HasCheckConstraint("ck_region_code_format", "code ~ '^[a-z0-9-]{2,32}$'");

                            t.HasCheckConstraint("ck_region_name_not_blank", "btrim(name) <> ''");
                        });
                });

            modelBuilder.Entity("Unwalked.Domain.Regions.RegionPack", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("BuiltAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("built_at");

                    b.Property<bool>("IsCurrent")
                        .HasColumnType("boolean")
                        .HasColumnName("is_current");

                    b.Property<string>("ObjectKey")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("object_key");

                    b.Property<int>("RegionId")
                        .HasColumnType("integer")
                        .HasColumnName("region_id");

                    b.Property<string>("Sha256")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character(64)")
                        .HasColumnName("sha256")
                        .IsFixedLength();

                    b.Property<long>("SizeBytes")
                        .HasColumnType("bigint")
                        .HasColumnName("size_bytes");

                    b.Property<int>("Version")
                        .HasColumnType("integer")
                        .HasColumnName("version");

                    b.HasKey("Id")
                        .HasName("pk_region_pack");

                    b.HasIndex("ObjectKey")
                        .IsUnique()
                        .HasDatabaseName("ix_region_pack_object_key");

                    b.HasIndex("RegionId")
                        .IsUnique()
                        .HasDatabaseName("ux_region_pack_current")
                        .HasFilter("is_current");

                    b.HasIndex("RegionId", "Version")
                        .IsUnique()
                        .HasDatabaseName("ix_region_pack_region_id_version");

                    b.ToTable("region_pack", null, t =>
                        {
                            t.HasCheckConstraint("ck_region_pack_sha256_hex", "sha256 ~ '^[0-9a-f]{64}$'");

                            t.HasCheckConstraint("ck_region_pack_size_positive", "size_bytes > 0");

                            t.HasCheckConstraint("ck_region_pack_version_positive", "version > 0");
                        });
                });

            modelBuilder.Entity("Unwalked.Domain.Sharing.AchievementShare", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateOnly>("AchievedOn")
                        .HasColumnType("date")
                        .HasColumnName("achieved_on");

                    b.Property<string>("AchievementName")
                        .IsRequired()
                        .HasMaxLength(80)
                        .HasColumnType("character varying(80)")
                        .HasColumnName("achievement_name");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<byte[]>("Image")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("image");

                    b.Property<string>("ImageContentType")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasColumnName("image_content_type");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uuid")
                        .HasColumnName("owner_id");

                    b.HasKey("Id")
                        .HasName("pk_achievement_share");

                    b.HasIndex("OwnerId")
                        .HasDatabaseName("ix_achievement_share_owner_id");

                    b.ToTable("achievement_share", null, t =>
                        {
                            t.HasCheckConstraint("ck_achievement_share_image_size", "octet_length(image) BETWEEN 1 AND 307200");

                            t.HasCheckConstraint("ck_achievement_share_image_type", "image_content_type IN ('image/png', 'image/jpeg', 'image/webp')");

                            t.HasCheckConstraint("ck_achievement_share_name_not_blank", "btrim(achievement_name) <> ''");
                        });
                });

            modelBuilder.Entity("Unwalked.Domain.Users.UserAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(254)
                        .HasColumnType("character varying(254)")
                        .HasColumnName("email");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("password_hash");

                    b.HasKey("Id")
                        .HasName("pk_user_account");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("ix_user_account_email");

                    b.ToTable("user_account", null, t =>
                        {
                            t.HasCheckConstraint("ck_user_account_email_lowercase", "email = lower(email)");
                        });
                });

            modelBuilder.Entity("Unwalked.Domain.Feedback.FeedbackDailyTotal", b =>
                {
                    b.HasOne("Unwalked.Domain.Places.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_feedback_daily_total_category");
                });

            modelBuilder.Entity("Unwalked.Domain.Feedback.RecommendationFeedback", b =>
                {
                    b.HasOne("Unwalked.Domain.Places.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_recommendation_feedback_category");
                });

            modelBuilder.Entity("Unwalked.Domain.Places.Place", b =>
                {
                    b.HasOne("Unwalked.Domain.Places.Category", "PrimaryCategory")
                        .WithMany()
                        .HasForeignKey("PrimaryCategoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_place_primary_category");

                    b.HasOne("Unwalked.Domain.Regions.Region", "Region")
                        .WithMany()
                        .HasForeignKey("RegionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_place_region");

                    b.Navigation("PrimaryCategory");

                    b.Navigation("Region");
                });

            modelBuilder.Entity("Unwalked.Domain.Places.PlaceAlternateCategory", b =>
                {
                    b.HasOne("Unwalked.Domain.Places.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_place_alternate_category_category");

                    b.HasOne("Unwalked.Domain.Places.Place", null)
                        .WithMany("AlternateCategories")
                        .HasForeignKey("PlaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_place_alternate_category_place");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("Unwalked.Domain.Regions.RegionPack", b =>
                {
                    b.HasOne("Unwalked.Domain.Regions.Region", "Region")
                        .WithMany()
                        .HasForeignKey("RegionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("fk_region_pack_region");

                    b.Navigation("Region");
                });

            modelBuilder.Entity("Unwalked.Domain.Sharing.AchievementShare", b =>
                {
                    b.HasOne("Unwalked.Domain.Users.UserAccount", null)
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_achievement_share_owner");
                });

            modelBuilder.Entity("Unwalked.Domain.Places.Place", b =>
                {
                    b.Navigation("AlternateCategories");
                });
#pragma warning restore 612, 618
        }
    }
}
