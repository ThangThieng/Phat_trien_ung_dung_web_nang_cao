import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { Dialog, DialogContent, DialogTitle, DialogTrigger } from './dialog';

it('closes on Escape and returns focus to its trigger', async () => {
  render(<Dialog><DialogTrigger>Open</DialogTrigger><DialogContent><DialogTitle>Dialog title</DialogTitle></DialogContent></Dialog>);
  const trigger = screen.getByRole('button', { name: 'Open' });
  fireEvent.click(trigger);
  expect(screen.getByText('Dialog title')).toBeInTheDocument();
  fireEvent.keyDown(document, { key: 'Escape' });
  expect(screen.queryByText('Dialog title')).not.toBeInTheDocument();
  await waitFor(() => expect(trigger).toHaveFocus());
});
