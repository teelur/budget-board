/* eslint-disable @typescript-eslint/strict-boolean-expressions */
import { AuthContext } from '@/components/auth-provider';
import ResponsiveButton from '@/components/responsive-button';
import { Card } from '@/components/ui/card';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { useToast } from '@/components/ui/use-toast';
import { translateAxiosError } from '@/lib/requests';
import { zodResolver } from '@hookform/resolvers/zod';
import { AxiosError } from 'axios';
import React from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';

const ResetPassword = (): JSX.Element => {
  const [loading, setLoading] = React.useState<boolean>(false);
  const { toast } = useToast();

  const formSchema = z
    .object({
      oldPassword: z
        .string()
        .min(7, { message: 'Password must be at least 7 characters' }),
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
      oldPassword: '',
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
      url: '/api/manage/info',
      method: 'POST',
      data: {
        newPassword: values.newPassword,
        oldPassword: values.oldPassword,
      },
    })
      .then(() => {
        toast({
          variant: 'default',
          title: 'Success!',
          description: 'Password successfully updated.',
        });
      })
      .catch((error: AxiosError) => {
        toast({
          variant: 'destructive',
          title: 'Uh oh! Something went wrong.',
          description: translateAxiosError(error),
        });
      })
      .finally(() => {
        setLoading(false);
      });
  };

  return (
    <Card className="mt-5 p-6">
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
            name="oldPassword"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Old Password</FormLabel>
                <FormControl>
                  <Input {...field} type="password" />
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
    </Card>
  );
};

export default ResetPassword;
