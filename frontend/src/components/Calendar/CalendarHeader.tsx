import React from 'react';
import { format, addMonths, subMonths } from 'date-fns';
import { fr } from 'date-fns/locale';
import { useAppDispatch, useAppSelector } from '../../hooks';
import {
  goToPreviousMonth,
  goToNextMonth,
  goToToday,
} from '../../store/slices/calendarSlice';

const CalendarHeader: React.FC = () => {
  const dispatch = useAppDispatch();
  const { currentMonth, currentYear } = useAppSelector((s) => s.calendar);

  const currentDate = new Date(currentYear, currentMonth, 1);
  const label = format(currentDate, 'MMMM yyyy', { locale: fr });
  const prevLabel = format(subMonths(currentDate, 1), 'MMM', { locale: fr });
  const nextLabel = format(addMonths(currentDate, 1), 'MMM', { locale: fr });

  return (
    <header className="calendar-header">
      <button
        className="nav-btn"
        onClick={() => dispatch(goToPreviousMonth())}
        aria-label={`Mois précédent : ${prevLabel}`}
      >
        ‹ {prevLabel}
      </button>

      <div className="header-center">
        <h2 className="month-title">{label}</h2>
        <button className="today-btn" onClick={() => dispatch(goToToday())}>
          Aujourd'hui
        </button>
      </div>

      <button
        className="nav-btn"
        onClick={() => dispatch(goToNextMonth())}
        aria-label={`Mois suivant : ${nextLabel}`}
      >
        {nextLabel} ›
      </button>
    </header>
  );
};

export default CalendarHeader;
