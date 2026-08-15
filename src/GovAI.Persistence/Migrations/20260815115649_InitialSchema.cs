using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GovAI.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "govai");

            migrationBuilder.CreateTable(
                name: "audit_log",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    action = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    entity_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    user_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    user_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    payload_json = table.Column<string>(type: "jsonb", nullable: true),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    legal_name = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    tax_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    legal_type = table.Column<int>(type: "integer", nullable: false),
                    founded_on = table.Column<DateOnly>(type: "date", nullable: true),
                    employee_count = table.Column<int>(type: "integer", nullable: false),
                    women_employee_count = table.Column<int>(type: "integer", nullable: false),
                    young_employee_count = table.Column<int>(type: "integer", nullable: false),
                    rnd_employee_count = table.Column<int>(type: "integer", nullable: false),
                    disabled_employee_count = table.Column<int>(type: "integer", nullable: false),
                    annual_revenue = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: false),
                    balance_size = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: false),
                    equity = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: false),
                    export_revenue = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    fiscal_year = table.Column<int>(type: "integer", nullable: true),
                    export_flag = table.Column<bool>(type: "boolean", nullable: false),
                    technology_flag = table.Column<bool>(type: "boolean", nullable: false),
                    previous_successful_applications = table.Column<int>(type: "integer", nullable: false),
                    last_synced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    profile_version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    opportunity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    deduplication_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    channel = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delivery_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    delivery_attempt_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scenario_simulations",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    changes_json = table.Column<string>(type: "jsonb", nullable: false),
                    baseline_eligible_count = table.Column<int>(type: "integer", nullable: false),
                    simulated_eligible_count = table.Column<int>(type: "integer", nullable: false),
                    baseline_average_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    simulated_average_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scenario_simulations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sources",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    base_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    cron_expression = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    configuration_json = table.Column<string>(type: "jsonb", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    last_run_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_run_status = table.Column<int>(type: "integer", nullable: false),
                    last_run_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    consecutive_failure_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    plan = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    max_companies = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "company_certificates",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    issued_on = table.Column<DateOnly>(type: "date", nullable: true),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    document_uri = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_certificates", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_certificates_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "govai",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_investments",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    related_category = table.Column<int>(type: "integer", nullable: false),
                    planned_budget = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: false),
                    planned_start = table.Column<DateOnly>(type: "date", nullable: true),
                    planned_end = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_investments", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_investments_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "govai",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_locations",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    district = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nuts2code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    is_headquarters = table.Column<bool>(type: "boolean", nullable: false),
                    is_in_technopark = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_locations", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_locations_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "govai",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "company_nace_codes",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_nace_codes", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_nace_codes_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "govai",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scenario_impacts",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scenario_simulation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    opportunity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    opportunity_title = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    baseline_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    simulated_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    baseline_verdict = table.Column<int>(type: "integer", nullable: false),
                    simulated_verdict = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scenario_impacts", x => x.id);
                    table.ForeignKey(
                        name: "fk_scenario_impacts_scenario_simulations_scenario_simulation_id",
                        column: x => x.scenario_simulation_id,
                        principalSchema: "govai",
                        principalTable: "scenario_simulations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "opportunities",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_type = table.Column<int>(type: "integer", nullable: false),
                    support_category = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    publisher = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    source_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    budget_min = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    budget_max = table.Column<decimal>(type: "numeric(20,2)", precision: 20, scale: 2, nullable: true),
                    budget_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    support_rate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    rule_extraction_confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    is_reviewed_by_consultant = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_opportunities", x => x.id);
                    table.ForeignKey(
                        name: "fk_opportunities_sources_source_id",
                        column: x => x.source_id,
                        principalSchema: "govai",
                        principalTable: "sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "source_documents",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    title = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    raw_content = table.Column<string>(type: "text", nullable: false),
                    normalized_text = table.Column<string>(type: "text", nullable: true),
                    media_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    content_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    collected_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    processing_error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    revision = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_source_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_source_documents_sources_source_id",
                        column: x => x.source_id,
                        principalSchema: "govai",
                        principalTable: "sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    full_name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    password_hash = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    external_subject_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    failed_login_count = table.Column<int>(type: "integer", nullable: false),
                    locked_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    scoped_company_ids_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "govai",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eligibility_assessments",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    opportunity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    verdict = table.Column<int>(type: "integer", nullable: false),
                    final_score = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    has_blocking_failure = table.Column<bool>(type: "boolean", nullable: false),
                    company_profile_version = table.Column<int>(type: "integer", nullable: false),
                    blocking_failure_count = table.Column<int>(type: "integer", nullable: false),
                    missing_condition_count = table.Column<int>(type: "integer", nullable: false),
                    data_gap_count = table.Column<int>(type: "integer", nullable: false),
                    missing_mandatory_document_count = table.Column<int>(type: "integer", nullable: false),
                    detail_json = table.Column<string>(type: "jsonb", nullable: false),
                    executive_summary = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    summary_generated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    summary_model = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    is_latest = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eligibility_assessments", x => x.id);
                    table.ForeignKey(
                        name: "fk_eligibility_assessments_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "govai",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_eligibility_assessments_opportunities_opportunity_id",
                        column: x => x.opportunity_id,
                        principalSchema: "govai",
                        principalTable: "opportunities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "opportunity_documents",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    opportunity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    issuing_authority = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_opportunity_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_opportunity_documents_opportunities_opportunity_id",
                        column: x => x.opportunity_id,
                        principalSchema: "govai",
                        principalTable: "opportunities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "opportunity_rules",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    opportunity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    @operator = table.Column<int>(name: "operator", type: "integer", nullable: false),
                    value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    dimension = table.Column<int>(type: "integer", nullable: false),
                    severity = table.Column<int>(type: "integer", nullable: false),
                    human_readable = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    source_excerpt = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    is_manually_overridden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_opportunity_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_opportunity_rules_opportunities_opportunity_id",
                        column: x => x.opportunity_id,
                        principalSchema: "govai",
                        principalTable: "opportunities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assessment_dimensions",
                schema: "govai",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    eligibility_assessment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dimension = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    weight = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    contribution = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false),
                    rationale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assessment_dimensions", x => x.id);
                    table.ForeignKey(
                        name: "fk_assessment_dimensions_assessments_eligibility_assessment_id",
                        column: x => x.eligibility_assessment_id,
                        principalSchema: "govai",
                        principalTable: "eligibility_assessments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_assessment_dimensions_eligibility_assessment_id",
                schema: "govai",
                table: "assessment_dimensions",
                column: "eligibility_assessment_id");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_entity_type_entity_id",
                schema: "govai",
                table: "audit_log",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_occurred_at",
                schema: "govai",
                table: "audit_log",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_user_email",
                schema: "govai",
                table: "audit_log",
                column: "user_email");

            migrationBuilder.CreateIndex(
                name: "ix_companies_tenant_id",
                schema: "govai",
                table: "companies",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_companies_tenant_id_tax_number",
                schema: "govai",
                table: "companies",
                columns: new[] { "tenant_id", "tax_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_company_certificates_company_id_code",
                schema: "govai",
                table: "company_certificates",
                columns: new[] { "company_id", "code" });

            migrationBuilder.CreateIndex(
                name: "ix_company_investments_company_id",
                schema: "govai",
                table: "company_investments",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_locations_company_id",
                schema: "govai",
                table: "company_locations",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_company_locations_nuts2code",
                schema: "govai",
                table: "company_locations",
                column: "nuts2code");

            migrationBuilder.CreateIndex(
                name: "ix_company_nace_codes_company_id_code",
                schema: "govai",
                table: "company_nace_codes",
                columns: new[] { "company_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_eligibility_assessments_company_id_is_latest_final_score",
                schema: "govai",
                table: "eligibility_assessments",
                columns: new[] { "company_id", "is_latest", "final_score" });

            migrationBuilder.CreateIndex(
                name: "ix_eligibility_assessments_company_id_opportunity_id_is_latest",
                schema: "govai",
                table: "eligibility_assessments",
                columns: new[] { "company_id", "opportunity_id", "is_latest" });

            migrationBuilder.CreateIndex(
                name: "ix_eligibility_assessments_evaluated_at",
                schema: "govai",
                table: "eligibility_assessments",
                column: "evaluated_at");

            migrationBuilder.CreateIndex(
                name: "ix_eligibility_assessments_opportunity_id",
                schema: "govai",
                table: "eligibility_assessments",
                column: "opportunity_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_deduplication_key",
                schema: "govai",
                table: "notifications",
                column: "deduplication_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_sent_at",
                schema: "govai",
                table: "notifications",
                column: "sent_at");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_tenant_id_company_id_created_at",
                schema: "govai",
                table: "notifications",
                columns: new[] { "tenant_id", "company_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_opportunities_deadline",
                schema: "govai",
                table: "opportunities",
                column: "deadline");

            migrationBuilder.CreateIndex(
                name: "ix_opportunities_published_at",
                schema: "govai",
                table: "opportunities",
                column: "published_at");

            migrationBuilder.CreateIndex(
                name: "ix_opportunities_source_document_id",
                schema: "govai",
                table: "opportunities",
                column: "source_document_id",
                unique: true,
                filter: "source_document_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_opportunities_source_id",
                schema: "govai",
                table: "opportunities",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "ix_opportunities_support_category",
                schema: "govai",
                table: "opportunities",
                column: "support_category");

            migrationBuilder.CreateIndex(
                name: "ix_opportunity_documents_opportunity_id",
                schema: "govai",
                table: "opportunity_documents",
                column: "opportunity_id");

            migrationBuilder.CreateIndex(
                name: "ix_opportunity_rules_dimension",
                schema: "govai",
                table: "opportunity_rules",
                column: "dimension");

            migrationBuilder.CreateIndex(
                name: "ix_opportunity_rules_opportunity_id",
                schema: "govai",
                table: "opportunity_rules",
                column: "opportunity_id");

            migrationBuilder.CreateIndex(
                name: "ix_scenario_impacts_scenario_simulation_id",
                schema: "govai",
                table: "scenario_impacts",
                column: "scenario_simulation_id");

            migrationBuilder.CreateIndex(
                name: "ix_scenario_simulations_company_id",
                schema: "govai",
                table: "scenario_simulations",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_source_documents_content_hash",
                schema: "govai",
                table: "source_documents",
                column: "content_hash");

            migrationBuilder.CreateIndex(
                name: "ix_source_documents_source_id_url",
                schema: "govai",
                table: "source_documents",
                columns: new[] { "source_id", "url" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_source_documents_status",
                schema: "govai",
                table: "source_documents",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_sources_is_enabled",
                schema: "govai",
                table: "sources",
                column: "is_enabled");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_slug",
                schema: "govai",
                table: "tenants",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "govai",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_tenant_id",
                schema: "govai",
                table: "users",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assessment_dimensions",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "audit_log",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "company_certificates",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "company_investments",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "company_locations",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "company_nace_codes",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "opportunity_documents",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "opportunity_rules",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "scenario_impacts",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "source_documents",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "users",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "eligibility_assessments",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "scenario_simulations",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "opportunities",
                schema: "govai");

            migrationBuilder.DropTable(
                name: "sources",
                schema: "govai");
        }
    }
}
