-- ============================================================
-- 02_seed_data.sql
-- Données de démonstration : créneaux pour juin 2026
-- ============================================================

-- Nettoyage préalable (idempotent)
TRUNCATE bookings, slots RESTART IDENTITY CASCADE;

-- ── Mardi 30 juin 2026 – Créneaux de 30 minutes entre 17h et 18h30 ────

INSERT INTO slots (date, start_time, end_time, title, description, capacity) VALUES
('2026-06-30', '17:00', '17:30', 'Fête de l''école élémentaire – 17h00-17h30', NULL, 10),
('2026-06-30', '17:30', '18:00', 'Fête de l''école élémentaire – 17h30-18h00', NULL, 10),
('2026-06-30', '18:00', '18:30', 'Fête de l''école élémentaire – 18h00-18h30', NULL, 10);