import { act, fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import type { Category } from '@/types/category';
import type { RecipeSearchHit, RecipeSearchResult } from '@/types/search';
import SearchResults from './SearchResults';

let mockSearchParams = new URLSearchParams();
jest.mock('next/navigation', () => ({
  useSearchParams: () => mockSearchParams,
  usePathname: () => '/search',
}));

const mockSearchRecipes = jest.fn();
jest.mock('@/features/recipes/api', () => {
  const actual = jest.requireActual('@/features/recipes/api');
  return { ...actual, searchRecipes: (...args: unknown[]) => mockSearchRecipes(...args) };
});

const MON_VIET = '11111111-1111-4111-8111-111111111111';
const MON_AU = '22222222-2222-4222-8222-222222222222';
const categories: Category[] = [
  { id: MON_VIET, name: 'Món Việt', slug: 'mon-viet', recipeCount: 30 } as Category,
  { id: MON_AU, name: 'Món Âu', slug: 'mon-au', recipeCount: 12 } as Category,
];

const hit = (n: number): RecipeSearchHit =>
  ({
    id: `00000000-0000-4000-8000-${String(n).padStart(12, '0')}`,
    title: `Gà món ${n}`,
    slug: `ga-mon-${n}`,
    description: 'Gà ta',
    prepTimeMinutes: 15,
    cookTimeMinutes: 30,
    servings: 4,
    difficulty: 'Easy',
    status: 'Published',
    primaryImageUrl: null,
    category: { id: MON_VIET, name: 'Món Việt', slug: 'mon-viet' },
    author: { id: 'a', displayName: 'Tác giả', avatarUrl: null },
    publishedAt: '2026-10-01T00:00:00Z',
    createdAt: '2026-10-01T00:00:00Z',
    totalTimeMinutes: 45,
    relevanceScore: 0.5,
  }) as RecipeSearchHit;

const page = (pageNumber: number, totalCount: number, count = 12): RecipeSearchResult => ({
  items: Array.from({ length: count }, (_, i) => hit((pageNumber - 1) * 12 + i + 1)),
  page: pageNumber,
  pageSize: 12,
  totalCount,
  totalPages: Math.ceil(totalCount / 12),
  hasNextPage: pageNumber * 12 < totalCount,
  hasPreviousPage: pageNumber > 1,
});

function renderAt(url: string, result: RecipeSearchResult = page(1, 57)) {
  mockSearchParams = new URL(url, 'http://localhost').searchParams;
  mockSearchRecipes.mockResolvedValue(result);
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  const ui = () => (
    <QueryClientProvider client={client}>
      <SearchResults categories={categories} />
    </QueryClientProvider>
  );
  const view = render(ui());
  return {
    ...view,
    rerenderAt: (next: string) => {
      mockSearchParams = new URL(next, 'http://localhost').searchParams;
      view.rerender(ui());
    },
  };
}

// jsdom không có matchMedia — giả lập màn hình < 1024 px (nút "Bộ lọc" mở Sheet).
beforeAll(() => {
  Object.defineProperty(window, 'matchMedia', {
    configurable: true,
    value: (query: string) => ({
      matches: false,
      media: query,
      addEventListener: () => undefined,
      removeEventListener: () => undefined,
    }),
  });
});

let pushState: jest.SpyInstance;
beforeEach(() => {
  mockSearchRecipes.mockReset();
  window.history.replaceState(null, '', '/search');
  pushState = jest.spyOn(window.history, 'pushState');
});
afterEach(() => pushState.mockRestore());

const lastPushedUrl = () => pushState.mock.calls.at(-1)?.[2];
const filters = () => screen.getByRole('complementary', { name: 'Bộ lọc công thức' });

describe('<SearchResults />', () => {
  it('gọi API với đủ 8 tham số đọc từ URL (CR-2026-05 / MT-66) và hiện "Hiển thị 13–24 trên 57 kết quả"', async () => {
    renderAt(
      `/search?q=ga&categoryId=${MON_VIET}&difficulty=Easy&maxCookTime=30&maxPrepTime=15&minServings=2&page=2`,
      page(2, 57),
    );

    expect(await screen.findByText('Hiển thị 13–24 trên 57 kết quả')).toBeInTheDocument();
    expect(screen.getByText('Tìm thấy 57 công thức cho “ga”.')).toBeInTheDocument();
    expect(mockSearchRecipes).toHaveBeenCalledTimes(1);
    expect(mockSearchRecipes.mock.calls[0][0]).toEqual({
      q: 'ga',
      page: 2,
      pageSize: 12,
      categoryId: MON_VIET,
      difficulty: 'Easy',
      maxCookTime: 30,
      maxPrepTime: 15,
      minServings: 2,
    });
  });

  it('đổi bộ lọc gọi history.pushState và về trang 1, giữ q cùng các bộ lọc khác', async () => {
    renderAt('/search?q=ga&difficulty=Easy&page=3', page(3, 57));
    await screen.findByText('Hiển thị 25–36 trên 57 kết quả');

    const cookTime = within(filters()).getByRole('group', { name: 'Thời gian nấu tối đa' });
    fireEvent.click(within(cookTime).getByRole('link', { name: '≤ 30 phút' }));
    expect(lastPushedUrl()).toBe('/search?q=ga&difficulty=Easy&maxCookTime=30');

    fireEvent.click(within(filters()).getByRole('link', { name: 'Chuyên gia' }));
    expect(lastPushedUrl()).toBe('/search?q=ga&difficulty=Expert');

    fireEvent.click(within(filters()).getByRole('link', { name: 'Từ 4 khẩu phần' }));
    expect(lastPushedUrl()).toBe('/search?q=ga&difficulty=Easy&minServings=4');
  });

  it('đổi danh mục trong Select → pushState có categoryId, về trang 1; "Tất cả danh mục" gỡ categoryId', async () => {
    renderAt(`/search?q=ga&page=2&categoryId=${MON_VIET}`, page(2, 57));
    await screen.findByText('Hiển thị 13–24 trên 57 kết quả');

    const select = within(filters()).getByRole('combobox', { name: 'Danh mục' });
    expect(select).toHaveValue(MON_VIET);
    fireEvent.change(select, { target: { value: MON_AU } });
    expect(lastPushedUrl()).toBe(`/search?q=ga&categoryId=${MON_AU}`);
    fireEvent.change(select, { target: { value: 'all' } });
    expect(lastPushedUrl()).toBe('/search?q=ga');
  });

  it('lựa chọn đang áp dụng có aria-current, mỗi nhóm có "Bất kỳ" để gỡ', async () => {
    renderAt('/search?q=ga&maxCookTime=60');
    await screen.findByText('Hiển thị 1–12 trên 57 kết quả');

    const cook = within(filters()).getByRole('group', { name: 'Thời gian nấu tối đa' });
    expect(within(cook).getByRole('link', { name: '≤ 60 phút' })).toHaveAttribute(
      'aria-current',
      'true',
    );
    expect(within(cook).getByRole('link', { name: 'Bất kỳ' })).toHaveAttribute(
      'href',
      '/search?q=ga',
    );
  });

  it('link phân trang (next/link của Pagination) bị chặn: pushState thay cho điều hướng của Next.js', async () => {
    renderAt('/search?q=ga&maxCookTime=30');
    await screen.findByText('Hiển thị 1–12 trên 57 kết quả');

    const nav = screen.getByRole('navigation', { name: 'Phân trang' });
    const next = within(nav).getByRole('link', { name: 'Trang 2' });
    const notPrevented = fireEvent.click(next);
    expect(notPrevented).toBe(false);
    expect(lastPushedUrl()).toBe('/search?q=ga&maxCookTime=30&page=2');
  });

  it('Ctrl+click (mở tab mới) không bị chặn', async () => {
    renderAt('/search?q=ga');
    await screen.findByText('Hiển thị 1–12 trên 57 kết quả');

    const notPrevented = fireEvent.click(within(filters()).getByRole('link', { name: 'Khó' }), {
      ctrlKey: true,
    });
    expect(notPrevented).toBe(true);
    expect(pushState).not.toHaveBeenCalled();
  });

  it('nhãn bộ lọc đang bật: gỡ từng bộ lọc hoặc xóa tất cả', async () => {
    renderAt(`/search?q=ga&categoryId=${MON_AU}&maxPrepTime=15`);
    await screen.findByText('Hiển thị 1–12 trên 57 kết quả');

    const chips = screen.getByRole('list', { name: 'Bộ lọc đang áp dụng' });
    fireEvent.click(within(chips).getByRole('link', { name: 'Gỡ bộ lọc Danh mục: Món Âu' }));
    expect(lastPushedUrl()).toBe('/search?q=ga&maxPrepTime=15');
    fireEvent.click(within(chips).getByRole('link', { name: 'Xóa bộ lọc' }));
    expect(lastPushedUrl()).toBe('/search?q=ga');
  });

  it('không có kết quả: liệt kê bộ lọc đang bật để gỡ và gợi ý danh mục nhiều công thức', async () => {
    renderAt('/search?q=ga&difficulty=Expert&maxCookTime=15', page(1, 0, 0));

    expect(
      await screen.findByRole('heading', { name: 'Không có kết quả phù hợp' }),
    ).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'Bỏ “Độ khó: Chuyên gia”' })).toHaveAttribute(
      'href',
      '/search?q=ga&maxCookTime=15',
    );
    expect(screen.getByRole('link', { name: 'Bỏ “Nấu ≤ 15 phút”' })).toHaveAttribute(
      'href',
      '/search?q=ga&difficulty=Expert',
    );
    const suggested = screen
      .getAllByRole('link', { name: /^Món (Việt|Âu)/ })
      .map((a) => a.getAttribute('href'));
    expect(suggested).toEqual(['/categories/mon-viet', '/categories/mon-au']);
    expect(screen.queryByRole('navigation', { name: 'Phân trang' })).not.toBeInTheDocument();
  });

  it('từ khóa dưới 2 ký tự: không gọi API (sẽ là 400), hiện gợi ý', () => {
    renderAt('/search?q=g');
    expect(screen.getByText('Từ khóa cần ít nhất 2 ký tự.')).toBeInTheDocument();
    expect(mockSearchRecipes).not.toHaveBeenCalled();
  });

  it('gửi form từ khóa → pushState giữ bộ lọc, về trang 1', async () => {
    renderAt('/search?q=ga&difficulty=Hard&page=2', page(2, 57));
    await screen.findByText('Hiển thị 13–24 trên 57 kết quả');

    const input = screen.getByRole('searchbox', { name: 'Từ khóa tìm kiếm' });
    fireEvent.change(input, { target: { value: '  phở bò ' } });
    fireEvent.submit(input.closest('form')!);
    expect(lastPushedUrl()).toBe('/search?q=ph%E1%BB%9F+b%C3%B2&difficulty=Hard');
  });

  it('URL đổi (pushState / back) → đúng một lần gọi API cho trạng thái mới, kết quả cũ hiện mờ trong lúc chờ', async () => {
    const view = renderAt('/search?q=ga');
    await screen.findByText('Hiển thị 1–12 trên 57 kết quả');

    let resolveNext: (value: RecipeSearchResult) => void = () => undefined;
    mockSearchRecipes.mockReturnValueOnce(
      new Promise<RecipeSearchResult>((resolve) => {
        resolveNext = resolve;
      }),
    );
    view.rerenderAt('/search?q=ga&maxCookTime=30');
    await waitFor(() => expect(mockSearchRecipes).toHaveBeenCalledTimes(2));
    expect(mockSearchRecipes.mock.calls[1][0]).toMatchObject({ q: 'ga', maxCookTime: 30, page: 1 });
    // keepPreviousData: trong lúc chờ vẫn là kết quả cũ, đánh dấu aria-busy và làm mờ.
    expect(screen.getByText('Hiển thị 1–12 trên 57 kết quả')).toBeInTheDocument();
    expect(
      screen.getByRole('heading', { name: 'Gà món 1' }).closest('[aria-busy]'),
    ).toHaveAttribute('aria-busy', 'true');

    await act(async () => resolveNext(page(1, 20)));
    expect(await screen.findByText('Hiển thị 1–12 trên 20 kết quả')).toBeInTheDocument();
  });

  it('dưới 1024 px: nút "Bộ lọc" có số bộ lọc đang bật, mở Sheet, đóng bằng Escape và trả focus', async () => {
    renderAt('/search?q=ga&difficulty=Easy&minServings=2');
    await screen.findByText('Hiển thị 1–12 trên 57 kết quả');

    const open = screen.getByRole('button', { name: /Bộ lọc/ });
    expect(within(open).getByLabelText('2 bộ lọc đang bật')).toHaveTextContent('2');
    open.focus();
    fireEvent.click(open);
    const dialog = screen.getByRole('dialog', { name: 'Bộ lọc' });
    expect(dialog).toContainElement(document.activeElement as HTMLElement);

    // Chọn trong drawer cũng đổi URL tại chỗ (React chuyển sự kiện qua portal về cây của SearchResults).
    const dialogCook = within(dialog).getByRole('group', { name: 'Thời gian nấu tối đa' });
    fireEvent.click(within(dialogCook).getByRole('link', { name: '≤ 120 phút' }));
    expect(lastPushedUrl()).toBe('/search?q=ga&difficulty=Easy&maxCookTime=120&minServings=2');
    expect(within(dialog).getByRole('button', { name: 'Xem 57 kết quả' })).toBeInTheDocument();

    fireEvent.keyDown(document, { key: 'Escape' });
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument();
    expect(open).toHaveFocus();
  });
});

describe('<SearchResults /> — back/forward trong 60 giây lấy từ cache', () => {
  it('quay lại URL đã xem: không gọi API lần nữa (staleTime 60 giây = TTL search:)', async () => {
    const view = renderAt('/search?q=ga');
    await screen.findByText('Hiển thị 1–12 trên 57 kết quả');
    mockSearchRecipes.mockResolvedValue(page(1, 20));
    view.rerenderAt('/search?q=ga&maxCookTime=30');
    await screen.findByText('Hiển thị 1–12 trên 20 kết quả');
    expect(mockSearchRecipes).toHaveBeenCalledTimes(2);

    view.rerenderAt('/search?q=ga');
    expect(await screen.findByText('Hiển thị 1–12 trên 57 kết quả')).toBeInTheDocument();
    expect(mockSearchRecipes).toHaveBeenCalledTimes(2);
  });
});
