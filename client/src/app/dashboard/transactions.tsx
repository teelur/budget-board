import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { formatDate, getTransactions } from '@/lib/transactions';
import { useQuery } from '@tanstack/react-query';
import AddTransaction from './add-transaction';
import { Button } from '@/components/ui/button';
import { useState } from 'react';
import { type Transaction } from '@/types/transaction';

const Transactions = (): JSX.Element => {
  const [isOpen, setIsOpen] = useState(false);
  const { isPending, isError, data, error } = useQuery({
    queryKey: ['transactions'],
    queryFn: async () => {
      const response = await getTransactions();
      return response;
    },
  });

  // TODO: Make this not ugly
  if (isPending) {
    return <span>Loading...</span>;
  }

  // TODO: This should probably just be an error alert
  if (isError) {
    return <span>Error: {error.message}</span>;
  }

  const toggle = (): void => {
    setIsOpen((isOpen) => !isOpen);
  };

  return (
    <Card className="">
      <CardHeader className="grid w-screen grid-cols-2">
        <CardTitle className="justify-self-start">Transactions</CardTitle>
        <div className="justify-self-end">
          <Button onClick={toggle}>Add Transaction</Button>
        </div>
      </CardHeader>
      {isOpen && <AddTransaction />}
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Date</TableHead>
              <TableHead>Merchant</TableHead>
              <TableHead>Category</TableHead>
              <TableHead>Subcategory</TableHead>
              <TableHead>Amount</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.data.map((transaction: Transaction) => (
              <TableRow key={transaction.id}>
                <TableCell>{formatDate(transaction.date)}</TableCell>
                <TableCell>{transaction.merchantName}</TableCell>
                <TableCell>{transaction.category}</TableCell>
                <TableCell>{transaction.subcategory}</TableCell>
                <TableCell>
                  {transaction.amount?.toLocaleString('en-US', {
                    style: 'currency',
                    currency: 'USD',
                  })}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
};

export default Transactions;
