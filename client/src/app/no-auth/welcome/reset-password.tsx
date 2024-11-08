/* eslint-disable @typescript-eslint/strict-boolean-expressions */
import { AuthContext } from '@/components/auth-provider';
import ResponsiveButton from '@/components/responsive-button';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { translateAxiosError } from '@/lib/requests';
import { zodResolver } from '@hookform/resolvers/zod';
import { AxiosError } from 'axios';
import React from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { LoginCardState } from './welcome';
import { toast } from 'sonner';

interface ResetPasswordProps {
  setLoginCardState: (loginCardState: LoginCardState) => void;
  email: string;
}

const ResetPassword = (props: ResetPasswordProps): JSX.Element => {
  const [loading, setLoading] = React.useState<boolean>(false);

  const formSchema = z
    .object({
      resetCode: z.string(),
      newPassword: z
        .string()
        .min(7, { message: 'Password must be at least 7 characters' }),
      confirm: z.string().min(7, { message: 'Password must be at least 7 characters' }),
    })
    .superRefine(({ confirm, newPassword }, ctx) => {
      if (confirm !== newPassword) {
        ctx.addIssue({
          code: 'custom',
          message: 'The passwords did not match',
          path: ['confirm'],
        });
      }
    });
  const form = useForm<z.infer<typeof formSchema>>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      resetCode: '',
      newPassword: '',
      confirm: '',
    },
  });

  const { request } = React.useContext<any>(AuthContext);

  const submitPasswordUpdate = async (
    values: z.infer<typeof formSchema>,
    e: any
  ): Promise<void> => {
    e.preventDefault();
    setLoading(true);

    request({
      url: '/api/resetPassword',
      method: 'POST',
      data: {
        email: props.email,
        resetCode: values.resetCode,
        newPassword: values.newPassword,
      },
    })
      .then(() => {
        props.setLoginCardState(LoginCardState.Login);
        toast.success('Password successfully updated. Please log in.');
      })
      .catch((error: AxiosError) => {
        toast.error(translateAxiosError(error));
      })
      .finally(() => {
        setLoading(false);
      });
  };

  return (
    <Form {...form}>
      <h1 className="text-xl font-bold">Reset Password</h1>
      <form
        // eslint-disable-next-line @typescript-eslint/no-misused-promises
        onSubmit={form.handleSubmit(async (data, event) => {
          await submitPasswordUpdate(data, event);
        })}
        className="space-y-4"
      >
        <FormField
          control={form.control}
          name="resetCode"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Reset Code</FormLabel>
              <FormControl>
                <Input {...field} type="text" />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="newPassword"
          render={({ field }) => (
            <FormItem>
              <FormLabel>New Password</FormLabel>
              <FormControl>
                <Input {...field} type="password" />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="confirm"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Confirm Password</FormLabel>
              <FormControl>
                <Input {...field} type="password" />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <ResponsiveButton loading={loading}>Submit</ResponsiveButton>
      </form>
    </Form>
  );
};

export default ResetPassword;
