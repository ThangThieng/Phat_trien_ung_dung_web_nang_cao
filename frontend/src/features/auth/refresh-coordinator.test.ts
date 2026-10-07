import { coordinateRefresh } from './refresh-coordinator';

describe('cross-tab refresh coordination', () => {
  const originalLocks = Object.getOwnPropertyDescriptor(navigator, 'locks');

  afterEach(() => {
    if (originalLocks) Object.defineProperty(navigator, 'locks', originalLocks);
    else Reflect.deleteProperty(navigator, 'locks');
  });

  function installLocks() {
    let queue = Promise.resolve();
    const request = jest.fn((_name: string, callback: () => Promise<unknown>) => {
      const result = queue.then(callback);
      queue = result.then(() => undefined, () => undefined);
      return result;
    });
    Object.defineProperty(navigator, 'locks', { configurable: true, value: { request } });
    return request;
  }

  it('serializes two runtimes and reads R2 only after the first persists it', async () => {
    const request = installLocks();
    let persisted = 'R';
    let releaseFirst: () => void = () => {};
    let firstStarted: () => void = () => {};
    const started = new Promise<void>((resolve) => { firstStarted = resolve; });
    const gate = new Promise<void>((resolve) => { releaseFirst = resolve; });
    const sent: string[] = [];
    const tabA = coordinateRefresh(() => persisted, async (token) => {
      sent.push(token);
      firstStarted();
      await gate;
      persisted = 'R2';
      return 'access-A';
    });
    const tabB = coordinateRefresh(() => persisted, async (token) => {
      sent.push(token);
      persisted = 'R3';
      return 'access-B';
    });
    await started;
    expect(sent).toEqual(['R']);
    releaseFirst();
    await expect(Promise.all([tabA, tabB])).resolves.toEqual(['access-A', 'access-B']);
    expect(sent).toEqual(['R', 'R2']);
    expect(request).toHaveBeenNthCalledWith(1, 'culinaryblog-auth-refresh', expect.any(Function));
    expect(request).toHaveBeenNthCalledWith(2, 'culinaryblog-auth-refresh', expect.any(Function));
  });

  it('does not send a stale token when logout removes storage while a tab waits', async () => {
    installLocks();
    let persisted: string | null = 'R';
    let release: () => void = () => {};
    const gate = new Promise<void>((resolve) => { release = resolve; });
    const holder = navigator.locks.request('culinaryblog-auth-refresh', () => gate);
    const rotate = jest.fn().mockResolvedValue('access');
    const waiting = coordinateRefresh(() => persisted, rotate);
    const rejected = expect(waiting).rejects.toThrow('No refresh token');
    persisted = null;
    release();
    await holder;
    await rejected;
    expect(rotate).not.toHaveBeenCalled();
    expect(persisted).toBeNull();
  });

  it('falls back gracefully when Web Locks is unavailable', async () => {
    Object.defineProperty(navigator, 'locks', { configurable: true, value: undefined });
    const rotate = jest.fn().mockResolvedValue('memory-access');
    await expect(coordinateRefresh(() => 'R', rotate)).resolves.toBe('memory-access');
    expect(rotate).toHaveBeenCalledWith('R');
  });
});
