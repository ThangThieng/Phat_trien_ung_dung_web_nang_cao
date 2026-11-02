import { render, screen } from '@testing-library/react';
import { RecipeStatusBadge } from './RecipeStatusBadge';

it.each(['Draft', 'Published', 'Archived'] as const)('renders %s status', (status) => {
  render(<RecipeStatusBadge status={status} />);
  expect(screen.getByText(status)).toBeInTheDocument();
});
