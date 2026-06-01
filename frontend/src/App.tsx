import React from 'react';
import CalendarHeader from './components/Calendar/CalendarHeader';
import CalendarGrid from './components/Calendar/CalendarGrid';
import SlotList from './components/Slots/SlotList';
import BookingModal from './components/Slots/BookingModal';
import './App.css';

const App: React.FC = () => {
  return (
    <div className="app">
      <header className="app-header">
        <h1 className="app-title">📅 Réservation de créneaux</h1>
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
