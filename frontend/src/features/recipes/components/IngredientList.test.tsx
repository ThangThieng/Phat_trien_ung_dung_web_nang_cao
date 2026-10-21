import { fireEvent, render, screen } from '@testing-library/react';
import IngredientList from './IngredientList';

const ingredients = [
  {
    id: 'flour',
    name: 'Bột mì',
    quantity: 200,
    quantityText: null,
    unit: 'g',
    notes: null,
    orderIndex: 0,
  },
  {
    id: 'salt',
    name: 'Muối',
    quantity: null,
    quantityText: 'vừa đủ',
    unit: null,
    notes: null,
    orderIndex: 1,
  },
];

it('scales numerical quantities but preserves quantityText verbatim', () => {
  render(<IngredientList ingredients={ingredients} servings={2} />);
  fireEvent.change(screen.getByLabelText('Số khẩu phần'), { target: { value: '4' } });
  expect(screen.getByText('400 g')).toBeInTheDocument();
  expect(screen.getByText('vừa đủ')).toBeInTheDocument();
});

it('does not accept zero or negative servings', () => {
  render(<IngredientList ingredients={ingredients} servings={2} />);
  const servings = screen.getByLabelText('Số khẩu phần');
  fireEvent.change(servings, { target: { value: '0' } });
  expect(servings).toHaveValue(1);
  fireEvent.change(servings, { target: { value: '-3' } });
  expect(servings).toHaveValue(1);
});
