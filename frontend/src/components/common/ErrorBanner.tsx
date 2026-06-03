import React, { useEffect, useState } from 'react';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { clearError } from '../../store/slices/errorSlice';
import './ErrorBanner.css';

const ErrorBanner: React.FC = () => {
  const dispatch = useAppDispatch();
  const { message, id } = useAppSelector((state) => state.error);
  const [isVisible, setIsVisible] = useState(false);

  // Montre la bannière quand un message d'erreur apparaît
  useEffect(() => {
    if (message) {
      setIsVisible(true);
      const timer = setTimeout(() => {
        setIsVisible(false);
        dispatch(clearError());
      }, 6000);
      return () => clearTimeout(timer);
    }
  }, [id, message, dispatch]);

  if (!isVisible || !message) return null;

  return (
    <div
      className="error-banner"
      role="alert"
      aria-live="assertive"
      aria-atomic="true"
    >
      <div className="error-banner__content">
        <span className="error-banner__icon">⚠️</span>
        <span className="error-banner__message">{message}</span>
      </div>
      <button
        className="error-banner__close"
        onClick={() => {
          setIsVisible(false);
          dispatch(clearError());
        }}
        aria-label="Fermer le message d'erreur"
        type="button"
      >
        ✕
      </button>
    </div>
  );
};

export default ErrorBanner;
