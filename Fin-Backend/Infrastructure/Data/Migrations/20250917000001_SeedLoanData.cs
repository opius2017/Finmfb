using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTech.Infrastructure.Data.Migrations
{
    public partial class LoanManagementSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert initial loan product data
            migrationBuilder.InsertData(
                schema: "loans",
                table: "LoanProducts",
                columns: new[] { "Id", "Name", "Description", "InterestRate", "MinAmount", "MaxAmount", "MinTerm", "MaxTerm", "InterestType", "RepaymentFrequency", "ProcessingFeePercentage", "Status", "RequiredDocuments", "EligibilityCriteria", "CreatedDate", "CreatedBy", "LastModifiedDate", "LastModifiedBy" },
                values: new object[,]
                {
                    {
                        "LP-001",
                        "Personal Loan",
                        "Short-term personal loan for customers with good credit history",
                        10.5m,
                        50000m,
                        500000m,
                        3,
                        12,
                        "Reducing Balance",
                        "Monthly",
                        1.5m,
                        "Active",
                        "ID Card, Proof of Income, Bank Statement (6 months), Utility Bill",
                        "Must be an existing customer with at least 6 months relationship. Age 21-60 years.",
                        DateTime.UtcNow,
                        "System",
                        null,
                        null
                    },
                    {
                        "LP-002",
                        "Business Loan",
                        "Medium-term business loan for SMEs with collateral",
                        14.0m,
                        500000m,
                        5000000m,
                        12,
                        36,
                        "Reducing Balance",
                        "Monthly",
                        2.0m,
                        "Active",
                        "Business Registration, Financial Statements, Tax Clearance, Collateral Documents",
                        "Business must be operational for at least 2 years with audited accounts.",
                        DateTime.UtcNow,
                        "System",
                        null,
                        null
                    },
                    {
                        "LP-003",
                        "Micro Loan",
                        "Small loan for micro-entrepreneurs and small businesses",
                        18.0m,
                        10000m,
                        100000m,
                        1,
                        6,
                        "Flat",
                        "Weekly",
                        1.0m,
                        "Active",
                        "ID Card, Business Registration (if available), Guarantor Form",
                        "Must be an existing customer with proven business activity.",
                        DateTime.UtcNow,
                        "System",
                        null,
                        null
                    },
                    {
                        "LP-004",
                        "Mortgage Loan",
                        "Long-term loan for home purchase or construction",
                        12.0m,
                        2000000m,
                        20000000m,
                        60,
                        180,
                        "Reducing Balance",
                        "Monthly",
                        1.0m,
                        "Active",
                        "ID Card, Proof of Income, Property Documents, Valuation Report, Insurance Policy",
                        "Must be between 25-55 years of age. Property must be in approved location.",
                        DateTime.UtcNow,
                        "System",
                        null,
                        null
                    },
                    {
                        "LP-005",
                        "Agricultural Loan",
                        "Seasonal loan for farmers and agricultural businesses",
                        9.0m,
                        100000m,
                        1000000m,
                        3,
                        12,
                        "Flat",
                        "Quarterly",
                        1.0m,
                        "Active",
                        "ID Card, Proof of Land Ownership or Lease, Farm Business Plan",
                        "Must be involved in agricultural activities for at least 2 farming seasons.",
                        DateTime.UtcNow,
                        "System",
                        null,
                        null
                    }
                });

            // Insert standard loan fees
            migrationBuilder.InsertData(
                schema: "loans",
                table: "LoanFees",
                columns: new[] { "Id", "LoanProductId", "FeeType", "Name", "Description", "Amount", "Percentage", "IsRequired", "AppliedAt", "CreatedDate", "LastModifiedDate" },
                values: new object[,]
                {
                    {
                        "LF-001",
                        null,
                        "Processing",
                        "Processing Fee",
                        "Fee charged for processing the loan application",
                        null,
                        2.0m,
                        true,
                        "Disbursement",
                        DateTime.UtcNow,
                        null
                    },
                    {
                        "LF-002",
                        null,
                        "Insurance",
                        "Credit Life Insurance",
                        "Insurance to cover outstanding loan balance in case of borrower's death",
                        null,
                        0.5m,
                        true,
                        "Disbursement",
                        DateTime.UtcNow,
                        null
                    },
                    {
                        "LF-003",
                        null,
                        "Administrative",
                        "Administrative Fee",
                        "Fee for loan documentation and administration",
                        5000m,
                        null,
                        true,
                        "Disbursement",
                        DateTime.UtcNow,
                        null
                    },
                    {
                        "LF-004",
                        null,
                        "Late Payment",
                        "Late Payment Penalty",
                        "Fee charged when payment is received after due date",
                        null,
                        5.0m,
                        true,
                        "OnLatePayment",
                        DateTime.UtcNow,
                        null
                    },
                    {
                        "LF-005",
                        null,
                        "Early Settlement",
                        "Early Settlement Fee",
                        "Fee charged when loan is fully repaid before maturity",
                        null,
                        1.0m,
                        true,
                        "OnSettlement",
                        DateTime.UtcNow,
                        null
                    }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove seed data
            migrationBuilder.DeleteData(
                schema: "loans",
                table: "LoanFees",
                keyColumn: "Id",
                keyValues: new object[] { "LF-001", "LF-002", "LF-003", "LF-004", "LF-005" });

            migrationBuilder.DeleteData(
                schema: "loans",
                table: "LoanProducts",
                keyColumn: "Id",
                keyValues: new object[] { "LP-001", "LP-002", "LP-003", "LP-004", "LP-005" });
        }
    }
}