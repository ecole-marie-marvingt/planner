-- ============================================================
-- 03_add_cancellation_token.sql
-- Ajout d'un token d'annulation unique par réservation
-- ============================================================

ALTER TABLE bookings
    ADD COLUMN IF NOT EXISTS cancellation_token UUID NOT NULL DEFAULT gen_random_uuid();

CREATE UNIQUE INDEX IF NOT EXISTS idx_bookings_cancellation_token
    ON bookings (cancellation_token);
