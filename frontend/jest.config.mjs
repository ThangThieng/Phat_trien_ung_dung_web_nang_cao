import nextJest from 'next/jest.js';

const createJestConfig = nextJest({ dir: './' });

/** @type {import('jest').Config} */
const config = {
  // jsdom + Fetch API của Node — `ky` (lib/api-client) dựng `Request`/`Response` thật.
  testEnvironment: '<rootDir>/test-utils/jsdom-fetch-environment.js',
  setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],
  moduleNameMapper: { '^@/(.*)$': '<rootDir>/src/$1' },
  testPathIgnorePatterns: ['<rootDir>/e2e/', '<rootDir>/.next/'],
  modulePathIgnorePatterns: ['<rootDir>/.next/'],
  passWithNoTests: true,
};

/** Gói ESM thuần trong node_modules phải được SWC biên dịch như code của dự án. */
const ESM_PACKAGES = ['ky'];

export default async () => {
  const resolved = await createJestConfig(config)();
  // next/jest luôn chèn '/node_modules/(?!.pnpm)(?!(geist)/)'. Jest bỏ qua file khớp BẤT KỲ mẫu nào,
  // nên chỉ thêm ngoại lệ là chưa đủ – phải thay hẳn mẫu node_modules của next/jest.
  return {
    ...resolved,
    transformIgnorePatterns: [
      `/node_modules/(?!(${ESM_PACKAGES.join('|')})/)`,
      ...resolved.transformIgnorePatterns.filter((pattern) => !pattern.startsWith('/node_modules/')),
    ],
  };
};
