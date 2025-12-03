import { ActionIcon } from "@mantine/core";
import { GoalType } from "~/models/goal";
import React from "react";
import SaveGoalForm from "./SaveGoalForm/SaveGoalForm";
import PayGoalForm from "./PayGoalForm/PayGoalForm";
import { PlusIcon } from "lucide-react";
import { useDisclosure } from "@mantine/hooks";
import Modal from "~/components/core/Modal/Modal";
import Select from "~/components/core/Select/Select/Select";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

const goalTypes: { label: string; value: string }[] = [
  { label: "Grow my funds", value: GoalType.SaveGoal },
  { label: "Pay off debt", value: GoalType.PayGoal },
];

const AddGoalModal = (): React.ReactNode => {
  const [selectedGoalType, setSelectedGoalType] = React.useState<string | null>(
    null
  );

  const [isOpen, { open, close }] = useDisclosure();

  return (
    <>
      <ActionIcon size="input-sm" onClick={open}>
        <PlusIcon />
      </ActionIcon>
      <Modal
        opened={isOpen}
        onClose={close}
        title={<PrimaryText size="md">Add Goal</PrimaryText>}
      >
        <Select
          data={goalTypes}
          placeholder="I want to set a goal to..."
          value={selectedGoalType}
          onChange={(value) => setSelectedGoalType(value)}
          elevation={1}
        />
        {selectedGoalType === GoalType.SaveGoal && <SaveGoalForm />}
        {selectedGoalType === GoalType.PayGoal && <PayGoalForm />}
      </Modal>
    </>
  );
};

export default AddGoalModal;
