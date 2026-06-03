-- ============================================================
-- 02_seed_data.sql
-- Données de démonstration : créneaux pour juin 2026
-- ============================================================

-- Nettoyage préalable (idempotent)
TRUNCATE bookings, slots RESTART IDENTITY CASCADE;

-- ── Mardi 30 juin 2026 ────

INSERT INTO slots (date, start_time, end_time, title, description, capacity) VALUES
('2026-06-30', '13:20', '16:20', 'Préparatifs – 13h20-16h20', NULL, 8),
('2026-06-30', '16:45', '17:15', 'Stand – 16h45-17h15', NULL, 12),
('2026-06-30', '17:15', '17:45', 'Stand – 17h15-17h45', NULL, 12),
('2026-06-30', '17:45', '18:15', 'Stand – 17h45-18h15', NULL, 12),
('2026-06-30', '18:15', '18:45', 'Stand – 18h15-18h45', NULL, 12),
('2026-06-30', '18:45', '19:15', 'Rangement – 18h45-19h15', NULL, 8);