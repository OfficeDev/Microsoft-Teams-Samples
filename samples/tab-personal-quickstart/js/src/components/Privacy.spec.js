import React from 'react';
import { render, screen } from '@testing-library/react';
import Privacy from './Privacy';

it('renders Privacy Statement', () => {
  render(<Privacy />);
  expect(screen.getByText('Privacy Statement')).toBeInTheDocument();
});