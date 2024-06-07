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
import { useToast } from '@/components/ui/use-toast';
import { translateAxiosError } from '@/lib/requests';
import { zodResolver } from '@hookform/resolvers/zod';
import { AxiosError } from 'axios';
import React from 'react';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import * as z from 'zod';

const Signup = (): JSX.Element => {
  const [loading, setLoading] = useState<boolean>(false);
  const { toast } = useToast();

  const formSchema = z
    .object({
      email: z
        .string()
        .min(1, { message: 'This field must be filled.' })
        .email('This is not a valid email'),
      password: z.string().min(7, { message: 'Password must be at least 7 characters' }),
      confirm: z.string().min(7, { message: 'Password must be at least 7 characters' }),
    })
    .superRefine(({ confirm, password }, ctx) => {
      if (confirm !== password) {
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
      email: '',
      password: '',
    },
  });

  const { request } = React.useContext<any>(AuthContext);

  const submitUserSignup = async (
    values: z.infer<typeof formSchema>,
    e: any
  ): Promise<void> => {
    e.preventDefault();
    setLoading(true);

    const email = values.email;
    const password = values.password;

    request({
      url: '/api/register',
      method: 'POST',
      data: {
        email,
        password,
      },
    })
      .then(() => {
        toast({
          variant: 'default',
          title: 'Success!',
          description: 'Account created! Check your email for confirmation.',
        });
      })
      .catch((error: AxiosError) => {
        toast({
          variant: 'destructive',
          title: 'Error',
          description: translateAxiosError(error),
        });
      })
      .finally(() => {
        setLoading(false);
      });
  };

  return (
    <Form {...form}>
      <h1 className="text-xl font-bold">Sign Up</h1>
      <form
        // eslint-disable-next-line @typescript-eslint/no-misused-promises
        onSubmit={form.handleSubmit(async (data, event) => {
          await submitUserSignup(data, event);
        })}
        className="space-y-8"
      >
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
              <FormControl>
                <Input type="email" placeholder="Enter your email" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Password</FormLabel>
              <FormControl>
                <Input type="password" placeholder="Enter your password" {...field} />
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
                <Input type="password" placeholder="Confirm your password" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <ResponsiveButton {...{ loading }}>Submit</ResponsiveButton>
      </form>
    </Form>
  );
};

export default Signup;
