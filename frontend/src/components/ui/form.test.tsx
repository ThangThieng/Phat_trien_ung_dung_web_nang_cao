import { useEffect } from 'react';
import { render, screen } from '@testing-library/react';
import { useForm } from 'react-hook-form';
import { Input } from './input';
import { Form, FormControl, FormField, FormItem, FormMessage } from './form';

function Harness() {
  const form = useForm<{ name: string }>();
  useEffect(() => { form.setError('name', { message: 'Required' }); }, [form]);
  return <Form {...form}><FormField control={form.control} name="name" render={({ field }) => <FormItem><FormControl><Input aria-label="Name" {...field} /></FormControl><FormMessage /></FormItem>} /></Form>;
}

it('links a field error with aria-invalid and aria-describedby', async () => {
  render(<Harness />);
  const input = await screen.findByRole('textbox', { name: 'Name' });
  expect(input).toHaveAttribute('aria-invalid', 'true');
  expect(input).toHaveAttribute('aria-describedby');
});
