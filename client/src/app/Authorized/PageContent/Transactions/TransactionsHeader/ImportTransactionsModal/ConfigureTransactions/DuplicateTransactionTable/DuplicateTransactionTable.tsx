import {
  ActionIcon,
  Divider,
  Flex,
  Pagination,
  Stack,
  Table,
} from "@mantine/core";
import { CornerDownRightIcon, Undo2Icon } from "lucide-react";
import React from "react";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import {
  ITransaction,
  ITransactionImportTableData,
  TransactionImportDuplicateField,
} from "~/models/transaction";
import { useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useAccountsQuery } from "~/hooks/queries/useAccountsQuery";

interface DuplicateTransactionTableProps {
  tableData: Map<ITransactionImportTableData, ITransaction>;
  matchFields: Map<number, TransactionImportDuplicateField[]>;
  restoreTransaction: (uid: number) => void;
}

const DuplicateTransactionTable = (
  props: DuplicateTransactionTableProps,
): React.ReactNode => {
  const itemsPerPage = 5;

  const { t } = useTranslation();
  const { dayjs, dateFormat } = useLocale();
  const accountsQuery = useAccountsQuery();

  const [page, setPage] = React.useState(1);

  const numberOfPages = Math.ceil(props.tableData.size / itemsPerPage);

  React.useEffect(() => {
    if (props.tableData.size > 0 && page > numberOfPages) {
      setPage(numberOfPages);
    }
  }, [props.tableData]);

  const accountIDToNameMap = React.useMemo(() => {
    const map = new Map<string, string>();
    props.tableData.values().forEach((value) => {
      const account = accountsQuery.data?.find(
        (account) => account.id === value.accountID,
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
      <Divider label={t("duplicate_transactions")} labelPosition="center" />
      <Table.ScrollContainer minWidth={900} maxHeight={400}>
        <Table striped>
          <Table.Thead>
            <Table.Tr>
              <Table.Th />
              <Table.Th>{t("transaction")}</Table.Th>
              <Table.Th>{t("date")}</Table.Th>
              <Table.Th>{t("merchant_name")}</Table.Th>
              <Table.Th>{t("amount")}</Table.Th>
              <Table.Th>{t("account")}</Table.Th>
              <Table.Th>{t("matched_on")}</Table.Th>
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
                (page - 1) * itemsPerPage + itemsPerPage,
              )
              .map((row, index) => (
                <React.Fragment key={row.importedTransaction.uid}>
                  <Table.Tr>
                    <Table.Td>
                      <ActionIcon
                        size="sm"
                        variant="subtle"
                        aria-label={t("restore_transaction")}
                        onClick={() => {
                          props.restoreTransaction(row.importedTransaction.uid);
                        }}
                      >
                        <Undo2Icon />
                      </ActionIcon>
                    </Table.Td>
                    <Table.Td>
                      <PrimaryText size="sm">
                        {t("imported_transaction")}
                      </PrimaryText>
                    </Table.Td>
                    <Table.Td>
                      {dayjs(row.importedTransaction.date).format(dateFormat)}
                    </Table.Td>
                    <Table.Td>{row.importedTransaction.merchantName}</Table.Td>
                    <Table.Td>
                      <SensitiveAmount
                        amount={row.importedTransaction.amount ?? 0}
                      />
                    </Table.Td>
                    <Table.Td>{row.importedTransaction.account}</Table.Td>
                    <Table.Td>
                      <Stack gap={0}>
                        <PrimaryText size="xs">{t("matched_on")}:</PrimaryText>
                        <PrimaryText size="xs">
                          {props.matchFields
                            .get(row.importedTransaction.uid)
                            ?.map((field) =>
                              t(
                                field === "merchantName"
                                  ? "merchant_name"
                                  : field,
                              ),
                            )
                            .join(", ")}
                        </PrimaryText>
                      </Stack>
                    </Table.Td>
                  </Table.Tr>
                  <Table.Tr key={index + props.tableData.size}>
                    <Table.Td />
                    <Table.Td>
                      <Flex align="center" gap="0.25rem">
                        <CornerDownRightIcon size="1rem" />
                        <PrimaryText size="sm">
                          {t("existing_transaction")}
                        </PrimaryText>
                      </Flex>
                    </Table.Td>
                    <Table.Td>
                      {dayjs(row.existingTransaction.date).format(dateFormat)}
                    </Table.Td>
                    <Table.Td>{row.existingTransaction.merchantName}</Table.Td>
                    <Table.Td>
                      <SensitiveAmount
                        amount={row.existingTransaction.amount ?? 0}
                      />
                    </Table.Td>
                    <Table.Td>
                      {accountIDToNameMap.get(
                        row.existingTransaction.accountID,
                      )}
                    </Table.Td>
                    <Table.Td />
                  </Table.Tr>
                </React.Fragment>
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
