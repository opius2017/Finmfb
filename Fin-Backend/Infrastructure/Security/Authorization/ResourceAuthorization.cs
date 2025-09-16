using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace FinTech.Infrastructure.Security.Authorization
{
    public enum ResourceOperation
    {
        Create,
        Read,
        Update,
        Delete,
        Approve,
        Reject,
        Process
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class ResourceAuthorizationAttribute : AuthorizeAttribute
    {
        public ResourceAuthorizationAttribute(string resource, ResourceOperation operation)
        {
            Policy = $"{resource}_{operation}";
        }
    }

    public class ResourceAuthorizationHandler : AuthorizationHandler<ResourceAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            ResourceAuthorizationRequirement requirement)
        {
            // Get user claims
            var userRoleClaim = context.User.FindFirst(c => c.Type == "role");
            var userDepartmentClaim = context.User.FindFirst(c => c.Type == "department");
            var userPermissionsClaim = context.User.FindFirst(c => c.Type == "permissions");
            
            // Check if user has permission based on role hierarchy
            if (IsAuthorizedByRole(userRoleClaim?.Value, requirement))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            
            // Check if user has permission based on department
            if (IsAuthorizedByDepartment(userDepartmentClaim?.Value, requirement))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            
            // Check if user has explicit permission
            if (HasExplicitPermission(userPermissionsClaim?.Value, requirement))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            
            // Authorization failed
            return Task.CompletedTask;
        }
        
        private bool IsAuthorizedByRole(string role, ResourceAuthorizationRequirement requirement)
        {
            if (string.IsNullOrEmpty(role))
                return false;
                
            // Role-based authorization logic
            switch (role.ToLower())
            {
                case "administrator":
                    // Administrators have full access to all resources
                    return true;
                    
                case "manager":
                    // Managers have full access to all resources except system configuration
                    return requirement.Resource != "SystemConfiguration";
                    
                case "supervisor":
                    // Supervisors can perform all operations except delete on most resources
                    return requirement.Operation != ResourceOperation.Delete;
                    
                case "accountant":
                    // Accountants have specific permissions to financial resources
                    return IsFinancialResource(requirement.Resource) && 
                           (requirement.Operation == ResourceOperation.Read || 
                            requirement.Operation == ResourceOperation.Create);
                            
                case "teller":
                    // Tellers can only perform limited operations
                    return IsTellerResource(requirement.Resource) && 
                           (requirement.Operation == ResourceOperation.Read || 
                            requirement.Operation == ResourceOperation.Create);
                            
                default:
                    return false;
            }
        }
        
        private bool IsAuthorizedByDepartment(string department, ResourceAuthorizationRequirement requirement)
        {
            if (string.IsNullOrEmpty(department))
                return false;
                
            // Department-based authorization logic
            switch (department.ToLower())
            {
                case "finance":
                    return IsFinancialResource(requirement.Resource);
                    
                case "operations":
                    return IsOperationalResource(requirement.Resource);
                    
                case "it":
                    return IsTechnicalResource(requirement.Resource);
                    
                case "hr":
                    return IsHrResource(requirement.Resource);
                    
                default:
                    return false;
            }
        }
        
        private bool HasExplicitPermission(string permissions, ResourceAuthorizationRequirement requirement)
        {
            if (string.IsNullOrEmpty(permissions))
                return false;
                
            // Check if the user has the specific permission string
            string requiredPermission = $"{requirement.Resource}_{requirement.Operation}";
            return permissions.Contains(requiredPermission);
        }
        
        private bool IsFinancialResource(string resource)
        {
            return resource == "BankAccount" || 
                   resource == "Loan" || 
                   resource == "JournalEntry" || 
                   resource == "FixedAsset" ||
                   resource == "ChartOfAccount";
        }
        
        private bool IsOperationalResource(string resource)
        {
            return resource == "Customer" || 
                   resource == "Transaction" || 
                   resource == "BranchOperation";
        }
        
        private bool IsTechnicalResource(string resource)
        {
            return resource == "SystemConfiguration" || 
                   resource == "UserManagement" || 
                   resource == "SecuritySettings";
        }
        
        private bool IsHrResource(string resource)
        {
            return resource == "Employee" || 
                   resource == "Payroll" || 
                   resource == "StaffLoan";
        }
        
        private bool IsTellerResource(string resource)
        {
            return resource == "CustomerAccount" || 
                   resource == "Transaction" || 
                   resource == "CashDeposit" ||
                   resource == "CashWithdrawal";
        }
    }

    public class ResourceAuthorizationRequirement : IAuthorizationRequirement
    {
        public string Resource { get; }
        public ResourceOperation Operation { get; }
        
        public ResourceAuthorizationRequirement(string resource, ResourceOperation operation)
        {
            Resource = resource;
            Operation = operation;
        }
    }
}