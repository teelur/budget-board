import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { Button, LoadingOverlay, Stack, Switch, Tabs } from "@mantine/core";
import { hasLength, useForm } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { IGoalCreateRequest } from "~/models/goal";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import { getCurrencySymbol } from "~/helpers/currency";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import { useTranslation } from "react-i18next";
import AccountMultiSelect from "~/components/core/Select/AccountMultiSelect/AccountMultiSelect";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface FormValues {
  goalName: string;
  goalAccounts: string[];
  goalAmount: number;
  goalCompleteDate: Date | null;
  goalMonthlyContribution: string | number;
  goalApplyAccountAmount: boolean;
}

const SaveGoalForm = (): React.ReactNode => {
  const form = useForm<FormValues>({
    mode: "uncontrolled",
    initialValues: {
      goalName: "",
      goalAccounts: [],
      goalAmount: 0,
      goalCompleteDate: null,
      goalMonthlyContribution: "",
      goalApplyAccountAmount: false,
    },

    validate: {
      goalAccounts: hasLength({ min: 1 }),
    },
  });

  const { t } = useTranslation();
  const {
    dayjs,
    dayjsLocale,
    longDateFormat,
    thousandsSeparator,
    decimalSeparator,
  } = useLocale();
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
  const doAddGoal = useMutation({
    mutationFn: async (newGoal: IGoalCreateRequest) =>
      await request({
        url: "/api/goal",
        method: "POST",
        data: newGoal,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["goals"] });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const submitGoal = (values: FormValues): any => {
    const parsedCompleteDate = dayjs(values.goalCompleteDate);

    const newGoal: IGoalCreateRequest = {
      name: values.goalName,
      completeDate: parsedCompleteDate.isValid()
        ? parsedCompleteDate.toDate()
        : null,
      amount: values.goalAmount,
      initialAmount: values.goalApplyAccountAmount ? 0 : null,
      monthlyContribution:
        values.goalMonthlyContribution === ""
          ? null
          : (values.goalMonthlyContribution as number),
      accountIds: values.goalAccounts,
    };

    doAddGoal.mutate(newGoal);
  };

  return (
    <form onSubmit={form.onSubmit((values) => submitGoal(values))}>
      <LoadingOverlay visible={doAddGoal.isPending} />
      <Stack gap="sm">
        <TextInput
          label={<PrimaryText size="sm">{t("goal_name")}</PrimaryText>}
          placeholder={t("enter_goal_name")}
          key={form.key("goalName")}
          {...form.getInputProps("goalName")}
          elevation={1}
        />
        <AccountMultiSelect
          label={<PrimaryText size="sm">{t("accounts")}</PrimaryText>}
          key={form.key("goalAccounts")}
          {...form.getInputProps("goalAccounts")}
          elevation={1}
        />
        <NumberInput
          label={<PrimaryText size="sm">{t("target_amount")}</PrimaryText>}
          placeholder={t("enter_target_amount")}
          prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
          min={0}
          decimalScale={2}
          thousandSeparator={thousandsSeparator}
          decimalSeparator={decimalSeparator}
          key={form.key("goalAmount")}
          {...form.getInputProps("goalAmount")}
          elevation={1}
        />
        <Switch
          label={
            <DimmedText size="sm">
              {t("apply_existing_account_amount_to_goal")}
            </DimmedText>
          }
          key={form.key("goalApplyAccountAmount")}
          {...form.getInputProps("goalApplyAccountAmount")}
        />
        <Stack gap={5}>
          <PrimaryText size="sm">
            {t("create_a_goal_with_a_specified")}
          </PrimaryText>
          <Tabs variant="outline" defaultValue="completeDate">
            <Tabs.List>
              <Tabs.Tab value="completeDate">
                <PrimaryText size="sm">{t("complete_date")}</PrimaryText>
              </Tabs.Tab>
              <Tabs.Tab value="monthlyContribution">
                <PrimaryText size="sm">{t("monthly_contribution")}</PrimaryText>
              </Tabs.Tab>
            </Tabs.List>
            <Tabs.Panel value="completeDate">
              <DateInput
                label={
                  <PrimaryText size="sm">{t("complete_date")}</PrimaryText>
                }
                placeholder={t("select_a_completion_date")}
                clearable
                key={form.key("goalCompleteDate")}
                {...form.getInputProps("goalCompleteDate")}
                locale={dayjsLocale}
                valueFormat={longDateFormat}
                elevation={1}
              />
            </Tabs.Panel>
            <Tabs.Panel value="monthlyContribution">
              <NumberInput
                label={
                  <PrimaryText size="sm">
                    {t("monthly_contribution")}
                  </PrimaryText>
                }
                placeholder={t("enter_monthly_contribution")}
                prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
                min={0}
                decimalScale={2}
                thousandSeparator={thousandsSeparator}
                decimalSeparator={decimalSeparator}
                key={form.key("goalMonthlyContribution")}
                {...form.getInputProps("goalMonthlyContribution")}
                elevation={1}
              />
            </Tabs.Panel>
          </Tabs>
        </Stack>
        <Button type="submit">{t("create_goal")}</Button>
      </Stack>
    </form>
  );
};

export default SaveGoalForm;
