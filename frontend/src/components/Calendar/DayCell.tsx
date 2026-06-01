import React from 'react';
import { format, isToday, isPast } from 'date-fns';
import { fr } from 'date-fns/locale';
import type { Slot } from '../../types';
import { useAppDispatch, useAppSelector } from '../../hooks';
import { selectDate } from '../../store/slices/calendarSlice';

interface Props {
  date: Date;
  slots: Slot[];
  isCurrentMonth: boolean;
}

const DayCell: React.FC<Props> = ({ date, slots, isCurrentMonth }) => {
  const dispatch = useAppDispatch();
  const selectedDate = useAppSelector((s) => s.calendar.selectedDate);

  const dateStr = format(date, 'yyyy-MM-dd');
  const isSelected = selectedDate === dateStr;
  const today = isToday(date);
  const past = isPast(date) && !today;

  const availableCount = slots.filter((s) => s.status === 'available').length;
  const bookedByMe = slots.some((s) => s.isBookedByMe);

  let dotClass = '';
  if (bookedByMe) dotClass = 'dot dot--booked';
  else if (availableCount > 0) dotClass = 'dot dot--available';
  else if (slots.length > 0) dotClass = 'dot dot--full';

  const classes = [
    'day-cell',
    !isCurrentMonth ? 'day-cell--other-month' : '',
    today ? 'day-cell--today' : '',
    isSelected ? 'day-cell--selected' : '',
    past ? 'day-cell--past' : '',
  ]
    .filter(Boolean)
    .join(' ');

  return (
    <button
      className={classes}
      onClick={() => dispatch(selectDate(dateStr))}
      aria-label={format(date, 'd MMMM yyyy', { locale: fr })}
      aria-pressed={isSelected}
    >
      <span className="day-number">{format(date, 'd')}</span>
      {dotClass && <span className={dotClass} aria-hidden="true" />}
      {availableCount > 0 && (
        <span className="slot-count">{availableCount}</span>
      )}
    </button>
  );
};

export default DayCell;
