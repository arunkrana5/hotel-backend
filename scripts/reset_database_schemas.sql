-- ==============================================================================
-- HotelOS - COMPLETE SCHEMA PURGE SCRIPT
-- Drops all 18 custom application schemas and cascades all tables/views/indices.
-- ==============================================================================

DROP SCHEMA IF EXISTS identity CASCADE;
DROP SCHEMA IF EXISTS masters CASCADE;
DROP SCHEMA IF EXISTS configuration CASCADE;
DROP SCHEMA IF EXISTS workflow CASCADE;
DROP SCHEMA IF EXISTS audit CASCADE;
DROP SCHEMA IF EXISTS pms CASCADE;
DROP SCHEMA IF EXISTS housekeeping CASCADE;
DROP SCHEMA IF EXISTS restaurant CASCADE;
DROP SCHEMA IF EXISTS banquet CASCADE;
DROP SCHEMA IF EXISTS operations CASCADE;
DROP SCHEMA IF EXISTS purchase CASCADE;
DROP SCHEMA IF EXISTS inventory CASCADE;
DROP SCHEMA IF EXISTS finance CASCADE;
DROP SCHEMA IF EXISTS hr CASCADE;
DROP SCHEMA IF EXISTS maintenance CASCADE;
DROP SCHEMA IF EXISTS security CASCADE;
DROP SCHEMA IF EXISTS platform CASCADE;
DROP SCHEMA IF EXISTS public CASCADE;

CREATE SCHEMA public;

DO $$
BEGIN
    RAISE NOTICE 'All HotelOS database schemas purged successfully.';
    RAISE NOTICE 'Starting backend or applying EF Core migrations will recreate and re-seed all tables.';
END $$;
