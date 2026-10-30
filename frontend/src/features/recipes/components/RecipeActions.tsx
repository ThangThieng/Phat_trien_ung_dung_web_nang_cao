'use client';

import { Button } from '@/components/ui/button';

/** In bằng CSS hệ thống và chia sẻ progressive enhancement, không thêm SDK bên thứ ba. */
export default function RecipeActions({
  title,
  description,
}: {
  title: string;
  description: string;
}) {
  const share = async () => {
    const url = window.location.href;
    try {
      if (navigator.share) {
        await navigator.share({ title, text: description, url });
        return;
      }
      await navigator.clipboard.writeText(url);
      const { toast } = await import('sonner');
      toast.success('Đã sao chép liên kết');
    } catch (error) {
      if ((error as DOMException).name !== 'AbortError') {
        const { toast } = await import('sonner');
        toast.error('Không thể chia sẻ liên kết. Vui lòng thử lại.');
      }
    }
  };
  return (
    <div data-recipe-actions className="flex flex-wrap gap-2">
      <Button
        type="button"
        variant="outline"
        onClick={() => window.print()}
        aria-label="In công thức"
      >
        In công thức
      </Button>
      <Button type="button" variant="outline" onClick={share} aria-label="Chia sẻ công thức">
        Chia sẻ
      </Button>
    </div>
  );
}
