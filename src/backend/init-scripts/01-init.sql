-- Initialize Smart CV Filter Database
-- This script runs when the PostgreSQL container starts
-- Create development database
CREATE DATABASE "smart_cv_filter_db";
-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE "smart_cv_filter_db" TO postgres;