import { Group, Pagination, Stack } from "@mantine/core";
import React from "react";
import { IBalanceResponse } from "~/models/balance";
import BalanceItem from "./BalanceItem/BalanceItem";

interface BalanceItemsProps {
  balances: IBalanceResponse[];
  currency: string;
}

const BalanceItems = (props: BalanceItemsProps) => {
  const itemsPerPage = 20;

  const [page, setPage] = React.useState(1);

  React.useEffect(() => {
    setPage(1);
  }, [props.balances]);

  return (
    <Stack gap="0.5rem">
      {props.balances
        .slice((page - 1) * itemsPerPage, page * itemsPerPage)
        .map((balance) => (
          <BalanceItem
            key={balance.id}
            balance={balance}
            userCurrency={props.currency}
          />
        ))}
      <Group justify="center">
        <Pagination
          total={Math.ceil(props.balances.length / itemsPerPage)}
          value={page}
          onChange={setPage}
        />
      </Group>
    </Stack>
  );
};

export default BalanceItems;
