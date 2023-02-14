import React from 'react';
import { render, screen } from '@testing-library/react';
import TermsOfUse from '../TermsOfUse';

it('renders Terms of Use', () => {
  render(<TermsOfUse />);
  expect(screen.getByText('Terms of Use')).toBeInTheDocument();
});
