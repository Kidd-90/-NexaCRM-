-- Create db_customers table for database customer management
CREATE TABLE IF NOT EXISTS db_customers (
  id BIGSERIAL PRIMARY KEY,
  contact_id INTEGER UNIQUE NOT NULL,
  customer_name TEXT,
  contact_number TEXT,
  "group" TEXT,
  assigned_to TEXT,
  assigner TEXT,
  assigned_date TIMESTAMPTZ,
  last_contact_date TIMESTAMPTZ,
  status TEXT,
  is_starred BOOLEAN DEFAULT FALSE,
  is_archived BOOLEAN DEFAULT FALSE,
  gender TEXT,
  address TEXT,
  region TEXT,
  status_detail TEXT,
  consultation_count INTEGER DEFAULT 0,
  purchase_amount DECIMAL(18, 2),
  notes TEXT,
  tags TEXT,
  referral_source TEXT,
  last_updated_by TEXT,
  created_at TIMESTAMPTZ DEFAULT NOW(),
  updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_db_customers_contact_id ON db_customers(contact_id);
CREATE INDEX IF NOT EXISTS idx_db_customers_assigned_to ON db_customers(assigned_to);
CREATE INDEX IF NOT EXISTS idx_db_customers_status ON db_customers(status);
CREATE INDEX IF NOT EXISTS idx_db_customers_is_archived ON db_customers(is_archived);
CREATE INDEX IF NOT EXISTS idx_db_customers_is_starred ON db_customers(is_starred);
CREATE INDEX IF NOT EXISTS idx_db_customers_assigned_date ON db_customers(assigned_date DESC);
CREATE INDEX IF NOT EXISTS idx_db_customers_last_contact_date ON db_customers(last_contact_date DESC);

-- Add comments for documentation
COMMENT ON TABLE db_customers IS 'Database customer management and tracking';
COMMENT ON COLUMN db_customers.contact_id IS 'Reference to the contacts table';
COMMENT ON COLUMN db_customers.assigned_to IS 'User assigned to manage this customer';
COMMENT ON COLUMN db_customers.status IS 'Current status of the customer (e.g., Active, Inactive, Prospect)';
COMMENT ON COLUMN db_customers.is_starred IS 'Flag for starred/important customers';
COMMENT ON COLUMN db_customers.is_archived IS 'Flag for archived customers';

-- Create trigger to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_db_customers_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trigger_db_customers_updated_at
  BEFORE UPDATE ON db_customers
  FOR EACH ROW
  EXECUTE FUNCTION update_db_customers_updated_at();
