import { Group, Stack } from "@mantine/core";
import React from "react";
import { IBalanceResponse } from "~/models/balance";
import BalanceItem from "./BalanceItem/BalanceItem";
import { Pagination } from "@teelur/budget-board-ui";

interface BalanceItemsProps {
  balances: IBalanceResponse[];
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
          <BalanceItem key={balance.id} balance={balance} />
        ))}
      <Group justify="center">
        <Pagination
          size="sm"
          total={Math.ceil(props.balances.length / itemsPerPage)}
          value={page}
          onChange={setPage}
        />
      </Group>
    </Stack>
  );
};

export default BalanceItems;
