import React from 'react';
import type { Slot } from '../../types';
import { useAppDispatch } from '../../hooks';
import { openBookingModal } from '../../store/slices/bookingFormSlice';
import { cancelBooking } from '../../store/slices/slotsSlice';

interface Props {
  slot: Slot;
}

const statusLabel: Record<Slot['status'], string> = {
  available: 'Disponible',
  full: 'Complet',
};

const SlotCard: React.FC<Props> = ({ slot }) => {
  const dispatch = useAppDispatch();

  const handleCancel = () => {
    if (slot.bookingId) {
      dispatch(cancelBooking({ slotId: slot.id, bookingId: slot.bookingId }));
    }
  };

  return (
    <article className={`slot-card slot-card--${slot.status}`}>
      <div className="slot-card__time">
        {slot.startTime.slice(0, 5)} – {slot.endTime.slice(0, 5)}
      </div>
      <div className="slot-card__info">
        <h3 className="slot-card__title">{slot.title}</h3>
        {slot.description && (
          <p className="slot-card__desc">{slot.description}</p>
        )}
        <div className="slot-card__meta">
          <span
            className={`slot-badge slot-badge--${slot.status}`}
            aria-label={`Statut : ${statusLabel[slot.status]}`}
          >
            {statusLabel[slot.status]}
          </span>
          <span className="slot-capacity">
            {slot.capacity - slot.bookedCount}/{slot.capacity} place
            {slot.capacity > 1 ? 's' : ''}
          </span>
        </div>
      </div>

      <div className="slot-card__actions">
        {slot.isBookedByMe ? (
          <button
            className="btn btn--danger"
            onClick={handleCancel}
            aria-label={`Annuler ma réservation pour ${slot.title}`}
          >
            Annuler
          </button>
        ) : slot.status === 'available' ? (
          <button
            className="btn btn--primary"
            onClick={() => dispatch(openBookingModal(slot))}
            aria-label={`Réserver ${slot.title}`}
          >
            Réserver
          </button>
        ) : (
          <button className="btn btn--disabled" disabled>
            Complet
          </button>
        )}
      </div>
    </article>
  );
};

export default SlotCard;
