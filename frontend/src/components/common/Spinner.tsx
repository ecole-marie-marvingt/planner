import React from 'react';

interface Props {
  size?: number;
  label?: string;
}

const Spinner: React.FC<Props> = ({ size = 24, label = 'Chargement…' }) => (
  <span
    role="status"
    aria-label={label}
    className="spinner"
    style={{ width: size, height: size }}
  />
);

export default Spinner;
