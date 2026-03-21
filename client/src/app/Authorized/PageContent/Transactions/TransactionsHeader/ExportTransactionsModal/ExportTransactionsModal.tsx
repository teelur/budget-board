import classes from "./ExportTransactionsModal.module.css";

import React from "react";
import { Button, Flex, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { FileUpIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import FilterCard from "../FilterCard/FilterCard";
import FieldSelectionCard from "./FieldSelectionCard/FieldSelectionCard";
import ColumnOrderCard from "./ColumnOrderCard/ColumnOrderCard";
import useIsMobile from "~/hooks/useIsMobile";
import { useTransactionFilters } from "~/providers/TransactionFiltersProvider/TransactionFiltersProvider";
import { useTransactionCategories } from "~/providers/TransactionCategoryProvider/TransactionCategoryProvider";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { ITransaction, Filters } from "~/models/transaction";
import { IInstitution } from "~/models/institution";
import {
  getFilteredTransactions,
  buildTransactionsCsv,
} from "~/helpers/transactions";

export interface IExportField {
  key: string;
  labelKey: string;
}

export const EXPORT_FIELDS: IExportField[] = [
  { key: "date", labelKey: "date" },
  { key: "merchantName", labelKey: "merchant_name" },
  { key: "amount", labelKey: "amount" },
  { key: "category", labelKey: "category" },
  { key: "account", labelKey: "account" },
  { key: "pending", labelKey: "pending" },
  { key: "source", labelKey: "source" },
];

const DEFAULT_FIELDS = EXPORT_FIELDS.map((f) => f.key);

const ExportTransactionsModal = (): React.ReactNode => {
  const [opened, { open, close }] = useDisclosure(false);
  const [orderedFields, setOrderedFields] =
    React.useState<string[]>(DEFAULT_FIELDS);

  const { t } = useTranslation();
  const { transactionFilters } = useTransactionFilters();
  const { transactionCategories } = useTransactionCategories();
  const { request } = useAuth();
  const isMobile = useIsMobile();

  const transactionsQuery = useQuery({
    queryKey: ["transactions", { getHidden: false }],
    queryFn: async (): Promise<ITransaction[]> => {
      const res: AxiosResponse = await request({
        url: "/api/transaction",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as ITransaction[];
      }

      return [];
    },
  });

  const institutionsQuery = useQuery({
    queryKey: ["institutions"],
    queryFn: async (): Promise<IInstitution[]> => {
      const res: AxiosResponse = await request({
        url: "/api/institution",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IInstitution[];
      }

      return [];
    },
  });

  const accountLookup = React.useMemo<Record<string, string>>(() => {
    if (!institutionsQuery.data) return {};

    return institutionsQuery.data.reduce<Record<string, string>>(
      (acc, institution) => {
        for (const account of institution.accounts) {
          acc[account.id] = account.name;
        }
        return acc;
      },
      {},
    );
  }, [institutionsQuery.data]);

  const filteredTransactions = React.useMemo(
    () =>
      getFilteredTransactions(
        transactionsQuery.data ?? [],
        transactionFilters ?? new Filters(),
        transactionCategories,
      ),
    [transactionsQuery.data, transactionFilters, transactionCategories],
  );

  const handleFieldsChange = (newSelected: string[]) => {
    const kept = orderedFields.filter((f) => newSelected.includes(f));
    const added = newSelected.filter((f) => !orderedFields.includes(f));
    setOrderedFields([...kept, ...added]);
  };

  const handleExport = () => {
    const fieldLabels = Object.fromEntries(
      EXPORT_FIELDS.map((f) => [f.key, t(f.labelKey)]),
    );

    const csv = buildTransactionsCsv(
      filteredTransactions,
      orderedFields,
      fieldLabels,
      accountLookup,
    );

    const blob = new Blob([csv], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "transactions.csv";
    a.click();
    setTimeout(() => {
      URL.revokeObjectURL(url);
    }, 0);
  };

  return (
    <>
      <Button
        size="sm"
        rightSection={<FileUpIcon size="1rem" />}
        onClick={open}
      >
        {t("export")}
      </Button>
      <Modal
        opened={opened}
        onClose={close}
        fullScreen={isMobile}
        size={"80rem"}
        title={<PrimaryText>{t("export_transactions")}</PrimaryText>}
      >
        <Stack className={classes.container} gap="0.5rem">
          <FilterCard />
          <Flex
            className={classes.fieldsContainer}
            gap="0.5rem"
            align="flex-start"
          >
            <FieldSelectionCard
              selectedFields={orderedFields}
              onChange={handleFieldsChange}
            />
            <ColumnOrderCard
              orderedFields={orderedFields}
              onChange={setOrderedFields}
            />
          </Flex>
          <Button
            onClick={handleExport}
            disabled={orderedFields.length === 0}
            rightSection={<FileUpIcon size="1rem" />}
          >
            {t("export_csv")}
          </Button>
        </Stack>
      </Modal>
    </>
  );
};

export default ExportTransactionsModal;
