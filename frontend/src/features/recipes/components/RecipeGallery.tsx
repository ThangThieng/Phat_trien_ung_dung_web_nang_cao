'use client';

import { useState } from 'react';
import Image from 'next/image';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import type { RecipeImage } from '@/types/recipe';

/** Ảnh chính 800×600 là LCP nên luôn `priority`; ảnh phụ mở bằng dialog. */
export default function RecipeGallery({ images, title }: { images: RecipeImage[]; title: string }) {
  const ordered = [...images].sort((a, b) => a.orderIndex - b.orderIndex);
  const primary = ordered.find((image) => image.isPrimary) ?? ordered[0];
  const [selected, setSelected] = useState<RecipeImage | null>(null);

  if (!primary) {
    return (
      <div
        aria-label={`Ảnh ${title}`}
        className="flex aspect-[4/3] items-center justify-center rounded-2xl bg-gradient-to-br from-orange-100 to-amber-50 text-8xl"
      >
        🍲
      </div>
    );
  }

  const imageUrl = (image: RecipeImage) => image.mediumUrl ?? image.originalUrl;
  return (
    <section aria-label={`Ảnh ${title}`}>
      <button
        type="button"
        onClick={() => setSelected(primary)}
        className="relative block aspect-[4/3] w-full overflow-hidden rounded-2xl bg-orange-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-orange-700"
      >
        <Image
          src={imageUrl(primary)}
          alt={primary.altText ?? title}
          fill
          priority
          unoptimized
          sizes="(max-width: 1024px) 100vw, 50vw"
          className="object-cover"
        />
      </button>
      {ordered.length > 1 && (
        <ul className="mt-3 flex gap-2 overflow-x-auto pb-1" aria-label="Ảnh khác">
          {ordered.map((image) => (
            <li key={image.id}>
              <button
                type="button"
                onClick={() => setSelected(image)}
                className="relative block size-16 overflow-hidden rounded-lg ring-offset-2 focus-visible:outline-2 focus-visible:outline-orange-700"
              >
                <Image
                  src={image.thumbnailUrl ?? imageUrl(image)}
                  alt={image.altText ?? title}
                  fill
                  unoptimized
                  sizes="64px"
                  className="object-cover"
                />
              </button>
            </li>
          ))}
        </ul>
      )}
      <Dialog open={selected !== null} onOpenChange={(open) => !open && setSelected(null)}>
        <DialogContent className="max-w-4xl p-2" aria-describedby={undefined}>
          <DialogTitle className="sr-only">{selected?.altText ?? title}</DialogTitle>
          {selected && (
            <div className="relative aspect-[4/3] overflow-hidden rounded-lg">
              <Image
                src={selected.originalUrl}
                alt={selected.altText ?? title}
                fill
                unoptimized
                sizes="(max-width: 1024px) 100vw, 900px"
                className="object-contain"
              />
            </div>
          )}
        </DialogContent>
      </Dialog>
    </section>
  );
}
