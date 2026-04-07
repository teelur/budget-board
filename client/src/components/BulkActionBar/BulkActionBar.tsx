import {
  Button,
  Collapse,
  Flex,
  Group,
  Portal,
  Stack,
  Text,
  ActionIcon,
  Transition,
} from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { TrashIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import CategorySelect from "~/components/core/Select/CategorySelect/CategorySelect";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import { getIsParentCategory, getParentCategory } from "~/helpers/category";
import { getCurrencySymbol } from "~/helpers/currency";
import { translateAxiosError } from "~/helpers/requests";
import { ICategory } from "~/models/category";
import { ITransaction, ITransactionUpdateRequest } from "~/models/transaction";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import SplitTransaction from "~/components/core/Card/TransactionCard/TransactionCardBase/EditableTransactionCardContent/SplitTransaction/SplitTransaction";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import useIsMobile from "~/hooks/useIsMobile";

interface BulkActionBarProps {
  selectedIds: Set<string>;
  currentPageTransactions: ITransaction[];
  onClearSelection: () => void;
  onSelectAll: (ids: string[]) => void;
  categories: ICategory[];
  zIndex?: number | string;
}

const FIELDS = {
  date: "date",
  merchant: "merchant",
  category: "category",
  amount: "amount",
} as const;

const BulkActionBar = (props: BulkActionBarProps): React.ReactNode => {
  const { t } = useTranslation();
  const isMobile = useIsMobile();

  // Hold the bar element in state so the effect re-runs as soon as Mantine's
  // Transition renders it (which happens in a child render cycle, not the same
  // commit as selectedIds changing).
  const [barElement, setBarElement] = React.useState<HTMLDivElement | null>(
    null,
  );
  const barRefCallback = React.useCallback(
    (node: HTMLDivElement | null) => setBarElement(node),
    [],
  );

  React.useEffect(() => {
    if (props.selectedIds.size === 0 || !barElement) {
      document.documentElement.style.setProperty("--bulk-bar-height", "0px");
      return;
    }

    // offsetHeight reads synchronously and includes padding — no async ResizeObserver
    // delay on first mount.
    const update = () =>
      document.documentElement.style.setProperty(
        "--bulk-bar-height",
        `${barElement.offsetHeight}px`,
      );

    update();

    // Keep updating when content changes (e.g. Collapse opens edit fields)
    const observer = new ResizeObserver(update);
    observer.observe(barElement);
    return () => observer.disconnect();
  }, [props.selectedIds.size, barElement]);

  React.useEffect(() => {
    return () => {
      document.documentElement.style.setProperty("--bulk-bar-height", "0px");
    };
  }, []);

  const { request } = useAuth();
  const {
    dayjsLocale,
    dayjs,
    longDateFormat,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();

  const [merchantValue, setMerchantValue] = React.useState("");
  const [categoryValue, setCategoryValue] = React.useState("");
  const [dateValue, setDateValue] = React.useState<Date | null>(null);
  const [amountValue, setAmountValue] = React.useState<number | string>("");
  const [touched, setTouched] = React.useState<Set<string>>(new Set());
  const [showDeleteConfirm, setShowDeleteConfirm] = React.useState(false);

  const touch = (field: string) =>
    setTouched((prev) => new Set(prev).add(field));

  const resetFields = () => {
    setMerchantValue("");
    setCategoryValue("");
    setDateValue(null);
    setAmountValue("");
    setTouched(new Set());
    setShowDeleteConfirm(false);
  };

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

  const queryClient = useQueryClient();

  const doBulkUpdate = useMutation({
    mutationFn: async (requests: ITransactionUpdateRequest[]) => {
      await request({
        url: "/api/transaction/batch",
        method: "PUT",
        data: requests,
      });
    },
    onMutate: async (requests: ITransactionUpdateRequest[]) => {
      await queryClient.cancelQueries({ queryKey: ["transactions"] });
      const previousTransactions: ITransaction[] =
        queryClient.getQueryData(["transactions", { getHidden: false }]) ?? [];
      queryClient.setQueryData(
        ["transactions", { getHidden: false }],
        (oldTransactions: ITransaction[]) =>
          oldTransactions.map((t) => {
            const req = requests.find((r) => r.id === t.id);
            if (!req) return t;
            return {
              ...t,
              amount: req.amount,
              date: req.date,
              category: req.category,
              subcategory: req.subcategory,
              merchantName: req.merchantName,
            };
          }),
      );
      return { previousTransactions };
    },
    onError: (error: AxiosError, _variables, context) => {
      queryClient.setQueryData(
        ["transactions", { getHidden: false }],
        context?.previousTransactions ?? [],
      );
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
    onSettled: async () => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
      await queryClient.invalidateQueries({ queryKey: ["balances"] });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
    },
    onSuccess: () => {
      props.onClearSelection();
      resetFields();
    },
  });

  const doBulkDelete = useMutation({
    mutationFn: async (ids: string[]) => {
      await request({
        url: "/api/transaction/batch",
        method: "DELETE",
        data: ids,
      });
    },
    onMutate: async (ids: string[]) => {
      await queryClient.cancelQueries({ queryKey: ["transactions"] });
      const previousTransactions: ITransaction[] =
        queryClient.getQueryData(["transactions", { getHidden: false }]) ?? [];
      queryClient.setQueryData(
        ["transactions", { getHidden: false }],
        (oldTransactions: ITransaction[]) =>
          oldTransactions.filter((t) => !ids.includes(t.id)),
      );
      return { previousTransactions };
    },
    onError: (error: AxiosError, _variables, context) => {
      queryClient.setQueryData(
        ["transactions", { getHidden: false }],
        context?.previousTransactions ?? [],
      );
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
    onSettled: async () => {
      await queryClient.invalidateQueries({ queryKey: ["transactions"] });
      await queryClient.invalidateQueries({ queryKey: ["balances"] });
      await queryClient.invalidateQueries({ queryKey: ["accounts"] });
      await queryClient.invalidateQueries({ queryKey: ["institutions"] });
    },
    onSuccess: () => {
      props.onClearSelection();
      resetFields();
    },
  });

  const allTransactions: ITransaction[] =
    queryClient.getQueryData(["transactions", { getHidden: false }]) ?? [];
  const selectedTransactions = allTransactions.filter((t) =>
    props.selectedIds.has(t.id),
  );
  const singleSelected =
    selectedTransactions.length === 1 ? selectedTransactions[0] : null;

  // When a single transaction is selected, pre-populate all fields with its data
  React.useEffect(() => {
    if (singleSelected) {
      const categoryVal =
        (singleSelected.subcategory ?? "").length > 0
          ? (singleSelected.subcategory ?? "")
          : (singleSelected.category ?? "");
      setDateValue(new Date(singleSelected.date));
      setMerchantValue(singleSelected.merchantName ?? "");
      setCategoryValue(categoryVal);
      setAmountValue(singleSelected.amount);
      setTouched(new Set(Object.values(FIELDS)));
    } else {
      resetFields();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [singleSelected?.id]);

  const handleApply = () => {
    const requests: ITransactionUpdateRequest[] = selectedTransactions.map(
      (t) => ({
        id: t.id,
        amount: touched.has(FIELDS.amount) ? (amountValue as number) : t.amount,
        date: touched.has(FIELDS.date) ? dateValue! : new Date(t.date),
        merchantName: touched.has(FIELDS.merchant)
          ? merchantValue
          : t.merchantName,
        category: touched.has(FIELDS.category)
          ? getParentCategory(categoryValue, props.categories)
          : t.category,
        subcategory: touched.has(FIELDS.category)
          ? getIsParentCategory(categoryValue, props.categories)
            ? ""
            : categoryValue
          : t.subcategory,
      }),
    );
    doBulkUpdate.mutate(requests);
  };

  const handleDeleteClick = () => {
    if (props.selectedIds.size > 1) {
      setShowDeleteConfirm(true);
    } else {
      doBulkDelete.mutate(Array.from(props.selectedIds));
    }
  };

  const handleDateChange = (val: string | null) => {
    if (!val) {
      setDateValue(null);
      setTouched((prev) => {
        const next = new Set(prev);
        next.delete(FIELDS.date);
        return next;
      });
      return;
    }
    const parsed = dayjs(val);
    if (!parsed.isValid()) {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("invalid_date"),
      });
      return;
    }
    setDateValue(parsed.toDate());
    touch(FIELDS.date);
  };

  const isAmountInvalid =
    touched.has(FIELDS.amount) && typeof amountValue !== "number";
  const isApplyDisabled =
    touched.size === 0 || props.selectedIds.size === 0 || isAmountInvalid;
  const isPending = doBulkUpdate.isPending || doBulkDelete.isPending;

  return (
    <Portal>
      <Transition
        mounted={props.selectedIds.size > 0}
        transition="slide-up"
        duration={200}
        timingFunction="ease"
      >
        {(transitionStyles) => (
          <Stack
            ref={barRefCallback}
            gap="0.5rem"
            p="0.75rem"
            style={{
              ...transitionStyles,
              position: "fixed",
              bottom: 0,
              left: isMobile ? 0 : "var(--app-shell-navbar-width, 60px)",
              right: 0,
              zIndex: props.zIndex ?? 200,
              backgroundColor: "var(--background-color-surface)",
              borderTop: "2px solid var(--surface-color-border)",
              boxShadow: "0 -4px 12px rgba(0, 0, 0, 0.1)",
            }}
          >
            {/* Selection controls row */}
            <Group gap="0.5rem" wrap="wrap">
              <Text size="sm" fw={600}>
                {t("n_selected", { count: props.selectedIds.size })}
              </Text>
              <Button
                size="compact-xs"
                variant="subtle"
                onClick={() =>
                  props.onSelectAll(
                    props.currentPageTransactions.map((t) => t.id),
                  )
                }
              >
                {t("select_all")} ({props.currentPageTransactions.length})
              </Button>
              <Button
                size="compact-xs"
                variant="subtle"
                onClick={() => {
                  props.onClearSelection();
                  resetFields();
                }}
              >
                {t("clear_selection")}
              </Button>
            </Group>

            {/* Fields + actions row */}
            <Flex gap="0.5rem" wrap="wrap" align="flex-end">
              <DateInput
                label={<Text size="xs">{t("date")}</Text>}
                value={dateValue}
                valueFormat={longDateFormat}
                locale={dayjsLocale}
                onChange={handleDateChange}
                clearable
                w={190}
                elevation={1}
              />
              <TextInput
                label={<Text size="xs">{t("merchant_name")}</Text>}
                value={merchantValue}
                onChange={(e) => {
                  setMerchantValue(e.currentTarget.value);
                  touch(FIELDS.merchant);
                }}
                placeholder={t("enter_merchant_name")}
                miw={180}
                style={{ flex: "1 1 180px" }}
                elevation={1}
              />
              <CategorySelect
                label={<Text size="xs">{t("category")}</Text>}
                categories={props.categories}
                value={categoryValue || null}
                onChange={(val) => {
                  setCategoryValue(val);
                  touch(FIELDS.category);
                }}
                withinPortal
                w={220}
                elevation={1}
              />
              <NumberInput
                label={<Text size="xs">{t("amount")}</Text>}
                value={amountValue}
                onChange={(val) => {
                  setAmountValue(val);
                  if (typeof val === "number") touch(FIELDS.amount);
                  else
                    setTouched((prev) => {
                      const next = new Set(prev);
                      next.delete(FIELDS.amount);
                      return next;
                    });
                }}
                prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
                thousandSeparator={thousandsSeparator}
                decimalSeparator={decimalSeparator}
                decimalScale={2}
                fixedDecimalScale
                w={140}
                elevation={1}
              />

              <Group
                gap="0.5rem"
                align="flex-end"
                style={{ marginLeft: "auto" }}
              >
                {singleSelected && (
                  <SplitTransaction
                    id={singleSelected.id}
                    originalAmount={singleSelected.amount}
                    categories={props.categories}
                    elevation={1}
                  />
                )}
                <ActionIcon
                  color="var(--button-color-destructive)"
                  onClick={handleDeleteClick}
                  loading={doBulkDelete.isPending}
                  title={t("delete_transactions")}
                >
                  <TrashIcon size="1rem" />
                </ActionIcon>
                <Button
                  size="compact-sm"
                  variant="subtle"
                  onClick={() => {
                    props.onClearSelection();
                    resetFields();
                  }}
                >
                  {t("cancel")}
                </Button>
                <Button
                  size="compact-sm"
                  disabled={isApplyDisabled}
                  loading={doBulkUpdate.isPending}
                  onClick={handleApply}
                >
                  {t("apply_changes")}
                </Button>
              </Group>
            </Flex>

            {/* Inline delete confirmation */}
            <Collapse expanded={showDeleteConfirm}>
              <Group gap="0.5rem" align="center">
                <Text size="sm">
                  {t("confirm_delete_transactions_message", {
                    count: props.selectedIds.size,
                  })}
                </Text>
                <Button
                  size="compact-sm"
                  color="var(--button-color-destructive)"
                  loading={isPending}
                  onClick={() => {
                    doBulkDelete.mutate(Array.from(props.selectedIds));
                    setShowDeleteConfirm(false);
                  }}
                >
                  {t("delete")}
                </Button>
                <Button
                  size="compact-sm"
                  variant="subtle"
                  onClick={() => setShowDeleteConfirm(false)}
                >
                  {t("cancel")}
                </Button>
              </Group>
            </Collapse>
          </Stack>
        )}
      </Transition>
    </Portal>
  );
};

export default BulkActionBar;
