import React, { useEffect } from 'react';
import {
  endOfMonth,
  startOfWeek,
  endOfWeek,
  eachDayOfInterval,
  isSameMonth,
  format,
} from 'date-fns';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { fetchSlotsForRange } from '../../store/slices/slotsSlice';
import DayCell from './DayCell';

const WEEK_DAYS = ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim'];

const CalendarGrid: React.FC = () => {
  const dispatch = useAppDispatch();
  const { currentMonth, currentYear } = useAppSelector((s) => s.calendar);
  const slotsByDate = useAppSelector((s) => s.slots.byDate);

  const monthStart = new Date(currentYear, currentMonth, 1);
  const monthEnd = endOfMonth(monthStart);

  // Semaine commence le lundi (weekStartsOn: 1)
  const gridStart = startOfWeek(monthStart, { weekStartsOn: 1 });
  const gridEnd = endOfWeek(monthEnd, { weekStartsOn: 1 });

  const days = eachDayOfInterval({ start: gridStart, end: gridEnd });

  useEffect(() => {
    const startDate = format(monthStart, 'yyyy-MM-dd');
    const endDate = format(monthEnd, 'yyyy-MM-dd');
    dispatch(fetchSlotsForRange({ startDate, endDate }));
  }, [currentMonth, currentYear]);

  return (
    <div className="calendar-grid">
      {/* En-têtes des jours */}
      {WEEK_DAYS.map((day) => (
        <div key={day} className="weekday-header" aria-hidden="true">
          {day}
        </div>
      ))}

      {/* Cellules de jours */}
      {days.map((day) => {
        const dateStr = format(day, 'yyyy-MM-dd');
        const slots = slotsByDate[dateStr] ?? [];
        return (
          <DayCell
            key={dateStr}
            date={day}
            slots={slots}
            isCurrentMonth={isSameMonth(day, monthStart)}
          />
        );
      })}
    </div>
  );
};

export default CalendarGrid;
