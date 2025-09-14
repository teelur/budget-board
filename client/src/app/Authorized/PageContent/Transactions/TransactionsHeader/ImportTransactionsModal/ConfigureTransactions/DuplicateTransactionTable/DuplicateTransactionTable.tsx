import {
  ActionIcon,
  Divider,
  Flex,
  Pagination,
  Stack,
  Table,
} from "@mantine/core";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { CornerDownRightIcon, Undo2Icon } from "lucide-react";
import React from "react";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { convertNumberToCurrency } from "~/helpers/currency";
import { IAccount } from "~/models/account";
import {
  ITransaction,
  ITransactionImportTableData,
} from "~/models/transaction";
import { IUserSettings } from "~/models/userSettings";

interface DuplicateTransactionTableProps {
  tableData: Map<ITransactionImportTableData, ITransaction>;
  restoreTransaction: (uid: number) => void;
}

const DuplicateTransactionTable = (
  props: DuplicateTransactionTableProps
): React.ReactNode => {
  const [page, setPage] = React.useState(1);
  const itemsPerPage = 5;

  const numberOfPages = Math.ceil(props.tableData.size / itemsPerPage);

  React.useEffect(() => {
    if (props.tableData.size > 0 && page > numberOfPages) {
      setPage(numberOfPages);
    }
  }, [props.tableData]);

  const { request } = React.useContext<any>(AuthContext);

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const accountsQuery = useQuery({
    queryKey: ["accounts"],
    queryFn: async (): Promise<IAccount[]> => {
      const res: AxiosResponse = await request({
        url: "/api/account",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IAccount[];
      }

      return [];
    },
  });

  const accountIDToNameMap = React.useMemo(() => {
    const map = new Map<string, string>();
    props.tableData.values().forEach((value) => {
      const account = accountsQuery.data?.find(
        (account) => account.id === value.accountID
      );
      if (!account) {
        return;
      }
      map.set(value.accountID, account.name);
    });
    return map;
  }, [props.tableData, accountsQuery.data]);

  if (props.tableData.size === 0) {
    return null;
  }

  return (
    <Stack gap={0} justify="center">
      <Divider label="Duplicate Transactions" labelPosition="center" />
      <Table.ScrollContainer minWidth={800} maxHeight={400}>
        <Table striped>
          <Table.Thead>
            <Table.Tr>
              <Table.Th />
              <Table.Th>Date</Table.Th>
              <Table.Th>Description</Table.Th>
              <Table.Th>Amount</Table.Th>
              <Table.Th>Account</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {Array.from(props.tableData)
              .map(([importedTransaction, existingTransaction]) => {
                return {
                  importedTransaction,
                  existingTransaction,
                };
              })
              .slice(
                (page - 1) * itemsPerPage,
                (page - 1) * itemsPerPage + itemsPerPage
              )
              .map((row, index) => (
                <>
                  <Table.Tr key={index}>
                    <Table.Td>
                      <Flex justify="center" align="center">
                        <ActionIcon
                          size="sm"
                          variant="subtle"
                          onClick={() => {
                            props.restoreTransaction(
                              row.importedTransaction.uid
                            );
                          }}
                        >
                          <Undo2Icon />
                        </ActionIcon>
                      </Flex>
                    </Table.Td>
                    <Table.Td>
                      {row.importedTransaction.date?.toLocaleDateString()}
                    </Table.Td>
                    <Table.Td>{row.importedTransaction.description}</Table.Td>
                    <Table.Td>
                      {userSettingsQuery.isPending
                        ? null
                        : convertNumberToCurrency(
                            row.importedTransaction.amount ?? 0,
                            true,
                            userSettingsQuery.data?.currency ?? "USD"
                          )}
                    </Table.Td>
                    <Table.Td>{row.importedTransaction.account}</Table.Td>
                  </Table.Tr>
                  <Table.Tr key={index + props.tableData.size}>
                    <Table.Td>
                      <Flex justify="center" align="center">
                        <CornerDownRightIcon size="1rem" />
                      </Flex>
                    </Table.Td>
                    <Table.Td>
                      {new Date(
                        row.existingTransaction.date
                      ).toLocaleDateString()}
                    </Table.Td>
                    <Table.Td>{row.existingTransaction.merchantName}</Table.Td>
                    <Table.Td>
                      {userSettingsQuery.isPending
                        ? null
                        : convertNumberToCurrency(
                            row.existingTransaction.amount ?? 0,
                            true,
                            userSettingsQuery.data?.currency ?? "USD"
                          )}
                    </Table.Td>
                    <Table.Td>
                      {accountIDToNameMap.get(
                        row.existingTransaction.accountID
                      )}
                    </Table.Td>
                  </Table.Tr>
                </>
              ))}
          </Table.Tbody>
        </Table>
      </Table.ScrollContainer>
      {props.tableData.size > itemsPerPage && (
        <Flex w="100%" justify="center">
          <Pagination
            total={numberOfPages}
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

export default DuplicateTransactionTable;
