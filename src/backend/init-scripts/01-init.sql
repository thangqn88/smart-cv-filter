-- Initialize Smart CV Filter Database
-- This script runs when the PostgreSQL container starts

-- Create development database
CREATE DATABASE "SmartCVFilterDB_Dev";

-- Create production database
CREATE DATABASE "SmartCVFilterDB_Prod";

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE "SmartCVFilterDB" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "SmartCVFilterDB_Dev" TO postgres;
GRANT ALL PRIVILEGES ON DATABASE "SmartCVFilterDB_Prod" TO postgres;

