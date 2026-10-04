'use client';

import { GoogleLogin } from '@react-oauth/google';
import { toast } from 'sonner';
import { ApiError, getErrorMessage } from '@/lib/api-client';
import { useAuth } from '../auth-context';

interface GoogleSignInProps {
  onSuccess: () => void;
}

export default function GoogleSignIn({ onSuccess }: GoogleSignInProps) {
  const { loginWithGoogle } = useAuth();
  const clientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;

  if (!clientId) return null;

  return (
    <div className="flex flex-col gap-2">
      <div className="flex items-center gap-3 text-xs text-gray-500">
        <span className="h-px flex-1 bg-gray-200" />
        <span>hoặc</span>
        <span className="h-px flex-1 bg-gray-200" />
      </div>
      <div className="flex justify-center">
        <GoogleLogin
          onSuccess={async ({ credential }) => {
            if (!credential) {
              toast.error('Google không trả về ID token. Vui lòng thử lại.');
              return;
            }
            try {
              await loginWithGoogle(credential);
              onSuccess();
            } catch (error) {
              if (error instanceof ApiError && error.status === 502) {
                toast.error('Dịch vụ Google đang tạm thời không khả dụng. Vui lòng thử lại sau.');
                return;
              }
              toast.error(getErrorMessage(error));
            }
          }}
          onError={() => toast.error('Không thể mở đăng nhập Google. Vui lòng thử lại.')}
          text="signin_with"
          locale="vi"
        />
      </div>
    </div>
  );
}
