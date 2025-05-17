import { Divider, Stack } from "@mantine/core";
import React from "react";
import {
  ITransaction,
  ITransactionImportTableData,
} from "~/models/transaction";

interface DuplicateTransactionTableProps {
  tableDataMap: Map<ITransactionImportTableData, ITransaction>;
}

const DuplicateTransactionTable = (
  props: DuplicateTransactionTableProps
): React.ReactNode => {
  if (props.tableDataMap.size === 0) {
    return null;
  }

  return (
    <Stack gap={0} justify="center">
      <Divider label="Duplicate Transactions" labelPosition="center" />
    </Stack>
  );
};

export default DuplicateTransactionTable;
