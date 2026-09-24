import { Divider, Flex, Pagination, Stack, Table } from "@mantine/core";
import { ActionIcon } from "@teelur/budget-board-ui";
import { SquareXIcon } from "lucide-react";
import React from "react";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import { ITransactionImportTableData } from "~/models/transaction";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface TransactionsTableProps {
  tableData: ITransactionImportTableData[];
  delete: (uid: number) => void;
}

const TransactionsTable = (props: TransactionsTableProps): React.ReactNode => {
  const itemsPerPage = 10;

  const { t } = useTranslation();
  const { dayjs, dateFormat } = useLocale();

  const [page, setPage] = React.useState(1);

  return (
    <Stack gap={0} justify="center">
      <Divider label={t("transactions")} labelPosition="center" />
      <Table.ScrollContainer minWidth={950} maxHeight={400}>
        <Table striped>
          <Table.Thead>
            <Table.Tr>
              <Table.Th />
              <Table.Th>{t("date")}</Table.Th>
              <Table.Th>{t("merchant_name")}</Table.Th>
              <Table.Th>{t("category")}</Table.Th>
              <Table.Th>{t("amount")}</Table.Th>
              <Table.Th>{t("account")}</Table.Th>
              <Table.Th>{t("notes")}</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {props.tableData
              .slice(
                (page - 1) * itemsPerPage,
                (page - 1) * itemsPerPage + itemsPerPage,
              )
              .map((row, index) => (
                <Table.Tr key={index}>
                  <Table.Td>
                    <Flex justify="center" align="center">
                      <ActionIcon
                        variant="ghost"
                        color="error"
                        size="compact-xs"
                        onClick={() => {
                          props.delete(row.uid);
                        }}
                      >
                        <SquareXIcon size={20} />
                      </ActionIcon>
                    </Flex>
                  </Table.Td>
                  <Table.Td>
                    {dayjs(row.date).isValid()
                      ? dayjs(row.date).format(dateFormat)
                      : null}
                  </Table.Td>
                  <Table.Td>{row.merchantName}</Table.Td>
                  <Table.Td>{row.category}</Table.Td>
                  <Table.Td>
                    <SensitiveAmount amount={row.amount ?? 0} />
                  </Table.Td>
                  <Table.Td>{row.account}</Table.Td>
                  <Table.Td>{row.notes}</Table.Td>
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
