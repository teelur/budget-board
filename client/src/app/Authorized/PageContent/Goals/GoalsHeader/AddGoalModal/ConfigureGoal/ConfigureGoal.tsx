import React from "react";
import { GoalType } from "~/models/goal";
import { Button, Group, Stack } from "@mantine/core";
import { MoveLeftIcon, MoveRightIcon } from "lucide-react";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import AccountMultiSelect from "~/components/core/Select/AccountMultiSelect/AccountMultiSelect";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import { useField } from "@mantine/form";
import SaveGoalOptions from "./SaveGoalOptions/SaveGoalOptions";
import { useDidUpdate } from "@mantine/hooks";

export interface IGoalConfiguration {
  name: string;
  accounts: string[];
  targetAmount: number;
  applyAccountAmount: boolean;
}

interface ConfigureGoalProps {
  selectedGoalType: string | null;
  goalConfiguration: IGoalConfiguration;
  setGoalConfiguration: React.Dispatch<
    React.SetStateAction<IGoalConfiguration>
  >;
  goBackToPreviousDialog: () => void;
  launchNextDialog: () => void;
}

const ConfigureGoal = (props: ConfigureGoalProps): React.ReactNode => {
  const { t } = useTranslation();

  const goalNameField = useField<string>({
    initialValue: props.goalConfiguration.name,
  });
  const goalAccountsField = useField<string[]>({
    initialValue: props.goalConfiguration.accounts,
  });
  const goalTargetAmountField = useField<number>({
    initialValue: props.goalConfiguration.targetAmount,
  });
  const goalApplyAccountAmountField = useField<boolean>({
    initialValue: props.goalConfiguration.applyAccountAmount,
  });

  useDidUpdate(() => {
    goalNameField.reset();
    goalAccountsField.reset();
    goalTargetAmountField.reset();
    goalApplyAccountAmountField.reset();
    props.setGoalConfiguration({
      name: "",
      accounts: [],
      targetAmount: 0,
      applyAccountAmount: false,
    });
  }, [props.selectedGoalType]);

  useDidUpdate(() => {
    props.setGoalConfiguration({
      name: goalNameField.getValue(),
      accounts: goalAccountsField.getValue(),
      targetAmount: goalTargetAmountField.getValue(),
      applyAccountAmount: goalApplyAccountAmountField.getValue(),
    });
  }, [
    goalNameField.getValue(),
    goalAccountsField.getValue(),
    goalTargetAmountField.getValue(),
    goalApplyAccountAmountField.getValue(),
  ]);

  const isNextButtonDisabled = () => {
    let disabled =
      goalNameField.getValue().trim() === "" ||
      goalAccountsField.getValue().length === 0;

    if (props.selectedGoalType === GoalType.SaveGoal) {
      disabled = disabled || goalTargetAmountField.getValue() <= 0;
    }

    return disabled;
  };

  return (
    <Stack gap={"1rem"}>
      <Stack gap={"0.5rem"}>
        <TextInput
          label={<PrimaryText size="sm">{t("goal_name")}</PrimaryText>}
          placeholder={t("enter_goal_name")}
          {...goalNameField.getInputProps()}
          elevation={1}
        />
        <AccountMultiSelect
          label={<PrimaryText size="sm">{t("accounts")}</PrimaryText>}
          {...goalAccountsField.getInputProps()}
          elevation={1}
        />
        {props.selectedGoalType === GoalType.SaveGoal && (
          <SaveGoalOptions
            targetAmountField={goalTargetAmountField}
            applyAccountAmountField={goalApplyAccountAmountField}
          />
        )}
      </Stack>
      <Group w="100%">
        <Button flex="1 1 auto" onClick={() => props.goBackToPreviousDialog()}>
          {<MoveLeftIcon size={16} />}
        </Button>
        <Button
          flex="1 1 auto"
          disabled={isNextButtonDisabled()}
          onClick={() => {
            props.setGoalConfiguration({
              name: goalNameField.getValue(),
              accounts: goalAccountsField.getValue(),
              targetAmount: goalTargetAmountField.getValue(),
              applyAccountAmount: goalApplyAccountAmountField.getValue(),
            });
            props.launchNextDialog();
          }}
        >
          {<MoveRightIcon size={16} />}
        </Button>
      </Group>
    </Stack>
  );
};

export default ConfigureGoal;
