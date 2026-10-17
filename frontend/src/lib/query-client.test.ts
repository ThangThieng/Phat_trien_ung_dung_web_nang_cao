import { toast } from 'sonner';
import { ApiError } from './api-error';
import { makeQueryClient, shouldRetryQuery } from './query-client';

jest.mock('sonner', () => ({ toast: { error: jest.fn(), warning: jest.fn() } }));

const toastError = toast.error as jest.Mock;

const forbidden = () => new ApiError(403, { type: 'RECIPE_FORBIDDEN', status: 403 });

beforeEach(() => toastError.mockClear());

describe('shouldRetryQuery', () => {
  it('retries network and 5xx failures at most twice', () => {
    const network = new TypeError('Failed to fetch');
    const server = new ApiError(503, { type: 'FILE_STORAGE_UNAVAILABLE', status: 503 });
    expect(shouldRetryQuery(0, network)).toBe(true);
    expect(shouldRetryQuery(1, server)).toBe(true);
    expect(shouldRetryQuery(2, server)).toBe(false);
  });

  it.each([400, 401, 403, 404, 409, 429])('never retries %s', (status) => {
    expect(shouldRetryQuery(0, new ApiError(status, { status }))).toBe(false);
  });
});

describe('global error handling through QueryCache and MutationCache', () => {
  it('toasts a failed query', async () => {
    const client = makeQueryClient();
    await client.prefetchQuery({
      queryKey: ['a'],
      queryFn: () => Promise.reject(forbidden()),
      retry: false,
    });
    expect(toastError).toHaveBeenCalledTimes(1);
  });

  it('stays quiet for a query that declares suppressGlobalError', async () => {
    const client = makeQueryClient();
    await client.prefetchQuery({
      queryKey: ['b'],
      queryFn: () => Promise.reject(forbidden()),
      retry: false,
      meta: { suppressGlobalError: true },
    });
    expect(toastError).not.toHaveBeenCalled();
  });

  it('toasts a failed mutation without its own onError', async () => {
    const client = makeQueryClient();
    const mutation = client
      .getMutationCache()
      .build(client, { mutationFn: () => Promise.reject(forbidden()) });
    await expect(mutation.execute(undefined)).rejects.toBeInstanceOf(ApiError);
    expect(toastError).toHaveBeenCalledTimes(1);
  });

  it('leaves a mutation with its own onError (or suppressGlobalError) to handle the error itself', async () => {
    const client = makeQueryClient();
    const onError = jest.fn();
    const own = client
      .getMutationCache()
      .build(client, { mutationFn: () => Promise.reject(forbidden()), onError });
    const suppressed = client.getMutationCache().build(client, {
      mutationFn: () => Promise.reject(forbidden()),
      meta: { suppressGlobalError: true },
    });
    await expect(own.execute(undefined)).rejects.toBeInstanceOf(ApiError);
    await expect(suppressed.execute(undefined)).rejects.toBeInstanceOf(ApiError);
    expect(onError).toHaveBeenCalledTimes(1);
    expect(toastError).not.toHaveBeenCalled();
  });
});
