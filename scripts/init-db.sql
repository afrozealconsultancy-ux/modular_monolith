-- Initialize database for Modular Monolith
-- Create schemas for each module

-- Keycloak database (if not using separate database)
CREATE DATABASE IF NOT EXISTS keycloak;

-- Create schemas for modules
CREATE SCHEMA IF NOT EXISTS identity;
CREATE SCHEMA IF NOT EXISTS storage;
CREATE SCHEMA IF NOT EXISTS notifications;
CREATE SCHEMA IF NOT EXISTS customers;
CREATE SCHEMA IF NOT EXISTS orders;
CREATE SCHEMA IF NOT EXISTS catalog;
CREATE SCHEMA IF NOT EXISTS outbox;

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm"; -- For text search

-- Grant permissions
GRANT ALL PRIVILEGES ON SCHEMA identity TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA storage TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA notifications TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA customers TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA orders TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA catalog TO postgres;
GRANT ALL PRIVILEGES ON SCHEMA outbox TO postgres;

-- Create outbox table (shared across modules)
CREATE TABLE IF NOT EXISTS outbox.outbox_messages (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    type VARCHAR(500) NOT NULL,
    content TEXT NOT NULL,
    occurred_on TIMESTAMP NOT NULL DEFAULT NOW(),
    processed_on TIMESTAMP NULL,
    error TEXT NULL,
    retry_count INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_outbox_messages_processed ON outbox.outbox_messages(processed_on) WHERE processed_on IS NULL;
CREATE INDEX IF NOT EXISTS idx_outbox_messages_occurred ON outbox.outbox_messages(occurred_on);

-- Create inbox table (for idempotent event processing)
CREATE TABLE IF NOT EXISTS outbox.inbox_messages (
    id UUID PRIMARY KEY,
    processed_on TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_inbox_messages_id ON outbox.inbox_messages(id);

COMMENT ON SCHEMA identity IS 'Identity and authentication module';
COMMENT ON SCHEMA storage IS 'File storage and virus scanning module';
COMMENT ON SCHEMA notifications IS 'Multi-channel notification module';
COMMENT ON SCHEMA customers IS 'Customer management module';
COMMENT ON SCHEMA orders IS 'Order management module';
COMMENT ON SCHEMA catalog IS 'Product catalog module';
COMMENT ON SCHEMA outbox IS 'Outbox pattern for reliable messaging';
