using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace FinTech.Infrastructure.Data.Migrations
{
    public partial class AddLoanGovernanceEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create loans governance schema
            migrationBuilder.EnsureSchema(
                name: "loans");

            // Create LoanTypes table
            migrationBuilder.CreateTable(
                name: "LoanTypes",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MinInterestRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MaxInterestRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DefaultInterestRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MinTerm = table.Column<int>(type: "int", nullable: false),
                    MaxTerm = table.Column<int>(type: "int", nullable: false),
                    LoanMultiplier = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MaxMultiplier = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MinMultiplier = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GuarantorRequired = table.Column<bool>(type: "bit", nullable: false),
                    CollateralRequired = table.Column<bool>(type: "bit", nullable: false),
                    ProcessingFeePercentage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CommitteeApprovalThreshold = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AutoApprovalEligible = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanTypes", x => x.Id);
                });

            // Create LoanConfigurations table
            migrationBuilder.CreateTable(
                name: "LoanConfigurations",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValueType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MinValue = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    MaxValue = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    RequiresBoardApproval = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreviousValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangeReason = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanConfigurations", x => x.Id);
                });

            // Create LoanEligibilityRules table
            migrationBuilder.CreateTable(
                name: "LoanEligibilityRules",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RuleDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RuleExpression = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinSavingsRatio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MaxSavingsRatio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MinCreditScore = table.Column<int>(type: "int", nullable: false),
                    MaxCreditScore = table.Column<int>(type: "int", nullable: true),
                    MaxDebtToIncomeRatio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DeductionCeiling = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    GuarantorsRequired = table.Column<int>(type: "int", nullable: false),
                    MinMembershipMonths = table.Column<int>(type: "int", nullable: false),
                    MaxActiveLoanCount = table.Column<int>(type: "int", nullable: false),
                    AutoApproval = table.Column<bool>(type: "bit", nullable: false),
                    FailureAction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanEligibilityRules", x => x.Id);
                });

            // Create LoanCommitteeApprovals table
            migrationBuilder.CreateTable(
                name: "LoanCommitteeApprovals",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalRefNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LoanApplicationId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReferralReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuarantorVettingRequired = table.Column<bool>(type: "bit", nullable: false),
                    RepaymentHistoryScore = table.Column<int>(type: "int", nullable: false),
                    PreviousLoanDefaultCount = table.Column<int>(type: "int", nullable: false),
                    CommitteeMembers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeetingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CommitteeDecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VotingDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConditionsText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppealSubmitted = table.Column<bool>(type: "bit", nullable: false),
                    AppealDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppealDecision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AppealJustification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanCommitteeApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanCommitteeApprovals_LoanApplications",
                        column: x => x.LoanApplicationId,
                        principalSchema: "loans",
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create LoanRegisters table
            migrationBuilder.CreateTable(
                name: "LoanRegisters",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegisterNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LoanNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MemberId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MemberName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GuarantorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LoanAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisbursementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaturityDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RepaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DelinquencyDays = table.Column<int>(type: "int", nullable: false),
                    PrincipalPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InterestPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CollateralDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommodityDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficerInCharge = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MonthlyRegisterEntry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRegisters", x => x.Id);
                });

            // Create CommodityLoans table
            migrationBuilder.CreateTable(
                name: "CommodityLoans",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CommodityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitOfMeasurement = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupplierId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StorageLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WarehouseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReleaseSchedule = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    QualityRating = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InsuranceCoverage = table.Column<bool>(type: "bit", nullable: false),
                    InsurancePolicyNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InsurancePremium = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    InsuranceExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MarketPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastPriceUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShelfLife = table.Column<int>(type: "int", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleaseHistory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommodityLoans", x => x.Id);
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_LoanTypes_TypeCode",
                schema: "loans",
                table: "LoanTypes",
                column: "TypeCode",
                isUnique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanConfigurations_ConfigKey",
                schema: "loans",
                table: "LoanConfigurations",
                column: "ConfigKey");

            migrationBuilder.CreateIndex(
                name: "IX_LoanConfigurations_Category",
                schema: "loans",
                table: "LoanConfigurations",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_LoanEligibilityRules_RuleCode",
                schema: "loans",
                table: "LoanEligibilityRules",
                column: "RuleCode",
                isUnique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanEligibilityRules_IsActive",
                schema: "loans",
                table: "LoanEligibilityRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LoanCommitteeApprovals_ApprovalRefNumber",
                schema: "loans",
                table: "LoanCommitteeApprovals",
                column: "ApprovalRefNumber",
                isUnique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanCommitteeApprovals_LoanApplicationId",
                schema: "loans",
                table: "LoanCommitteeApprovals",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanCommitteeApprovals_ApprovalStatus",
                schema: "loans",
                table: "LoanCommitteeApprovals",
                column: "ApprovalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRegisters_RegisterNumber",
                schema: "loans",
                table: "LoanRegisters",
                column: "RegisterNumber",
                isUnique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanRegisters_LoanNumber",
                schema: "loans",
                table: "LoanRegisters",
                column: "LoanNumber",
                isUnique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoanRegisters_MemberId",
                schema: "loans",
                table: "LoanRegisters",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRegisters_RepaymentStatus",
                schema: "loans",
                table: "LoanRegisters",
                column: "RepaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CommodityLoans_LoanNumber",
                schema: "loans",
                table: "CommodityLoans",
                column: "LoanNumber",
                isUnique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommodityLoans_CommodityType",
                schema: "loans",
                table: "CommodityLoans",
                column: "CommodityType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_CommodityLoans_CommodityType",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_CommodityLoans_LoanNumber",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanRegisters_RepaymentStatus",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanRegisters_MemberId",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanRegisters_LoanNumber",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanRegisters_RegisterNumber",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanCommitteeApprovals_ApprovalStatus",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanCommitteeApprovals_LoanApplicationId",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanCommitteeApprovals_ApprovalRefNumber",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanEligibilityRules_IsActive",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanEligibilityRules_RuleCode",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanConfigurations_Category",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanConfigurations_ConfigKey",
                schema: "loans");

            migrationBuilder.DropIndex(
                name: "IX_LoanTypes_TypeCode",
                schema: "loans");

            // Drop tables
            migrationBuilder.DropTable(
                name: "CommodityLoans",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanRegisters",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanCommitteeApprovals",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanEligibilityRules",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanConfigurations",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanTypes",
                schema: "loans");
        }
    }
}
