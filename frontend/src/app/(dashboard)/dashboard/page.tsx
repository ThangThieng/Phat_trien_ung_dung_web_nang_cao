'use client';

import Link from 'next/link';
import { useQueries } from '@tanstack/react-query';
import { BookOpen, PenSquare } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { RECIPE_STATUS_LABEL } from '@/features/recipes/format';
import { getMyRecipes } from '@/features/recipes/my-recipes-api';
import { recipeKeys } from '@/features/recipes/query-keys';
import type { RecipeStatus } from '@/types/recipe';

const statuses: RecipeStatus[] = ['Draft', 'Published', 'Archived'];

/** Dashboard landing route. Authentication UX is provided by the route-group layout's AuthGuard. */
export default function DashboardPage() {
  const results = useQueries({
    queries: statuses.map((status) => ({
      queryKey: recipeKeys.mine({ status, pageSize: 1 }),
      queryFn: () => getMyRecipes({ status, pageSize: 1 }),
      staleTime: 0,
    })),
  });
  const isEmpty = results.every(
    (result) => result.isSuccess && (result.data?.totalCount ?? 0) === 0,
  );
  return (
    <section className="mx-auto max-w-5xl px-4 py-10">
      <h1 className="text-ink text-2xl font-bold">Bảng điều khiển</h1>
      <p className="text-ink-soft mt-2">
        Quản lý công thức của bạn. Xem công thức của mọi người ở trang chủ như khi chưa đăng nhập.
      </p>
      <div className="mt-4 flex flex-wrap gap-3">
        <Link
          href="/dashboard/recipes/new"
          className="bg-navy hover:bg-navy-2 inline-flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-semibold text-white"
        >
          <PenSquare aria-hidden className="h-4 w-4" />
          {isEmpty ? 'Viết công thức đầu tiên' : 'Viết công thức'}
        </Link>
        <Link
          href="/recipes"
          className="border-line-strong text-ink hover:bg-gold-soft inline-flex items-center gap-2 rounded-lg border px-4 py-2 text-sm font-semibold"
        >
          <BookOpen aria-hidden className="h-4 w-4" />
          Xem công thức của mọi người
        </Link>
      </div>
      {isEmpty && (
        <p className="border-line bg-gold-soft text-ink mt-4 rounded-lg border px-4 py-3 text-sm">
          Bạn chưa có công thức nào. Công thức bạn viết sẽ hiện ở đây theo trạng thái: bản nháp, đã
          xuất bản, đã lưu trữ.
        </p>
      )}
      <div className="mt-6 grid gap-4 sm:grid-cols-3">
        {statuses.map((status, index) => (
          <Card key={status}>
            <CardHeader>
              <CardTitle>{RECIPE_STATUS_LABEL[status]}</CardTitle>
            </CardHeader>
            <CardContent>
              {results[index].isLoading ? (
                <Skeleton className="h-8 w-16" />
              ) : (
                <p className="text-3xl font-bold">{results[index].data?.totalCount ?? 0}</p>
              )}
            </CardContent>
          </Card>
        ))}
      </div>
    </section>
  );
}
