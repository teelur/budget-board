import AccountSelectInput from "~/components/AccountSelectInput";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  Button,
  LoadingOverlay,
  NumberInput,
  Stack,
  Tabs,
  Text,
  TextInput,
} from "@mantine/core";
import { DatePickerInput, DateValue } from "@mantine/dates";
import { hasLength, useForm } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { IGoalCreateRequest } from "~/models/goal";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import React from "react";

interface FormValues {
  goalName: string;
  goalAccounts: string[];
  goalCompleteDate: DateValue;
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

  const { request } = React.useContext<any>(AuthContext);

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
    const newGoal: IGoalCreateRequest = {
      name: values.goalName,
      completeDate: values.goalCompleteDate,
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
          label="Goal Name"
          placeholder="Enter goal name"
          required
          key={form.key("goalName")}
          {...form.getInputProps("goalName")}
        />
        <AccountSelectInput
          label="Accounts"
          placeholder="Select account"
          required
          key={form.key("goalAccounts")}
          {...form.getInputProps("goalAccounts")}
        />
        <Stack gap={5}>
          <Text size="sm">Create a goal with a specified:</Text>
          <Tabs variant="outline" defaultValue="completeDate">
            <Tabs.List>
              <Tabs.Tab value="completeDate">Complete Date</Tabs.Tab>
              <Tabs.Tab value="monthlyContribution">
                Monthly Contribution
              </Tabs.Tab>
            </Tabs.List>
            <Tabs.Panel value="completeDate">
              <DatePickerInput
                label="Complete Date"
                placeholder="Choose an end date"
                clearable
                key={form.key("goalCompleteDate")}
                {...form.getInputProps("goalCompleteDate")}
              />
            </Tabs.Panel>
            <Tabs.Panel value="monthlyContribution">
              <NumberInput
                label="Monthly Contribution"
                placeholder="Enter monthly contribution"
                prefix="$"
                min={0}
                decimalScale={2}
                thousandSeparator=","
                key={form.key("goalMonthlyContribution")}
                {...form.getInputProps("goalMonthlyContribution")}
              />
            </Tabs.Panel>
          </Tabs>
        </Stack>
        <Button type="submit">Create Goal</Button>
      </Stack>
    </form>
  );
};

export default PayGoalForm;
