import React from 'react';
import { format, parseISO } from 'date-fns';
import { fr } from 'date-fns/locale';
import { useAppSelector } from '../../hooks';
import SlotCard from './SlotCard';
import Spinner from '../common/Spinner';

const SlotList: React.FC = () => {
  const selectedDate = useAppSelector((s) => s.calendar.selectedDate);
  const slotsByDate = useAppSelector((s) => s.slots.byDate);
  const loadingDates = useAppSelector((s) => s.slots.loadingDates);
  const errorByDate = useAppSelector((s) => s.slots.errorByDate);

  if (!selectedDate) {
    return (
      <div className="slot-list slot-list--empty">
        <p>Sélectionnez un jour pour voir les créneaux disponibles.</p>
      </div>
    );
  }

  const isLoading = loadingDates.includes(selectedDate);
  const error = errorByDate[selectedDate];
  const slots = slotsByDate[selectedDate] ?? [];

  const dateLabel = format(parseISO(selectedDate), 'EEEE d MMMM yyyy', {
    locale: fr,
  });

  return (
    <section className="slot-list" aria-label={`Créneaux du ${dateLabel}`}>
      <h2 className="slot-list__title">{dateLabel}</h2>

      {isLoading && (
        <div className="slot-list__loading">
          <Spinner size={32} />
          <span>Chargement des créneaux…</span>
        </div>
      )}

      {!isLoading && error && (
        <p className="slot-list__error" role="alert">
          {error}
        </p>
      )}

      {!isLoading && !error && slots.length === 0 && (
        <p className="slot-list__empty">Aucun créneau pour cette journée.</p>
      )}

      {!isLoading && !error && slots.length > 0 && (
        <ul className="slot-list__items">
          {slots
            .slice()
            .sort((a, b) => a.startTime.localeCompare(b.startTime))
            .map((slot) => (
              <li key={slot.id}>
                <SlotCard slot={slot} />
              </li>
            ))}
        </ul>
      )}
    </section>
  );
};

export default SlotList;
