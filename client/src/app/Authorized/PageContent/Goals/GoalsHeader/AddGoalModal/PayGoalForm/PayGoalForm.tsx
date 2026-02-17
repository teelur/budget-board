import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import { Button, LoadingOverlay, Stack, Tabs } from "@mantine/core";
import { hasLength, useForm } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { IGoalCreateRequest } from "~/models/goal";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import React from "react";
import { getCurrencySymbol } from "~/helpers/currency";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DateInput from "~/components/core/Input/DateInput/DateInput";
import NumberInput from "~/components/core/Input/NumberInput/NumberInput";
import { useTranslation } from "react-i18next";
import AccountMultiSelect from "~/components/core/Select/AccountMultiSelect/AccountMultiSelect";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface FormValues {
  goalName: string;
  goalAccounts: string[];
  goalCompleteDate: Date | null;
  goalMonthlyContribution: string | number;
}

const PayGoalForm = (): React.ReactNode => {
  const form = useForm<FormValues>({
    mode: "uncontrolled",
    initialValues: {
      goalName: "",
      goalAccounts: [],
      goalCompleteDate: null,
      goalMonthlyContribution: "",
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
      amount: 0,
      initialAmount: null,
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
        <Stack gap="0.5rem">
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

export default PayGoalForm;
