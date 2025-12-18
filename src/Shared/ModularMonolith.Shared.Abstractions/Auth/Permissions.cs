namespace ModularMonolith.Shared.Abstractions.Auth;

/// <summary>
/// Defines all permissions in the system.
/// Format: {module}.{resource}.{action}
/// </summary>
public static class Permissions
{
    /// <summary>
    /// Staff/System permissions (requires staff realm).
    /// </summary>
    public static class System
    {
        public const string ViewAllTenants = "system.tenants.viewall";
        public const string ManageTenants = "system.tenants.manage";
        public const string ImpersonateUsers = "system.users.impersonate";
        public const string ViewSystemLogs = "system.logs.view";
        public const string ManageSystemSettings = "system.settings.manage";
    }

    /// <summary>
    /// Tenant management permissions.
    /// </summary>
    public static class Tenants
    {
        public const string View = "tenants.view";
        public const string Update = "tenants.update";
        public const string ManageSettings = "tenants.settings.manage";
        public const string ManageBilling = "tenants.billing.manage";
        public const string Delete = "tenants.delete";
    }

    /// <summary>
    /// User management permissions (within tenant).
    /// </summary>
    public static class Users
    {
        public const string View = "users.view";
        public const string Invite = "users.invite";
        public const string Update = "users.update";
        public const string Delete = "users.delete";
        public const string ManageRoles = "users.roles.manage";
        public const string ManagePermissions = "users.permissions.manage";
    }

    /// <summary>
    /// Role management permissions.
    /// </summary>
    public static class Roles
    {
        public const string View = "roles.view";
        public const string Create = "roles.create";
        public const string Update = "roles.update";
        public const string Delete = "roles.delete";
        public const string ManagePermissions = "roles.permissions.manage";
    }

    /// <summary>
    /// Customer management permissions.
    /// </summary>
    public static class Customers
    {
        public const string View = "customers.view";
        public const string Create = "customers.create";
        public const string Update = "customers.update";
        public const string Delete = "customers.delete";
        public const string Export = "customers.export";
        public const string Import = "customers.import";
    }

    /// <summary>
    /// Order management permissions.
    /// </summary>
    public static class Orders
    {
        public const string View = "orders.view";
        public const string Create = "orders.create";
        public const string Update = "orders.update";
        public const string Cancel = "orders.cancel";
        public const string Process = "orders.process";
        public const string Refund = "orders.refund";
        public const string Export = "orders.export";
    }

    /// <summary>
    /// Catalog management permissions.
    /// </summary>
    public static class Catalog
    {
        public const string View = "catalog.view";
        public const string Create = "catalog.create";
        public const string Update = "catalog.update";
        public const string Delete = "catalog.delete";
        public const string ManageInventory = "catalog.inventory.manage";
        public const string ManagePricing = "catalog.pricing.manage";
    }

    /// <summary>
    /// File/Storage permissions.
    /// </summary>
    public static class Files
    {
        public const string View = "files.view";
        public const string Upload = "files.upload";
        public const string Download = "files.download";
        public const string Delete = "files.delete";
        public const string Share = "files.share";
    }

    /// <summary>
    /// Notification permissions.
    /// </summary>
    public static class Notifications
    {
        public const string View = "notifications.view";
        public const string Send = "notifications.send";
        public const string ManageTemplates = "notifications.templates.manage";
        public const string ManageChannels = "notifications.channels.manage";
    }

    /// <summary>
    /// Reporting permissions.
    /// </summary>
    public static class Reports
    {
        public const string ViewSales = "reports.sales.view";
        public const string ViewFinancial = "reports.financial.view";
        public const string ViewInventory = "reports.inventory.view";
        public const string ViewCustomers = "reports.customers.view";
        public const string Export = "reports.export";
    }
}

/// <summary>
/// Defines built-in roles in the system.
/// </summary>
public static class Roles
{
    /// <summary>
    /// System-wide roles (staff realm).
    /// </summary>
    public static class Staff
    {
        public const string SystemAdmin = "SystemAdmin";
        public const string Support = "Support";
        public const string Developer = "Developer";
    }

    /// <summary>
    /// Tenant-scoped roles (tenant realm).
    /// </summary>
    public static class Tenant
    {
        public const string Owner = "TenantOwner";
        public const string Admin = "TenantAdmin";
        public const string Member = "TenantMember";

        // Business roles (examples - tenants can define custom ones)
        public const string SalesManager = "SalesManager";
        public const string InventoryManager = "InventoryManager";
        public const string SupportAgent = "SupportAgent";
        public const string Viewer = "Viewer";
    }
}
