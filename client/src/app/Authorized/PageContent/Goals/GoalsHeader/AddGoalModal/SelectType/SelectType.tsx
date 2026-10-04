import { Flex, Group, Stack } from "@mantine/core";
import { Button, Select } from "@teelur/budget-board-ui";
import { MoveRightIcon } from "lucide-react";
import { useTranslation } from "react-i18next";
import { GoalType } from "~/models/goal";

interface SelectTypeProps {
  selectedGoalType: string | null;
  setSelectedGoalType: (goalType: string | null) => void;
  launchNextDialog: () => void;
}

const SelectType = (props: SelectTypeProps): React.ReactNode => {
  const { t } = useTranslation();

  const goalTypes: { label: string; value: string }[] = [
    { label: t("grow_my_funds"), value: GoalType.SaveGoal },
    { label: t("pay_off_debt"), value: GoalType.PayGoal },
  ];

  return (
    <Stack>
      <Select
        data={goalTypes}
        label={t("i_want_to_set_a_goal_to")}
        value={props.selectedGoalType}
        onChange={(value) => props.setSelectedGoalType(value)}
      />
      <Group w="100%">
        <Flex flex={"1 1 auto"} />
        <Button
          variant="filled"
          color="primary"
          size="sm"
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

export default SelectType;
