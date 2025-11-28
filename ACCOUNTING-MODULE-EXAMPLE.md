# Accounting Module - Complete Implementation Example

## Domain Layer - Chart of Accounts & Journal Entries

```csharp
// Core/Domain/Entities/LedgerAccount.cs
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.Entities;

public class LedgerAccount : BaseEntity
{
    public required string AccountCode { get; private set; }
    public required string AccountName { get; private set; }
    public required AccountType AccountType { get; private set; }
    public AccountStatus Status { get; private set; }
    public decimal Balance { get; private set; }
    public decimal DebitTotal { get; private set; }
    public decimal CreditTotal { get; private set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }

    public static LedgerAccount Create(
        string accountCode,
        string accountName,
        AccountType accountType)
    {
        return new LedgerAccount
        {
            Id = Guid.NewGuid(),
            AccountCode = accountCode,
            AccountName = accountName,
            AccountType = accountType,
            Status = AccountStatus.Active,
            Balance = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void PostDebit(decimal amount)
    {
        if (amount <= 0) throw new InvalidOperationException("Amount must be positive");
        DebitTotal += amount;
        UpdateBalance();
    }

    public void PostCredit(decimal amount)
    {
        if (amount <= 0) throw new InvalidOperationException("Amount must be positive");
        CreditTotal += amount;
        UpdateBalance();
    }

    private void UpdateBalance()
    {
        Balance = AccountType switch
        {
            AccountType.Asset => DebitTotal - CreditTotal,
            AccountType.Liability => CreditTotal - DebitTotal,
            AccountType.Equity => CreditTotal - DebitTotal,
            AccountType.Revenue => CreditTotal - DebitTotal,
            AccountType.Expense => DebitTotal - CreditTotal,
            _ => 0
        };
    }

    public void Close()
    {
        Status = AccountStatus.Closed;
    }
}

public enum AccountType { Asset, Liability, Equity, Revenue, Expense }
public enum AccountStatus { Active, Inactive, Closed }
```

```csharp
// Core/Domain/Entities/JournalEntry.cs
using FinTech.Core.Domain.Common;

namespace FinTech.Core.Domain.Entities;

public class JournalEntry : BaseEntity
{
    public required string EntryNumber { get; private set; }
    public required string Description { get; private set; }
    public DateTime PostDate { get; private set; }
    public JournalEntryStatus Status { get; private set; }
    public string? Reference { get; private set; }
    public string? Source { get; private set; } // "FixedAsset", "Loan", "Sales", etc.
    public Guid? SourceId { get; private set; }
    public decimal TotalDebits { get; private set; }
    public decimal TotalCredits { get; private set; }
    public ICollection<JournalLine> Lines { get; private set; } = [];
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public DateTime? ReversedAt { get; set; }
    public Guid? ReversedBy { get; set; }
    public string? ReversalReason { get; set; }

    public static JournalEntry Create(
        string entryNumber,
        string description,
        string source,
        Guid? sourceId = null)
    {
        return new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = entryNumber,
            Description = description,
            Source = source,
            SourceId = sourceId,
            Status = JournalEntryStatus.Draft,
            PostDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void AddLine(Guid accountId, string description, decimal debit, decimal credit)
    {
        if (debit > 0 && credit > 0)
            throw new InvalidOperationException("A line cannot have both debit and credit");

        if (debit == 0 && credit == 0)
            throw new InvalidOperationException("A line must have either debit or credit");

        var line = new JournalLine
        {
            Id = Guid.NewGuid(),
            JournalEntryId = Id,
            AccountId = accountId,
            Description = description,
            DebitAmount = debit,
            CreditAmount = credit
        };

        Lines.Add(line);
        TotalDebits += debit;
        TotalCredits += credit;
    }

    public void Post(Guid postedBy)
    {
        if (Status != JournalEntryStatus.Draft)
            throw new InvalidOperationException("Only draft entries can be posted");

        if (Math.Abs(TotalDebits - TotalCredits) > 0.01m)
            throw new InvalidOperationException("Entry must balance before posting");

        if (Lines.Count == 0)
            throw new InvalidOperationException("Entry must have at least one line");

        Status = JournalEntryStatus.Posted;
        PostedAt = DateTime.UtcNow;
        PostedBy = postedBy;

        this.AddDomainEvent(new JournalEntryPostedEvent(Id, EntryNumber, TotalDebits));
    }

    public void Reverse(string reason, Guid reversedBy)
    {
        if (Status != JournalEntryStatus.Posted)
            throw new InvalidOperationException("Only posted entries can be reversed");

        Status = JournalEntryStatus.Reversed;
        ReversedAt = DateTime.UtcNow;
        ReversedBy = reversedBy;
        ReversalReason = reason;

        this.AddDomainEvent(new JournalEntryReversedEvent(Id, EntryNumber, reason));
    }
}

public class JournalLine
{
    public Guid Id { get; set; }
    public Guid JournalEntryId { get; set; }
    public Guid AccountId { get; set; }
    public string Description { get; set; } = "";
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
}

public enum JournalEntryStatus { Draft, Posted, Reversed }
```

## Application Layer

```csharp
// Core/Application/Features/Accounting/Commands/CreateJournalEntry/CreateJournalEntryCommand.cs
using MediatR;

namespace FinTech.Core.Application.Features.Accounting.Commands.CreateJournalEntry;

public record CreateJournalEntryCommand : IRequest<Result<CreateJournalEntryResponse>>
{
    public required string Description { get; init; }
    public required string Source { get; init; }
    public Guid? SourceId { get; init; }
    public required List<JournalLineDto> Lines { get; init; }
}

public record JournalLineDto
{
    public required Guid AccountId { get; init; }
    public required string Description { get; init; }
    public decimal DebitAmount { get; init; }
    public decimal CreditAmount { get; init; }
}

public record CreateJournalEntryResponse
{
    public Guid EntryId { get; init; }
    public string EntryNumber { get; init; }
    public string Message { get; init; }
}
```

```csharp
// Core/Application/Features/Accounting/Commands/CreateJournalEntry/CreateJournalEntryValidator.cs
using FluentValidation;

namespace FinTech.Core.Application.Features.Accounting.Commands.CreateJournalEntry;

public class CreateJournalEntryValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Source)
            .NotEmpty()
            .Must(s => new[] { "FixedAsset", "Loan", "Sales", "Purchase", "Depreciation", "Manual" }.Contains(s))
            .WithMessage("Invalid source");

        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithMessage("Entry must have at least one line")
            .Must(lines => lines.Count >= 2)
            .WithMessage("Entry must have at least 2 lines");

        RuleForEach(x => x.Lines)
            .Must(line => line.DebitAmount >= 0 && line.CreditAmount >= 0)
            .WithMessage("Amounts must be non-negative")
            .Must(line => !(line.DebitAmount > 0 && line.CreditAmount > 0))
            .WithMessage("A line cannot have both debit and credit");

        RuleFor(x => x.Lines)
            .Must(BalanceCheck)
            .WithMessage("Total debits must equal total credits");
    }

    private bool BalanceCheck(List<JournalLineDto> lines)
    {
        var totalDebits = lines.Sum(l => l.DebitAmount);
        var totalCredits = lines.Sum(l => l.CreditAmount);
        return Math.Abs(totalDebits - totalCredits) < 0.01m;
    }
}
```

```csharp
// Core/Application/Features/Accounting/Commands/CreateJournalEntry/CreateJournalEntryHandler.cs
public class CreateJournalEntryHandler : IRequestHandler<CreateJournalEntryCommand, Result<CreateJournalEntryResponse>>
{
    private readonly IJournalEntryRepository _entryRepository;
    private readonly ILedgerAccountRepository _accountRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuditService _auditService;
    private readonly ILogger<CreateJournalEntryHandler> _logger;

    public async Task<Result<CreateJournalEntryResponse>> Handle(
        CreateJournalEntryCommand command,
        CancellationToken cancellationToken)
    {
        // Authorization
        if (!await _authorizationService.AuthorizeAsync("CreateJournalEntry"))
            return Result.Failure<CreateJournalEntryResponse>(
                new Error("Authorization.Denied", "You cannot create journal entries"));

        // Generate entry number
        var entryNumber = $"JE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var entry = JournalEntry.Create(entryNumber, command.Description, command.Source, command.SourceId);

        // Add lines and validate accounts exist
        foreach (var lineDto in command.Lines)
        {
            var account = await _accountRepository.GetByIdAsync(lineDto.AccountId, cancellationToken);
            if (account == null)
                return Result.Failure<CreateJournalEntryResponse>(
                    new Error("Account.NotFound", $"Account {lineDto.AccountId} not found"));

            entry.AddLine(lineDto.AccountId, lineDto.Description, lineDto.DebitAmount, lineDto.CreditAmount);
        }

        await _entryRepository.AddAsync(entry, cancellationToken);
        await _entryRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            "JournalEntry.Created",
            entry.Id,
            $"Created journal entry: {entryNumber}",
            cancellationToken);

        _logger.LogInformation("Journal entry created: {EntryNumber} ({Source})", entryNumber, command.Source);

        return Result.Success(new CreateJournalEntryResponse
        {
            EntryId = entry.Id,
            EntryNumber = entryNumber,
            Message = "Journal entry created successfully"
        });
    }
}
```

```csharp
// Core/Application/Features/Accounting/Commands/PostJournalEntry/PostJournalEntryCommand.cs
public record PostJournalEntryCommand(Guid EntryId) : IRequest<Result<Unit>>;

public class PostJournalEntryHandler : IRequestHandler<PostJournalEntryCommand, Result<Unit>>
{
    private readonly IJournalEntryRepository _entryRepository;
    private readonly ILedgerAccountRepository _accountRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuditService _auditService;

    public async Task<Result<Unit>> Handle(PostJournalEntryCommand command, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.AuthorizeAsync("PostJournalEntry"))
            return Result.Failure<Unit>(new Error("Authorization.Denied", "Cannot post entries"));

        var entry = await _entryRepository.GetByIdAsync(command.EntryId, cancellationToken);
        if (entry == null)
            return Result.Failure<Unit>(new Error("Entry.NotFound", "Entry not found"));

        var userId = _authorizationService.GetUserId();

        // Post to accounts
        foreach (var line in entry.Lines)
        {
            var account = await _accountRepository.GetByIdAsync(line.AccountId, cancellationToken);
            if (account == null)
                return Result.Failure<Unit>(new Error("Account.NotFound", "Account not found"));

            if (line.DebitAmount > 0)
                account.PostDebit(line.DebitAmount);
            else
                account.PostCredit(line.CreditAmount);

            await _accountRepository.UpdateAsync(account, cancellationToken);
        }

        entry.Post(userId);
        await _entryRepository.UpdateAsync(entry, cancellationToken);
        await _entryRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync(
            "JournalEntry.Posted",
            entry.Id,
            $"Posted journal entry: {entry.EntryNumber}",
            cancellationToken);

        return Result.Success(Unit.Value);
    }
}
```

```csharp
// Core/Application/Features/Accounting/Commands/ReverseJournalEntry/ReverseJournalEntryCommand.cs
public record ReverseJournalEntryCommand : IRequest<Result<Unit>>
{
    public required Guid EntryId { get; init; }
    public required string Reason { get; init; }
}

public class ReverseJournalEntryHandler : IRequestHandler<ReverseJournalEntryCommand, Result<Unit>>
{
    private readonly IJournalEntryRepository _entryRepository;
    private readonly ILedgerAccountRepository _accountRepository;
    private readonly IAuthorizationService _authorizationService;

    public async Task<Result<Unit>> Handle(ReverseJournalEntryCommand command, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.AuthorizeAsync("ReverseJournalEntry"))
            return Result.Failure<Unit>(new Error("Authorization.Denied", "Cannot reverse entries"));

        var entry = await _entryRepository.GetByIdAsync(command.EntryId, cancellationToken);
        if (entry == null)
            return Result.Failure<Unit>(new Error("Entry.NotFound", "Entry not found"));

        // Reverse account postings
        foreach (var line in entry.Lines)
        {
            var account = await _accountRepository.GetByIdAsync(line.AccountId, cancellationToken);
            if (account == null) continue;

            if (line.DebitAmount > 0)
                account.PostCredit(line.DebitAmount); // Reverse debit
            else
                account.PostDebit(line.CreditAmount); // Reverse credit

            await _accountRepository.UpdateAsync(account, cancellationToken);
        }

        var userId = _authorizationService.GetUserId();
        entry.Reverse(command.Reason, userId);
        await _entryRepository.UpdateAsync(entry, cancellationToken);
        await _entryRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}
```

## Queries - Balance Sheet, Trial Balance

```csharp
// Core/Application/Features/Accounting/Queries/GetTrialBalance/GetTrialBalanceQuery.cs
public record GetTrialBalanceQuery : IRequest<Result<TrialBalanceDto>>;

public record TrialBalanceDto
{
    public List<TrialBalanceLineDto> Lines { get; init; } = [];
    public decimal TotalDebits { get; init; }
    public decimal TotalCredits { get; init; }
    public DateTime AsOfDate { get; init; }
}

public record TrialBalanceLineDto
{
    public Guid AccountId { get; init; }
    public string AccountCode { get; init; }
    public string AccountName { get; init; }
    public AccountType AccountType { get; init; }
    public decimal DebitBalance { get; init; }
    public decimal CreditBalance { get; init; }
}

public class GetTrialBalanceHandler : IRequestHandler<GetTrialBalanceQuery, Result<TrialBalanceDto>>
{
    private readonly ILedgerAccountRepository _accountRepository;

    public async Task<Result<TrialBalanceDto>> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetAllAsync(cancellationToken);
        var lines = new List<TrialBalanceLineDto>();
        decimal totalDebits = 0;
        decimal totalCredits = 0;

        foreach (var account in accounts.Where(a => a.Status == AccountStatus.Active))
        {
            var debitBalance = account.AccountType switch
            {
                AccountType.Asset => account.Balance > 0 ? account.Balance : 0,
                AccountType.Expense => account.Balance > 0 ? account.Balance : 0,
                _ => 0
            };

            var creditBalance = account.AccountType switch
            {
                AccountType.Liability => account.Balance > 0 ? account.Balance : 0,
                AccountType.Equity => account.Balance > 0 ? account.Balance : 0,
                AccountType.Revenue => account.Balance > 0 ? account.Balance : 0,
                _ => 0
            };

            if (debitBalance != 0 || creditBalance != 0)
            {
                lines.Add(new TrialBalanceLineDto
                {
                    AccountId = account.Id,
                    AccountCode = account.AccountCode,
                    AccountName = account.AccountName,
                    AccountType = account.AccountType,
                    DebitBalance = debitBalance,
                    CreditBalance = creditBalance
                });

                totalDebits += debitBalance;
                totalCredits += creditBalance;
            }
        }

        return Result.Success(new TrialBalanceDto
        {
            Lines = lines.OrderBy(l => l.AccountCode).ToList(),
            TotalDebits = totalDebits,
            TotalCredits = totalCredits,
            AsOfDate = DateTime.UtcNow
        });
    }
}
```

## Presentation Layer

```csharp
// Controllers/AccountingController.cs
[ApiController]
[Route("api/v1/accounting")]
[Authorize(Policy = "CanViewAccounting")]
public class AccountingController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost("journal-entries")]
    [Authorize(Policy = "CanCreateJournalEntry")]
    public async Task<IActionResult> CreateJournalEntry([FromBody] CreateJournalEntryCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Description });

        return CreatedAtAction(nameof(GetJournalEntry), new { id = result.Value.EntryId }, result.Value);
    }

    [HttpPost("journal-entries/{id}/post")]
    [Authorize(Policy = "CanPostJournalEntry")]
    public async Task<IActionResult> PostJournalEntry(Guid id)
    {
        var result = await _mediator.Send(new PostJournalEntryCommand(id));
        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Description });

        return NoContent();
    }

    [HttpPost("journal-entries/{id}/reverse")]
    [Authorize(Policy = "CanReverseJournalEntry")]
    public async Task<IActionResult> ReverseJournalEntry(Guid id, [FromBody] ReverseRequest request)
    {
        var result = await _mediator.Send(new ReverseJournalEntryCommand { EntryId = id, Reason = request.Reason });
        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Description });

        return NoContent();
    }

    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalance()
    {
        var result = await _mediator.Send(new GetTrialBalanceQuery());
        if (result.IsFailure)
            return BadRequest(new { error = result.Error.Description });

        return Ok(result.Value);
    }
}

public record ReverseRequest(string Reason);
```

