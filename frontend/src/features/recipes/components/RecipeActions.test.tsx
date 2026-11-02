import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import RecipeActions from './RecipeActions';

const toastSuccess = jest.fn();
jest.mock('sonner', () => ({
  toast: { success: (...args: unknown[]) => toastSuccess(...args), error: jest.fn() },
}));

it('copies the current URL when Web Share is unavailable', async () => {
  const writeText = jest.fn().mockResolvedValue(undefined);
  Object.assign(navigator, { clipboard: { writeText }, share: undefined });
  render(<RecipeActions title="Bún chả" description="Mô tả" />);
  fireEvent.click(screen.getByRole('button', { name: 'Chia sẻ công thức' }));
  await waitFor(() => expect(writeText).toHaveBeenCalledWith(window.location.href));
  expect(toastSuccess).toHaveBeenCalledWith('Đã sao chép liên kết');
});
