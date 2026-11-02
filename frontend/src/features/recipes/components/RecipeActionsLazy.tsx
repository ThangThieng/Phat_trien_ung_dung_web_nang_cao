'use client';

import dynamic from 'next/dynamic';

const RecipeActions = dynamic(() => import('./RecipeActions'), { ssr: false });

/** Các nút in/chia sẻ chỉ cần tải sau khi trang chi tiết đã hiện nội dung chính. */
export default RecipeActions;
