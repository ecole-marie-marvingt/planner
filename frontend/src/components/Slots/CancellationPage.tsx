import React, { useEffect, useState } from 'react';
import { slotsApi } from '../../api/slotsApi';
import Spinner from '../common/Spinner';

interface Props {
  cancellationToken: string;
}

type Status = 'loading' | 'success' | 'already-cancelled' | 'error';

const CancellationPage: React.FC<Props> = ({ cancellationToken }) => {
  const [status, setStatus] = useState<Status>('loading');
  const [slotTitle, setSlotTitle] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    slotsApi
      .cancelByToken(cancellationToken)
      .then((res) => {
        if (!cancelled) {
          setSlotTitle(res.slot?.title ?? null);
          setStatus('success');
          // Nettoyer l'URL pour éviter une double-annulation au rechargement
          window.history.replaceState({}, document.title, window.location.pathname);
        }
      })
      .catch((err: Error) => {
        if (!cancelled) {
          const msg = err.message ?? '';
          setStatus(msg.includes('introuvable') ? 'already-cancelled' : 'error');
        }
      });

    return () => {
      cancelled = true;
    };
  }, [cancellationToken]);

  return (
    <div className="cancellation-page">
      <header className="app-header">
        <h1 className="app-title">📅 Réservation de créneaux</h1>
        <p className="app-subtitle">École Marie Marvingt</p>
      </header>

      <main className="cancellation-page__main">
        <div className="cancellation-page__card">
          {status === 'loading' && (
            <>
              <Spinner size={32} label="Annulation en cours…" />
              <p>Annulation en cours…</p>
            </>
          )}

          {status === 'success' && (
            <>
              <div className="cancellation-page__icon">✅</div>
              <h2>Réservation annulée</h2>
              {slotTitle && (
                <p>
                  Votre inscription au créneau <strong>{slotTitle}</strong> a bien été annulée.
                </p>
              )}
              <p className="cancellation-page__sub">
                Vous pouvez vous réinscrire à tout moment depuis le calendrier.
              </p>
              <a href={window.location.pathname} className="btn btn--primary">
                Retour au calendrier
              </a>
            </>
          )}

          {status === 'already-cancelled' && (
            <>
              <div className="cancellation-page__icon">ℹ️</div>
              <h2>Réservation déjà annulée</h2>
              <p>Cette réservation a déjà été annulée ou n'existe pas.</p>
              <a href={window.location.pathname} className="btn btn--secondary">
                Retour au calendrier
              </a>
            </>
          )}

          {status === 'error' && (
            <>
              <div className="cancellation-page__icon">❌</div>
              <h2>Une erreur est survenue</h2>
              <p>Impossible d'annuler la réservation. Veuillez réessayer ultérieurement.</p>
              <button className="btn btn--secondary" onClick={() => window.location.reload()}>
                Réessayer
              </button>
            </>
          )}
        </div>
      </main>
    </div>
  );
};

export default CancellationPage;
