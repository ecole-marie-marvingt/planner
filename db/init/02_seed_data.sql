-- ============================================================
-- 02_seed_data.sql
-- Données de démonstration : créneaux pour juin 2026
-- ============================================================

-- Nettoyage préalable (idempotent)
TRUNCATE bookings, slots RESTART IDENTITY CASCADE;

-- ── Semaine du 1er au 7 juin 2026 ───────────────────────────

INSERT INTO slots (date, start_time, end_time, title, description, capacity) VALUES
-- Lundi 1 juin
('2026-06-01', '09:00', '10:00', 'Cours de natation – débutants',
 'Apprentissage des bases : flottaison, respiration, crawl.', 12),
('2026-06-01', '11:00', '12:00', 'Cours de natation – intermédiaires',
 'Perfectionnement du crawl et introduction à la brasse.', 10),
('2026-06-01', '14:00', '15:30', 'Aquagym',
 'Séance tonique en musique, tous niveaux.', 20),

-- Mardi 2 juin
('2026-06-02', '09:30', '10:30', 'Plongeon – initiation', NULL, 8),
('2026-06-02', '14:00', '15:00', 'Cours de natation – avancés',
 'Optimisation des techniques de nage et virages.', 8),

-- Mercredi 3 juin
('2026-06-03', '10:00', '11:00', 'Cours de natation – débutants',
 'Apprentissage des bases : flottaison, respiration, crawl.', 12),
('2026-06-03', '13:00', '14:00', 'Baby natation (3-5 ans)',
 'Initiation à l\'eau pour les tout-petits, avec les parents.', 6),
('2026-06-03', '15:00', '16:00', 'Natation synchronisée – initiation', NULL, 10),

-- Jeudi 4 juin
('2026-06-04', '09:00', '10:00', 'Cours de natation – débutants',
 'Apprentissage des bases : flottaison, respiration, crawl.', 12),
('2026-06-04', '11:00', '12:30', 'Aquagym',
 'Séance tonique en musique, tous niveaux.', 20),

-- Vendredi 5 juin
('2026-06-05', '09:30', '10:30', 'Cours de natation – intermédiaires', NULL, 10),
('2026-06-05', '14:00', '15:00', 'Plongeon – perfectionnement', NULL, 6),

-- Samedi 6 juin
('2026-06-06', '09:00', '10:30', 'Cours collectif week-end – tous niveaux',
 'Créneau familial en petit groupe.', 16),
('2026-06-06', '11:00', '12:00', 'Natation libre surveillée', NULL, 30),

-- ── Semaine suivante (exemple) ─────────────────────────────
('2026-06-08', '09:00', '10:00', 'Cours de natation – débutants',
 'Apprentissage des bases : flottaison, respiration, crawl.', 12),
('2026-06-08', '14:00', '15:30', 'Aquagym',
 'Séance tonique en musique, tous niveaux.', 20),
('2026-06-09', '09:30', '10:30', 'Plongeon – initiation', NULL, 8),
('2026-06-10', '10:00', '11:00', 'Cours de natation – débutants', NULL, 12),
('2026-06-10', '13:00', '14:00', 'Baby natation (3-5 ans)', NULL, 6),
('2026-06-11', '09:00', '10:00', 'Cours de natation – avancés', NULL, 8),
('2026-06-12', '09:30', '10:30', 'Cours de natation – intermédiaires', NULL, 10),
('2026-06-13', '09:00', '10:30', 'Cours collectif week-end – tous niveaux', NULL, 16);

-- ── Quelques réservations de démonstration ──────────────────

-- Récupère les IDs des créneaux du 1er juin matin pour les réserver
DO $$
DECLARE
    v_slot_id UUID;
BEGIN
    -- Réservation slot 09h00 du 1er juin
    SELECT id INTO v_slot_id FROM slots
    WHERE date = '2026-06-01' AND start_time = '09:00' LIMIT 1;

    IF v_slot_id IS NOT NULL THEN
        INSERT INTO bookings (slot_id, user_name, email) VALUES
            (v_slot_id, 'Marie Dupont',   'marie.dupont@exemple.fr'),
            (v_slot_id, 'Paul Martin',    'paul.martin@exemple.fr'),
            (v_slot_id, 'Sophie Leroy',   'sophie.leroy@exemple.fr');
    END IF;

    -- Réservation slot aquagym du 1er juin (complet pour la démo)
    SELECT id INTO v_slot_id FROM slots
    WHERE date = '2026-06-01' AND start_time = '14:00' LIMIT 1;

    IF v_slot_id IS NOT NULL THEN
        -- Remplir à la capacité (20 places)
        INSERT INTO bookings (slot_id, user_name, email)
        SELECT v_slot_id,
               'Participant ' || gs,
               'participant' || gs || '@exemple.fr'
        FROM generate_series(1, 20) gs
        ON CONFLICT DO NOTHING;
    END IF;
END;
$$;
