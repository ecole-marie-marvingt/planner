-- ============================================================
-- 01_create_tables.sql
-- Initialisation du schéma de la base planner
-- ============================================================

-- Extension pour les UUID
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ── Table slots ──────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS slots (
    id           UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    date         DATE        NOT NULL,
    start_time   TIME        NOT NULL,
    end_time     TIME        NOT NULL,
    title        VARCHAR(150) NOT NULL,
    description  TEXT,
    capacity     INT         NOT NULL CHECK (capacity > 0),
    created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_slots_date ON slots (date);

-- ── Table bookings ───────────────────────────────────────────
CREATE TABLE IF NOT EXISTS bookings (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    slot_id             UUID        NOT NULL REFERENCES slots(id) ON DELETE CASCADE,
    user_name           VARCHAR(200) NOT NULL,
    email               VARCHAR(320) NOT NULL,
    booked_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
    cancellation_token  UUID        NOT NULL DEFAULT gen_random_uuid(),
    CONSTRAINT uq_booking_slot_email       UNIQUE (slot_id, email),
    CONSTRAINT uq_booking_cancellation_token UNIQUE (cancellation_token)
);

CREATE INDEX IF NOT EXISTS idx_bookings_slot_id ON bookings (slot_id);
CREATE INDEX IF NOT EXISTS idx_bookings_email   ON bookings (email);

-- ── Trigger : mise à jour automatique de updated_at ──────────
CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER LANGUAGE plpgsql AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS trg_slots_updated_at ON slots;
CREATE TRIGGER trg_slots_updated_at
    BEFORE UPDATE ON slots
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();
