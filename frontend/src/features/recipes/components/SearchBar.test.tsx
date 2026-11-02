import { act, fireEvent, render, screen } from '@testing-library/react';
import SearchBar from './SearchBar';

const mockRouter = { push: jest.fn(), replace: jest.fn() };
let mockPathname = '/search';
let mockSearchParams = new URLSearchParams();
jest.mock('next/navigation', () => ({
  useRouter: () => mockRouter,
  usePathname: () => mockPathname,
  useSearchParams: () => mockSearchParams,
}));

let replaceState: jest.SpyInstance;
beforeEach(() => {
  jest.useFakeTimers();
  mockRouter.push.mockReset();
  mockRouter.replace.mockReset();
  replaceState = jest.spyOn(window.history, 'replaceState');
});
afterEach(() => {
  replaceState.mockRestore();
  jest.useRealTimers();
});

describe('<SearchBar /> trên header', () => {
  it('ở /search: gõ xong 300 ms → history.replaceState (giữ bộ lọc, bỏ page), không render lại server', () => {
    mockPathname = '/search';
    mockSearchParams = new URLSearchParams('q=ga&maxCookTime=30&page=3');
    render(<SearchBar />);

    fireEvent.change(screen.getByRole('searchbox'), { target: { value: 'pho bo' } });
    act(() => {
      jest.advanceTimersByTime(300);
    });

    expect(replaceState).toHaveBeenLastCalledWith(null, '', '/search?q=pho+bo&maxCookTime=30');
    expect(mockRouter.replace).not.toHaveBeenCalled();
    expect(mockRouter.push).not.toHaveBeenCalled();
  });

  it('ở trang khác: không tự đổi URL khi gõ; Enter → router.push tới /search', () => {
    mockPathname = '/recipes';
    mockSearchParams = new URLSearchParams();
    render(<SearchBar />);

    const input = screen.getByRole('searchbox');
    fireEvent.change(input, { target: { value: 'chè' } });
    act(() => {
      jest.advanceTimersByTime(300);
    });
    expect(replaceState).not.toHaveBeenCalled();

    fireEvent.submit(input.closest('form')!);
    expect(mockRouter.push).toHaveBeenCalledWith('/search?q=ch%C3%A8');
  });
});
