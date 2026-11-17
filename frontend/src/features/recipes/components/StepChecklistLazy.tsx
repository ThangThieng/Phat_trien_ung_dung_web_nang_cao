'use client';

import dynamic from 'next/dynamic';

const StepChecklist = dynamic(() => import('./StepChecklist'), { ssr: false });

/** Checklist là tương tác phía trình duyệt, tải sau phần nội dung chi tiết. */
export default StepChecklist;
