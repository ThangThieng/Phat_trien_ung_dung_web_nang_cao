import { apiFetch } from '@/lib/api-client';
import { getRecipeBySlug } from './api';

jest.mock('@/lib/api-client', () => ({ apiFetch: jest.fn() }));

it('keeps recipe detail data cache aligned with the five-minute API TTL', () => {
  getRecipeBySlug('bun-cha');
  expect(apiFetch).toHaveBeenCalledWith('/recipes/bun-cha', {
    next: { revalidate: 300, tags: ['recipe:bun-cha'] },
  });
});
