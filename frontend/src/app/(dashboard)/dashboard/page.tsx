'use client';

import { useQueries } from '@tanstack/react-query';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { getMyRecipes } from '@/features/recipes/my-recipes-api';
import type { RecipeStatus } from '@/types/recipe';

const statuses: RecipeStatus[] = ['Draft', 'Published', 'Archived'];

/** Dashboard landing route. Authentication UX is provided by the route-group layout's AuthGuard. */
export default function DashboardPage() {
  const results = useQueries({ queries: statuses.map((status) => ({ queryKey: ['my-recipes', status, 1], queryFn: () => getMyRecipes({ status, pageSize: 1 }), staleTime: 0 })) });
  return (
    <section className="mx-auto max-w-5xl px-4 py-10">
      <h1 className="text-2xl font-bold text-gray-900">Bảng điều khiển</h1>
      <p className="mt-2 text-gray-600">Quản lý nội dung và phiên đăng nhập của bạn.</p>
      <div className="mt-6 grid gap-4 sm:grid-cols-3">
        {statuses.map((status, index) => <Card key={status}><CardHeader><CardTitle>{status}</CardTitle></CardHeader><CardContent>{results[index].isLoading ? <Skeleton className="h-8 w-16" /> : <p className="text-3xl font-bold">{results[index].data?.totalCount ?? 0}</p>}</CardContent></Card>)}
      </div>
    </section>
  );
}
