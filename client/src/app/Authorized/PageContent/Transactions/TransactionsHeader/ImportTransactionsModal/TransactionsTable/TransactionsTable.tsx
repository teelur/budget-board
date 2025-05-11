import { ActionIcon, Divider, Flex, Stack, Table } from "@mantine/core";
import { SquareXIcon } from "lucide-react";
import React from "react";
import { convertNumberToCurrency } from "~/helpers/currency";
import {
  ITransactionImport,
  ITransactionImportTableData,
} from "~/models/transaction";

interface TransactionsTableProps {
  tableData: ITransactionImport[];
  setTableData: React.Dispatch<
    React.SetStateAction<ITransactionImportTableData[]>
  >;
  setCsvData: React.Dispatch<React.SetStateAction<unknown[]>>;
  setImportedData: React.Dispatch<React.SetStateAction<ITransactionImport[]>>;
}

const TransactionsTable = (props: TransactionsTableProps): React.ReactNode => {
  return (
    <Stack gap={0}>
      <Divider label="Transactions" labelPosition="center" />
      <Table.ScrollContainer minWidth={800} maxHeight={400}>
        <Table striped>
          <Table.Thead>
            <Table.Tr>
              <Table.Th />
              <Table.Th>Date</Table.Th>
              <Table.Th>Description</Table.Th>
              <Table.Th>Category</Table.Th>
              <Table.Th>Amount</Table.Th>
              <Table.Th>Account</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {props.tableData.map((row, index) => (
              <Table.Tr key={index}>
                <Table.Td>
                  <Flex justify="center" align="center">
                    <ActionIcon
                      size="sm"
                      color="red"
                      variant="subtle"
                      onClick={() => {
                        props.setTableData((prev) =>
                          prev.filter((_, i) => i !== index)
                        );
                        props.setCsvData((prev) =>
                          prev.filter((_, i) => i !== index)
                        );
                        props.setImportedData((prev) =>
                          prev.filter((_, i) => i !== index)
                        );
                      }}
                    >
                      <SquareXIcon />
                    </ActionIcon>
                  </Flex>
                </Table.Td>
                <Table.Td>{row.date?.toLocaleDateString()}</Table.Td>
                <Table.Td>{row.description}</Table.Td>
                <Table.Td>{row.category}</Table.Td>
                <Table.Td>
                  {convertNumberToCurrency(row.amount ?? 0, true)}
                </Table.Td>
                <Table.Td>{row.account}</Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      </Table.ScrollContainer>
    </Stack>
  );
};

export default TransactionsTable;
