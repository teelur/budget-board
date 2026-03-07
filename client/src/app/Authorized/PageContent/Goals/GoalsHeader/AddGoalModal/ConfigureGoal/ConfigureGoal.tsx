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

interface ConfigureGoalProps {
  selectedGoalType: string | null;
  goBackToPreviousDialog: () => void;
  launchNextDialog: () => void;
}

const ConfigureGoal = (props: ConfigureGoalProps): React.ReactNode => {
  const { t } = useTranslation();

  const goalNameField = useField<string>({
    initialValue: "",
  });
  const goalAccountsField = useField<string[]>({
    initialValue: [],
  });
  const goalTargetAmountField = useField<number>({
    initialValue: 0,
  });
  const goalApplyAccountAmountField = useField<boolean>({
    initialValue: false,
  });

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
          onClick={() => props.launchNextDialog()}
          disabled={props.selectedGoalType === null}
        >
          {<MoveRightIcon size={16} />}
        </Button>
      </Group>
    </Stack>
  );
};

export default ConfigureGoal;
