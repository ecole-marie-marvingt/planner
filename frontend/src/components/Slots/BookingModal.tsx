import React, { useEffect, useRef } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import {
  closeBookingModal,
  setUserName,
  setEmail,
  setPhoneNumber,
} from '../../store/slices/bookingFormSlice';
import { bookSlot } from '../../store/slices/slotsSlice';
import Spinner from '../common/Spinner';

const BookingModal: React.FC = () => {
  const dispatch = useAppDispatch();
  const { isOpen, selectedSlot, userName, email, phoneNumber, isSubmitting, error, successMessage } =
    useAppSelector((s) => s.bookingForm);

  const dialogRef = useRef<HTMLDialogElement>(null);
  const firstInputRef = useRef<HTMLInputElement>(null);

  // Ouvre / ferme le dialog natif
  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) return;
    if (isOpen && !dialog.open) {
      dialog.showModal();
      firstInputRef.current?.focus();
    } else if (!isOpen && dialog.open) {
      dialog.close();
    }
  }, [isOpen]);

  if (!isOpen || !selectedSlot) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    dispatch(bookSlot({ slotId: selectedSlot.id, payload: { userName, email, phoneNumber } }));
  };

  const handleClose = () => dispatch(closeBookingModal());

  // Ferme la modale si clic sur le backdrop
  const handleBackdropClick = (e: React.MouseEvent<HTMLDialogElement>) => {
    if (e.target === dialogRef.current) handleClose();
  };

  return (
    <dialog
      ref={dialogRef}
      className="booking-modal"
      onClose={handleClose}
      onClick={handleBackdropClick}
      aria-labelledby="modal-title"
    >
      <div className="booking-modal__content">
        <button
          className="booking-modal__close"
          onClick={handleClose}
          aria-label="Fermer"
          type="button"
        >
          ✕
        </button>

        <h2 id="modal-title" className="booking-modal__title">
          {successMessage ? '✅ Confirmation' : 'Réserver un créneau'}
        </h2>

        {!successMessage ? (
          <>
            <div className="booking-modal__slot-info">
              <strong>{selectedSlot.title}</strong>
              <span>
                {selectedSlot.date} · {selectedSlot.startTime}–{selectedSlot.endTime}
              </span>
            </div>

            <form onSubmit={handleSubmit} className="booking-form" noValidate>
              <label className="form-field">
                <span>Nom complet</span>
                <input
                  ref={firstInputRef}
                  type="text"
                  value={userName}
                  onChange={(e) => dispatch(setUserName(e.target.value))}
                  required
                  minLength={2}
                  placeholder="Marie Dupont"
                  disabled={isSubmitting}
                />
              </label>

              <label className="form-field">
                <span>Adresse e-mail</span>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => dispatch(setEmail(e.target.value))}
                  required
                  placeholder="marie@exemple.fr"
                  disabled={isSubmitting}
                />
              </label>

              <label className="form-field">
                <span>Numéro de téléphone</span>
                <input
                  type="tel"
                  value={phoneNumber}
                  onChange={(e) => dispatch(setPhoneNumber(e.target.value))}
                  required
                  placeholder="+33 6 12 34 56 78"
                  disabled={isSubmitting}
                />
              </label>

              {error && (
                <p className="form-error" role="alert">
                  {error}
                </p>
              )}

              <div className="booking-form__actions">
                <button
                  type="button"
                  className="btn btn--secondary"
                  onClick={handleClose}
                  disabled={isSubmitting}
                >
                  Annuler
                </button>
                <button
                  type="submit"
                  className="btn btn--primary"
                  disabled={isSubmitting || !userName || !email || !phoneNumber}
                >
                  {isSubmitting ? <Spinner size={16} label="Envoi…" /> : 'Confirmer'}
                </button>
              </div>
            </form>
          </>
        ) : (
          <div className="booking-modal__success">
            <p>{successMessage}</p>
            <div className="booking-modal__slot-info">
              <strong>{selectedSlot.title}</strong>
              <span>
                {selectedSlot.date} · {selectedSlot.startTime}–{selectedSlot.endTime}
              </span>
            </div>
            <button className="btn btn--primary" onClick={handleClose}>
              Fermer
            </button>
          </div>
        )}
      </div>
    </dialog>
  );
};

export default BookingModal;
