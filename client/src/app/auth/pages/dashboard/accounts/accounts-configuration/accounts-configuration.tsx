import { Button } from '@/components/ui/button';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet';
import { type Account } from '@/types/account';
import DeletedAccountsCards from './delete/deleted-accounts-cards';
import { ScrollArea } from '@/components/ui/scroll-area';
import { cn } from '@/lib/utils';
import React from 'react';
import AccountsConfigurationGroups from './accounts-configuration-groups';
import { Institution, InstitutionIndexRequest } from '@/types/institution';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { AuthContext } from '@/components/auth-provider';
import { AxiosError } from 'axios';
import { toast } from 'sonner';
import { translateAxiosError } from '@/lib/requests';
import ResponsiveButton from '@/components/responsive-button';
import { SettingsIcon } from 'lucide-react';

interface AccountsConfigurationProps {
  institutions: Institution[];
  accounts: Account[];
}

const AccountsConfiguration = (props: AccountsConfigurationProps): JSX.Element => {
  const [sortedInstitutions, setSortedInstitutions] = React.useState<Institution[]>(
    props.institutions.sort((a, b) => a.index - b.index)
  );
  const [isReorder, setIsReorder] = React.useState(false);

  const { request } = React.useContext<any>(AuthContext);
  const queryClient = useQueryClient();
  const doIndexInstitutions = useMutation({
    mutationFn: async (institutions: InstitutionIndexRequest[]) =>
      await request({
        url: '/api/institution/setindices',
        method: 'PUT',
        data: institutions,
      }),
    onSuccess: async () =>
      await queryClient.invalidateQueries({
        queryKey: ['institutions'],
      }),
    onError: (error: AxiosError) => toast.error(translateAxiosError(error)),
  });

  const onReorderClick = () => {
    if (isReorder) {
      const indexedInstitutions: InstitutionIndexRequest[] = sortedInstitutions.map(
        (inst, index) => ({
          id: inst.id,
          index,
        })
      );
      doIndexInstitutions.mutate(indexedInstitutions);
    }
    setIsReorder(!isReorder);
  };

  return (
    <Sheet>
      <SheetTrigger asChild>
        <Button variant="ghost" className="h-8 w-9 p-0">
          <SettingsIcon className="h-5 w-5" />
        </Button>
      </SheetTrigger>
      <SheetTitle hidden />
      <SheetContent side="top" className="flex h-full w-full flex-row justify-center">
        <div className="w-full gap-3 pt-2 2xl:max-w-screen-2xl">
          <ScrollArea className="h-full pr-4" type="auto">
            <div className="flex flex-col gap-3">
              <div className="flex flex-row items-center justify-between">
                <SheetHeader className="text-lg font-semibold">
                  Accounts Configuration
                </SheetHeader>
                <ResponsiveButton
                  className={cn(
                    isReorder ? 'border-success text-success hover:text-success' : ''
                  )}
                  variant="outline"
                  onClick={onReorderClick}
                  loading={doIndexInstitutions.isPending}
                >
                  {isReorder ? 'Save' : 'Reorder'}
                </ResponsiveButton>
              </div>
              <AccountsConfigurationGroups
                sortedInstitutions={sortedInstitutions}
                accounts={props.accounts}
                setSortedInstitutions={setSortedInstitutions}
                isReorder={isReorder}
              />
              <DeletedAccountsCards
                deletedAccounts={props.accounts.filter(
                  (a: Account) => a.deleted !== null
                )}
              />
            </div>
          </ScrollArea>
        </div>
      </SheetContent>
    </Sheet>
  );
};

export default AccountsConfiguration;
