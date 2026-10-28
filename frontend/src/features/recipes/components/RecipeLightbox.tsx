'use client';

import Image from 'next/image';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import type { RecipeImage } from '@/types/recipe';

type RecipeLightboxProps = {
  image: RecipeImage | null;
  title: string;
  opener: HTMLButtonElement | null;
  onOpenChange: (open: boolean) => void;
};

/** Chỉ tải sau thao tác mở ảnh để Radix Dialog không nằm trong First Load JS. */
export default function RecipeLightbox({ image, title, opener, onOpenChange }: RecipeLightboxProps) {
  return (
    <Dialog open={image !== null} onOpenChange={onOpenChange}>
      <DialogContent
        className="max-w-4xl p-2"
        aria-describedby={undefined}
        onCloseAutoFocus={(event) => {
          event.preventDefault();
          opener?.focus();
        }}
      >
        <DialogTitle className="sr-only">{image?.altText ?? title}</DialogTitle>
        {image && (
          <div className="relative aspect-[4/3] overflow-hidden rounded-lg">
            <Image
              src={image.originalUrl}
              alt={image.altText ?? title}
              fill
              unoptimized
              sizes="(max-width: 1024px) 100vw, 900px"
              className="object-contain"
            />
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
