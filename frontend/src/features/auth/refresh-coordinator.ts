const REFRESH_LOCK_NAME = 'culinaryblog-auth-refresh';

/** Serialize rotation across same-origin tabs, reading the token only after acquiring the lock. */
export async function coordinateRefresh<T>(
  readLatestToken: () => string | null,
  rotateAndPersist: (token: string) => Promise<T>,
): Promise<T> {
  const run = () => {
    const token = readLatestToken();
    if (!token) throw new Error('No refresh token');
    return rotateAndPersist(token);
  };
  if (typeof navigator !== 'undefined' && navigator.locks?.request) {
    return navigator.locks.request(REFRESH_LOCK_NAME, run);
  }
  return run();
}
