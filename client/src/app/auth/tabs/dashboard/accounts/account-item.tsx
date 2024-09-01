import { convertNumberToCurrency } from '@/lib/utils';
import { type Account } from '@/types/account';

interface AccountItemProps {
  account: Account;
}

const AccountItem = (props: AccountItemProps): JSX.Element => {
  return (
    <div className="grid grid-rows-2 px-1">
      <div className="row-span-1 grid grid-cols-10">
        <div className="col-span-6 text-left">
          <span className="text-base tracking-tight">{props.account.name}</span>
        </div>
        <span className="col-span-4 text-right">
          {convertNumberToCurrency(props.account.currentBalance, true)}
        </span>
      </div>
      <span className="row-span-1 text-left text-sm text-muted-foreground">
        Last updated:{' '}
        {props.account.balanceDate
          ? new Date(props.account.balanceDate).toLocaleString()
          : 'Never!'}
      </span>
    </div>
  );
};

export default AccountItem;
