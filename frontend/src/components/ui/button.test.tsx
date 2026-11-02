import { render, screen } from '@testing-library/react';
import { Button } from './button';

it('disables and marks a loading button busy', () => {
  render(<Button isLoading>Save</Button>);
  expect(screen.getByRole('button', { name: /save/i })).toBeDisabled();
  expect(screen.getByRole('button', { name: /save/i })).toHaveAttribute('aria-busy', 'true');
});
