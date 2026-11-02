import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import RecipeGallery from './RecipeGallery';

const image = {
  id: 'image-1',
  originalUrl: 'https://example.test/original.webp',
  mediumUrl: 'https://example.test/medium.webp',
  thumbnailUrl: 'https://example.test/thumb.webp',
  altText: 'Bún chả hoàn chỉnh',
  isPrimary: true,
  orderIndex: 0,
};

it('loads the lightbox on demand and keeps the Dialog Escape focus behavior', async () => {
  render(<RecipeGallery images={[image]} title="Bún chả" />);

  const opener = screen.getByRole('button', { name: 'Bún chả hoàn chỉnh' });
  fireEvent.click(opener);
  expect(await screen.findByRole('dialog')).toBeInTheDocument();

  fireEvent.keyDown(document, { key: 'Escape' });
  await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());
  expect(opener).toHaveFocus();
});
