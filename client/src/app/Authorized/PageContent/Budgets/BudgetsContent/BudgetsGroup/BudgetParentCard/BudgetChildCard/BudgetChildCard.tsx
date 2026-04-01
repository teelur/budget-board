import classes from "./BudgetChildCard.module.css";

import {
  convertNumberToCurrency,
  getCurrencySymbol,
  SignDisplay,
} from "~/helpers/currency";
import { ActionIcon, Flex, Group, LoadingOverlay, Stack } from "@mantine/core";
import { IBudget, IBudgetUpdateRequest } from "~/models/budget";
import React from "react";
import { useField } from "@mantine/form";
import { CornerDownRight, PencilIcon, TrashIcon } from "lucide-react";
import { StatusColorType } from "~/helpers/budgets";
import { roundAwayFromZero } from "~/helpers/utils";
import { useDisclosure } from "@mantine/hooks";
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
import Progress from "~/components/core/Progress/Progress";
import { ProgressType } from "~/components/core/Progress/ProgressBase/ProgressBase";
import { Trans, useTranslation } from "react-i18next";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface BudgetChildCardProps {
  id: string;
  categoryValue: string;
  amount: number;
  limit: number;
  isIncome: boolean;
  selectedDate: Date;
  openDetails: (category: string, month: Date) => void;
}

const BudgetChildCard = (props: BudgetChildCardProps): React.ReactNode => {
  const [isSelected, { toggle }] = useDisclosure(false);

  const { t } = useTranslation();
  const { intlLocale, thousandsSeparator, decimalSeparator } = useLocale();
  const { request } = useAuth();

  const newLimitField = useField<number | string>({
    initialValue: props.limit ?? 0,
    validate: (value) => (value !== "" ? null : t("invalid_limit")),
  });

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
    if (props.id.length === 0) {
      return;
    }
    doEditBudget.mutate({
      id: props.id,
      limit: Number(newLimit),
    });
  };

  const percentComplete = roundAwayFromZero(
    ((props.amount * (props.isIncome ? 1 : -1)) / props.limit) * 100,
  );

  return (
    <Group wrap="nowrap">
      <CornerDownRight />
      <Card
        p="0.25rem 0.5rem"
        w="100%"
        onClick={() => {
          if (props.id.length > 0) {
            props.openDetails(props.categoryValue, props.selectedDate);
          }
        }}
        hoverEffect={!isSelected}
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
            >
              <Group gap="0.25rem" align="center">
                <PrimaryText className={classes.title}>
                  {props.categoryValue}
                </PrimaryText>
                <ActionIcon
                  variant={isSelected ? "outline" : "transparent"}
                  size="sm"
                  onClick={(e) => {
                    e.stopPropagation();
                    if (props.id.length > 0) {
                      newLimitField.setValue(props.limit);
                      toggle();
                    }
                  }}
                >
                  <PencilIcon size={16} />
                </ActionIcon>
              </Group>
              <Group gap="0.25rem" justify="flex-end" align="center">
                {isSelected ? (
                  <>
                    <Trans
                      i18nKey="budget_amount_fraction_editable_total_styled"
                      values={{
                        amount: convertNumberToCurrency(
                          props.amount * (props.isIncome ? 1 : -1),
                          false,
                          userSettingsQuery.data?.currency ?? "USD",
                          SignDisplay.Auto,
                          intlLocale,
                        ),
                        total: convertNumberToCurrency(
                          props.limit,
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
                        min={0}
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
                        key="total-edit"
                        elevation={2}
                      />
                    </Flex>
                  </>
                ) : (
                  <Trans
                    i18nKey="budget_amount_fraction_styled"
                    values={{
                      amount: convertNumberToCurrency(
                        props.amount * (props.isIncome ? 1 : -1),
                        false,
                        userSettingsQuery.data?.currency ?? "USD",
                        SignDisplay.Auto,
                        intlLocale,
                      ),
                      total: convertNumberToCurrency(
                        props.limit,
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
              align="center"
              style={{ containerType: "inline-size" }}
            >
              <Flex style={{ flex: "1 1 auto" }}>
                <Progress
                  size={14}
                  percentComplete={percentComplete}
                  amount={props.amount}
                  limit={props.limit}
                  type={
                    props.isIncome ? ProgressType.Income : ProgressType.Expense
                  }
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
                    roundAwayFromZero(
                      props.limit - props.amount * (props.isIncome ? 1 : -1),
                    ),
                    false,
                    userSettingsQuery.data?.currency ?? "USD",
                    SignDisplay.Auto,
                    intlLocale,
                  ),
                }}
                components={[
                  <StatusText
                    amount={roundAwayFromZero(props.amount)}
                    total={props.limit}
                    type={
                      props.isIncome
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
            <Group style={{ alignSelf: "stretch" }}>
              <ActionIcon
                color="var(--button-color-destructive)"
                onClick={(e) => {
                  e.stopPropagation();
                  doDeleteBudget.mutate(props.id);
                }}
                h="100%"
              >
                <TrashIcon size="1rem" />
              </ActionIcon>
            </Group>
          )}
        </Group>
      </Card>
    </Group>
  );
};

export default BudgetChildCard;
