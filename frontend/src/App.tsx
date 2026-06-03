import React from 'react';
import CalendarHeader from './components/Calendar/CalendarHeader';
import CalendarGrid from './components/Calendar/CalendarGrid';
import SlotList from './components/Slots/SlotList';
import BookingModal from './components/Slots/BookingModal';
import CancellationPage from './components/Slots/CancellationPage';
import './App.css';

// Détection du token d'annulation dans l'URL (?cancel=<token>)
const cancelToken = new URLSearchParams(window.location.search).get('cancel');

const App: React.FC = () => {
  if (cancelToken) {
    return <CancellationPage cancellationToken={cancelToken} />;
  }

  return (
    <div className="app">
      <header className="app-header">
        <h1 className="app-title">📅 Fête de l'école élémentaire</h1>
        <p className="app-subtitle">École Marie Marvingt</p>
      </header>

      <main className="app-main">
        <div className="calendar-panel">
          <CalendarHeader />
          <CalendarGrid />
        </div>

        <div className="slots-panel">
          <SlotList />
        </div>
      </main>

      <BookingModal />
    </div>
  );
};

export default App;
