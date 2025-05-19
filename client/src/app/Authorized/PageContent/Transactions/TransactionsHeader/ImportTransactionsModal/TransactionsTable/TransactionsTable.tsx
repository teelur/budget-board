import {
  ActionIcon,
  Divider,
  Flex,
  Pagination,
  Stack,
  Table,
} from "@mantine/core";
import { SquareXIcon } from "lucide-react";
import React from "react";
import { convertNumberToCurrency } from "~/helpers/currency";
import { ITransactionImportTableData } from "~/models/transaction";

interface TransactionsTableProps {
  tableData: ITransactionImportTableData[];
  delete: (transaction: ITransactionImportTableData) => void;
}

const TransactionsTable = (props: TransactionsTableProps): React.ReactNode => {
  const [page, setPage] = React.useState(1);
  const itemsPerPage = 10;

  return (
    <Stack gap={0} justify="center">
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
            {props.tableData
              .slice(
                (page - 1) * itemsPerPage,
                (page - 1) * itemsPerPage + itemsPerPage
              )
              .map((row, index) => (
                <Table.Tr key={index}>
                  <Table.Td>
                    <Flex justify="center" align="center">
                      <ActionIcon
                        size="sm"
                        color="red"
                        variant="subtle"
                        onClick={() => {
                          props.delete(row);
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
      {props.tableData.length > itemsPerPage && (
        <Flex w="100%" justify="center">
          <Pagination
            total={Math.ceil(props.tableData.length / itemsPerPage)}
            value={page}
            onChange={setPage}
            mt="sm"
            size="sm"
          />
        </Flex>
      )}
    </Stack>
  );
};

export default TransactionsTable;
