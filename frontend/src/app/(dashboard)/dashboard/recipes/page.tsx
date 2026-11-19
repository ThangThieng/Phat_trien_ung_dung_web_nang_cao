'use client';

import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { FilePenLine, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import Pagination from '@/components/shared/Pagination';
import { useAuth } from '@/features/auth/auth-context';
import RecipeLifecycleActions from '@/features/recipes/components/RecipeLifecycleActions';
import { getMyRecipes } from '@/features/recipes/my-recipes-api';
import { RecipeStatusBadge } from '@/features/recipes/components/RecipeStatusBadge';
import { recipeKeys } from '@/features/recipes/query-keys';
import { RECIPE_STATUSES, type RecipeStatus } from '@/types/recipe';
import { formatDate } from '@/features/recipes/format';

const PAGE_SIZE = 10;
const STATUS_LABEL: Record<RecipeStatus, string> = {
  Draft: 'Bản nháp',
  Published: 'Đã xuất bản',
  Archived: 'Đã lưu trữ',
};
const TABS: { label: string; status?: RecipeStatus }[] = [
  { label: 'Tất cả' },
  ...RECIPE_STATUSES.map((status) => ({ status, label: STATUS_LABEL[status] })),
];

function isStatus(value: string | null): value is RecipeStatus {
  return !!value && (RECIPE_STATUSES as readonly string[]).includes(value);
}

/** FR-RCP-011 — danh sách riêng tư: chỉ gọi GET /recipes/mine. */
export default function MyRecipesPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const rawStatus = searchParams.get('status');
  const status = isStatus(rawStatus) ? rawStatus : undefined;
  const rawPage = Number(searchParams.get('page'));
  const page = Number.isInteger(rawPage) && rawPage > 0 ? rawPage : 1;
  const params = { ...(status ? { status } : {}), page, pageSize: PAGE_SIZE };
  const { accessToken } = useAuth();
  const recipes = useQuery({
    queryKey: recipeKeys.mine(params),
    queryFn: () => getMyRecipes(params),
    placeholderData: keepPreviousData,
    staleTime: 0,
  });

  const hrefFor = (nextPage: number, nextStatus = status) => {
    const query = new URLSearchParams();
    if (nextStatus) query.set('status', nextStatus);
    if (nextPage > 1) query.set('page', String(nextPage));
    const text = query.toString();
    return `/dashboard/recipes${text ? `?${text}` : ''}`;
  };

  if (recipes.isError)
    return (
      <section className="mx-auto max-w-6xl px-4 py-10" role="alert">
        <h1 className="text-2xl font-bold">Không thể tải công thức</h1>
        <p className="text-ink-soft mt-2">Vui lòng thử lại sau.</p>
      </section>
    );
  const { data } = recipes;
  return (
    <section className="mx-auto max-w-6xl px-4 py-10">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-ink text-2xl font-bold">Công thức của tôi</h1>
          <p className="text-ink-soft mt-1 text-sm">
            Quản lý các bản nháp và công thức đã xuất bản.
          </p>
        </div>
        <Button asChild>
          <Link href="/dashboard/recipes/new">
            <Plus aria-hidden />
            Viết công thức
          </Link>
        </Button>
      </div>
      <div
        role="tablist"
        aria-label="Trạng thái công thức"
        className="border-line mt-6 flex gap-1 overflow-x-auto border-b"
      >
        {TABS.map((tab) => {
          const selected = tab.status === status || (!tab.status && !status);
          return (
            <button
              key={tab.label}
              type="button"
              role="tab"
              aria-selected={selected}
              onClick={() => router.replace(hrefFor(1, tab.status))}
              className={`shrink-0 border-b-2 px-3 py-2 text-sm font-medium ${selected ? 'border-gold-ink text-gold-ink' : 'text-ink-soft hover:text-ink border-transparent'}`}
            >
              {tab.label}
            </button>
          );
        })}
      </div>
      {recipes.isLoading && <p className="text-ink-soft py-12 text-center">Đang tải công thức…</p>}
      {!recipes.isLoading && data?.items.length ? (
        <>
          <div className="border-line bg-paper mt-6 rounded-xl border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Tiêu đề</TableHead>
                  <TableHead>Trạng thái</TableHead>
                  <TableHead>Danh mục</TableHead>
                  <TableHead>Ngày tạo</TableHead>
                  <TableHead>
                    <span className="sr-only">Thao tác</span>
                  </TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.items.map((recipe) => (
                  <TableRow key={recipe.id}>
                    <TableCell className="font-medium">
                      <Link
                        href={`/dashboard/recipes/${recipe.id}/edit`}
                        className="hover:text-gold-ink hover:underline"
                      >
                        {recipe.title}
                      </Link>
                    </TableCell>
                    <TableCell>
                      <RecipeStatusBadge status={recipe.status} />
                    </TableCell>
                    <TableCell>{recipe.category.name}</TableCell>
                    <TableCell>{formatDate(recipe.createdAt)}</TableCell>
                    <TableCell className="space-y-1 text-right">
                      <Button asChild size="sm" variant="ghost">
                        <Link
                          href={`/dashboard/recipes/${recipe.id}/edit`}
                          aria-label={`Sửa ${recipe.title}`}
                        >
                          <FilePenLine aria-hidden />
                          Sửa
                        </Link>
                      </Button>
                      {accessToken && (
                        <RecipeLifecycleActions recipe={recipe} accessToken={accessToken} compact />
                      )}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            hasNextPage={data.hasNextPage}
            hasPreviousPage={data.hasPreviousPage}
            buildHref={hrefFor}
          />
        </>
      ) : (
        !recipes.isLoading && (
          <div className="border-line-strong bg-paper mt-8 rounded-xl border border-dashed p-10 text-center">
            <h2 className="text-ink text-lg font-semibold">Bạn chưa có công thức nào</h2>
            <p className="text-ink-soft mt-2 text-sm">
              Hãy bắt đầu lưu lại món ăn yêu thích của bạn.
            </p>
            <Button asChild className="mt-5">
              <Link href="/dashboard/recipes/new">
                <Plus aria-hidden />
                Viết công thức đầu tiên
              </Link>
            </Button>
          </div>
        )
      )}
    </section>
  );
}
