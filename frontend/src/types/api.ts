/** Kiểu dùng chung cho mọi miền – khớp DTO của Backend (.NET), JSON camelCase. Chủ file: Dev 4. */

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface UploadedFile {
  key: string;
  url: string;
  contentType: string;
  size: number;
}
