import { Group, Pagination, Stack } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import { ICategory } from "~/models/category";
import React from "react";
import TransactionCard from "~/components/core/Card/TransactionCard/TransactionCard";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import BulkActionBar from "~/components/BulkActionBar/BulkActionBar";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";

interface TransactionCardsProps {
  transactions: ITransaction[];
  categories: ICategory[];
}

const TransactionCards = (props: TransactionCardsProps): React.ReactNode => {
  const [page, setPage] = React.useState(1);
  const itemsPerPage = 5;
  const [selectedIds, setSelectedIds] = React.useState<Set<string>>(new Set());

  const { t } = useTranslation();
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });
      if (res.status === 200) return res.data as IUserSettings;
      return undefined;
    },
  });

  const currency = userSettingsQuery.data?.currency ?? "USD";

  const paginatedItems: ITransaction[] = props.transactions.slice(
    (page - 1) * itemsPerPage,
    page * itemsPerPage,
  );

  const onToggleSelect = (id: string) =>
    setSelectedIds((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });

  const onClearSelection = () => setSelectedIds(new Set());

  const onSelectAll = (ids: string[]) =>
    setSelectedIds((prev) => {
      const next = new Set(prev);
      ids.forEach((id) => next.add(id));
      return next;
    });

  if (props.transactions.length === 0) {
    return (
      <Group justify="center">
        <DimmedText size="sm">{t("no_transactions")}</DimmedText>
      </Group>
    );
  }

  return (
    <>
      <Stack gap="0.5rem" pb="var(--bulk-bar-height, 0)">
        {paginatedItems.map((transaction) => (
          <TransactionCard
            key={transaction.id}
            transaction={transaction}
            categories={props.categories}
            elevation={2}
            currency={currency}
            isSelected={selectedIds.has(transaction.id)}
            onToggleSelect={onToggleSelect}
          />
        ))}
        {props.transactions.length > itemsPerPage && (
          <Group justify="center">
            <Pagination
              total={Math.ceil(props.transactions.length / itemsPerPage)}
              value={page}
              onChange={setPage}
            />
          </Group>
        )}
      </Stack>
      <BulkActionBar
        selectedIds={selectedIds}
        currentPageTransactions={paginatedItems}
        onClearSelection={onClearSelection}
        onSelectAll={onSelectAll}
        categories={props.categories}
        zIndex="calc(var(--mantine-z-index-modal) + 1)"
      />
    </>
  );
};

export default TransactionCards;
