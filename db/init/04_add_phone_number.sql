-- ============================================================
-- 04_add_phone_number.sql
-- Ajout du champ phone_number à la table bookings
-- ============================================================

ALTER TABLE bookings
ADD COLUMN IF NOT EXISTS phone_number VARCHAR(20);
