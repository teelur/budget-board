import classes from "./BudgetParentCard.module.css";

import {
  convertNumberToCurrency,
  getCurrencySymbol,
  SignDisplay,
} from "~/helpers/currency";
import {
  ActionIcon,
  Button,
  Flex,
  Group,
  LoadingOverlay,
  Popover as MantinePopover,
  Stack,
} from "@mantine/core";
import { IBudget, IBudgetUpdateRequest } from "~/models/budget";
import React from "react";
import { useDisclosure } from "@mantine/hooks";
import { useField } from "@mantine/form";
import { PencilIcon, TrashIcon } from "lucide-react";
import { StatusColorType } from "~/helpers/budgets";
import { areStringsEqual, roundAwayFromZero } from "~/helpers/utils";
import { ICategoryNode } from "~/models/category";
import BudgetChildCard from "./BudgetChildCard/BudgetChildCard";
import UnbudgetChildCard from "./UnbudgetChildCard/UnbudgetChildCard";
import { notifications } from "@mantine/notifications";
import { useQueryClient, useMutation, useQuery } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { IUserSettings } from "~/models/userSettings";
import Card from "~/components/core/Card/Card";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import Popover from "~/components/core/Popover/Popover";
import Progress from "~/components/core/Progress/Progress";
import { ProgressType } from "~/components/core/Progress/ProgressBase/ProgressBase";
import { useTranslation, Trans } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

export interface BudgetParentCardProps {
  categoryTree: ICategoryNode;
  categoryToBudgetsMap: Map<string, IBudget[]>;
  categoryToLimitsMap: Map<string, number>;
  categoryToTransactionsTotalMap: Map<string, number>;
  selectedDate: Date | null;
  openDetails: (category: string, month: Date | null) => void;
}

const BudgetParentCard = (props: BudgetParentCardProps): React.ReactNode => {
  const [isSelected, { toggle, close }] = useDisclosure(false);

  const { t } = useTranslation();
  const { dayjs, intlLocale, thousandsSeparator, decimalSeparator } =
    useLocale();

  const isIncome = areStringsEqual(props.categoryTree.value, "income");
  const limit =
    props.categoryToLimitsMap.get(props.categoryTree.value.toLowerCase()) ?? 0;
  const amount =
    props.categoryToTransactionsTotalMap.get(
      props.categoryTree.value.toLowerCase(),
    ) ?? 0;

  const budgets =
    props.categoryToBudgetsMap.get(props.categoryTree.value.toLowerCase()) ??
    [];
  const id =
    budgets.length === 1 && props.selectedDate ? (budgets[0]?.id ?? "") : "";

  const newLimitField = useField<number | string>({
    initialValue: limit ?? 0,
    validate: (value) => (value !== "" ? null : t("invalid_limit")),
  });

  const percentComplete = roundAwayFromZero(
    (((props.categoryToTransactionsTotalMap.get(
      props.categoryTree.value.toLowerCase(),
    ) ?? 0) *
      (isIncome ? 1 : -1)) /
      limit) *
      100,
  );

  const { request } = useAuth();

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

  const queryClient = useQueryClient();
  const doEditBudget = useMutation({
    mutationFn: async (newBudget: IBudgetUpdateRequest) =>
      await request({
        url: "/api/budget",
        method: "PUT",
        data: newBudget,
      }),
    onMutate: async (variables: IBudgetUpdateRequest) => {
      await queryClient.cancelQueries({ queryKey: ["budgets"] });

      const previousBudgets: IBudget[] =
        queryClient.getQueryData(["budgets"]) ?? [];

      queryClient.setQueryData(["budgets"], (oldBudgets: IBudget[]) =>
        oldBudgets?.map((oldBudget) =>
          oldBudget.id === variables.id
            ? { ...oldBudget, limit: variables.limit }
            : oldBudget,
        ),
      );

      return { previousBudgets };
    },
    onError: (error: AxiosError, _variables: IBudgetUpdateRequest, context) => {
      queryClient.setQueryData(["budgets"], context?.previousBudgets ?? []);
      notifications.show({
        message: translateAxiosError(error),
        color: "var(--button-color-destructive)",
      });
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: ["budgets"] }),
  });

  const doDeleteBudget = useMutation({
    mutationFn: async (id: string) =>
      await request({
        url: "/api/budget",
        method: "DELETE",
        params: { guid: id },
      }),
    onSuccess: async () =>
      await queryClient.invalidateQueries({ queryKey: ["budgets"] }),
  });

  const handleEdit = (newLimit?: number | string) => {
    if (newLimit === "") {
      return;
    }
    if (id.length === 0) {
      return;
    }
    doEditBudget.mutate({
      id,
      limit: Number(newLimit),
    });
  };

  const childLimitsTotal = props.categoryTree.subCategories.reduce(
    (acc, subCategory) => {
      const limit =
        props.categoryToLimitsMap.get(subCategory.value.toLowerCase()) ?? 0;
      return acc + limit;
    },
    0,
  );

  interface ChildCards {
    budgetChildCards: React.ReactNode[];
    unbudgetChildCards: React.ReactNode[];
  }

  const buildChildren = (): ChildCards => {
    const budgetChildCards: React.ReactNode[] = [];
    const unbudgetChildCards: React.ReactNode[] = [];

    props.categoryTree.subCategories.forEach((subCategory) => {
      if (
        props.categoryToBudgetsMap.has(subCategory.value.toLocaleLowerCase())
      ) {
        const budgets =
          props.categoryToBudgetsMap.get(
            subCategory.value.toLocaleLowerCase(),
          ) ?? [];
        const budgetId =
          budgets.length === 1 && props.selectedDate
            ? (budgets[0]?.id ?? "")
            : "";
        budgetChildCards.push(
          <BudgetChildCard
            key={subCategory.value}
            id={budgetId}
            categoryValue={subCategory.value}
            amount={
              props.categoryToTransactionsTotalMap.get(
                subCategory.value.toLowerCase(),
              ) ?? 0
            }
            limit={
              props.categoryToLimitsMap.get(subCategory.value.toLowerCase()) ??
              0
            }
            isIncome={isIncome}
            selectedDate={props.selectedDate ?? dayjs().toDate()}
            openDetails={props.openDetails}
          />,
        );
      } else if (
        props.categoryToTransactionsTotalMap.has(
          subCategory.value.toLocaleLowerCase(),
        )
      ) {
        unbudgetChildCards.push(
          <UnbudgetChildCard
            key={subCategory.value}
            category={subCategory.value}
            amount={
              props.categoryToTransactionsTotalMap.get(
                subCategory.value.toLowerCase(),
              ) ?? 0
            }
            selectedDate={props.selectedDate}
            isIncome={isIncome}
            openDetails={props.openDetails}
          />,
        );
      }
    });

    return { budgetChildCards, unbudgetChildCards };
  };

  const { budgetChildCards, unbudgetChildCards } = buildChildren();

  return (
    <Card p="0.25rem" w="100%" elevation={1}>
      <Stack gap="0.25rem">
        <Card
          p="0.25rem 0.5rem"
          onClick={() => {
            if (id.length > 0) {
              props.openDetails(props.categoryTree.value, props.selectedDate);
            }
          }}
          hoverEffect
          elevation={2}
        >
          <LoadingOverlay
            visible={doEditBudget.isPending || doDeleteBudget.isPending}
          />
          <Group gap="0.75rem" align="flex-start" wrap="nowrap">
            <Stack gap={0} w="100%">
              <Group
                justify="space-between"
                align="center"
                style={{ containerType: "inline-size" }}
                gap={0}
              >
                <Group gap="0.25rem" align="center">
                  <PrimaryText className={classes.title}>
                    {props.categoryTree.value}
                  </PrimaryText>
                  <ActionIcon
                    variant={isSelected ? "outline" : "transparent"}
                    size="md"
                    onClick={(e) => {
                      e.stopPropagation();
                      if (id.length > 0) {
                        newLimitField.setValue(limit);
                        toggle();
                      }
                    }}
                  >
                    <PencilIcon size={16} />
                  </ActionIcon>
                </Group>
                <Group gap="0.5rem" justify="flex-end" align="center">
                  {isSelected ? (
                    <>
                      <Trans
                        i18nKey="budget_amount_fraction_editable_total_styled"
                        values={{
                          amount: convertNumberToCurrency(
                            amount * (isIncome ? 1 : -1),
                            false,
                            userSettingsQuery.data?.currency ?? "USD",
                            SignDisplay.Auto,
                            intlLocale,
                          ),
                        }}
                        components={[
                          <PrimaryText className={classes.text} key="amount" />,
                          <DimmedText size="sm" key="of" />,
                        ]}
                      />
                      <Flex onClick={(e) => e.stopPropagation()}>
                        <NumberInput
                          {...newLimitField.getInputProps()}
                          onBlur={() => handleEdit(newLimitField.getValue())}
                          thousandSeparator={thousandsSeparator}
                          decimalSeparator={decimalSeparator}
                          min={childLimitsTotal}
                          max={999999}
                          step={1}
                          prefix={getCurrencySymbol(
                            userSettingsQuery.data?.currency,
                          )}
                          placeholder={t("enter_limit")}
                          size="xs"
                          styles={{
                            root: {
                              maxWidth: "100px",
                            },
                            input: {
                              padding: "0 10px",
                              fontSize: "16px",
                            },
                          }}
                          elevation={2}
                        />
                      </Flex>
                    </>
                  ) : (
                    <Trans
                      i18nKey="budget_amount_fraction_styled"
                      values={{
                        amount: convertNumberToCurrency(
                          amount * (isIncome ? 1 : -1),
                          false,
                          userSettingsQuery.data?.currency ?? "USD",
                          SignDisplay.Auto,
                          intlLocale,
                        ),
                        total: convertNumberToCurrency(
                          limit,
                          false,
                          userSettingsQuery.data?.currency ?? "USD",
                          SignDisplay.Auto,
                          intlLocale,
                        ),
                      }}
                      components={[
                        <PrimaryText className={classes.text} key="amount" />,
                        <DimmedText size="sm" key="of" />,
                        <PrimaryText className={classes.text} key="total" />,
                      ]}
                    />
                  )}
                </Group>
              </Group>
              <Group
                gap="0.25rem"
                justify="flex-end"
                align="baseline"
                style={{ containerType: "inline-size" }}
              >
                <Flex style={{ flex: "1 1 auto" }}>
                  <Progress
                    size={16}
                    percentComplete={percentComplete}
                    amount={amount}
                    limit={limit}
                    type={isIncome ? ProgressType.Income : ProgressType.Expense}
                    warningThreshold={
                      userSettingsQuery.data?.budgetWarningThreshold ?? 80
                    }
                    elevation={2}
                  />
                </Flex>
                <Trans
                  i18nKey="budget_left_styled"
                  values={{
                    amount: convertNumberToCurrency(
                      roundAwayFromZero(limit - amount * (isIncome ? 1 : -1)),
                      false,
                      userSettingsQuery.data?.currency ?? "USD",
                      SignDisplay.Auto,
                      intlLocale,
                    ),
                  }}
                  components={[
                    <StatusText
                      amount={roundAwayFromZero(amount)}
                      total={limit}
                      type={
                        isIncome
                          ? StatusColorType.Income
                          : StatusColorType.Expense
                      }
                      warningThreshold={
                        userSettingsQuery.data?.budgetWarningThreshold ?? 80
                      }
                      size="md"
                      key="amount"
                    />,
                    <DimmedText size="md" key="amount" />,
                  ]}
                />
              </Group>
            </Stack>
            {isSelected && (
              <Flex
                style={{ alignSelf: "stretch" }}
                onClick={(e) => e.stopPropagation()}
              >
                <Popover>
                  <MantinePopover.Target>
                    <ActionIcon
                      color="var(--button-color-destructive)"
                      h="100%"
                    >
                      <TrashIcon size="1rem" />
                    </ActionIcon>
                  </MantinePopover.Target>
                  <MantinePopover.Dropdown p="0.5rem" maw={200}>
                    <Stack gap={5}>
                      <PrimaryText size="sm">
                        {t("confirm_delete_budget_message")}
                      </PrimaryText>
                      <DimmedText size="xs">
                        {t("all_children_will_also_be_deleted")}
                      </DimmedText>
                      <Button
                        color="var(--button-color-destructive)"
                        size="compact-xs"
                        onClick={() => {
                          doDeleteBudget.mutate(id);
                          close();
                        }}
                      >
                        {t("delete")}
                      </Button>
                    </Stack>
                  </MantinePopover.Dropdown>
                </Popover>
              </Flex>
            )}
          </Group>
        </Card>
        {props.categoryTree.subCategories.length > 0 &&
          (budgetChildCards.length > 0 || unbudgetChildCards.length > 0) && (
            <Stack gap="0.25rem">
              {budgetChildCards.length > 0 && budgetChildCards}
              {unbudgetChildCards.length > 0 && unbudgetChildCards}
            </Stack>
          )}
      </Stack>
    </Card>
  );
};

export default BudgetParentCard;
