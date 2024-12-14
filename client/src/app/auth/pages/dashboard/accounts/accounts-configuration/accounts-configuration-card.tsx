import ResponsiveButton from '@/components/responsive-button';
import { Card } from '@/components/ui/card';
import { Checkbox } from '@/components/ui/checkbox';
import { Input } from '@/components/ui/input';
import { translateAxiosError } from '@/lib/requests';
import { accountCategories, AccountEditRequest, type Account } from '@/types/account';
import { CheckIcon, Cross2Icon } from '@radix-ui/react-icons';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { type AxiosError } from 'axios';
import React from 'react';
import DeleteAccount from './delete/delete-account';
import { AuthContext } from '@/components/auth-provider';
import { Label } from '@/components/ui/label';
import CategoryInput from '@/components/category-input';
import { getIsParentCategory, getParentCategory } from '@/lib/category';
import { toast } from 'sonner';
import { SortableDragHandle } from '@/components/sortable';
import { GripVertical } from 'lucide-react';

interface AccountsConfigurationCardProps {
  account: Account;
  isReorder: boolean;
}

const AccountsConfigurationCard = (
  props: AccountsConfigurationCardProps
): JSX.Element => {
  const [hideTransactionsValue, setHideTransactionsValue] = React.useState(
    props.account.hideTransactions ?? false
  );
  const [hideAccountValue, setHideAccountValue] = React.useState(
    props.account.hideAccount ?? false
  );
  const [accountNameValue, setAccountNameValue] = React.useState(props.account.name);
  const [accountTypeValue, setAccountTypeValue] = React.useState(props.account.type);
  const [accountSubTypeValue, setAccountSubTypeValue] = React.useState(
    props.account.subtype
  );
  const [valueDirty, setValueDirty] = React.useState(false);

  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();

  const doUpdateAccount = useMutation({
    mutationFn: async () => {
      const editedAccount: AccountEditRequest = {
        id: props.account.id,
        name: accountNameValue,
        type: accountTypeValue,
        subtype: accountSubTypeValue,
        hideTransactions: hideTransactionsValue,
        hideAccount: hideAccountValue,
      };

      return await request({
        url: '/api/account',
        method: 'PUT',
        data: editedAccount,
      });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['accounts'] });
      await queryClient.invalidateQueries({ queryKey: ['transactions'] });
      setValueDirty(false);
    },
    onError: (error: AxiosError) => {
      toast.error(translateAxiosError(error));
    },
  });

  return (
    <Card className="flex flex-col items-center gap-2 p-2 md:flex-row">
      <div className="flex w-full flex-row items-center gap-2 md:w-1/3">
        <div className="self-stretch">
          {props.isReorder && (
            <SortableDragHandle variant="outline" size="icon" className="h-full w-7">
              <GripVertical />
            </SortableDragHandle>
          )}
        </div>
        <Input
          value={accountNameValue}
          onChange={(e: React.ChangeEvent<HTMLInputElement>) => {
            setAccountNameValue(e.target.value);
            setValueDirty(true);
          }}
        />
      </div>
      <div className="flex w-full flex-row flex-wrap items-center justify-between gap-2 md:w-2/3 md:flex-nowrap">
        <CategoryInput
          className="min-w-[185px] md:w-1/4"
          selectedCategory={
            accountSubTypeValue.length > 0 ? accountSubTypeValue : accountTypeValue
          }
          setSelectedCategory={(type: string) => {
            setAccountTypeValue(getParentCategory(type, accountCategories));
            getIsParentCategory(type, accountCategories)
              ? setAccountSubTypeValue('')
              : setAccountSubTypeValue(type);
            setValueDirty(true);
          }}
          categories={accountCategories}
        />
        <div className="flex flex-row justify-center gap-2 p-1 md:w-1/4">
          <Checkbox
            id="hidden"
            checked={hideAccountValue}
            onCheckedChange={() => {
              setHideAccountValue(!hideAccountValue);
              setValueDirty(true);
            }}
          />
          <Label className="md:hidden">Hide Account?</Label>
        </div>
        <div className="flex flex-row justify-center gap-2 p-1 md:w-1/4">
          <Checkbox
            id="hidden"
            checked={hideTransactionsValue}
            onCheckedChange={() => {
              setHideTransactionsValue(!hideTransactionsValue);
              setValueDirty(true);
            }}
          />
          <Label className="md:hidden">Hide Transactions?</Label>
        </div>
        <div className="flex min-w-[64px] flex-row justify-center p-2 md:w-1/4">
          {valueDirty ? (
            <div className="flex flex-row gap-2">
              <ResponsiveButton
                className="m-0 h-7 w-7 p-0"
                onClick={() => {
                  doUpdateAccount.mutate();
                }}
                loading={doUpdateAccount.isPending}
              >
                <CheckIcon className="h-4 w-4" />
              </ResponsiveButton>
              <ResponsiveButton
                className="m-0 h-7 w-7 p-0"
                onClick={() => {
                  setAccountNameValue(props.account.name);
                  setHideTransactionsValue(props.account.hideTransactions);
                  setHideAccountValue(props.account.hideAccount);
                  setAccountTypeValue(props.account.type);
                  setAccountSubTypeValue(props.account.subtype);
                  setValueDirty(false);
                }}
                loading={false}
              >
                <Cross2Icon className="h-4 w-4" />
              </ResponsiveButton>
            </div>
          ) : (
            <DeleteAccount accountId={props.account.id} />
          )}
        </div>
      </div>
    </Card>
  );
};

export default AccountsConfigurationCard;
