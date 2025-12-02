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
import dayjs from "dayjs";
import TextInput from "~/components/Input/TextInput/TextInput";
import NumberInput from "~/components/Input/NumberInput/NumberInput";
import AccountSelect from "~/components/Select/AccountSelect/AccountSelect";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/Text/DimmedText/DimmedText";
import DateInput from "~/components/Input/DateInput/DateInput";

interface FormValues {
  goalName: string;
  goalAccounts: string[];
  goalAmount: number;
  goalCompleteDate: string | null;
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
        color: "red",
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
          label={<PrimaryText size="sm">Goal Name</PrimaryText>}
          placeholder="Enter goal name"
          key={form.key("goalName")}
          {...form.getInputProps("goalName")}
          elevation={1}
        />
        <AccountSelect
          label={<PrimaryText size="sm">Accounts</PrimaryText>}
          placeholder="Select account"
          key={form.key("goalAccounts")}
          {...form.getInputProps("goalAccounts")}
          elevation={1}
        />
        <NumberInput
          label={<PrimaryText size="sm">Target Amount</PrimaryText>}
          placeholder="Enter target amount"
          prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
          min={0}
          decimalScale={2}
          thousandSeparator=","
          key={form.key("goalAmount")}
          {...form.getInputProps("goalAmount")}
          elevation={1}
        />
        <Switch
          label={
            <DimmedText size="sm">
              Apply existing account balance towards goal target?
            </DimmedText>
          }
          key={form.key("goalApplyAccountAmount")}
          {...form.getInputProps("goalApplyAccountAmount")}
        />
        <Stack gap={5}>
          <PrimaryText size="sm">Create a goal with a specified:</PrimaryText>
          <Tabs variant="outline" defaultValue="completeDate">
            <Tabs.List>
              <Tabs.Tab value="completeDate">
                <PrimaryText size="sm">Complete Date</PrimaryText>
              </Tabs.Tab>
              <Tabs.Tab value="monthlyContribution">
                <PrimaryText size="sm">Monthly Contribution</PrimaryText>
              </Tabs.Tab>
            </Tabs.List>
            <Tabs.Panel value="completeDate">
              <DateInput
                label={<PrimaryText size="sm">Complete Date</PrimaryText>}
                placeholder="Choose an end date"
                clearable
                key={form.key("goalCompleteDate")}
                {...form.getInputProps("goalCompleteDate")}
                elevation={1}
              />
            </Tabs.Panel>
            <Tabs.Panel value="monthlyContribution">
              <NumberInput
                label={
                  <PrimaryText size="sm">Monthly Contribution</PrimaryText>
                }
                placeholder="Enter monthly contribution"
                prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
                min={0}
                decimalScale={2}
                thousandSeparator=","
                key={form.key("goalMonthlyContribution")}
                {...form.getInputProps("goalMonthlyContribution")}
                elevation={1}
              />
            </Tabs.Panel>
          </Tabs>
        </Stack>
        <Button type="submit">Create Goal</Button>
      </Stack>
    </form>
  );
};

export default SaveGoalForm;
