import { render, screen } from '@testing-library/react';
import { RecipeStatusBadge } from './RecipeStatusBadge';

it.each([
  ['Draft', 'Bản nháp'],
  ['Published', 'Đã xuất bản'],
  ['Archived', 'Đã lưu trữ'],
] as const)('renders %s status in Vietnamese', (status, label) => {
  render(<RecipeStatusBadge status={status} />);
  expect(screen.getByText(label)).toBeInTheDocument();
  expect(screen.queryByText(status)).not.toBeInTheDocument();
});
