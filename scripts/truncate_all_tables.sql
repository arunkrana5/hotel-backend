-- ==============================================================================
-- HotelOS - TRUNCATE ALL TABLES & RESET DATA SCRIPT
-- Truncates all data in all schemas and restarts all identity sequences to 1.
-- ==============================================================================

DO $$
BEGIN
    RAISE NOTICE 'Truncating all domain and transactional tables across HotelOS schemas...';
END $$;

TRUNCATE TABLE 
    -- Audit Schema
    audit.audit_logs,

    -- Banquet Schema
    banquet.banquet_bookings,
    banquet.banquet_venues,

    -- Configuration Schema
    configuration.document_number_audits,
    configuration.document_number_configurations,
    configuration.dynamic_form_fields,
    configuration.dynamic_form_definitions,
    configuration.navigation_menus,

    -- Finance Schema
    finance.journal_lines,
    finance.journal_entries,
    finance.invoices,
    finance.accounts,

    -- Housekeeping Schema
    housekeeping.laundry_order_items,
    housekeeping.laundry_orders,
    housekeeping.housekeeping_tasks,
    housekeeping.linen_items,

    -- HR Schema
    hr.attendance_records,
    hr.employees,

    -- Identity Schema
    identity.email_verification_tokens,
    identity.login_attempts,
    identity.login_history,
    identity.password_history,
    identity.password_reset_tokens,
    identity.refresh_tokens,
    identity.user_property_assignments,
    identity.user_roles,
    identity.role_permissions,
    identity.user_sessions,
    identity.user_credentials,
    identity.users,
    identity.roles,
    identity.permissions,
    identity.security_events,

    -- Inventory Schema
    inventory.stock_transactions,
    inventory.inventory_items,

    -- Maintenance Schema
    maintenance.work_orders,
    maintenance.assets,

    -- Masters Schema
    masters.master_values,
    masters.master_definitions,
    masters.master_categories,

    -- Operations Schema
    operations.guest_service_requests,

    -- Platform Schema
    platform.notifications,

    -- PMS Schema
    pms.folio_items,
    pms.folios,
    pms.reservations,
    pms.guests,
    pms.rooms,

    -- Purchase Schema
    purchase.purchase_order_items,
    purchase.purchase_orders,
    purchase.purchase_requisition_items,
    purchase.purchase_requisitions,
    purchase.vendors,

    -- Restaurant Schema
    restaurant.restaurant_order_items,
    restaurant.restaurant_orders,
    restaurant.menu_items,
    restaurant.dining_tables,

    -- Security Schema
    security.incident_reports,
    security.visitor_logs,

    -- Workflow Schema
    workflow.workflow_histories,
    workflow.workflow_instances,
    workflow.workflow_transitions,
    workflow.workflow_steps,
    workflow.workflow_definitions
RESTART IDENTITY CASCADE;

DO $$
BEGIN
    RAISE NOTICE 'All HotelOS tables successfully truncated and sequences reset to 1.';
    RAISE NOTICE 'Run the backend service to automatically seed initial fresh data.';
END $$;
