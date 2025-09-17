using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace FinTech.Infrastructure.Data.Migrations
{
    public partial class LoanManagementSystem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create loans schema
            migrationBuilder.EnsureSchema(
                name: "loans");

            // Create LoanProducts table
            migrationBuilder.CreateTable(
                name: "LoanProducts",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InterestRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MinTerm = table.Column<int>(type: "int", nullable: false),
                    MaxTerm = table.Column<int>(type: "int", nullable: false),
                    InterestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RepaymentFrequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProcessingFeePercentage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequiredDocuments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EligibilityCriteria = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanProducts", x => x.Id);
                });

            // Create LoanApplications table
            migrationBuilder.CreateTable(
                name: "LoanApplications",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoanProductId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApplicationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RequestedTerm = table.Column<int>(type: "int", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ApprovedTerm = table.Column<int>(type: "int", nullable: true),
                    InterestRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RejectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanApplications_LoanProducts_LoanProductId",
                        column: x => x.LoanProductId,
                        principalSchema: "loans",
                        principalTable: "LoanProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create Loans table
            migrationBuilder.CreateTable(
                name: "Loans",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoanApplicationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoanProductId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoanAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LoanTerm = table.Column<int>(type: "int", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    InterestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RepaymentFrequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaturityDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DisbursementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OutstandingPrincipal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingInterest = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NextRepaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRepaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DisbursedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClosureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loans_LoanApplications_LoanApplicationId",
                        column: x => x.LoanApplicationId,
                        principalSchema: "loans",
                        principalTable: "LoanApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Loans_LoanProducts_LoanProductId",
                        column: x => x.LoanProductId,
                        principalSchema: "loans",
                        principalTable: "LoanProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create LoanTransactions table
            migrationBuilder.CreateTable(
                name: "LoanTransactions",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProcessedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanTransactions_Loans_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "loans",
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create LoanRepaymentSchedules table
            migrationBuilder.CreateTable(
                name: "LoanRepaymentSchedules",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstallmentNumber = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InterestAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRepaymentSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanRepaymentSchedules_Loans_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "loans",
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create LoanCollaterals table
            migrationBuilder.CreateTable(
                name: "LoanCollaterals",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CollateralType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalComments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RejectedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RejectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanCollaterals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanCollaterals_Loans_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "loans",
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create LoanGuarantors table
            migrationBuilder.CreateTable(
                name: "LoanGuarantors",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuarantorCustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RejectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanGuarantors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanGuarantors_Loans_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "loans",
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create LoanDocuments table
            migrationBuilder.CreateTable(
                name: "LoanDocuments",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    VerifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VerificationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RejectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanDocuments_Loans_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "loans",
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create LoanCollections table
            migrationBuilder.CreateTable(
                name: "LoanCollections",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountDue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountCollected = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CollectionMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CollectionAgentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CollectionNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NextFollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FollowUpAction = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanCollections_Loans_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "loans",
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create LoanFees table
            migrationBuilder.CreateTable(
                name: "LoanFees",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LoanProductId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FeeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Percentage = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    AppliedAt = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanFees_LoanProducts_LoanProductId",
                        column: x => x.LoanProductId,
                        principalSchema: "loans",
                        principalTable: "LoanProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoanFees_Loans_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "loans",
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create LoanCreditChecks table
            migrationBuilder.CreateTable(
                name: "LoanCreditChecks",
                schema: "loans",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoanId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreditBureauId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CheckDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreditScore = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    MaxEligibleAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ExistingLoans = table.Column<int>(type: "int", precision: 6, scale: 0, nullable: true),
                    OutstandingDebt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CheckedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanCreditChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanCreditChecks_Loans_LoanId",
                        column: x => x.LoanId,
                        principalSchema: "loans",
                        principalTable: "Loans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes for LoanProducts
            migrationBuilder.CreateIndex(
                name: "IX_LoanProducts_Name",
                schema: "loans",
                table: "LoanProducts",
                column: "Name",
                unique: true);

            // Create indexes for LoanApplications
            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_CustomerId_ApplicationDate",
                schema: "loans",
                table: "LoanApplications",
                columns: new[] { "CustomerId", "ApplicationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_LoanProductId",
                schema: "loans",
                table: "LoanApplications",
                column: "LoanProductId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_Status",
                schema: "loans",
                table: "LoanApplications",
                column: "Status");

            // Create indexes for Loans
            migrationBuilder.CreateIndex(
                name: "IX_Loans_AccountNumber",
                schema: "loans",
                table: "Loans",
                column: "AccountNumber",
                unique: true,
                filter: "[AccountNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_CustomerId",
                schema: "loans",
                table: "Loans",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_LoanApplicationId",
                schema: "loans",
                table: "Loans",
                column: "LoanApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_LoanProductId",
                schema: "loans",
                table: "Loans",
                column: "LoanProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_Status",
                schema: "loans",
                table: "Loans",
                column: "Status");

            // Create indexes for LoanTransactions
            migrationBuilder.CreateIndex(
                name: "IX_LoanTransactions_LoanId",
                schema: "loans",
                table: "LoanTransactions",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanTransactions_ReferenceNumber",
                schema: "loans",
                table: "LoanTransactions",
                column: "ReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_LoanTransactions_TransactionDate",
                schema: "loans",
                table: "LoanTransactions",
                column: "TransactionDate");

            // Create indexes for LoanRepaymentSchedules
            migrationBuilder.CreateIndex(
                name: "IX_LoanRepaymentSchedules_DueDate",
                schema: "loans",
                table: "LoanRepaymentSchedules",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRepaymentSchedules_LoanId",
                schema: "loans",
                table: "LoanRepaymentSchedules",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRepaymentSchedules_Status",
                schema: "loans",
                table: "LoanRepaymentSchedules",
                column: "Status");

            // Create indexes for LoanCollaterals
            migrationBuilder.CreateIndex(
                name: "IX_LoanCollaterals_LoanId",
                schema: "loans",
                table: "LoanCollaterals",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanCollaterals_Status",
                schema: "loans",
                table: "LoanCollaterals",
                column: "Status");

            // Create indexes for LoanGuarantors
            migrationBuilder.CreateIndex(
                name: "IX_LoanGuarantors_GuarantorCustomerId",
                schema: "loans",
                table: "LoanGuarantors",
                column: "GuarantorCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanGuarantors_IsApproved",
                schema: "loans",
                table: "LoanGuarantors",
                column: "IsApproved");

            migrationBuilder.CreateIndex(
                name: "IX_LoanGuarantors_LoanId",
                schema: "loans",
                table: "LoanGuarantors",
                column: "LoanId");

            // Create indexes for LoanDocuments
            migrationBuilder.CreateIndex(
                name: "IX_LoanDocuments_DocumentType",
                schema: "loans",
                table: "LoanDocuments",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_LoanDocuments_LoanId",
                schema: "loans",
                table: "LoanDocuments",
                column: "LoanId");

            // Create indexes for LoanCollections
            migrationBuilder.CreateIndex(
                name: "IX_LoanCollections_CollectionAgentId",
                schema: "loans",
                table: "LoanCollections",
                column: "CollectionAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanCollections_CustomerId",
                schema: "loans",
                table: "LoanCollections",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanCollections_DueDate",
                schema: "loans",
                table: "LoanCollections",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoanCollections_LoanId",
                schema: "loans",
                table: "LoanCollections",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanCollections_Status",
                schema: "loans",
                table: "LoanCollections",
                column: "Status");

            // Create indexes for LoanFees
            migrationBuilder.CreateIndex(
                name: "IX_LoanFees_FeeType",
                schema: "loans",
                table: "LoanFees",
                column: "FeeType");

            migrationBuilder.CreateIndex(
                name: "IX_LoanFees_LoanId",
                schema: "loans",
                table: "LoanFees",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanFees_LoanProductId",
                schema: "loans",
                table: "LoanFees",
                column: "LoanProductId");

            // Create indexes for LoanCreditChecks
            migrationBuilder.CreateIndex(
                name: "IX_LoanCreditChecks_CheckDate",
                schema: "loans",
                table: "LoanCreditChecks",
                column: "CheckDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoanCreditChecks_CustomerId",
                schema: "loans",
                table: "LoanCreditChecks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanCreditChecks_LoanId",
                schema: "loans",
                table: "LoanCreditChecks",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanCreditChecks_Status",
                schema: "loans",
                table: "LoanCreditChecks",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tables in reverse order of creation
            migrationBuilder.DropTable(
                name: "LoanCreditChecks",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanFees",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanCollections",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanDocuments",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanGuarantors",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanCollaterals",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanRepaymentSchedules",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanTransactions",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "Loans",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanApplications",
                schema: "loans");

            migrationBuilder.DropTable(
                name: "LoanProducts",
                schema: "loans");

            // Drop the loans schema
            migrationBuilder.DropSchema(
                name: "loans");
        }
    }
}