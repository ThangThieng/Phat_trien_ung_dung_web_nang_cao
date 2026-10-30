'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { toast } from 'sonner';
import FormField from '@/components/shared/FormField';
import { apiFetch, getErrorMessage, mapProblemDetailsToForm } from '@/lib/api-client';
import { useAuth } from '@/features/auth/auth-context';
import { useAuthStoreApi } from '@/stores/auth-store-provider';
import { authKeys } from '@/features/auth/query-keys';
import { registerSchema } from '@/features/auth/schemas';
import type { Session, User } from '@/types/auth';
import ImageUploader from '@/components/shared/ImageUploader';
import type { UploadedFile } from '@/types/api';

type Tab = 'Thông tin' | 'Bảo mật' | 'Phiên'; const tabs: Tab[] = ['Thông tin','Bảo mật','Phiên'];
const profileSchema=z.object({displayName:z.string().trim().min(2).max(100),bio:z.string().max(1000)});
const passwordSchema=z.object({currentPassword:z.string().min(1),newPassword:registerSchema.shape.password,confirmPassword:z.string()}).refine(v=>v.newPassword===v.confirmPassword,{path:['confirmPassword'],message:'Mật khẩu xác nhận không khớp.'});
const deleteFile=(file:UploadedFile)=>apiFetch(`/files/${file.key.split('/').map(encodeURIComponent).join('/')}`,{method:'DELETE'}).catch(()=>undefined);
export function ProfilePage(){
 const router=useRouter(); const queryClient=useQueryClient(); const store=useAuthStoreApi(); const {logout,accessToken}=useAuth();
 const [tab,setTab]=useState<Tab>('Thông tin'); const [message,setMessage]=useState(''); const [newAvatar,setNewAvatar]=useState<UploadedFile|null>(null); const [avatarBeforeUpload,setAvatarBeforeUpload]=useState<User|null>(null);
 const info=useForm<z.infer<typeof profileSchema>>({resolver:zodResolver(profileSchema)}); const security=useForm<z.infer<typeof passwordSchema>>({resolver:zodResolver(passwordSchema)});
 const me=useQuery({queryKey:authKeys.me(),queryFn:()=>apiFetch<User>('/auth/me'),meta:{suppressGlobalError:true}});
 const sessionsQuery=useQuery({queryKey:authKeys.sessions(),queryFn:()=>apiFetch<Session[]>('/auth/sessions'),enabled:tab==='Phiên',meta:{suppressGlobalError:true}});
 useEffect(()=>{if(me.data)info.reset({displayName:me.data.displayName,bio:me.data.bio??''});},[info,me.data]);
 useEffect(()=>{if(me.error||sessionsQuery.error)setMessage(getErrorMessage(me.error??sessionsQuery.error));},[me.error,sessionsQuery.error]);
 const changeAvatar=(file:UploadedFile|null)=>{setNewAvatar(file);if(file&&me.data){setAvatarBeforeUpload(current=>current??me.data!);const next={...me.data,avatarUrl:file.url};queryClient.setQueryData(authKeys.me(),next);store.getState().setUser(next);}};
 const save=info.handleSubmit(async values=>{const previous=avatarBeforeUpload??me.data;if(!previous)return;const next={...previous,displayName:values.displayName.trim(),bio:values.bio.trim()||null,avatarUrl:newAvatar?.url??previous.avatarUrl};queryClient.setQueryData(authKeys.me(),next);store.getState().setUser(next);try{const updated=await apiFetch<User>('/auth/me',{method:'PATCH',body:{displayName:next.displayName,bio:next.bio,avatarUrl:next.avatarUrl??''}});queryClient.setQueryData(authKeys.me(),updated);store.getState().setUser(updated);setNewAvatar(null);setAvatarBeforeUpload(null);setMessage('Đã lưu hồ sơ.');}catch(error){queryClient.setQueryData(authKeys.me(),previous);store.getState().setUser(previous);if(newAvatar)await deleteFile(newAvatar);if(!mapProblemDetailsToForm(error,['displayName','bio'],info.setError))setMessage(getErrorMessage(error));}});
 const change=security.handleSubmit(async values=>{try{await apiFetch('/auth/change-password',{method:'POST',body:{currentPassword:values.currentPassword,newPassword:values.newPassword}});security.reset();const text='Đã đổi mật khẩu. Các thiết bị khác đã bị đăng xuất.';toast.success(text);setMessage(text);}catch(error){if(!mapProblemDetailsToForm(error,['currentPassword','newPassword'],security.setError))security.setError('root',{message:getErrorMessage(error)});}});
 const revoke=async(s:Session)=>{if(!window.confirm('Bạn có chắc muốn thu hồi phiên này?'))return;try{await apiFetch(`/auth/sessions/${s.id}`,{method:'DELETE'});if(s.isCurrent){await logout();router.replace('/auth/login');}else queryClient.invalidateQueries({queryKey:authKeys.sessions()});}catch(error){setMessage(getErrorMessage(error));}};
 const revokeAll=async()=>{if(!window.confirm('Bạn có chắc muốn thu hồi tất cả phiên?'))return;try{await apiFetch('/auth/sessions/revoke-all',{method:'POST'});await logout();router.replace('/auth/login');}catch(error){setMessage(getErrorMessage(error));}};
 const onKey=(e:React.KeyboardEvent<HTMLButtonElement>,i:number)=>{if(e.key==='ArrowLeft'||e.key==='ArrowRight'){e.preventDefault();setTab(tabs[(i+(e.key==='ArrowRight'?1:tabs.length-1))%tabs.length]);}};
 return <main className="mx-auto max-w-2xl p-6"><h1 className="text-2xl font-bold">Hồ sơ</h1><div className="my-4 flex gap-2" role="tablist" aria-label="Hồ sơ">{tabs.map((item,i)=><button id={`profile-tab-${i}`} key={item} type="button" role="tab" aria-selected={tab===item} aria-controls={`profile-panel-${i}`} tabIndex={tab===item?0:-1} onKeyDown={e=>onKey(e,i)} onClick={()=>setTab(item)}>{item}</button>)}</div>{message&&<p role="status">{message}</p>}{tab==='Thông tin'&&<section role="tabpanel" id="profile-panel-0" aria-labelledby="profile-tab-0"><form onSubmit={save}><ImageUploader accessToken={accessToken} value={newAvatar} onChange={changeAvatar} deleteOnRemove={false}/><FormField label="Tên hiển thị" error={info.formState.errors.displayName?.message}{...info.register('displayName')}/><FormField label="Tiểu sử" error={info.formState.errors.bio?.message}{...info.register('bio')}/><button type="submit" disabled={info.formState.isSubmitting}>Lưu thay đổi</button></form></section>}{tab==='Bảo mật'&&<section role="tabpanel" id="profile-panel-1" aria-labelledby="profile-tab-1"><form onSubmit={change}>{security.formState.errors.root&&<p role="alert">{security.formState.errors.root.message}</p>}<FormField label="Mật khẩu hiện tại" type="password" error={security.formState.errors.currentPassword?.message}{...security.register('currentPassword')}/><FormField label="Mật khẩu mới" type="password" error={security.formState.errors.newPassword?.message}{...security.register('newPassword')}/><FormField label="Xác nhận mật khẩu mới" type="password" error={security.formState.errors.confirmPassword?.message}{...security.register('confirmPassword')}/><button type="submit" disabled={security.formState.isSubmitting}>Đổi mật khẩu</button></form></section>}{tab==='Phiên'&&<section role="tabpanel" id="profile-panel-2" aria-labelledby="profile-tab-2">{(sessionsQuery.data??[]).map(s=><div key={s.id}><span>{s.isCurrent?'Thiết bị này — ':''}IP: {s.createdByIp??'Không rõ'} — Tạo: {new Date(s.createdAt).toLocaleString()} — Hết hạn: {new Date(s.expiresAt).toLocaleString()}</span><button type="button" onClick={()=>revoke(s)}>Thu hồi</button></div>)}<button type="button" onClick={revokeAll}>Thu hồi tất cả</button></section>}</main>;
}
