-- Add extended fields to db_customers table
-- These fields are used for customer profiling and deduplication

ALTER TABLE db_customers
ADD COLUMN IF NOT EXISTS job_title TEXT,
ADD COLUMN IF NOT EXISTS marital_status TEXT,
ADD COLUMN IF NOT EXISTS proof_number TEXT,
ADD COLUMN IF NOT EXISTS db_price DECIMAL(18, 2),
ADD COLUMN IF NOT EXISTS headquarters TEXT,
ADD COLUMN IF NOT EXISTS insurance_name TEXT,
ADD COLUMN IF NOT EXISTS car_join_date TIMESTAMPTZ;

-- Add indexes for frequently queried fields
CREATE INDEX IF NOT EXISTS idx_db_customers_job_title ON db_customers(job_title);
CREATE INDEX IF NOT EXISTS idx_db_customers_headquarters ON db_customers(headquarters);
CREATE INDEX IF NOT EXISTS idx_db_customers_insurance_name ON db_customers(insurance_name);

-- Add comment for documentation
COMMENT ON COLUMN db_customers.job_title IS 'Customer job title/occupation';
COMMENT ON COLUMN db_customers.marital_status IS 'Customer marital status';
COMMENT ON COLUMN db_customers.proof_number IS 'Proof or verification number';
COMMENT ON COLUMN db_customers.db_price IS 'Database pricing information';
COMMENT ON COLUMN db_customers.headquarters IS 'Customer headquarters location';
COMMENT ON COLUMN db_customers.insurance_name IS 'Insurance provider name';
COMMENT ON COLUMN db_customers.car_join_date IS 'Car registration or join date';
