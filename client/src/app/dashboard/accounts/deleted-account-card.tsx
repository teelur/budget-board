import ResponsiveButton from '@/components/responsive-button';
import { Card } from '@/components/ui/card';
import { useToast } from '@/components/ui/use-toast';
import { restoreAccount } from '@/lib/accounts';
import { translateAxiosError } from '@/lib/request';
import { type Account } from '@/types/account';
import { ResetIcon } from '@radix-ui/react-icons';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { type AxiosError } from 'axios';

interface DeletedAccountCardProps {
  deletedAccount: Account;
}

const DeletedAccountCard = (props: DeletedAccountCardProps): JSX.Element => {
  const { toast } = useToast();

  const getDaysSinceDeleted = (date: Date): string => {
    const differenceInMs = new Date().getTime() - new Date(date).getTime();

    const differenceInDays = Math.round(differenceInMs / (1000 * 3600 * 24));

    return differenceInDays.toString();
  };

  const queryClient = useQueryClient();
  const doRestoreAccount = useMutation({
    mutationFn: async (id: string) => {
      return await restoreAccount(id);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
    onError: (error: AxiosError) => {
      toast({
        variant: 'destructive',
        title: 'Uh oh! Something went wrong.',
        description: translateAxiosError(error),
      });
    },
  });

  return (
    <Card key={props.deletedAccount.id} className="flex flex-row items-center p-2">
      <span className="w-1/5">{props.deletedAccount.name}</span>
      <span className="w-1/5">
        {getDaysSinceDeleted(props.deletedAccount.deleted) + ' days since deleted'}
      </span>
      <div className="w-1/5">
        <ResponsiveButton
          loading={doRestoreAccount.isPending}
          className="h-6 p-1"
          onClick={() => {
            doRestoreAccount.mutate(props.deletedAccount.id);
          }}
        >
          <ResetIcon className="h-4 w-4" />
        </ResponsiveButton>
      </div>
    </Card>
  );
};

export default DeletedAccountCard;
