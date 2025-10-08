-- Update existing sample data with correct status values
-- Status values must match DbStatus enum: New, InProgress, NoAnswer, Completed, OnHold

DELETE FROM db_customers WHERE contact_id BETWEEN 1001 AND 1015;
